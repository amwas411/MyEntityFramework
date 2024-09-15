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
var passport = new Passport("1234");
var p = new Person("Vasya", "Pupkin", 20, city, passport);
var p2 = new Person("Vasya", "Petrov", 30, city, passport);
unitOfWork.Add([passport,city,p,p2]);
Console.WriteLine("Rows affected: {0}", await unitOfWork.Commit());
await PrintPersons(unitOfWork);

p.Age++;
p2.Age++;
Console.WriteLine("Rows affected: {0}", await unitOfWork.Commit());

await PrintPersons(unitOfWork);

foreach (var i in (await unitOfWork.GetEntitiesAsync<Person>()).FindAll((i) => i.Name == "Vasya"))
{
	i.Age++;
}
Console.WriteLine("Rows affected: {0}", await unitOfWork.Commit());
await PrintPersons(unitOfWork);

p.Age++;
p2.Age++;
Console.WriteLine("Rows affected: {0}", await unitOfWork.Commit());

await PrintPersons(unitOfWork);

unitOfWork.Delete([p,p2,city,passport]);
Console.WriteLine("Rows affected: {0}", await unitOfWork.Commit());

async Task PrintPersons(UnitOfWork unitOfWork)
{
  Console.WriteLine("============");
  foreach (var i in await unitOfWork.GetEntitiesAsync<Person>())
  {
    Console.WriteLine(i);
  }
  Console.WriteLine("============");
}