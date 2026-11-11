USE holiday_maker;


-- Lägger in hotell
INSERT INTO Hotels
(name, location, distance_to_beach, distance_to_center, rating)
VALUES
('Seaside Resort', 'Malmö', 200, 1500, 4.5),
('Grand Palace Hotel', 'Malmö', 2500, 500, 5.0),
('Sunshine Hotel', 'Helsingborg', 300, 1000, 4.0),
('Coastal Dreams', 'Helsingborg', 100, 2000, 4.5),
('City Comfort Hotel', 'Lund', 12000, 300, 3.5);


-- Lägger in bekvämligheter
INSERT INTO Amenities (name)
VALUES
('Pool'),
('Restaurant'),
('Spa'),
('Gym');


-- Kopplar bekvämligheter till hotell
INSERT INTO hotel_x_amenities (hotel_id, amenities_id)
VALUES
(1, 1),
(1, 2),
(1, 3),
(2, 1),
(2, 2),
(2, 4),
(3, 2),
(4, 1),
(4, 3),
(5, 2),
(5, 4);


-- Lägger in testkunder
INSERT INTO customers (name, address, email, phone)
VALUES
('Maja Berg', 'Västra Hamnen 14, Malmö', 'maja.berg@example.com', '0706123456'),
('Daniel Ek', 'Södergatan 8, Helsingborg', 'daniel.ek@example.com', '0707234567'),
('Nora Persson', 'Klostergatan 21, Lund', 'nora.persson@example.com', '0708345678'),
('Filip Holm', 'Limhamnsvägen 32, Malmö', 'filip.holm@example.com', '0709456789'),
('Elin Sjöberg', 'Drottninggatan 6, Helsingborg', 'elin.sjoberg@example.com', '0701567890');


-- Lägger in rum på Seaside Resort
INSERT INTO rooms (price_per_night, capacity, hotel_id, room_number)
VALUES
(950.00, 2, 1, 101),
(1250.00, 4, 1, 102),
(750.00, 1, 1, 103),
(1100.00, 2, 1, 104),

-- Grand Palace Hotel
(1450.00, 2, 2, 201),
(1900.00, 4, 2, 202),
(1050.00, 1, 2, 203),
(1650.00, 2, 2, 204),

-- Sunshine Hotel
(850.00, 2, 3, 301),
(1300.00, 4, 3, 302),
(650.00, 1, 3, 303),
(1000.00, 2, 3, 304),

-- Coastal Dreams
(1050.00, 2, 4, 401),
(1500.00, 4, 4, 402),
(800.00, 1, 4, 403),
(1200.00, 2, 4, 404),

-- City Comfort Hotel
(700.00, 2, 5, 501),
(1150.00, 4, 5, 502),
(550.00, 1, 5, 503),
(900.00, 2, 5, 504);


-- Lägger in 25 testbokningar
INSERT INTO bookings
(customer_id, room_id, start_date, end_date, total_price, cancelled, number_of_occupants)
VALUES
(1, 1, '2026-12-01', '2026-12-04', 2850.00, 0, 2),
(2, 2, '2026-12-02', '2026-12-06', 5000.00, 0, 3),
(3, 3, '2026-12-03', '2026-12-05', 1500.00, 0, 1),
(4, 4, '2026-12-05', '2026-12-08', 3300.00, 0, 2),
(5, 5, '2026-12-01', '2026-12-05', 5800.00, 0, 2),

(1, 6, '2026-12-06', '2026-12-09', 5700.00, 0, 1),
(2, 7, '2026-12-08', '2026-12-11', 3150.00, 0, 1),
(3, 8, '2026-12-10', '2026-12-13', 4950.00, 0, 2),
(4, 9, '2026-12-02', '2026-12-05', 2550.00, 0, 2),
(5, 10, '2026-12-04', '2026-12-08', 5200.00, 0, 4),

(1, 11, '2026-12-11', '2026-12-14', 1950.00, 0, 1),
(2, 12, '2026-12-15', '2026-12-19', 4000.00, 0, 2),
(3, 13, '2026-12-01', '2026-12-04', 3150.00, 0, 2),
(4, 14, '2026-12-05', '2026-12-09', 6000.00, 0, 4),
(5, 15, '2026-12-10', '2026-12-12', 1600.00, 0, 1),

(1, 16, '2026-12-12', '2026-12-16', 4800.00, 0, 2),
(2, 17, '2026-12-03', '2026-12-06', 2100.00, 0, 2),
(3, 18, '2026-12-07', '2026-12-11', 4600.00, 0, 4),
(4, 19, '2026-12-14', '2026-12-17', 1650.00, 0, 1),
(5, 20, '2026-12-18', '2026-12-22', 3600.00, 0, 2),

(2, 1, '2026-12-06', '2026-12-09', 2850.00, 0, 2),
(3, 2, '2026-12-10', '2026-12-14', 5000.00, 0, 4),
(4, 3, '2026-12-08', '2026-12-10', 1500.00, 0, 1),
(5, 4, '2026-12-12', '2026-12-15', 3300.00, 0, 2),
(1, 5, '2026-12-10', '2026-12-13', 4350.00, 1, 2);