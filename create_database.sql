CREATE DATABASE IF NOT EXISTS holiday_maker;

USE holiday_maker;


-- Tabell för hotell
CREATE TABLE `Hotels` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `name` VARCHAR(100) NOT NULL,
    `location` VARCHAR(100) NOT NULL,
    `distance_to_beach` INT NOT NULL,
    `distance_to_center` INT NOT NULL,
    `rating` DECIMAL(2,1) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `check_hotels_beach_distance`
        CHECK (`distance_to_beach` >= 0),
    CONSTRAINT `check_hotels_center_distance`
        CHECK (`distance_to_center` >= 0),
    CONSTRAINT `check_hotels_rating`
        CHECK (`rating` >= 1 AND `rating` <= 5)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- Tabell för bekvämligheter
CREATE TABLE `Amenities` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `name` VARCHAR(50) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `unique_amenity_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- Tabell för kunder
CREATE TABLE `customers` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `name` VARCHAR(80) NOT NULL,
    `address` VARCHAR(100) NOT NULL,
    `email` VARCHAR(150) NOT NULL,
    `phone` VARCHAR(100) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `customers_pk` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- Tabell för hotellrum
CREATE TABLE `rooms` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `price_per_night` DECIMAL(10,2) NOT NULL,
    `capacity` INT NOT NULL,
    `hotel_id` BIGINT NOT NULL,
    `room_number` INT NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `unique_hotel_room` (`hotel_id`, `room_number`),
    CONSTRAINT `rooms_Hotels_id_fk`
        FOREIGN KEY (`hotel_id`) REFERENCES `Hotels` (`id`),
    CONSTRAINT `check_rooms_capacity`
        CHECK (`capacity` > 0),
    CONSTRAINT `check_rooms_price`
        CHECK (`price_per_night` >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- Tabell för bokningar
CREATE TABLE `bookings` (
    `booking_number` BIGINT NOT NULL AUTO_INCREMENT,
    `customer_id` BIGINT NOT NULL,
    `room_id` BIGINT NOT NULL,
    `start_date` DATE NOT NULL,
    `end_date` DATE NOT NULL,
    `total_price` DECIMAL(10,2) NOT NULL,
    `number_of_occupants` INT NOT NULL,
    PRIMARY KEY (`booking_number`),
    KEY `bookings_customers_id_fk` (`customer_id`),
    KEY `bookings_rooms_id_fk` (`room_id`),
    CONSTRAINT `bookings_customers_id_fk`
        FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`),
    CONSTRAINT `bookings_rooms_id_fk`
        FOREIGN KEY (`room_id`) REFERENCES `rooms` (`id`),
    CONSTRAINT `check_bookings_dates`
        CHECK (`end_date` > `start_date`),
    CONSTRAINT `check_bookings_occupants`
        CHECK (`number_of_occupants` > 0),
    CONSTRAINT `check_bookings_total_price`
        CHECK (`total_price` >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- Kopplingstabell mellan hotell och bekvämligheter
CREATE TABLE `hotel_x_amenities` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `hotel_id` BIGINT NOT NULL,
    `amenities_id` BIGINT NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `unique_hotel_amenity` (`hotel_id`, `amenities_id`),
    KEY `hotel_x_amenities_Amenities_id_fk` (`amenities_id`),
    CONSTRAINT `hotel_x_amenities_Amenities_id_fk`
        FOREIGN KEY (`amenities_id`) REFERENCES `Amenities` (`id`),
    CONSTRAINT `hotel_x_amenities_Hotels_id_fk`
        FOREIGN KEY (`hotel_id`) REFERENCES `Hotels` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;