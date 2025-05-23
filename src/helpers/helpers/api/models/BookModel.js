const { DataTypes } = require("sequelize");

module.exports = (sequelize) => {
  const Book = sequelize.define(
    "Book",
    {
      id: {
        type: DataTypes.UUID,
        defaultValue: DataTypes.UUIDV4,
        primaryKey: true,
      },
      title: {
        type: DataTypes.STRING,
        allowNull: false,
      },
      epub: {
        type: DataTypes.BLOB("long"),
      },
      coverImage: {
        type: DataTypes.BLOB("long"),
        allowNull: true,
      },
      lastReadPage: {
        type: DataTypes.INTEGER,
        defaultValue: 0,
      },
    },
    {
      timestamps: true,
      underscored: false,
    }
  );

  return Book;
};
