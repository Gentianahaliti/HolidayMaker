using MySqlConnector;
using Microsoft.Extensions.Configuration;
using HolidayMaker.Data;
using System.Globalization;

class Program
{
    static async Task Main()
    {
        // Hämtar anslutningssträngen från User Secrets.
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string? connectionString =
            configuration.GetConnectionString("DefaultConnection");

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

            bool running = true;

            while (running)
            {
                Console.WriteLine("\n=== HOLIDAY MAKER ===");
                Console.WriteLine("1. Välj kund");
                Console.WriteLine("2. Registrera ny kund");
                Console.WriteLine("3. Avsluta");
                Console.Write("Välj: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        int customerId = await SelectCustomer(connection);

                        if (customerId != 0)
                        {
                            await CustomerMenu(connection, customerId);
                        }
                        break;

                    case "2":
                        await RegisterCustomer(connection);
                        break;

                    case "3":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Ogiltigt val.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ett fel uppstod: " + ex.Message);
        }
    }

    // Visar kunderna och låter användaren välja en.
    static async Task<int> SelectCustomer(MySqlConnection connection)
    {
        string sql = "SELECT id, name FROM customers ORDER BY id";

        using var command = new MySqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        Console.WriteLine("\n=== KUNDER ===");

        while (await reader.ReadAsync())
        {
            Console.WriteLine($"{reader["id"]}. {reader["name"]}");
        }

        await reader.CloseAsync();

        Console.Write("Ange kundens nummer: ");

        if (!int.TryParse(Console.ReadLine(), out int customerId))
        {
            Console.WriteLine("Ange ett giltigt kundnummer.");
            return 0;
        }

        string checkSql = "SELECT COUNT(*) FROM customers WHERE id = @id";

        using var checkCommand = new MySqlCommand(checkSql, connection);
        checkCommand.Parameters.AddWithValue("@id", customerId);

        int exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

        if (exists == 0)
        {
            Console.WriteLine("Kunden finns inte.");
            return 0;
        }

        return customerId;
    }

    // Registrerar en ny kund.
    static async Task RegisterCustomer(MySqlConnection connection)
    {
        Console.WriteLine("\n=== REGISTRERA KUND ===");

        Console.Write("Namn: ");
        string name = Console.ReadLine() ?? "";

        Console.Write("Adress: ");
        string address = Console.ReadLine() ?? "";

        Console.Write("E-post: ");
        string email = Console.ReadLine() ?? "";

        Console.Write("Telefon: ");
        string phone = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(address) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) ||
            !email.Contains('@'))
        {
            Console.WriteLine("Kontrollera att alla uppgifter är ifyllda och att e-posten är giltig.");
            return;
        }

        string sql = @"
            INSERT INTO customers (name, address, email, phone)
            VALUES (@name, @address, @email, @phone)";

        using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@address", address);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@phone", phone);

        try
        {
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Kunden har registrerats!");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            Console.WriteLine("E-postadressen finns redan registrerad.");
        }
    }

    // Menyn som visas när en kund har valts.
    static async Task CustomerMenu(
        MySqlConnection connection,
        int customerId)
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n=== KUNDMENY ===");
            Console.WriteLine("1. Sök och boka rum");
            Console.WriteLine("2. Visa mina bokningar");
            Console.WriteLine("3. Ändra antal personer");
            Console.WriteLine("4. Avboka bokning");
            Console.WriteLine("5. Tillbaka");
            Console.Write("Välj: ");

            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    await SearchAndBook(connection, customerId);
                    break;

                case "2":
                    await ShowBookings(connection, customerId);
                    break;

                case "3":
                    await ChangeOccupants(connection, customerId);
                    break;

                case "4":
                    await CancelBooking(connection, customerId);
                    break;

                case "5":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Ogiltigt val.");
                    break;
            }
        }
    }

    // Söker efter rum som är lediga under hela perioden.
    static async Task SearchAndBook(
        MySqlConnection connection,
        int customerId)
    {
        Console.Write("\nAnge ort: ");
        string location = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(location))
        {
            Console.WriteLine("Du måste ange en ort.");
            return;
        }

        Console.Write("Hur många personer ska bo? ");

        if (!int.TryParse(Console.ReadLine(), out int people) || people <= 0)
        {
            Console.WriteLine("Ange ett giltigt antal personer.");
            return;
        }

        if (!ReadDate("Incheckningsdatum", out DateTime startDate) ||
            !ReadDate("Utcheckningsdatum", out DateTime endDate))
        {
            Console.WriteLine("Ogiltigt datum. Använd formatet ÅÅÅÅ-MM-DD.");
            return;
        }

        // Alla bokningar ska vara under december 2026.
        if (startDate.Year != 2026 || startDate.Month != 12 ||
            endDate.Year != 2026 || endDate.Month != 12 ||
            startDate.Date < DateTime.Today ||
            endDate.Date <= startDate.Date)
        {
            Console.WriteLine("Välj giltiga datum i december 2026.");
            return;
        }

        Console.Write("Måste hotellet ha pool? (j/n): ");
        bool pool = ReadYesNo();

        Console.Write("Måste hotellet ha restaurang? (j/n): ");
        bool restaurant = ReadYesNo();

        Console.Write("Max avstånd till stranden i meter (Enter = inget krav): ");
        string beachInput = Console.ReadLine() ?? "";
        int? maxBeach = null;

        if (beachInput != "")
        {
            if (!int.TryParse(beachInput, out int beach) || beach < 0)
            {
                Console.WriteLine("Ange ett giltigt avstånd.");
                return;
            }

            maxBeach = beach;
        }

        Console.Write("Max avstånd till centrum i meter (Enter = inget krav): ");
        string centerInput = Console.ReadLine() ?? "";
        int? maxCenter = null;

        if (centerInput != "")
        {
            if (!int.TryParse(centerInput, out int center) || center < 0)
            {
                Console.WriteLine("Ange ett giltigt avstånd.");
                return;
            }

            maxCenter = center;
        }

        Console.WriteLine("\nSortera efter:");
        Console.WriteLine("1. Pris – billigast först");
        Console.WriteLine("2. Hotellbetyg – högst först");
        Console.Write("Välj: ");

        string sortChoice = Console.ReadLine() ?? "";

        // Här väljs endast mellan två fasta sorteringsalternativ.
        string orderBy = sortChoice == "2"
            ? "Hotels.rating DESC"
            : "rooms.price_per_night ASC";

        string sql = $@"
            SELECT
                rooms.id,
                Hotels.name,
                Hotels.rating,
                Hotels.distance_to_beach,
                Hotels.distance_to_center,
                rooms.room_number,
                rooms.capacity,
                rooms.price_per_night
            FROM rooms
            JOIN Hotels ON rooms.hotel_id = Hotels.id
            WHERE Hotels.location = @location
            AND rooms.capacity >= @people

            AND (@maxBeach IS NULL
                OR Hotels.distance_to_beach <= @maxBeach)

            AND (@maxCenter IS NULL
                OR Hotels.distance_to_center <= @maxCenter)

            AND (@pool = 0 OR EXISTS (
                SELECT 1
                FROM hotel_x_amenities hxa
                JOIN Amenities a ON hxa.amenities_id = a.id
                WHERE hxa.hotel_id = Hotels.id
                AND a.name = 'Pool'
            ))

            AND (@restaurant = 0 OR EXISTS (
                SELECT 1
                FROM hotel_x_amenities hxa
                JOIN Amenities a ON hxa.amenities_id = a.id
                WHERE hxa.hotel_id = Hotels.id
                AND a.name = 'Restaurant'
            ))

            AND NOT EXISTS (
                SELECT 1
                FROM bookings
                WHERE bookings.room_id = rooms.id
                AND bookings.cancelled = 0
                AND bookings.start_date < @endDate
                AND bookings.end_date > @startDate
            )

            ORDER BY {orderBy}";

        using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@location", location);
        command.Parameters.AddWithValue("@people", people);
        command.Parameters.AddWithValue("@startDate", startDate);
        command.Parameters.AddWithValue("@endDate", endDate);
        command.Parameters.AddWithValue("@pool", pool ? 1 : 0);
        command.Parameters.AddWithValue("@restaurant", restaurant ? 1 : 0);
        command.Parameters.AddWithValue("@maxBeach",
            maxBeach.HasValue ? maxBeach.Value : DBNull.Value);
        command.Parameters.AddWithValue("@maxCenter",
            maxCenter.HasValue ? maxCenter.Value : DBNull.Value);

        using var reader = await command.ExecuteReaderAsync();

        var availableRooms = new List<RoomOption>();

        while (await reader.ReadAsync())
        {
            availableRooms.Add(new RoomOption
            {
                Id = Convert.ToInt32(reader["id"]),
                HotelName = reader["name"].ToString() ?? "",
                RoomNumber = Convert.ToInt32(reader["room_number"]),
                Capacity = Convert.ToInt32(reader["capacity"]),
                Price = Convert.ToDecimal(reader["price_per_night"]),
                Rating = Convert.ToDecimal(reader["rating"])
            });
        }

        await reader.CloseAsync();

        if (availableRooms.Count == 0)
        {
            Console.WriteLine("Inga lediga rum hittades.");
            return;
        }

        Console.WriteLine("\n=== LEDIGA RUM ===");

        foreach (var room in availableRooms)
        {
            Console.WriteLine(
                $"ID: {room.Id} | {room.HotelName} | Rum {room.RoomNumber} | " +
                $"{room.Capacity} personer | {room.Price} kr/natt | " +
                $"Betyg: {room.Rating}");
        }

        Console.Write("\nAnge ID för rummet du vill boka: ");

        if (!int.TryParse(Console.ReadLine(), out int roomId))
        {
            Console.WriteLine("Ogiltigt rums-ID.");
            return;
        }

        RoomOption? selectedRoom =
            availableRooms.FirstOrDefault(r => r.Id == roomId);

        if (selectedRoom == null)
        {
            Console.WriteLine("Rummet finns inte i sökresultatet.");
            return;
        }

        int nights = (endDate - startDate).Days;
        decimal totalPrice = selectedRoom.Price * nights;

        Console.WriteLine($"\nAntal nätter: {nights}");
        Console.WriteLine($"Totalpris: {totalPrice} kr");

        Console.Write("Vill du genomföra bokningen? (j/n): ");

        if (!ReadYesNo())
        {
            Console.WriteLine("Bokningen avbröts.");
            return;
        }

        // Kontrollerar att rummet fortfarande är ledigt innan det bokas.
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            string checkSql = @"
                SELECT COUNT(*)
                FROM bookings
                WHERE room_id = @roomId
                AND cancelled = 0
                AND start_date < @endDate
                AND end_date > @startDate";

            using var checkCommand = new MySqlCommand(
                checkSql, connection, transaction);

            checkCommand.Parameters.AddWithValue("@roomId", roomId);
            checkCommand.Parameters.AddWithValue("@startDate", startDate);
            checkCommand.Parameters.AddWithValue("@endDate", endDate);

            int occupied = Convert.ToInt32(
                await checkCommand.ExecuteScalarAsync());

            if (occupied > 0)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("Rummet har redan blivit bokat.");
                return;
            }

            string insertSql = @"
                INSERT INTO bookings
                (customer_id, room_id, start_date, end_date,
                 total_price, cancelled, number_of_occupants)
                VALUES
                (@customerId, @roomId, @startDate, @endDate,
                 @totalPrice, 0, @people)";

            using var insertCommand = new MySqlCommand(
                insertSql, connection, transaction);

            insertCommand.Parameters.AddWithValue("@customerId", customerId);
            insertCommand.Parameters.AddWithValue("@roomId", roomId);
            insertCommand.Parameters.AddWithValue("@startDate", startDate);
            insertCommand.Parameters.AddWithValue("@endDate", endDate);
            insertCommand.Parameters.AddWithValue("@totalPrice", totalPrice);
            insertCommand.Parameters.AddWithValue("@people", people);

            await insertCommand.ExecuteNonQueryAsync();

            long bookingNumber = insertCommand.LastInsertedId;

            await transaction.CommitAsync();

            Console.WriteLine("\nBokningen är genomförd!");
            Console.WriteLine($"Bokningsnummer: {bookingNumber}");
            Console.WriteLine($"Totalpris: {totalPrice} kr");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Visar alla bokningar för den valda kunden.
    static async Task ShowBookings(
        MySqlConnection connection,
        int customerId)
    {
        string sql = @"
            SELECT
                bookings.booking_number,
                Hotels.name,
                rooms.room_number,
                bookings.start_date,
                bookings.end_date,
                bookings.number_of_occupants,
                bookings.total_price,
                bookings.cancelled
            FROM bookings
            JOIN rooms ON bookings.room_id = rooms.id
            JOIN Hotels ON rooms.hotel_id = Hotels.id
            WHERE bookings.customer_id = @customerId
            ORDER BY bookings.start_date";

        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@customerId", customerId);

        using var reader = await command.ExecuteReaderAsync();

        Console.WriteLine("\n=== MINA BOKNINGAR ===");

        bool found = false;

        while (await reader.ReadAsync())
        {
            found = true;

            string status = Convert.ToBoolean(reader["cancelled"])
                ? "Avbokad"
                : "Aktiv";

            Console.WriteLine(
                $"Bokning {reader["booking_number"]} | " +
                $"{reader["name"]} | Rum {reader["room_number"]} | " +
                $"{Convert.ToDateTime(reader["start_date"]):yyyy-MM-dd} till " +
                $"{Convert.ToDateTime(reader["end_date"]):yyyy-MM-dd} | " +
                $"{reader["number_of_occupants"]} personer | " +
                $"{reader["total_price"]} kr | {status}");
        }

        if (!found)
        {
            Console.WriteLine("Du har inga bokningar.");
        }
    }

    // Ändrar antalet personer om det nya antalet ryms i rummet.
    static async Task ChangeOccupants(
        MySqlConnection connection,
        int customerId)
    {
        Console.Write("Ange bokningsnummer: ");

        if (!long.TryParse(Console.ReadLine(), out long bookingNumber))
        {
            Console.WriteLine("Ogiltigt bokningsnummer.");
            return;
        }

        string sql = @"
            SELECT rooms.capacity
            FROM bookings
            JOIN rooms ON bookings.room_id = rooms.id
            WHERE bookings.booking_number = @bookingNumber
            AND bookings.customer_id = @customerId
            AND bookings.cancelled = 0";

        using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@bookingNumber", bookingNumber);
        command.Parameters.AddWithValue("@customerId", customerId);

        object? result = await command.ExecuteScalarAsync();

        if (result == null)
        {
            Console.WriteLine("Ingen aktiv bokning hittades.");
            return;
        }

        int capacity = Convert.ToInt32(result);

        Console.WriteLine($"Rummets maxkapacitet är {capacity} personer.");
        Console.Write("Ange nytt antal personer: ");

        if (!int.TryParse(Console.ReadLine(), out int people) ||
            people <= 0 || people > capacity)
        {
            Console.WriteLine("Antalet personer är inte giltigt för rummet.");
            return;
        }

        string updateSql = @"
            UPDATE bookings
            SET number_of_occupants = @people
            WHERE booking_number = @bookingNumber
            AND customer_id = @customerId
            AND cancelled = 0";

        using var updateCommand = new MySqlCommand(updateSql, connection);

        updateCommand.Parameters.AddWithValue("@people", people);
        updateCommand.Parameters.AddWithValue("@bookingNumber", bookingNumber);
        updateCommand.Parameters.AddWithValue("@customerId", customerId);

        await updateCommand.ExecuteNonQueryAsync();

        Console.WriteLine("Antalet personer har ändrats.");
    }

    // Avbokar en bokning genom att ta bort den ur databasen.
static async Task CancelBooking(
    MySqlConnection connection,
    int customerId)
{
    Console.Write("Ange bokningsnummer: ");

    if (!long.TryParse(Console.ReadLine(), out long bookingNumber))
    {
        Console.WriteLine("Ogiltigt bokningsnummer.");
        return;
    }

    Console.Write("Vill du verkligen avboka? (j/n): ");

    if (!ReadYesNo())
    {
        Console.WriteLine("Avbokningen avbröts.");
        return;
    }

    string sql = @"
        DELETE FROM bookings
        WHERE booking_number = @bookingNumber
        AND customer_id = @customerId";

    using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue("@bookingNumber", bookingNumber);
    command.Parameters.AddWithValue("@customerId", customerId);

    int rows = await command.ExecuteNonQueryAsync();

    if (rows > 0)
    {
        Console.WriteLine("Bokningen har avbokats.");
    }
    else
    {
        Console.WriteLine("Ingen bokning hittades.");
    }
}

    // Läser ett datum i formatet ÅÅÅÅ-MM-DD.
    static bool ReadDate(string label, out DateTime date)
    {
        Console.Write($"{label} (ÅÅÅÅ-MM-DD): ");

        return DateTime.TryParseExact(
            Console.ReadLine(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date
        );
    }

    // Läser ett ja- eller nej-svar.
    static bool ReadYesNo()
    {
        string answer = (Console.ReadLine() ?? "").ToLower();

        return answer == "j" || answer == "ja";
    }
}

// Används för att spara information om rummen som hittas i sökningen.
class RoomOption
{
    public int Id { get; set; }
    public string HotelName { get; set; } = "";
    public int RoomNumber { get; set; }
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public decimal Rating { get; set; }
}