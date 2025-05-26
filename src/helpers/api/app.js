const express = require("express");
const Sequelize = require("sequelize");
const FS = require("fs");
const app = express();
const port = 3000;
const multer = require("multer");
const BookModel = require("./models/BookModel");
const TagModel = require("./models/TagModel");
const mysql = require('mysql2/promise');
const path = require('path');
const fs = require('fs').promises;

// Configure multer for file uploads
const storage = multer.diskStorage({
    destination: './uploads/',
    filename: (req, file, cb) => {
        cb(null, `${Date.now()}-${file.originalname}`);
    }
});

/*multer is a middleware for Express.js that allows you to accept files from multipart/form-data requests 
(for example, when uploading an .epub file from a MAUI application).
When you send a file via HttpClient with MultipartFormDataContent,
the file is not sent as JSON, but as "form-data" (as in HTML <form enctype="multipart/form-data">). 
Express can't handle this on its own, and multer is needed to parse the file and put it into req.file. */
const upload = multer({ storage: storage });

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
                        max: 3
                    }
                }
            );
            await sequelize.authenticate();
            console.log('Database connection established successfully.');
            return sequelize;
        } catch (err) {
            console.log(`Failed to connect to database (attempt ${i + 1}/${retries}):`, err.message);
            if (i < retries - 1) {
                console.log(`Retrying in ${delay/1000} seconds...`);
                await new Promise(resolve => setTimeout(resolve, delay));
            }
        }
    }
    throw new Error('Failed to connect to database after multiple retries');
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
                console.log('Default book created successfully');
            } catch (error) {
                console.error('Error creating default book:', error.message);
            }
        }
    } catch (error) {
        console.error('Failed to initialize database:', error.message);
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
  // Check if the request contains a file
  if (!req.file) {
    return res.status(400).send("Aucun fichier était uploadé");
  }

  // Récupérer le titre à partir du nom du fichier
  const title = req.file.originalname.replace(".epub", "");

  // Check if the book already exists in the database
  const existingBook = await Book.findOne({ where: { title } });
  if (existingBook) {
    return res.status(409).send("Book already exists.");
  }

  //create a new book
  try {
    await Book.create({
      title: req.file.originalname.replace(".epub", ""),
      epub: req.file.buffer,
    });
    res.status(200).send("Fichier uploadé avec succès");
  } catch (error) {
    console.error("Erreur lors de l'upload du fichier:", error);
    res.status(500).send("Erreur lors de l'upload du fichier");
  }
});

//Getting a list of books sorted by date
app.get("/books", async (req, res) => {
  try {
    const books = await Book.findAll({
      attributes: ["id", "title", "createdAt"],
      order: [["createdAt", "DESC"]],
    });
    res.json(books);
  } catch (error) {
    res.status(500).send("Error while receiving books");
  }
});

// Creating a tag
app.post("/tags", async (req, res) => {
  try {
    const { name } = req.body;
    if (!name || name.trim() === "") {
      return res.status(400).send("The name of the tag is a requirement");
    }

    const existingTag = await Tag.findOne({ where: { name } });
    if (existingTag) {
      return res.status(409).send("This is what happens");
    }

    const tag = await Tag.create({ name });
    res.status(201).json(tag);
  } catch (error) {
    console.error("Erreur lors de la creation du tag:", error);
    res.status(500).send("Please serve");
  }
});

// Get all tags
app.get("/tags", async (req, res) => {
  try {
    const tags = await Tag.findAll();
    res.json(tags);
  } catch (error) {
    res.status(500).send("Error getting tags");
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

// Get books with optional tag filter
app.get("/books/filter", async (req, res) => {
  try {
    const { tagIds } = req.query;
    let whereClause = {};
    let includeClause = [];

    if (tagIds) {
      const tagIdsArray = tagIds.split(",");
      includeClause = [
        {
          model: Tag,
          where: {
            id: tagIdsArray,
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
app.get("/books/:id/cover", async (req, res) => {
  try {
    const { id } = req.params;
    const book = await Book.findByPk(id);

    if (!book || !book.coverImage) {
      return res.status(404).send("Cover image not found");
    }

    res.header("Content-Type", "image/jpeg").send(book.coverImage);
  } catch (error) {
    console.error("Error getting cover image:", error);
    res.status(500).send("Server error");
  }
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'healthy' });
});

// Start server only after database is initialized
async function startServer() {
    await initializeDatabase();
    
    app.listen(port, "0.0.0.0", () => {
        console.log(`Server listening on port ${port} on all interfaces`);
    });
}

startServer().catch(error => {
    console.error('Failed to start server:', error.message);
    process.exit(1);
});

// MySQL connection configuration
const dbConfig = {
    host: process.env.MYSQL_HOST || 'db',
    user: 'root',
    password: process.env.MYSQL_PASSWORD || 'root123',
    database: process.env.MYSQL_DATABASE || 'passionlecture'
};

// Get all books
app.get('/api/books', async (req, res) => {
    try {
        const connection = await mysql.createConnection(dbConfig);
        const [rows] = await connection.execute('SELECT * FROM Books');
        await connection.end();
        res.json(rows);
    } catch (error) {
        console.error('Error fetching books:', error);
        res.status(500).json({ error: 'Failed to fetch books' });
    }
});

// Get books filtered by tags
app.get('/api/books/filter', async (req, res) => {
    try {
        const tagIds = req.query.tagIds.split(',').map(Number);
        const connection = await mysql.createConnection(dbConfig);
        const [rows] = await connection.execute(
            `SELECT DISTINCT b.* FROM Books b 
             INNER JOIN BookTags bt ON b.id = bt.BookId 
             WHERE bt.TagId IN (?)`,
            [tagIds]
        );
        await connection.end();
        res.json(rows);
    } catch (error) {
        console.error('Error filtering books:', error);
        res.status(500).json({ error: 'Failed to filter books' });
    }
});

// Get all tags
app.get('/api/tags', async (req, res) => {
    try {
        const connection = await mysql.createConnection(dbConfig);
        const [rows] = await connection.execute('SELECT * FROM Tags');
        await connection.end();
        res.json(rows);
    } catch (error) {
        console.error('Error fetching tags:', error);
        res.status(500).json({ error: 'Failed to fetch tags' });
    }
});

// Create a new tag
app.post('/api/tags', async (req, res) => {
    try {
        const { name } = req.body;
        const connection = await mysql.createConnection(dbConfig);
        const [result] = await connection.execute(
            'INSERT INTO Tags (name, createdAt, updatedAt) VALUES (?, NOW(), NOW())',
            [name]
        );
        const [newTag] = await connection.execute('SELECT * FROM Tags WHERE id = ?', [result.insertId]);
        await connection.end();
        res.status(201).json(newTag[0]);
    } catch (error) {
        console.error('Error creating tag:', error);
        res.status(500).json({ error: 'Failed to create tag' });
    }
});

// Get a specific book
app.get('/api/books/:id', async (req, res) => {
    try {
        const connection = await mysql.createConnection(dbConfig);
        const [rows] = await connection.execute('SELECT * FROM Books WHERE id = ?', [req.params.id]);
        await connection.end();
        if (rows.length === 0) {
            res.status(404).json({ error: 'Book not found' });
        } else {
            res.json(rows[0]);
        }
    } catch (error) {
        console.error('Error fetching book:', error);
        res.status(500).json({ error: 'Failed to fetch book' });
    }
});

// Get book's epub content
app.get('/api/books/:id/epub', async (req, res) => {
    try {
        const connection = await mysql.createConnection(dbConfig);
        const [rows] = await connection.execute('SELECT epub FROM Books WHERE id = ?', [req.params.id]);
        await connection.end();
        if (rows.length === 0 || !rows[0].epub) {
            res.status(404).json({ error: 'Epub content not found' });
        } else {
            res.setHeader('Content-Type', 'application/epub+zip');
            res.send(rows[0].epub);
        }
    } catch (error) {
        console.error('Error fetching epub:', error);
        res.status(500).json({ error: 'Failed to fetch epub content' });
    }
});

// Update last read page
app.put('/api/books/:id/lastpage', async (req, res) => {
    try {
        const { lastReadPage } = req.body;
        const connection = await mysql.createConnection(dbConfig);
        await connection.execute(
            'UPDATE Books SET lastReadPage = ?, updatedAt = NOW() WHERE id = ?',
            [lastReadPage, req.params.id]
        );
        await connection.end();
        res.json({ success: true });
    } catch (error) {
        console.error('Error updating last read page:', error);
        res.status(500).json({ error: 'Failed to update last read page' });
    }
});

// Upload a new book
app.post('/api/books/upload', upload.fields([
    { name: 'epub', maxCount: 1 },
    { name: 'cover', maxCount: 1 }
]), async (req, res) => {
    try {
        const { title } = req.body;
        const epubFile = req.files['epub'][0];
        const coverFile = req.files['cover'] ? req.files['cover'][0] : null;

        const epubContent = await fs.readFile(epubFile.path);
        const coverContent = coverFile ? await fs.readFile(coverFile.path) : null;

        const connection = await mysql.createConnection(dbConfig);
        const [result] = await connection.execute(
            'INSERT INTO Books (title, epub, coverImage, createdAt, updatedAt) VALUES (?, ?, ?, NOW(), NOW())',
            [title, epubContent, coverContent]
        );

        // Clean up uploaded files
        await fs.unlink(epubFile.path);
        if (coverFile) {
            await fs.unlink(coverFile.path);
        }

        await connection.end();
        res.status(201).json({ id: result.insertId });
    } catch (error) {
        console.error('Error uploading book:', error);
        res.status(500).json({ error: 'Failed to upload book' });
    }
});

// Create uploads directory if it doesn't exist
fs.mkdir('./uploads').catch(() => {});
