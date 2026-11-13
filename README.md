# Holiday Maker

Holiday Maker is a simple booking application built in C# that uses a MySQL database. It lets users search for available hotel rooms and manage customer bookings.

## Requirements

To run the application, you need:

- .NET 10 SDK
- A MySQL server
- A MySQL client, such as DataGrip or DVbeaver
- Docker, if you want to run MySQL in a container

## Database

Create the database by running the SQL files in this order:

1. Open and run `create_database.sql`.
2. Open and run `insert_testdata.sql`.

The test data file contains:

- 5 hotels
- 20 rooms
- 5 customers
- 25 bookings

Run the test data file only once when you have created a new, empty database. It adds data and does not delete or update existing rows.

## Connection and running the application

The application uses .NET User Secrets to store the database connection string.

Open a terminal in the project folder and run the following command. Replace the username and password with your own local MySQL credentials:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=holiday_maker;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD"
```

Once the database is running, start the application with:

```powershell
dotnet run
```

## Features

The application lets you:

- Register a new customer or select an existing one.
- Search for available hotel rooms by location and number of guests.
- Filter results by pool, restaurant, and distance.
- Sort results by price or hotel rating.
- Book a room and view the total price.
- View a customer's bookings.
- Change the number of people on a booking.
- Cancel a booking.

Bookings are limited to December 2026. The checkout date does not count as a booked night, so another party can book the same room starting on that date.

## Data model

The database contains tables for hotels, rooms, customers, bookings, and amenities. Hotels and amenities are connected through a separate junction table.

The ER diagram shows how the tables are related.

![Holiday Maker ER diagram](holidaymaker-er-diagram.svg)

## Manual test checklist

Run these tests against a separate test database. Check each box only after you have tried the corresponding feature.

- [ ] Search for available rooms and verify that occupied rooms are not shown.
- [ ] Search for rooms using a date range that overlaps an existing booking.
- [ ] Try a booking whose date range is fully contained within another booking.
- [ ] Verify that a booking can start on the same day an earlier booking ends.
- [ ] Verify that rooms with insufficient capacity are not shown.
- [ ] Try the pool, restaurant, and distance filters.
- [ ] Try sorting by both price and hotel rating.
- [ ] Change the number of people to a valid value.
- [ ] Try entering more people than the room allows.
- [ ] Cancel a booking and verify that the room becomes available again.
- [ ] Restart the application and verify that the bookings are still there.
- [ ] Register a new customer and try selecting an existing customer.
- [ ] Run a search that should return no rooms.

## Submission

The project contains the C# source code, the SQL files for the database and test data, and the `holidaymaker-er-diagram.svg` ER diagram.

I will submit the individual reflection separately.