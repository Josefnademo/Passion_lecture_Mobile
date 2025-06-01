const { DataTypes } = require("sequelize");

module.exports = (sequelize) => {
  const Tag = sequelize.define("Tag", {
    id: {
      type: DataTypes.UUID,
      defaultValue: DataTypes.UUIDV4,
      primaryKey: true,
    },
    name: {
      type: DataTypes.STRING,
      unique: true,
      allowNull: false,
    },
  });

  Tag.associate = (models) => {
    Tag.belongsToMany(models.Book, {
      through: "BookTags",
      foreignKey: "TagId",
      otherKey: "BookId",
    });
  };

  return Tag;
};
