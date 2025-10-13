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

            // Enable foreign keys
            var pragmaCommand = connection.CreateCommand();
            pragmaCommand.CommandText = "PRAGMA foreign_keys = ON;";
            pragmaCommand.ExecuteNonQuery();

            // Create Users table for authentication
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    FullNameArabic TEXT,
                    Role TEXT NOT NULL CHECK(Role IN ('Admin', 'Cashier')),
                    IsActive INTEGER DEFAULT 1,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastLoginDate TEXT
                )
            ");

            // Create Categories table
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    NameArabic TEXT,
                    Description TEXT,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                )
            ");

            // Create Products table (INV-01)
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductName TEXT NOT NULL,
                    ProductNameArabic TEXT,
                    CategoryId INTEGER NOT NULL,
                    SerialNumber TEXT UNIQUE,
                    Barcode TEXT,
                    PurchasePrice REAL NOT NULL,
                    SalePrice REAL NOT NULL,
                    CurrentStock INTEGER NOT NULL DEFAULT 0,
                    MinimumStock INTEGER DEFAULT 5,
                    Description TEXT,
                    IsActive INTEGER DEFAULT 1,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedDate TEXT,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                )
            ");

            // Create index for fast product search (INV-03)
            ExecuteNonQuery(connection, @"
                CREATE INDEX IF NOT EXISTS idx_products_name 
                ON Products(ProductName)
            ");

            ExecuteNonQuery(connection, @"
                CREATE INDEX IF NOT EXISTS idx_products_serial 
                ON Products(SerialNumber)
            ");

            // Create Sales Invoices table (INV-02)
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS SalesInvoices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNumber TEXT NOT NULL UNIQUE,
                    InvoiceDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CustomerName TEXT,
                    CustomerPhone TEXT,
                    Subtotal REAL NOT NULL,
                    DiscountAmount REAL DEFAULT 0,
                    TotalAmount REAL NOT NULL,
                    TotalProfit REAL DEFAULT 0,
                    Notes TEXT,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                )
            ");

            // Create Sales Invoice Items table
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS SalesInvoiceItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    ProductName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    UnitCost REAL NOT NULL,
                    TotalPrice REAL NOT NULL,
                    Profit REAL NOT NULL,
                    FOREIGN KEY (InvoiceId) REFERENCES SalesInvoices(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                )
            ");

            // Create Purchase Invoices table (INV-04)
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS PurchaseInvoices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNumber TEXT NOT NULL UNIQUE,
                    InvoiceDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    SupplierName TEXT NOT NULL,
                    SupplierPhone TEXT,
                    TotalAmount REAL NOT NULL,
                    Notes TEXT,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                )
            ");

            // Create Purchase Invoice Items table
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS PurchaseInvoiceItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    ProductName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitCost REAL NOT NULL,
                    TotalCost REAL NOT NULL,
                    FOREIGN KEY (InvoiceId) REFERENCES PurchaseInvoices(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                )
            ");

            // Create Stock Movements table for audit trail
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS StockMovements (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductId INTEGER NOT NULL,
                    MovementType TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    ReferenceType TEXT,
                    ReferenceId INTEGER,
                    Notes TEXT,
                    MovementDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                )
            ");

            // Insert default categories and admin user
            InsertDefaultData(connection);
        }

        private void InsertDefaultData(SqliteConnection connection)
        {
            // Check if users exist - create default admin if not
            var checkUserCommand = connection.CreateCommand();
            checkUserCommand.CommandText = "SELECT COUNT(*) FROM Users";
            var userCount = (long)checkUserCommand.ExecuteScalar()!;

            if (userCount == 0)
            {
                // Create default admin user (password: admin123)
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, FullName, FullNameArabic, Role, IsActive)
                    VALUES ($username, $passwordHash, $fullName, $fullNameAr, $role, 1)
                ";
                cmd.Parameters.AddWithValue("$username", "admin");
                cmd.Parameters.AddWithValue("$passwordHash", HashPassword("admin123"));
                cmd.Parameters.AddWithValue("$fullName", "Administrator");
                cmd.Parameters.AddWithValue("$fullNameAr", "المسؤول");
                cmd.Parameters.AddWithValue("$role", "Admin");
                cmd.ExecuteNonQuery();
            }

            // Check if categories already exist
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Categories";
            var count = (long)checkCommand.ExecuteScalar()!;

            if (count == 0)
            {
                // Insert default categories
                var categories = new[]
                {
                    ("Mobiles", "الهواتف المحمولة"),
                    ("Laptops", "أجهزة الكمبيوتر المحمولة"),
                    ("Tablets", "الأجهزة اللوحية"),
                    ("Accessories", "الإكسسوارات"),
                    ("Chargers", "الشواحن"),
                    ("Headphones", "سماعات الرأس"),
                    ("Cases", "الأغطية الواقية"),
                    ("Others", "أخرى")
                };

                foreach (var (name, nameArabic) in categories)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO Categories (Name, NameArabic) 
                        VALUES ($name, $nameArabic)
                    ";
                    cmd.Parameters.AddWithValue("$name", name);
                    cmd.Parameters.AddWithValue("$nameArabic", nameArabic);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Simple password hashing using SHA256
        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        // Public method to hash password for consistency
        public string HashPasswordPublic(string password)
        {
            return HashPassword(password);
        }

        private void ExecuteNonQuery(SqliteConnection connection, string commandText)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
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

        // Generate unique invoice number (INV-02)
        public string GenerateInvoiceNumber(string prefix)
        {
            using var connection = GetConnection();
            connection.Open();

            var tableName = prefix == "SALE" ? "SalesInvoices" : "PurchaseInvoices";
            var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT COUNT(*) FROM {tableName} 
                WHERE InvoiceNumber LIKE $pattern
            ";
            command.Parameters.AddWithValue("$pattern", $"{prefix}-{DateTime.Now.Year}-%");
            
            var count = (long)command.ExecuteScalar()!;
            return $"{prefix}-{DateTime.Now.Year}-{(count + 1):D5}";
        }

        // Seed database with sample data
        public void SeedDatabase()
        {
            using var connection = GetConnection();
            connection.Open();

            // Check if data already exists
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Products";
            var productCount = (long)checkCmd.ExecuteScalar()!;

            if (productCount > 0)
            {
                // Data already exists, don't seed again
                return;
            }

            using var transaction = connection.BeginTransaction();

            try
            {
                // Seed Categories (assuming Categories table has IDs 1-6 already)
                var categories = new[]
                {
                    (1, "Electronics", "إلكترونيات"),
                    (2, "Food", "أغذية"),
                    (3, "Clothing", "ملابس"),
                    (4, "Books", "كتب"),
                    (5, "Home", "منزلية"),
                    (6, "Sports", "رياضة")
                };

                // Seed Products
                var products = new[]
                {
                    // Electronics
                    ("Laptop Dell XPS", "لابتوب ديل", "LAP001", 1, 2500.00m, 3200.00m, 5, 3),
                    ("HP Laptop", "لابتوب اتش بي", "LAP002", 1, 1800.00m, 2400.00m, 8, 3),
                    ("Wireless Mouse", "فأرة لاسلكية", "MOU001", 1, 25.00m, 45.00m, 50, 10),
                    ("Keyboard Gaming", "كيبورد ألعاب", "KEY001", 1, 80.00m, 120.00m, 30, 10),
                    ("USB Cable", "كابل يو إس بي", "USB001", 1, 5.00m, 12.00m, 100, 20),
                    ("Headphones", "سماعات رأس", "HEA001", 1, 35.00m, 65.00m, 40, 10),
                    ("Webcam HD", "كاميرا ويب", "CAM001", 1, 45.00m, 80.00m, 25, 8),
                    ("Monitor 24 inch", "شاشة 24 بوصة", "MON001", 1, 400.00m, 600.00m, 12, 5),
                    
                    // Food Items
                    ("Rice 5kg", "رز 5 كيلو", "RIC001", 2, 15.00m, 22.00m, 200, 50),
                    ("Sugar 2kg", "سكر 2 كيلو", "SUG001", 2, 8.00m, 12.00m, 150, 40),
                    ("Olive Oil 1L", "زيت زيتون 1 لتر", "OIL001", 2, 25.00m, 38.00m, 80, 20),
                    ("Coffee 250g", "قهوة 250 جرام", "COF001", 2, 18.00m, 28.00m, 100, 25),
                    ("Tea Box", "علبة شاي", "TEA001", 2, 12.00m, 18.00m, 120, 30),
                    ("Pasta 500g", "معكرونة 500 جرام", "PAS001", 2, 6.00m, 10.00m, 180, 40),
                    ("Milk 1L", "حليب 1 لتر", "MIL001", 2, 5.00m, 8.00m, 90, 25),
                    
                    // Clothing
                    ("T-Shirt Cotton", "قميص قطني", "TSH001", 3, 25.00m, 50.00m, 60, 15),
                    ("Jeans Pants", "بنطال جينز", "JEA001", 3, 60.00m, 100.00m, 40, 10),
                    ("Sports Shoes", "حذاء رياضي", "SHO001", 3, 80.00m, 140.00m, 35, 10),
                    ("Jacket Winter", "جاكيت شتوي", "JAC001", 3, 120.00m, 200.00m, 25, 8),
                    ("Socks Pack", "شرابات عبوة", "SOC001", 3, 8.00m, 15.00m, 100, 20),
                    
                    // Books
                    ("Novel Arabic", "رواية عربية", "BOO001", 4, 15.00m, 30.00m, 50, 10),
                    ("Programming Book", "كتاب برمجة", "BOO002", 4, 40.00m, 70.00m, 30, 8),
                    ("Children Story", "قصة أطفال", "BOO003", 4, 12.00m, 22.00m, 60, 15),
                    ("Dictionary", "قاموس", "BOO004", 4, 35.00m, 60.00m, 25, 8),
                    
                    // Home Items
                    ("Dinner Plate Set", "طقم صحون", "PLA001", 5, 50.00m, 90.00m, 40, 10),
                    ("Glass Set", "طقم كاسات", "GLA001", 5, 30.00m, 55.00m, 50, 12),
                    ("Kitchen Knife", "سكين مطبخ", "KNI001", 5, 15.00m, 28.00m, 70, 15),
                    ("Cooking Pot", "قدر طبخ", "POT001", 5, 45.00m, 80.00m, 35, 10),
                    ("Bed Sheet Set", "طقم ملايات", "BED001", 5, 60.00m, 110.00m, 45, 12),
                    
                    // Sports
                    ("Football", "كرة قدم", "BAL001", 6, 30.00m, 55.00m, 50, 12),
                    ("Basketball", "كرة سلة", "BAL002", 6, 35.00m, 60.00m, 40, 10),
                    ("Yoga Mat", "سجادة يوغا", "YOG001", 6, 25.00m, 45.00m, 55, 15),
                    ("Dumbbells 5kg", "دمبل 5 كيلو", "DUM001", 6, 40.00m, 70.00m, 30, 10),
                    ("Jump Rope", "حبل قفز", "JUM001", 6, 8.00m, 15.00m, 80, 20)
                };

                foreach (var (name, nameArabic, barcode, categoryId, purchasePrice, salePrice, stock, minStock) in products)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO Products (ProductName, ProductNameArabic, Barcode, CategoryId, PurchasePrice, SalePrice, CurrentStock, MinimumStock, CreatedDate)
                        VALUES ($name, $nameArabic, $barcode, $categoryId, $purchasePrice, $salePrice, $stock, $minStock, $createdDate)
                    ";
                    cmd.Parameters.AddWithValue("$name", name);
                    cmd.Parameters.AddWithValue("$nameArabic", nameArabic);
                    cmd.Parameters.AddWithValue("$barcode", barcode);
                    cmd.Parameters.AddWithValue("$categoryId", categoryId);
                    cmd.Parameters.AddWithValue("$purchasePrice", purchasePrice);
                    cmd.Parameters.AddWithValue("$salePrice", salePrice);
                    cmd.Parameters.AddWithValue("$stock", stock);
                    cmd.Parameters.AddWithValue("$minStock", minStock);
                    cmd.Parameters.AddWithValue("$createdDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
