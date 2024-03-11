/// <summary>
/// Репозиторий.
/// </summary>
interface IRepository
{
	/// <summary>
	/// Асинхронное чтение данных.
	/// </summary>
	/// <typeparam name="T">Наследник класса <see cref="Entity"/>.</typeparam>
	/// <param name="columns">Колонки для выборки.</param>
	/// <returns>Массив данных.</returns>
	public Task<List<T>> ReadAsync<T>(HashSet<string>? columns) where T : Entity;
}