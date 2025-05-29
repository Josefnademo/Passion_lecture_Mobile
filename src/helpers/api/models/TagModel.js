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
    },
  });
  return Tag;
};
