const express = require("express");
const Sequelize = require("sequelize");
const FS = require("fs");
const app = express();
const port = 3000;
const multer = require("multer");
const BookModel = require("./models/BookModel");
const TagModel = require("./models/TagModel");
const mysql = require("mysql2/promise");
const path = require("path");
const fs = require("fs").promises;

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

/*multer is a middleware for Express.js that allows you to accept files from multipart/form-data requests 
(for example, when uploading an .epub file from a MAUI application).
When you send a file via HttpClient with MultipartFormDataContent,
the file is not sent as JSON, but as "form-data" (as in HTML <form enctype="multipart/form-data">). 
Express can't handle this on its own, and multer is needed to parse the file and put it into req.file. */
const upload = multer({ storage });

// Function to try connecting to database with retries
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

let sequelize;
let Book;
let Tag;
let BookTag;

// Initialize database connection and models
async function initializeDatabase() {
  try {
    sequelize = await connectWithRetry();
    Book = BookModel(sequelize);
    Tag = TagModel(sequelize);
    BookTag = sequelize.define("BookTag", {});

    Book.belongsToMany(Tag, { through: BookTag });
    Tag.belongsToMany(Book, { through: BookTag });

    await Book.sync({ alter: true });
    await Tag.sync({ alter: true });
    await BookTag.sync({ alter: true });

    // Initialize default book if needed
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
  } catch (error) {
    console.error("Failed to initialize database:", error.message);
    process.exit(1);
  }
}

app.use(express.json()); // permet de lire le body JSON des requêtes

//FROM FILE
app.get("/epub/1", function (req, res) {
  const file = `${__dirname}/Dickens, Charles - Oliver Twist.epub`;
  res.download(file);
});

//FROM DB
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

//use any epub by id
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
    // Check if the request contains a file
    if (!req.file) {
      return res.status(400).send("Aucun fichier était uploadé");
    }

    // Get title from the filename
    const title = req.file.originalname.replace(".epub", "");

    // Check if the book already exists
    const existingBook = await Book.findOne({ where: { title } });
    if (existingBook) {
      return res.status(409).send("Book already exists.");
    }

    // Extract cover image from EPUB
    let coverImage = null;
    try {
      const zip = new require("adm-zip")(req.file.buffer);
      const entries = zip.getEntries();

      // Try common cover image paths
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

      // If no cover found in common paths, try to find any image that might be a cover
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
    console.log("Titre:", title);
    console.log("Taille EPUB:", req.file?.buffer?.length);
    console.log("Type:", typeof req.file?.buffer);

    console.log("Contenu que l'on veut enregistrer :", {
      title: title,
      epub: req.file.buffer,
      coverImage: coverImage,
    });

    // Create the book with cover if found
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

// Getting a list of books sorted by date
app.get("/api/books", async (req, res) => {
  try {
    const books = await Book.findAll({
      attributes: ["id", "title", "createdAt", "lastReadPage", "coverImage"],
      order: [["createdAt", "DESC"]],
      include: [
        {
          model: Tag,
          through: { attributes: [] },
        },
      ],
    });

    const baseUrl = `${req.protocol}://${req.get("host")}`;

    // Convert the Sequelize model instances to plain objects and ensure IDs are strings
    const booksJson = books.map((book) => {
      const plainBook = book.get({ plain: true });
      plainBook.id = String(plainBook.id);
      // Add cover URL with full path
      plainBook.coverUrl = plainBook.coverImage
        ? `${baseUrl}/api/books/${plainBook.id}/cover`
        : null;
      // Remove the actual coverImage binary data from the response
      delete plainBook.coverImage;
      if (plainBook.Tags) {
        plainBook.Tags = plainBook.Tags.map((tag) => ({
          ...tag,
          id: String(tag.id),
        }));
      }
      return plainBook;
    });

    res.json(booksJson);
  } catch (error) {
    console.error("Error fetching books:", error);
    res.status(500).json({ error: "Failed to fetch books" });
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

    // Add booksCount to each tag and ensure IDs are strings
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
    // Convert ID to string before sending
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
    const { id } = req.params;
    const { tagIds } = req.body;

    const book = await Book.findByPk(id);
    if (!book) {
      return res.status(404).send("Book not found");
    }

    await book.setTags(tagIds);
    const updatedBook = await Book.findByPk(id, {
      include: [Tag],
    });

    res.json(updatedBook);
  } catch (error) {
    console.error("Error associating tags:", error);
    res.status(500).send("Server error");
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

    // Try to detect image type from the first few bytes
    const imageType = book.coverImage[0] === 0x89 ? "image/png" : "image/jpeg";

    res
      .header("Content-Type", imageType)
      .header("Cache-Control", "public, max-age=31536000") // Cache for 1 year
      .send(book.coverImage);
  } catch (error) {
    console.error("Error getting cover image:", error);
    res.status(500).send("Server error");
  }
});

// Get book EPUB content
app.get("/api/books/:id/epub", async (req, res) => {
  try {
    const { id } = req.params;
    console.log(`[API] Getting EPUB content for book ID: ${id}`);

    const book = await Book.findByPk(id);
    if (!book || !book.epub) {
      console.log(`[API] Book not found or no EPUB content for ID: ${id}`);
      return res.status(404).json({ error: "Book content not found" });
    }

    res
      .header("Content-Type", "application/epub+zip")
      .header(
        "Content-Disposition",
        `attachment; filename="${book.title}.epub"`
      )
      .header("Content-Length", book.epub.length)
      .send(book.epub);

    console.log(`[API] Successfully sent EPUB content for book: ${book.title}`);
  } catch (error) {
    console.error("[API] Error getting book content:", error);
    res.status(500).json({ error: "Server error" });
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
