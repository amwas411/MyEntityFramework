/// <summary>
/// Обёртка сущности.
/// </summary>
/// <param name="entity">Сущность.</param>
internal class WrappedEntity(Entity entity)
{
  /// <summary>
  /// Сущность.
  /// </summary>
  public Entity Entity { get; } = entity;

  /// <summary>
  /// Состояние.
  /// </summary>
  public Guid State { get; set; } = EntityState.Clean;

  /// <summary>
  /// Изменённые свойства.
  /// </summary>
  public HashSet<string> ChangedProperties { get; } = [];

  /// <summary>
  /// Индекс.
  /// </summary>
  public int Index { get; set; }
}