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

    // Hämta kunder från databasen
    string customerSql = "SELECT id, name FROM customers ORDER BY id";

    using var customerCommand = new MySqlCommand(customerSql, connection);
    using var customerReader = await customerCommand.ExecuteReaderAsync();

    Console.WriteLine("\nVälj kund:");

    while (await customerReader.ReadAsync())
    {
        Console.WriteLine($"{customerReader["id"]}. {customerReader["name"]}");
    }

    await customerReader.CloseAsync();

    // Välj kund
    Console.Write("Ange kundens nummer: ");
    string customerInput = Console.ReadLine() ?? "";

    if (!int.TryParse(customerInput, out int customerId))
    {
        Console.WriteLine("Ange ett giltigt kundnummer.");
        return;
    }

    // Kontrollera att kunden finns
    string checkCustomerSql = "SELECT COUNT(*) FROM customers WHERE id = @customerId";

    using var checkCustomerCommand = new MySqlCommand(checkCustomerSql, connection);
    checkCustomerCommand.Parameters.AddWithValue("@customerId", customerId);

    int customerExists = Convert.ToInt32(await checkCustomerCommand.ExecuteScalarAsync());

    if (customerExists == 0)
    {
        Console.WriteLine("Kunden finns inte.");
        return;
    }

    // Ange sökuppgifter
    Console.Write("Ange ort: ");
    string location = Console.ReadLine() ?? "";

    Console.Write("Hur många personer ska bo i rummet? ");
    string input = Console.ReadLine() ?? "";

    if (!int.TryParse(input, out int people) || people <= 0)
    {
        Console.WriteLine("Ange ett giltigt antal personer.");
        return;
    }

    // Ange incheckningsdatum
    Console.Write("Ange incheckningsdatum (ÅÅÅÅ-MM-DD): ");
    bool startDateValid = DateTime.TryParseExact(
        Console.ReadLine(),
        "yyyy-MM-dd",
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None,
        out DateTime startDate
    );

    // Ange utcheckningsdatum
    Console.Write("Ange utcheckningsdatum (ÅÅÅÅ-MM-DD): ");
    bool endDateValid = DateTime.TryParseExact(
        Console.ReadLine(),
        "yyyy-MM-dd",
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None,
        out DateTime endDate
    );

    if (!startDateValid || !endDateValid)
    {
        Console.WriteLine("Ogiltigt datum. Skriv datum i formatet ÅÅÅÅ-MM-DD.");
        return;
    }

    if (startDate.Year != 2026 || startDate.Month != 12 ||
        endDate.Year != 2026 || endDate.Month != 12 ||
        startDate.Date < DateTime.Today ||
        endDate.Date <= startDate.Date)
    {
        Console.WriteLine("Ogiltiga datum. Välj datum i december 2026 och kontrollera utcheckningen.");
        return;
    }

    // Hämta lediga rum från databasen
    string sql = @"
        SELECT Hotels.name, Hotels.location, rooms.room_number, rooms.capacity, rooms.price_per_night
        FROM rooms
        JOIN Hotels ON rooms.hotel_id = Hotels.id
        WHERE Hotels.location = @location
        AND NOT EXISTS (
            SELECT 1
            FROM bookings
            WHERE bookings.room_id = rooms.id
            AND bookings.cancelled = 0
            AND bookings.start_date < @endDate
            AND bookings.end_date > @startDate
        )
        ORDER BY Hotels.name, rooms.room_number";

    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@location", location);
    command.Parameters.AddWithValue("@startDate", startDate);
    command.Parameters.AddWithValue("@endDate", endDate);

    using var reader = await command.ExecuteReaderAsync();

    var availableRooms = new List<(string HotelName, int RoomNumber, int Capacity, decimal Price)>();

    while (await reader.ReadAsync())
    {
        availableRooms.Add((
            reader["name"].ToString() ?? "",
            Convert.ToInt32(reader["room_number"]),
            Convert.ToInt32(reader["capacity"]),
            Convert.ToDecimal(reader["price_per_night"])
        ));
    }

    await reader.CloseAsync();

    // Visa rum som rymmer hela sällskapet
    bool foundSingleRoom = false;

    foreach (var room in availableRooms)
    {
        if (room.Capacity >= people)
        {
            foundSingleRoom = true;
            Console.WriteLine($"{room.HotelName} - Rum {room.RoomNumber} - {room.Capacity} personer - {room.Price} kr/natt");
        }
    }

    // Om inget enskilt rum räcker, sök kombinationer på samma hotell
    if (!foundSingleRoom)
    {
        var combinations = new List<(string HotelName, int Room1, int Capacity1, decimal Price1,
            int Room2, int Capacity2, decimal Price2, decimal TotalPrice)>();

        for (int i = 0; i < availableRooms.Count; i++)
        {
            for (int j = i + 1; j < availableRooms.Count; j++)
            {
                var room1 = availableRooms[i];
                var room2 = availableRooms[j];

                if (room1.HotelName == room2.HotelName &&
                    room1.Capacity + room2.Capacity >= people)
                {
                    combinations.Add((
                        room1.HotelName,
                        room1.RoomNumber,
                        room1.Capacity,
                        room1.Price,
                        room2.RoomNumber,
                        room2.Capacity,
                        room2.Price,
                        room1.Price + room2.Price
                    ));
                }
            }
        }

        var sortedCombinations = combinations.OrderBy(c => c.TotalPrice);

        foreach (var combination in sortedCombinations)
        {
            Console.WriteLine($"Alternativ på {combination.HotelName}:");
            Console.WriteLine($"Rum {combination.Room1} – {combination.Capacity1} personer – {combination.Price1} kr/natt");
            Console.WriteLine($"Rum {combination.Room2} – {combination.Capacity2} personer – {combination.Price2} kr/natt");
            Console.WriteLine($"Totalpris: {combination.TotalPrice} kr/natt");
            Console.WriteLine();
        }

        if (combinations.Count == 0)
        {
            Console.WriteLine("Inga lämpliga rum eller rumskombinationer hittades.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Kunde inte ansluta till databasen: " + ex.Message);
}