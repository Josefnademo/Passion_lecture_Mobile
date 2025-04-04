//Auteur : jmy
//Date   : 24.04.2024
//Lieu   : ETML
//Descr. : squelette pour api avec blob

const express = require('express')
const Sequelize = require("sequelize");
const FS = require('fs');
const app = express()
const port = 3000

const sequelize = new Sequelize("passionlecture", "root", "123", {
   host: "127.0.0.1",
   dialect: "mariadb",
});

const Book = sequelize.define(
   "Book",
   {
      id: {
         type: Sequelize.UUID,
         defaultValue: Sequelize.UUIDV4,
         primaryKey: true,
      },

      title: {
         type: Sequelize.STRING,
         allowNull: false,
      },
      epub: {
         type: Sequelize.BLOB('long'),
      },
   },
);


//FROM FILE
app.get('/epub/1', function(req, res){
  const file = `${__dirname}/Dickens, Charles - Oliver Twist.epub`;
  res.download(file);
});

//FROM DB
app.get('/epub/2', function(req, res){
  
  Book.findAll({
      attributes: ["epub","title"],
   })
   .then((result) => {
		blob = result[0].epub;
		res
			.header("Content-Type","application/epub+zip")
			.header('Content-Disposition','attachment; filename="'+result[0].title+'.epub"')
			.header('Content-Length',blob.length)
			
			.send(blob);
   })
  
});


app.listen(port, () => {
  console.log(`Server listening on port ${port}`)
  sequelize.authenticate();
  Book.sync();
  
  /* Insert epub
  const epubPath = `${__dirname}/Dumas, Alexandre - Les trois mousquetaires.epub`;
  var epubData = FS.readFileSync(epubPath);
  Book.create({
       title: "mousquetaires",
	   epub: epubData
   })
   */
})