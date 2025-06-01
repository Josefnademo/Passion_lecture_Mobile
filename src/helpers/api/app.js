const express = require("express");
const Sequelize = require("sequelize");
const FS = require("fs");
const app = express();
const port = 3000;
const multer = require("multer");
const BookModel = require("./models/BookModel");
const TagModel = require("./models/TagModel");
const BookTagModel = require("./models/BookTag");
const mysql = require("mysql2/promise");
const path = require("path");
const { parseString } = require("xml2js");
const JSZip = require("jszip");
const fs = require("fs").promises;
const AdmZip = require("adm-zip");

// Enable CORS for all routes
app.use((req, res, next) => {
  res.header("Access-Control-Allow-Origin", "*");
  res.header("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
  res.header(
    "Access-Control-Allow-Headers",
    "Origin, X-Requested-With, Content-Type, Accept"
  );
  if (req.method === "OPTIONS") {
    return res.sendStatus(200);
  }
  next();
});

// Health check endpoint
app.get("/api/health", (req, res) => {
  res.json({ status: "ok", timestamp: new Date().toISOString() });
});

// Configure multer for file uploads
const storage = multer.memoryStorage();
const upload = multer({ storage });

async function connectWithRetry(retries = 5, delay = 5000) {
  for (let i = 0; i < retries; i++) {
    try {
      const sequelize = new Sequelize(
        process.env.MYSQL_DATABASE || "passionlecture",
        process.env.MYSQL_USER || "root",
        process.env.MYSQL_PASSWORD || "root123",
        {
          host: process.env.MYSQL_HOST || "db",
          dialect: "mysql",
          retry: {
            max: 3,
          },
        }
      );
      await sequelize.authenticate();
      console.log("Database connection established successfully.");
      return sequelize;
    } catch (err) {
      console.log(
        `Failed to connect to database (attempt ${i + 1}/${retries}):`,
        err.message
      );
      if (i < retries - 1) {
        console.log(`Retrying in ${delay / 1000} seconds...`);
        await new Promise((resolve) => setTimeout(resolve, delay));
      }
    }
  }
  throw new Error("Failed to connect to database after multiple retries");
}

let Book, Tag, BookTag;

async function initializeDatabase() {
  try {
    sequelize = await connectWithRetry();

    // Initialiser les modèles en passant la connexion sequelize
    Book = BookModel(sequelize, Sequelize.DataTypes);
    Tag = TagModel(sequelize, Sequelize.DataTypes);
    BookTag = BookTagModel(sequelize, Sequelize.DataTypes);

    // Configurer les associations
    Book.belongsToMany(Tag, {
      through: BookTag,
      foreignKey: "BookId",
      otherKey: "TagId",
    });
    Tag.belongsToMany(Book, {
      through: BookTag,
      foreignKey: "TagId",
      otherKey: "BookId",
    });

    // Synchroniser les modèles
    await Book.sync({ alter: true });
    await Tag.sync({ alter: true });
    await BookTag.sync({ alter: true });

    const exists = await Book.findOne({ where: { title: "mousquetaires" } });
    if (!exists) {
      try {
        const epubData = FS.readFileSync(
          `${__dirname}/Dumas, Alexandre - Les trois mousquetaires.epub`
        );
        await Book.create({ title: "mousquetaires", epub: epubData });
        console.log("Default book created successfully");
      } catch (error) {
        console.error("Error creating default book:", error.message);
      }
    }

    const books = await Book.findAll({ order: [["createdAt", "ASC"]] });
    for (let i = 0; i < books.length; i++) {
      if (!books[i].numericId) {
        books[i].numericId = i + 1;
        await books[i].save();
      }
    }
  } catch (error) {
    console.error("Failed to initialize database:", error.message);
    process.exit(1);
  }
}

app.use(express.json());

// FROM FILE
app.get("/epub/1", function (req, res) {
  const file = `${__dirname}/Dickens, Charles - Oliver Twist.epub`;
  res.download(file);
});

// FROM DB
app.get("/epub/2", function (req, res) {
  Book.findAll({
    attributes: ["epub", "title"],
  }).then((result) => {
    blob = result[0].epub;
    res
      .header("Content-Type", "application/epub+zip")
      .header(
        "Content-Disposition",
        'attachment; filename="' + result[0].title + '.epub"'
      )
      .header("Content-Length", blob.length)
      .send(blob);
  });
});

// use any epub by id
app.get("/epub/:id", async (req, res) => {
  const { id } = req.params;

  try {
    const book = await Book.findByPk(id);
    if (!book) return res.status(404).send("Livre non trouvé");

    res
      .header("Content-Type", "application/epub+zip")
      .header(
        "Content-Disposition",
        `attachment; filename="${book.title}.epub"`
      )
      .header("Content-Length", book.epub.length)
      .send(book.epub);
  } catch (error) {
    console.error("Erreur lors de l'envoi du livre:", error);
    res.status(500).send("Erreur serveur");
  }
});

app.post("/upload", upload.single("epub"), async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).send("Aucun fichier était uploadé");
    }

    const title = req.file.originalname.replace(".epub", "");
    const existingBook = await Book.findOne({ where: { title } });
    if (existingBook) {
      return res.status(409).send("Book already exists.");
    }

    let coverImage = null;
    try {
      const zip = new AdmZip(req.file.buffer);
      const entries = zip.getEntries();

      const coverPaths = [
        "OEBPS/Images/cover.png",
        "OEBPS/images/cover.png",
        "OEBPS/Images/cover.jpg",
        "OEBPS/images/cover.jpg",
        "OPS/images/cover.jpg",
        "OPS/Images/cover.jpg",
        "cover.jpg",
        "cover.png",
      ];

      for (const coverPath of coverPaths) {
        const entry = entries.find(
          (e) => e.entryName.toLowerCase() === coverPath.toLowerCase()
        );
        if (entry) {
          coverImage = entry.getData();
          break;
        }
      }

      if (!coverImage) {
        const imageEntry = entries.find(
          (e) =>
            e.entryName.toLowerCase().includes("cover") &&
            (e.entryName.endsWith(".jpg") || e.entryName.endsWith(".png"))
        );
        if (imageEntry) {
          coverImage = imageEntry.getData();
        }
      }
    } catch (error) {
      console.error("Error extracting cover:", error);
    }

    await Book.create({
      title: title,
      epub: req.file.buffer,
      coverImage: coverImage,
    });

    res.status(200).send("Fichier uploadé avec succès");
  } catch (error) {
    console.error("Erreur lors de l'upload du fichier:", error);
    res.status(500).send("Erreur lors de l'upload du fichier");
  }
});

// Getting a list of books
app.get("/api/books", async (req, res) => {
  try {
    const books = await Book.findAll({
      attributes: [
        "id",
        "numericId",
        "title",
        "createdAt",
        "lastReadPage",
        "coverImage",
      ],
      order: [["createdAt", "DESC"]],
      include: [
        {
          model: Tag,
          through: { attributes: [] },
        },
      ],
    });

    const baseUrl = `${req.protocol}://${req.get("host")}`;

    const booksJson = books.map((book) => {
      const plainBook = book.get({ plain: true });
      plainBook.id = String(plainBook.id);
      plainBook.numericId = book.numericId || null;
      plainBook.coverUrl = plainBook.coverImage
        ? `${baseUrl}/api/books/${plainBook.id}/cover`
        : null;
      delete plainBook.coverImage;

      if (plainBook.Tags) {
        plainBook.Tags = plainBook.Tags.map((tag) => ({
          id: String(tag.id),
          name: tag.name,
        }));
      }

      return plainBook;
    });

    res.json(booksJson);
  } catch (error) {
    console.error("Error fetching books:", error);
    res.status(500).json({
      error: "Failed to fetch books",
      details: error.message,
    });
  }
});

// Get books filtered by tags
app.get("/api/books/filter", async (req, res) => {
  try {
    const { tagIds } = req.query;
    let includeClause = [
      {
        model: Tag,
        through: { attributes: [] },
      },
    ];

    if (tagIds) {
      includeClause = [
        {
          model: Tag,
          where: {
            id: tagIds.split(",").map(Number),
          },
          through: { attributes: [] },
        },
      ];
    }

    const books = await Book.findAll({
      attributes: ["id", "title", "createdAt", "lastReadPage"],
      include: includeClause,
      order: [["createdAt", "DESC"]],
    });

    res.json(books);
  } catch (error) {
    console.error("Error filtering books:", error);
    res.status(500).json({ error: "Failed to filter books" });
  }
});

// Get all tags
app.get("/api/tags", async (req, res) => {
  try {
    const tags = await Tag.findAll({
      include: [
        {
          model: Book,
          attributes: ["id"],
          through: { attributes: [] },
        },
      ],
    });

    const tagsWithCount = tags.map((tag) => {
      const plainTag = tag.get({ plain: true });
      return {
        id: String(plainTag.id),
        name: plainTag.name,
        booksCount: plainTag.Books.length,
      };
    });

    res.json(tagsWithCount);
  } catch (error) {
    console.error("Error fetching tags:", error);
    res.status(500).json({ error: "Failed to fetch tags" });
  }
});

// Creating a tag
app.post("/api/tags", async (req, res) => {
  try {
    const { name } = req.body;
    if (!name || name.trim() === "") {
      return res.status(400).json({ error: "The name of the tag is required" });
    }

    const existingTag = await Tag.findOne({ where: { name } });
    if (existingTag) {
      return res.status(409).json({ error: "Tag already exists" });
    }

    const tag = await Tag.create({ name });
    const tagJson = {
      id: String(tag.id),
      name: tag.name,
    };
    res.status(201).json(tagJson);
  } catch (error) {
    console.error("Error creating tag:", error);
    res.status(500).json({ error: "Server error" });
  }
});

// Update last read page for a book
app.put("/books/:id/lastpage", async (req, res) => {
  try {
    const { id } = req.params;
    const { page } = req.body;

    if (typeof page !== "number" || page < 0) {
      return res.status(400).send("Invalid page number");
    }

    const book = await Book.findByPk(id);
    if (!book) {
      return res.status(404).send("Book not found");
    }

    book.lastReadPage = page;
    await book.save();
    res.json({ success: true });
  } catch (error) {
    console.error("Error updating last read page:", error);
    res.status(500).send("Server error");
  }
});

// Associate tags with a book
app.post("/books/:id/tags", async (req, res) => {
  try {
    const bookId = req.params.id; // book id from URL
    const { tagIds } = req.body; // array of tag ids from the request body

    if (!Array.isArray(tagIds)) {
      return res.status(400).json({ error: "tagIds must be an array" });
    }

    // Find a book by id
    const book = await Book.findByPk(bookId);
    if (!book) {
      return res.status(404).json({ error: "Book not found" });
    }

    //Find tags by id to make sure they exist
    const tags = await Tag.findAll({
      where: { id: tagIds },
    });

    if (tags.length !== tagIds.length) {
      return res.status(400).json({ error: "One or more tags not found" });
    }

    // Update links (assign tags to book)
    await book.setTags(tags);

    // Send updated tags in response
    const updatedTags = await book.getTags();

    res.json(
      updatedTags.map((tag) => ({
        id: String(tag.id),
        name: tag.name,
      }))
    );
  } catch (error) {
    console.error("Error updating book tags:", error);
    res.status(500).json({ error: "Server error" });
  }
});

// Get tags for a specific book
app.get("/books/:id/tags", async (req, res) => {
  try {
    const { id } = req.params;
    const book = await Book.findByPk(id, {
      include: [Tag],
    });

    if (!book) {
      return res.status(404).send("Book not found");
    }

    res.json(book.Tags);
  } catch (error) {
    console.error("Error getting book tags:", error);
    res.status(500).send("Server error");
  }
});

// Update book cover image
app.put("/books/:id/cover", upload.single("cover"), async (req, res) => {
  try {
    const { id } = req.params;
    if (!req.file) {
      return res.status(400).send("No cover image provided");
    }

    const book = await Book.findByPk(id);
    if (!book) {
      return res.status(404).send("Book not found");
    }

    book.coverImage = req.file.buffer;
    await book.save();
    res.json({ success: true });
  } catch (error) {
    console.error("Error updating cover image:", error);
    res.status(500).send("Server error");
  }
});

// Get book cover image
app.get("/api/books/:id/cover", async (req, res) => {
  try {
    const { id } = req.params;
    const book = await Book.findByPk(id);

    if (!book || !book.coverImage) {
      return res.status(404).send("Cover image not found");
    }

    const imageType = book.coverImage[0] === 0x89 ? "image/png" : "image/jpeg";

    res
      .header("Content-Type", imageType)
      .header("Cache-Control", "public, max-age=31536000")
      .send(book.coverImage);
  } catch (error) {
    console.error("Error getting cover image:", error);
    res.status(500).send("Server error");
  }
});

// Get text by numeric ID
app.get("/api/books/numeric/:numericId/text", async (req, res) => {
  try {
    const numericId = parseInt(req.params.numericId);
    if (isNaN(numericId)) {
      return res.status(400).json({ error: "Invalid numeric ID" });
    }

    const book = await Book.findOne({
      where: { numericId },
      attributes: ["id", "numericId", "title", "epub"],
    });

    if (!book || !book.epub) {
      const availableBooks = await Book.findAll({
        attributes: ["numericId", "id", "title"],
        order: [["numericId", "ASC"]],
      });
      return res.status(404).json({
        error: "Book not found or has no content",
        availableBooks,
      });
    }

    const zip = await JSZip.loadAsync(book.epub);
    let fullText = "";

    await Promise.all(
      Object.entries(zip.files)
        .filter(([name]) => name.endsWith(".html") || name.endsWith(".xhtml"))
        .map(async ([name, file]) => {
          const content = await file.async("text");
          fullText +=
            content
              .replace(/<[^>]+>/g, " ")
              .replace(/\s+/g, " ")
              .trim() + "\n\n";
        })
    );

    res.json({
      id: book.id,
      numericId: book.numericId,
      title: book.title,
      text: fullText.trim(),
      length: fullText.length,
    });
  } catch (error) {
    console.error("Error in numeric text endpoint:", error);
    res.status(500).json({
      error: "Text extraction failed",
      details: error.message,
    });
  }
});

// Get book EPUB content
app.get("/api/books/:id/epub", async (req, res) => {
  try {
    const { id } = req.params;
    const book = await Book.findByPk(id, {
      attributes: ["id", "title", "epub"],
    });

    if (!book) {
      return res.status(404).json({
        error: "Book not found",
        details: `No book exists with ID ${id}`,
      });
    }

    if (!book.epub) {
      return res.status(404).json({
        error: "No EPUB content",
        details: `Book ${book.title} has no EPUB data`,
      });
    }

    res
      .header("Content-Type", "application/epub+zip")
      .header(
        "Content-Disposition",
        `attachment; filename="${book.title}.epub"`
      )
      .header("Content-Length", book.epub.length)
      .send(book.epub);
  } catch (error) {
    console.error("Error getting book content:", error);
    res.status(500).json({
      error: "Server error",
      details: error.message,
    });
  }
});

// Start server only after database is initialized
async function startServer() {
  await initializeDatabase();

  app.listen(port, "0.0.0.0", () => {
    console.log(`Server listening on port ${port} on all interfaces`);
  });
}

startServer().catch((error) => {
  console.error("Failed to start server:", error.message);
  process.exit(1);
});

// MySQL connection configuration
const dbConfig = {
  host: process.env.MYSQL_HOST || "db",
  user: "root",
  password: process.env.MYSQL_PASSWORD || "root123",
  database: process.env.MYSQL_DATABASE || "passionlecture",
};

// Create uploads directory if it doesn't exist
fs.mkdir("./uploads").catch(() => {});
