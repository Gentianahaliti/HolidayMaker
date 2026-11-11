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
    string sql = @"
    SELECT Hotels.name, rooms.room_number, rooms.capacity, rooms.price_per_night
    FROM rooms
    JOIN Hotels ON rooms.hotel_id = Hotels.id
    ORDER BY Hotels.name, rooms.room_number";

using var command = new MySqlCommand(sql, connection);
using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    string name = reader["name"].ToString() ?? "";
    string roomNumber = reader["room_number"].ToString() ?? "";
    string capacity = reader["capacity"].ToString() ?? "";
    string price = reader["price_per_night"].ToString() ?? "";

    Console.WriteLine($"{name} - Rum {roomNumber} - {capacity} personer - {price} kr/natt");
}
    Console.WriteLine("Anslutningen till databasen fungerar!");
}
catch (Exception ex)
{
    Console.WriteLine("Kunde inte ansluta till databasen: " + ex.Message);
}
