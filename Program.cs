using MySqlConnector;
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
    string sql = "SELECT name, location FROM Hotels";

using var command = new MySqlCommand(sql, connection);
using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    string name = reader["name"].ToString() ?? "";
    string location = reader["location"].ToString() ?? "";

    Console.WriteLine($"{name} - {location}");
}
    Console.WriteLine("Anslutningen till databasen fungerar!");
}
catch (Exception ex)
{
    Console.WriteLine("Kunde inte ansluta till databasen: " + ex.Message);
}
