using ChicoDesktopApp.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ChicoDesktopApp.Repositories
{
    public class AuthRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public AuthRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Authenticate user
        public User? AuthenticateUser(string username, string password)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, Username, PasswordHash, FullName, FullNameArabic, Role, 
                           IsActive, CreatedDate, LastLoginDate
                    FROM Users
                    WHERE Username = $username AND IsActive = 1
                ";
                command.Parameters.AddWithValue("$username", username);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var storedHash = reader.GetString(2);
                    var inputHash = HashPassword(password);

                    System.Diagnostics.Debug.WriteLine($"Attempting login for user: {username}");
                    System.Diagnostics.Debug.WriteLine($"Stored hash length: {storedHash.Length}");
                    System.Diagnostics.Debug.WriteLine($"Input hash length: {inputHash.Length}");
                    System.Diagnostics.Debug.WriteLine($"Hashes match: {storedHash == inputHash}");

                    if (storedHash == inputHash)
                    {
                        var user = MapUser(reader);
                        
                        // Close reader before update
                        reader.Close();
                        
                        // Update last login date
                        UpdateLastLogin(user.Id);
                        
                        System.Diagnostics.Debug.WriteLine($"Login successful for: {username}");
                        return user;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Password mismatch for user: {username}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"User not found: {username}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Add new user
        public int AddUser(User user, string password)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Username, PasswordHash, FullName, FullNameArabic, Role, IsActive)
                VALUES ($username, $passwordHash, $fullName, $fullNameAr, $role, $active);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$username", user.Username);
            command.Parameters.AddWithValue("$passwordHash", HashPassword(password));
            command.Parameters.AddWithValue("$fullName", user.FullName);
            command.Parameters.AddWithValue("$fullNameAr", user.FullNameArabic ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$role", user.Role.ToString());
            command.Parameters.AddWithValue("$active", user.IsActive ? 1 : 0);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        // Update user
        public void UpdateUser(User user)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users 
                SET FullName = $fullName,
                    FullNameArabic = $fullNameAr,
                    Role = $role,
                    IsActive = $active
                WHERE Id = $id
            ";
            command.Parameters.AddWithValue("$fullName", user.FullName);
            command.Parameters.AddWithValue("$fullNameAr", user.FullNameArabic ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$role", user.Role.ToString());
            command.Parameters.AddWithValue("$active", user.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("$id", user.Id);

            command.ExecuteNonQuery();
        }

        // Change password
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            // Verify old password
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT PasswordHash FROM Users WHERE Id = $id";
            checkCommand.Parameters.AddWithValue("$id", userId);
            
            var storedHash = checkCommand.ExecuteScalar()?.ToString();
            if (storedHash == null || storedHash != HashPassword(oldPassword))
            {
                return false;
            }

            // Update password
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE Users 
                SET PasswordHash = $passwordHash
                WHERE Id = $id
            ";
            updateCommand.Parameters.AddWithValue("$passwordHash", HashPassword(newPassword));
            updateCommand.Parameters.AddWithValue("$id", userId);
            updateCommand.ExecuteNonQuery();

            return true;
        }

        // Reset password (admin only)
        public void ResetPassword(int userId, string newPassword)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users 
                SET PasswordHash = $passwordHash
                WHERE Id = $id
            ";
            command.Parameters.AddWithValue("$passwordHash", HashPassword(newPassword));
            command.Parameters.AddWithValue("$id", userId);
            command.ExecuteNonQuery();
        }

        // Get all users
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Username, PasswordHash, FullName, FullNameArabic, Role, 
                       IsActive, CreatedDate, LastLoginDate
                FROM Users
                ORDER BY FullName
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        // Get user by ID
        public User? GetUserById(int id)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Username, PasswordHash, FullName, FullNameArabic, Role, 
                       IsActive, CreatedDate, LastLoginDate
                FROM Users
                WHERE Id = $id
            ";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapUser(reader);
            }

            return null;
        }

        // Check if any users exist (for setup verification)
        public bool HasUsers()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users";
            var count = (long)command.ExecuteScalar()!;
            
            return count > 0;
        }

        // Get user count
        public int GetUserCount()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        // Get admin count
        public int GetAdminCount()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Role = 'Admin' AND IsActive = 1";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        // Delete user (with admin count check)
        public bool DeleteUser(int userId)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            // Check if this is an admin
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT Role FROM Users WHERE Id = $id";
            checkCommand.Parameters.AddWithValue("$id", userId);
            var role = checkCommand.ExecuteScalar()?.ToString();

            // If deleting an admin, ensure at least one admin remains
            if (role == "Admin")
            {
                var adminCount = GetAdminCount();
                if (adminCount <= 1)
                {
                    return false; // Cannot delete the last admin
                }
            }

            // Delete the user
            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM Users WHERE Id = $id";
            deleteCommand.Parameters.AddWithValue("$id", userId);
            deleteCommand.ExecuteNonQuery();

            return true;
        }

        // Test method to verify admin account
        public void VerifyAdminAccount()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Username, PasswordHash, Role FROM Users WHERE Username = 'admin'";
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                System.Diagnostics.Debug.WriteLine($"Admin account found:");
                System.Diagnostics.Debug.WriteLine($"  Username: {reader.GetString(0)}");
                System.Diagnostics.Debug.WriteLine($"  PasswordHash: {reader.GetString(1)}");
                System.Diagnostics.Debug.WriteLine($"  Role: {reader.GetString(2)}");
                System.Diagnostics.Debug.WriteLine($"  Expected hash for 'admin123': {HashPassword("admin123")}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Admin account NOT found!");
            }
        }

        private void UpdateLastLogin(int userId)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users 
                SET LastLoginDate = CURRENT_TIMESTAMP
                WHERE Id = $id
            ";
            command.Parameters.AddWithValue("$id", userId);
            command.ExecuteNonQuery();
        }

        private User MapUser(SqliteDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.GetString(3),
                FullNameArabic = reader.IsDBNull(4) ? null : reader.GetString(4),
                Role = Enum.Parse<UserRole>(reader.GetString(5)),
                IsActive = reader.GetInt32(6) == 1,
                CreatedDate = DateTime.Parse(reader.GetString(7)),
                LastLoginDate = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8))
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
