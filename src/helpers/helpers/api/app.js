const express = require("express");
const Sequelize = require("sequelize");
const FS = require("fs");
const app = express();
const port = 3000;
const multer = require("multer");
const BookModel = require("./models/BookModel");
const TagModel = require("./models/TagModel");

/*multer is a middleware for Express.js that allows you to accept files from multipart/form-data requests 
(for example, when uploading an .epub file from a MAUI application). */
const upload = multer();
/*When you send a file via HttpClient with MultipartFormDataContent,
the file is not sent as JSON, but as "form-data" (as in HTML <form enctype="multipart/form-data">). 
Express can't handle this on its own, and multer is needed to parse the file and put it into req.file. */

const sequelize = new Sequelize(
  process.env.MYSQL_DATABASE || "passionlecture",
  process.env.MYSQL_USER || "root",
  process.env.MYSQL_PASSWORD || "root123",
  {
    host: process.env.MYSQL_HOST || "db", // "db" - name of service in Docker Compose
    dialect: "mysql",
  }
);

const Book = BookModel(sequelize);
const Tag = TagModel(sequelize);

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

// Add these routes after the existing ones.

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

// Many-to-many relationship between books and tags   (FK's)
const BookTag = sequelize.define("BookTag", {});
Book.belongsToMany(Tag, { through: BookTag });
Tag.belongsToMany(Book, { through: BookTag });

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

app.listen(port, async () => {
  console.log(`Server listening on port ${port}`);

  try {
    await sequelize.authenticate();
    // Use alter: true to safely update the structure
    await Book.sync({ alter: true });

    // Check if the book already exists
    const exists = await Book.findOne({ where: { title: "mousquetaires" } });
    if (!exists) {
      const epubData = FS.readFileSync(
        `${__dirname}/Dumas, Alexandre - Les trois mousquetaires.epub`
      );
      await Book.create({ title: "mousquetaires", epub: epubData });
    }
  } catch (error) {
    console.error("Database error:", error);
  }
});
/*
// Initialize and start server
(async () => {
  await initDB();

  try {
    await sequelize.authenticate();
    await sequelize.sync({ force: false });

    // Sample data insertion
    const exists = await Book.findOne({ where: { title: "mousquetaires" } });
    if (!exists) {
      const epubData = FS.readFileSync(
        `${__dirname}/Dumas, Alexandre - Les trois mousquetaires.epub`
      );
      await Book.create({ title: "mousquetaires", epub: epubData });
    }

    app.listen(port, () => console.log(`Server running on port ${port}`));
  } catch (error) {
    console.error("Database initialization failed:", error);
  }
})();
*/
