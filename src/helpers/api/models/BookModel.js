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
      numericId: {
        type: DataTypes.INTEGER,
        autoIncrement: true,
        unique: true,
      },
      title: {
        type: DataTypes.STRING,
        allowNull: false,
      },
      epub: {
        type: DataTypes.BLOB("long"),
        allowNull: false,
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

  Book.associate = (models) => {
    Book.belongsToMany(models.Tag, {
      through: "BookTags",
      foreignKey: "BookId",
      otherKey: "TagId",
    });
  };

  return Book;
};
