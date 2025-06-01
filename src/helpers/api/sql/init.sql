CREATE DATABASE IF NOT EXISTS passionlecture;
USE passionlecture;

-- Books table
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

-- Tags table
CREATE TABLE IF NOT EXISTS `Tags` (
  `id` CHAR(36) BINARY NOT NULL,       -- Changed from INTEGER to CHAR(36) for UUID consistency
  `name` VARCHAR(255) UNIQUE,
  `createdAt` DATETIME NOT NULL,
  `updatedAt` DATETIME NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Junction table BookTags
CREATE TABLE IF NOT EXISTS `BookTags` (
  `BookId` CHAR(36) BINARY NOT NULL,
  `TagId` CHAR(36) BINARY NOT NULL,
  PRIMARY KEY (`BookId`, `TagId`),
  FOREIGN KEY (`BookId`) REFERENCES `Books`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  FOREIGN KEY (`TagId`) REFERENCES `Tags`(`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample tags with UUIDs generated (replace with actual UUID strings)
INSERT INTO `Tags` (`id`, `name`, `createdAt`, `updatedAt`) VALUES
(UUID(), 'Fiction', NOW(), NOW()),
(UUID(), 'Non-Fiction', NOW(), NOW()),
(UUID(), 'Science', NOW(), NOW()),
(UUID(), 'History', NOW(), NOW()),
(UUID(), 'Technology', NOW(), NOW()),
(UUID(), 'Programming', NOW(), NOW());


-- Example of how to insert a book (you'll need to replace the binary data)
-- INSERT INTO Books (id, title, epub, createdAt, updatedAt)
-- VALUES (UUID(), 'Sample Book', LOAD_FILE('/path/to/book.epub'), NOW(), NOW());

-- Example of how to create a tag
-- INSERT INTO Tags (name, createdAt, updatedAt)
-- VALUES ('Fiction', NOW(), NOW());

-- Example of how to associate a book with a tag
-- INSERT INTO BookTags (BookId, TagId, createdAt, updatedAt)
-- SELECT b.id, t.id, NOW(), NOW()
-- FROM Books b, Tags t
-- WHERE b.title = 'Sample Book' AND t.name = 'Fiction'; 