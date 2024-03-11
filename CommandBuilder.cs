using System.Text;
using System.Data.Common;

/// <summary>
/// Построитель команд.
/// </summary>
/// <param name="command">Команда.</param>
internal class CommandBuilder(DbCommand command)
{
	/// <summary>
	/// Команда.
	/// </summary>
	protected DbCommand Command { get; set; } = command ?? throw new ArgumentNullException(nameof(command));

	// Шаблон INSERT запроса.
	private readonly string _insertCommandTemplate = "INSERT INTO \"{0}\" {1} VALUES {2};";
	
	// Шаблон DELETE запроса.
	private readonly string _deleteCommandTemplate = "DELETE FROM \"{0}\" WHERE \"Id\" In ({1});";

	// Шаблон UPDATE запроса.
	private readonly string _updateCommandTemplate = "UPDATE \"{0}\" SET {1} WHERE \"Id\"={2};";

	// Шаблон списка значений в INSERT запросе.
	private readonly string _insertQueryValueTemplate = "({0})";
	
	// Шаблон списка значений в UPDATE запросе.
	private readonly string _updateQueryValueTemplate = "{0}={1}";
	
	// Шаблон названия параметра запроса.
	private readonly string _parameterTemplate = "@P{0}";

	// Счетчик.
	private uint _counter = 0;

	/// <summary>
	/// Сгенерировать название параметра.
	/// </summary>
	/// <returns>Новое название параметра.</returns>
	private string GenerateParameterName()
	{
		return string.Format(_parameterTemplate, _counter++);
	}

	/// <summary>
	/// Добавить команду INSERT.
	/// </summary>
	/// <typeparam name="T">Наследник класса <see cref="Entity"/>.</typeparam>
	/// <param name="entity">Сущность.</param>
	/// <exception cref="ApplicationException">
	/// В типе нет публичных свойств.
	/// В запросе не заполнены параметры или столбцы.
	/// </exception>
	/// <exception cref="NullReferenceException">Не удалось привести свойство к типу <see cref="Entity"/>.</exception>
	public void AppendInsertCommand<T>(T entity) where T : Entity
	{
		ArgumentNullException.ThrowIfNull(entity);

		var entities = new List<T>() {entity};

		var type = entity.GetType();
		var properties = type.GetProperties();

		var columnNames = new Helper().ConvertToCsv(properties.Select(i => {
			if (i.PropertyType.BaseType == typeof(Entity))
			{
				return i.Name + "Id";
			}
			return i.Name;
		}).ToHashSet());
		if (columnNames == string.Empty)
		{
			throw new ApplicationException($"{type.Name} do not have any public properties");
		}
		var queryColumnNames = string.Format(_insertQueryValueTemplate, columnNames);

		var sbValues = new StringBuilder();
		var sbParameters = new StringBuilder();
		foreach (var item in entities)
		{
			foreach (var property in properties)
			{
				object? propertyValue;
				if (property.PropertyType.BaseType == typeof(Entity))
				{
					var value = property.GetValue(item);
					if (value == null)
					{
						propertyValue = DBNull.Value;
					}
					else
					{
						var entityValue = value as Entity ?? throw new NullReferenceException("{property.Name} is not an Entity");
						propertyValue = entityValue.Id;
					}
				}
				else
				{
					propertyValue = property.GetValue(item) ?? DBNull.Value;
				}

				var parameterName = GenerateParameterName();
				sbParameters.AppendFormat("{0},", parameterName);
				var parameter = Command.CreateParameter();
				parameter.ParameterName = parameterName;
				parameter.Value = propertyValue;
				Command.Parameters.Add(parameter);
			}

			var parameterNames = sbParameters.ToString();
			if (parameterNames == string.Empty)
			{
				throw new ApplicationException("Insert query do not have any parameters");
			}
			sbValues.AppendFormat(_insertQueryValueTemplate, parameterNames.Remove(parameterNames.Length - 1));
			sbValues.Append(',');
		}
		var insertValues = sbValues.ToString();
		if (insertValues == string.Empty)
		{
			throw new ApplicationException("Insert query do not have any values");
		}
		insertValues = insertValues.Remove(insertValues.Length - 1);
		Command.CommandText += string.Format(_insertCommandTemplate, 
			entity.GetType().Name, queryColumnNames, insertValues);
	}

	/// <summary>
	/// Добавить команду UPDATE.
	/// </summary>
	/// <param name="wrappedEntity"></param>
	/// <exception cref="NullReferenceException">Не удалось привести свойство к типу <see cref="Entity"/>.</exception>
	/// <exception cref="ApplicationException">Не заполнен UPDATE SET блок.</exception>
	public void AppendUpdateCommand(WrappedEntity wrappedEntity)
	{
		ArgumentNullException.ThrowIfNull(wrappedEntity);

		var entity = wrappedEntity.Entity;
		var type = entity.GetType();
		var commandText = string.Empty;
		var changedProperties = type.GetProperties().Where((i) => wrappedEntity.ChangedProperties.Contains(i.Name));
		if (!changedProperties.Any())
		{
			Console.WriteLine($"Tried to update Id {entity.Id}, but it do not have any changed properties.");
			return;
		}

		var sb = new StringBuilder();

		foreach (var property in changedProperties)
		{
			string? propertyName;
			object? propertyValue;
			if (property.PropertyType.BaseType == typeof(Entity))
			{
				propertyName = property.Name + "Id";
				var value = property.GetValue(entity);
				if (value == null)
				{
					propertyValue = DBNull.Value;
				}
				else
				{
					var entityValue = value as Entity ?? throw new NullReferenceException("{property.Name} is not an Entity");
					propertyValue = entityValue.Id;
				}
			}
			else
			{
				propertyName = property.Name;
				propertyValue = property.GetValue(entity) ?? DBNull.Value;
			}
			var parameterName = GenerateParameterName();
			var parameter = Command.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.Value = propertyValue;
			Command.Parameters.Add(parameter);
			sb.AppendFormat(_updateQueryValueTemplate, propertyName, parameterName);
			sb.Append(',');
		}
		var setBlock = sb.ToString();
		if (setBlock == string.Empty)
		{
			throw new ApplicationException("Update SET block is empty");
		}
		setBlock = setBlock.Remove(setBlock.Length - 1);
		
		var idParameterName = GenerateParameterName();
		var idParameter = Command.CreateParameter();
		idParameter.ParameterName = idParameterName;
		idParameter.Value = entity.Id;
		Command.Parameters.Add(idParameter);
		Command.CommandText += string.Format(_updateCommandTemplate, entity.GetType().Name, setBlock, idParameterName);
		
	}

	/// <summary>
	/// Добавить команду DELETE.
	/// </summary>
	/// <param name="id">Идентификатор сущности дял удаления.</param>
	/// <param name="tableName">Таблица.</param>
	/// <exception cref="ArgumentException">Заданы пустые аргументы.</exception>
	/// <exception cref="ApplicationException">DELETE блок не содержит фильтра по идентфиикатором удаляемых записей.</exception>
	public void AppendDeleteCommand(Guid id, string tableName)
	{
		ArgumentException.ThrowIfNullOrEmpty(tableName);
		if (id == default)
		{
			throw new ArgumentException($"{nameof(id)} cannot be an empty guid");
		}

		var sb = new StringBuilder();
		var ids = new List<Guid>() {id};

		foreach (var item in ids)
		{
			var parameterName = GenerateParameterName();
			sb.AppendFormat("{0},", parameterName);
			var parameter = Command.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.Value = item;
			Command.Parameters.Add(parameter);
		}
		var idsString = sb.ToString();
		if (idsString == string.Empty)
		{
			throw new ApplicationException("Delete query do not have any filter Ids");
		}
		idsString = idsString.Remove(idsString.Length - 1);
		Command.CommandText += string.Format(_deleteCommandTemplate, tableName, idsString);
	}
}