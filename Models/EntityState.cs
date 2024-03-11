/// <summary>
/// Состояние сущности.
/// </summary>
internal static class EntityState
{
	// Чистое.
	public static readonly Guid Clean = Guid.Parse("00000000-0000-0000-0000-000000000001");

	// Обновление.
	public static readonly Guid Update = Guid.Parse("00000000-0000-0000-0000-000000000002");

	// Добавление.
	public static readonly Guid Add = Guid.Parse("00000000-0000-0000-0000-000000000003");

	// Удаление.
	public static readonly Guid Delete = Guid.Parse("00000000-0000-0000-0000-000000000004");
}