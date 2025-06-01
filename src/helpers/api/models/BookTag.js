const { DataTypes } = require("sequelize");

module.exports = (sequelize) => {
  const BookTag = sequelize.define("BookTag", {
    BookId: {
      type: DataTypes.CHAR(36),
      allowNull: false,
      primaryKey: true,
    },
    TagId: {
      type: DataTypes.CHAR(36),
      allowNull: false,
      primaryKey: true,
    },
  });

  return BookTag;
};
