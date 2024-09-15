/// <summary>
/// Сущность.
/// </summary>
internal class Entity
{
	/// <summary>
	/// Идентификатор.
	/// </summary>
	public Guid Id { get; set; } = Guid.NewGuid();

	/// <summary>
	/// Поверхностное копирование (ссылочные свойства указывают на свойства копируемого объекта).
	/// </summary>
	/// <returns>Копия.</returns>
	public Entity ShallowClone()
	{
		return (Entity)MemberwiseClone();
	}

  /// <summary>
  /// Сравнить две сущности.
  /// </summary>
  /// <param name="entity">Сущность.</param>
  /// <returns>true, если сущности равны.</returns>
  public bool IsEqual(Entity entity)
  {
    if (entity == null)
    {
      return false;
    }
    return entity.Id == Id && entity.GetType() == GetType();
  }
}