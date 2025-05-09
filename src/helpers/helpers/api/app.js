const express = require("express");
const Sequelize = require("sequelize");
const FS = require("fs");
const app = express();
const port = 3000;
const multer = require("multer");
const BookModel = require("./models/BookModel");

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

app.post("/upload", upload.single("epub"), async (req, res) => {
  // Check if the request contains a file
  if (!req.file) {
    return res.status(400).send("Aucun fichier était uploadé");
  }

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

app.listen(port, async () => {
  console.log(`Server listening on port ${port}`);

  try {
    await sequelize.authenticate();
    // Используйте alter: true для безопасного обновления структуры
    await Book.sync({ alter: true });

    // Проверяем, существует ли уже книга
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
