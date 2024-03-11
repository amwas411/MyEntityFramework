/// <summary>
/// Персона.
/// </summary>
internal class Person : Entity
{
	/// <summary>
	/// Имя.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Фамилия.
	/// </summary>
	public string Surname { get; set; } = string.Empty;

	/// <summary>
	/// Номер паспорта.
	/// </summary>
	public string? PassportNumber { get; set; }
	
	/// <summary>
	/// Возраст.
	/// </summary>
	public int Age 
	{ 
		get => _age;
		set 
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(Age), "cannot be negative");
			}
			_age = value;
		} 
	}
	private int _age;

	/// <summary>
	/// Город.
	/// </summary>
	public City? City { get; set; }

	/// <summary>
	/// Создаёт персону.
	/// </summary>
	public Person() {}

	/// <summary>
	/// Создаёт персону.
	/// </summary>
	/// <param name="name">Имя.</param>
	/// <param name="surname">Фамилия.</param>
	public Person(string name, string surname)
	{
		ArgumentException.ThrowIfNullOrEmpty(name);
		ArgumentException.ThrowIfNullOrEmpty(surname);
		Name = name;
		Surname = surname;
	}

	/// <summary>
	/// Получить строковое представление.
	/// </summary>
	/// <returns>Строка со значениями некоторым свойств персоны.</returns>
	public override string ToString()
	{
		return string.Format("Name {0}, Surname {1}, Age {2}, City {3}", Name, Surname, Age, City?.Id);
	}
}