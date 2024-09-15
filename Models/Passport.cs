/// <summary>
/// Паспорт.
/// </summary>
internal class Passport : Entity
{
	/// <summary>
	/// Номер.
	/// </summary>
	public string Number { get; set; } = string.Empty;
	
	/// <summary>
	/// Создаёт паспорт.
	/// </summary>
	public Passport() {}

	/// <summary>
	/// Создаёт паспорт.
	/// </summary>
	/// <param name="number">Номер.</param>
	public Passport(string number) 
	{
		ArgumentException.ThrowIfNullOrEmpty(number);
		Number = number;
	}
}