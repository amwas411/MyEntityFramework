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
	/// Паспорт.
	/// </summary>
	public Passport? Passport { get; set; }
	
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
	/// <param name="age">Возраст.</param>
	public Person(string name, string surname, int age)
	{
		ArgumentException.ThrowIfNullOrEmpty(name);
		ArgumentException.ThrowIfNullOrEmpty(surname);
		Name = name;
		Surname = surname;
    Age = age;
	}

	/// <summary>
	/// Создаёт персону.
	/// </summary>
	/// <param name="name">Имя.</param>
	/// <param name="surname">Фамилия.</param>
	/// <param name="age">Возраст.</param>
	/// <param name="city">Город.</param>
	public Person(string name, string surname, int age, City? city) : this(name, surname, age)
	{
    City = city;
	}

	/// <summary>
	/// Создаёт персону.
	/// </summary>
	/// <param name="name">Имя.</param>
	/// <param name="surname">Фамилия.</param>
	/// <param name="age">Возраст.</param>
	/// <param name="city">Город.</param>
	/// <param name="passport">Паспорт.</param>
	public Person(string name, string surname, int age, City? city, Passport? passport) : this(name, surname, age, city)
	{
    Passport = passport;
	}

	/// <summary>
	/// Получить строковое представление.
	/// </summary>
	/// <returns>Строка со значениями свойств персоны.</returns>
	public override string ToString()
	{
		return string.Format("Name {0}, Surname {1}, Age {2}, City {3}, Passport {4}", Name, Surname, Age, City?.Id, Passport?.Id);
	}
}