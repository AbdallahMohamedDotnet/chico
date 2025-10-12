using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace ChicoDesktopApp
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseHelper()
        {
            // Set database path in the Data folder
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            
            // Create Data folder if it doesn't exist
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            _databasePath = Path.Combine(dataFolder, "chico.db");
            _connectionString = $"Data Source={_databasePath}";
            
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Create a sample table (you can customize this)
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS AppData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    CreatedDate TEXT NOT NULL
                )
            ";
            command.ExecuteNonQuery();
        }

        public SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public string GetDatabasePath()
        {
            return _databasePath;
        }

        // Example method to add data
        public void AddData(string name, string description)
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO AppData (Name, Description, CreatedDate)
                VALUES ($name, $description, $date)
            ";
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$description", description ?? "");
            command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            
            command.ExecuteNonQuery();
        }

        // Example method to get data count
        public long GetDataCount()
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM AppData";
            
            return (long)command.ExecuteScalar()!;
        }
    }
}
