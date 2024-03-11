using System.Data.Common;
using System.Text;

/// <summary>
/// Единица работы.
/// </summary>
/// <param name="connection">Соединение.</param>
/// <param name="repository">Репозиторий.</param>
internal class UnitOfWork(DbConnection connection, IRepository repository)
{
	/// <summary>
	/// Соединение.
	/// </summary>
	public DbConnection Connection { get; } = connection ?? throw new ArgumentNullException($"nameof(connection)");

	/// <summary>
	/// Репозиторий.
	/// </summary>
	protected IRepository Repository { get; } = repository ?? throw new ArgumentNullException($"nameof(repository)");

	/// <summary>
	/// Массив оберток зарегистрированных сущностей.
	/// </summary>
	protected List<WrappedEntity> WrappedEntities { get; } = [];

	/// <summary>
	/// Массив зарегистрированных сущностей в предыдущем состоянии.
	/// Испольуется для определения изменённых свойств сущности.
	/// </summary>
	protected List<Entity> OldEntities { get; } = [];

	/// <summary>
	/// Регистрация сущности.
	/// </summary>
	/// <param name="entity">Сущность.</param>
	public void Register(Entity entity) 
	{
		ArgumentNullException.ThrowIfNull(entity);
		if (WrappedEntities.Exists((existingEntity) => existingEntity.Entity.Id == entity.Id))
		{
			return;
		}
		
		WrappedEntities.Add(new WrappedEntity(entity));
		OldEntities.Add(entity.ShallowClone());
	}

	/// <summary>
	/// Регистрация сущности.
	/// </summary>
	/// <typeparam name="T">Наследник класса <see cref="Entity"/>.</typeparam>
	/// <param name="entities">Массив сущностей.</param>
	public void Register<T>(List<T> entities) where T : Entity
	{
		ArgumentNullException.ThrowIfNull(entities);
		foreach (var item in entities)
		{
			Register(item);
		}
	}

	/// <summary>
	/// Асинхронное чтение сущностей.
	/// </summary>
	/// <typeparam name="T">Наследник класса <see cref="Entity"/>.</typeparam>
	/// <param name="columns">Список колонок для чтения.</param>
	/// <returns>Список сущностей.</returns>
	public async Task<List<T>> GetEntitiesAsync<T>(HashSet<string>? columns = null) where T : Entity
	{
		if (columns == null)
		{
			columns = typeof(T).GetProperties().Select((i) => {
				if (i.PropertyType.BaseType == typeof(Entity))
				{
					return i.Name + "Id";
				}
				return i.Name;
			}).ToHashSet();
		}
		else
		{
			columns.Add("Id");
		}
		
		var entities = await Repository.ReadAsync<T>(columns);
		Register(entities);
		return entities;
	}

	/// <summary>
	/// Добавить.
	/// </summary>
	/// <param name="entity">Сущность.</param>
	public void Add(Entity entity)
	{
		RegisterAndReturn(entity).State = EntityState.Add;
	}

	/// <summary>
	/// Добавить.
	/// </summary>
	/// <param name="entities">Массив сущностей.</param>
	public void Add(IEnumerable<Entity> entities)
	{
		foreach (var entity in entities)
		{
			Add(entity);
		}
	}

	/// <summary>
	/// Удалить.
	/// </summary>
	/// <param name="entity">Сущность.</param>
	public void Delete(Entity entity)
	{
		RegisterAndReturn(entity).State = EntityState.Delete;
	}

	/// <summary>
	/// Коммит работы.
	/// </summary>
	/// <returns>Количество изменённых сущностей.</returns>
	/// <exception cref="NotImplementedException">Неизвестное состояние сущности.</exception>
	public async Task<int> Commit()
	{
		DetectChanges();
		var command = Connection.CreateCommand();
		var commandBuilder = new CommandBuilder(command);
		
		foreach (var wrappedEntity in WrappedEntities.Where((i) => i.State != EntityState.Clean))
		{
			var entity = wrappedEntity.Entity;
			var sb = new StringBuilder();

			if (wrappedEntity.State == EntityState.Update)
			{
				commandBuilder.AppendUpdateCommand(wrappedEntity);
			}
			else if (wrappedEntity.State == EntityState.Delete)
			{
				commandBuilder.AppendDeleteCommand(entity.Id, entity.GetType().Name);
			}
			else if (wrappedEntity.State == EntityState.Add)
			{
				commandBuilder.AppendInsertCommand(entity);
			}
			else 
			{
				throw new NotImplementedException($"Not implemented state Id {wrappedEntity.State}");
			}
		}

		Console.WriteLine(command.CommandText);
		if (command.CommandText == string.Empty)
		{
			return 0;
		}
		Connection.Open();
		var rowsAffected = await command.ExecuteNonQueryAsync();
		Connection.Close();
		Clear();
		return rowsAffected;
	}

	/// <summary>
	/// Очистить информацию о изменениях зарегистрированных сущностей.
	/// </summary>
	private void Clear()
	{
		OldEntities.Clear();
		foreach (var wrappedEntity in WrappedEntities)
		{
			OldEntities.Add(wrappedEntity.Entity.ShallowClone());
			wrappedEntity.ChangedProperties.Clear(); 
			wrappedEntity.State = EntityState.Clean;
		}
	}

	/// <summary>
	/// Определить изменения зарегистрированных сущностей.
	/// </summary>
	/// <exception cref="IndexOutOfRangeException">Не совпадение длин массивов зарегистрированных сущностей.</exception>
	private void DetectChanges()
	{
		if (WrappedEntities.Count != OldEntities.Count)
		{
			throw new IndexOutOfRangeException(
				string.Format("OldEntities length is {0} and WrappedEntities length is {1}, but these should be equal", 
					OldEntities.Count, WrappedEntities.Count));
		}

		var excludedStates = new Guid[] {EntityState.Add, EntityState.Delete};
		for (int i = 0; i < WrappedEntities.Count; i++)
		{
			if (excludedStates.Contains(WrappedEntities[i].State))
			{
				continue;
			}

			var entity = WrappedEntities[i].Entity;
			var oldEntity = OldEntities[i];

			foreach (var property in entity.GetType().GetProperties())
			{
				if (property.Name == "Id")
				{
					continue;
				}

				if (property.GetValue(entity)?.GetHashCode() != property.GetValue(oldEntity)?.GetHashCode())
				{
					WrappedEntities[i].State = EntityState.Update;
					WrappedEntities[i].ChangedProperties.Add(property.Name);
				}
			}
		}
	}

	/// <summary>
	/// Зарегистрировать сущность и вернуть её.
	/// </summary>
	/// <param name="entity">Сущность.</param>
	/// <returns>Сущность в обертке.</returns>
	/// <exception cref="ApplicationException">Не найдена зарегистрированныя сущность.</exception>
	private WrappedEntity RegisterAndReturn(Entity entity)
	{
		ArgumentNullException.ThrowIfNull(entity);
		Register(entity);
		var wrappedEntity = WrappedEntities.FirstOrDefault((i) => i.Entity.Id == entity.Id);
		if (wrappedEntity == default)
		{
			throw new ApplicationException($"Entity with Id {entity.Id} unexpectedly was not found");
		}
		return wrappedEntity;
	}
}