-- MySQL script to create a database and tables for a books application
-- This script creates a database named 'passionlecture' and a table named 'books', and  after that it adds some sample data to the table.
CREATE DATABASE IF NOT EXISTS passionlecture;
--USE passionlecture;

-- Table of books
--CREATE TABLE IF NOT EXISTS Books (
--    id CHAR(36) NOT NULL PRIMARY KEY, -- UUID
--    title VARCHAR(255) NOT NULL UNIQUE,
--    epub LONGBLOB
--);



-- Add books
--INSERT INTO books (title, author, description, cover_image, epub) VALUES 
--('Book Title 1', 'Author 1', 'Description of Book 1', NULL, NULL),
--('Book Title 2', 'Author 2', 'Description of Book 2', NULL, NULL),
--('Book Title 3', 'Author 3', 'Description of Book 3', NULL, NULL);
--
--
--INSERT INTO images (images) VALUES (LOAD_FILE('/var/lib/mysql-files/La fontaine.epub'));