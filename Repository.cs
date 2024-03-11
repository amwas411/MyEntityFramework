using System.Data;
using System.Data.Common;

/// <summary>
/// Репозиторий.
/// </summary>
/// <param name="connection">Соединение.</param>
internal class Repository(DbConnection connection) : IRepository
{
	// Шаблон SELECT запроса.
	private readonly string _selectCommandTemplate = "SELECT {0} FROM \"{1}\";";

	/// <summary>
	/// Соединение.
	/// </summary>
	public DbConnection Connection { get; } = connection ?? throw new ArgumentNullException($"nameof(connection)");

	/// <summary>
	/// Асинхронное чтение данных.
	/// </summary>
	/// <typeparam name="T">Наследник класса <see cref="Entity"/>.</typeparam>
	/// <param name="columns">Колонки для выборки.</param>
	/// <returns>Массив данных.</returns>
	/// <exception cref="ApplicationException">В сущности нет публичных свойств.</exception>
	/// <exception cref="KeyNotFoundException">В сущности по некоторому ключу не было найдено свойство.</exception>
	/// <exception cref="NullReferenceException">Не удалось создать сущность.</exception>
	public async Task<List<T>> ReadAsync<T>(HashSet<string>? columns) where T : Entity
	{
		var helper = new Helper();
		var columnNames = string.Empty;
		if (columns == null)
		{
			columns = typeof(T).GetProperties().Select((i) => i.Name).ToHashSet();
			columnNames = helper.ConvertToCsv(columns);
			if (columnNames == string.Empty)
			{
				throw new ApplicationException($"{typeof(T).Name} do not have any public properties");
			}
		}
		else 
		{
			columnNames = helper.ConvertToCsv(columns);
			if (columnNames == string.Empty)
			{
				columns = typeof(T).GetProperties().Select((i) => i.Name).ToHashSet();
				columnNames = helper.ConvertToCsv(columns);
				if (columnNames == string.Empty)
				{
					throw new ApplicationException($"{typeof(T).Name} do not have any public properties");
				}
			}
		}
		
		var command = Connection.CreateCommand();
		command.CommandText = string.Format(_selectCommandTemplate, columnNames, typeof(T).Name);
		Connection.Open();
		using var reader = await command.ExecuteReaderAsync();
		
		var result = new List<T>();
		while (reader.Read())
		{
			var entity = Activator.CreateInstance<T>();
			var type = typeof(T);
			foreach (var column in columns)
			{
				var isEntity = column != "Id" && column.Contains("Id");
				var propertyName = isEntity
					? column.Remove(column.Length - 2)
					: column;

				var property = type.GetProperty(propertyName) ?? throw new KeyNotFoundException($"Property {propertyName} not found in {type.Name}");
				var value = reader.GetValue(column);
				object? propertyValue;
				if (value == null || DBNull.Value.Equals(value))
				{
					propertyValue = null;
				}
				else 
				{
					if (isEntity)
					{
						dynamic instance = Activator.CreateInstance(property.PropertyType) 
							?? throw new NullReferenceException($"Could not create {property.PropertyType.Name} instance");
						instance.Id = (Guid)value;
						propertyValue = instance;
					}
					else 
					{
						propertyValue = value;
					}
				}
				property.SetValue(entity, propertyValue);
			}
			result.Add(entity);
		}
		Connection.Close();
		return result;
	}
}