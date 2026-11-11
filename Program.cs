using Microsoft.Extensions.Configuration;
using HolidayMaker.Data;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string? connectionString = configuration.GetConnectionString("DefaultConnection");

if (connectionString == null)
{
    Console.WriteLine("Anslutningssträngen saknas.");
    return;
}

DatabaseConnection database = new DatabaseConnection(connectionString);

using var connection = database.GetConnection();

try
{
    await connection.OpenAsync();
    Console.WriteLine("Anslutningen till databasen fungerar!");
}
catch (Exception ex)
{
    Console.WriteLine("Kunde inte ansluta till databasen: " + ex.Message);
}
