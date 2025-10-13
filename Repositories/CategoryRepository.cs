using ChicoDesktopApp.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ChicoDesktopApp.Repositories
{
    public class CategoryRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public CategoryRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<Category> GetAllCategories()
        {
            var categories = new List<Category>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Categories ORDER BY Name";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    NameArabic = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedDate = DateTime.Parse(reader.GetString(4))
                });
            }

            return categories;
        }

        public int AddCategory(Category category)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Categories (Name, NameArabic, Description)
                VALUES ($name, $nameAr, $desc);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$name", category.Name);
            command.Parameters.AddWithValue("$nameAr", category.NameArabic ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$desc", category.Description ?? (object)DBNull.Value);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public Category? GetCategoryById(int id)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Categories WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    NameArabic = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedDate = DateTime.Parse(reader.GetString(4))
                };
            }

            return null;
        }
    }
}
