using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

public  class DBHelper
{
    private readonly string _dbPath;

    public DBHelper()
    {
        _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortVault.db");
    }

    public async Task InitializeDatabase()
    {
        try
        {
            if (!File.Exists(_dbPath))
            {
                Console.WriteLine("Database not found! Creating new database...");
                File.Create(_dbPath).Close(); // Creates an empty database file
            }

            using var connection = GetConnection();
            connection.Open();

            // Create Tables if they do not exist
            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS MutualFunds (
                SchemeCode INTEGER PRIMARY KEY,
                SchemeName TEXT NOT NULL,
                ISINDivPayoutOrGrowth TEXT NULL,
                ISINDivReinvestment TEXT NULL,
                NetAssetValue REAL NOT NULL,
                NAVDate TEXT NOT NULL 
            );");

            await connection.ExecuteAsync(@"
            CREATE INDEX IF NOT EXISTS idx_mutualfunds_schemename 
            ON MutualFunds(SchemeName);
            ");
            await connection.ExecuteAsync(@"
            CREATE INDEX IF NOT EXISTS idx_mutualfunds_schemecode
            ON MutualFunds(SchemeCode);
            ");

            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS LastUpdateLog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EntityType TEXT NOT NULL UNIQUE CHECK (EntityType IN ('MutualFunds', 'Stocks')),
                LastUpdatedUtc DATETIME NOT NULL
            );");

            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT UNIQUE NOT NULL,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                PasswordHash TEXT NOT NULL
            );");

            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS UserPortfolios (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                PortfolioName TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );");

            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS UserMFPortfolio (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                PortfolioId INTEGER NOT NULL,
                SchemeCode INTEGER,
                SchemeName TEXT,
                Units REAL NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id),
                FOREIGN KEY (PortfolioId) REFERENCES UserPortfolios(Id),
                FOREIGN KEY (SchemeCode) REFERENCES MutualFunds(SchemeCode)
            );");

            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS UserStockPortfolio (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                PortfolioId INTEGER NOT NULL,
                Symbol TEXT NOT NULL,
                Name TEXT NOT NULL,
                LastPrice REAL NOT NULL,
                LastUpdated TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id),
                FOREIGN KEY (PortfolioId) REFERENCES UserPortfolios(Id)
            );");

            Console.WriteLine("Database Initialized Successfully.");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error initializing database: {ex.Message}");
        }
    }

    internal SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={_dbPath};");
    }
}
