using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false);
var cs = builder.Build().GetConnectionString("pgsql");

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<DbConnection, NpgsqlConnection>((_) => new NpgsqlConnection(cs));

serviceCollection.AddSingleton<IRepository, Repository>();
serviceCollection.AddSingleton<UnitOfWork>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var unitOfWork = serviceProvider.GetService<UnitOfWork>() ?? throw new ApplicationException("Unit of work service could not be created");
var city = new City("Kirov");
var person = new Person("John", "Doe")
{
	PassportNumber = "123",
	Age = 20,
	City = city
};

unitOfWork.Add(city);
unitOfWork.Add(person);

Console.WriteLine("Rows affected: {0}", await unitOfWork.Commit());
foreach (var i in await unitOfWork.GetEntitiesAsync<Person>())
{
	Console.WriteLine(i);
}
