/// <summary>
/// Город.
/// </summary>
internal class City : Entity
{
	/// <summary>
	/// Название.
	/// </summary>
	public string Name { get; set; } = string.Empty;
	
	/// <summary>
	/// Создаёт город.
	/// </summary>
	public City() {}

	/// <summary>
	/// Создаёт город.
	/// </summary>
	/// <param name="name">Название.</param>
	public City(string name) 
	{
		ArgumentException.ThrowIfNullOrEmpty(name);
		Name = name;
	}
}