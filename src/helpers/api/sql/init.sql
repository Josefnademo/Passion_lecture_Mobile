-- This script creates a database named 'passionlecture' and tables named 'books', 'Tags','BookTags', and  after that it adds some sample data to the table.
CREATE DATABASE IF NOT EXISTS passionlecture;
USE passionlecture;

-- Create the books table
CREATE TABLE IF NOT EXISTS `Books` (
  `id` CHAR(36) BINARY NOT NULL,
  `title` VARCHAR(255) NOT NULL,
  `epub` LONGBLOB,
  `coverImage` LONGBLOB,
  `lastReadPage` INT DEFAULT 0,
  `createdAt` DATETIME NOT NULL,
  `updatedAt` DATETIME NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create the tags table
CREATE TABLE IF NOT EXISTS `Tags` (
  `id` INTEGER NOT NULL auto_increment,
  `name` VARCHAR(255) UNIQUE,
  `createdAt` DATETIME NOT NULL,
  `updatedAt` DATETIME NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create the BookTags junction table
CREATE TABLE IF NOT EXISTS `BookTags` (
  `BookId` CHAR(36) BINARY NOT NULL,
  `TagId` INTEGER NOT NULL,
  `createdAt` DATETIME NOT NULL,
  `updatedAt` DATETIME NOT NULL,
  PRIMARY KEY (`BookId`, `TagId`),
  FOREIGN KEY (`BookId`) REFERENCES `Books` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  FOREIGN KEY (`TagId`) REFERENCES `Tags` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create some initial tags
INSERT INTO Tags (name, createdAt, updatedAt) VALUES
('Fiction', NOW(), NOW()),
('Non-Fiction', NOW(), NOW()),
('Science', NOW(), NOW()),
('History', NOW(), NOW()),
('Technology', NOW(), NOW()),
('Programming', NOW(), NOW()); 