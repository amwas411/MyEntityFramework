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
}