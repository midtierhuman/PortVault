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
                ISINDivPayoutOrGrowth TEXT NULL,
                ISINDivReinvestment TEXT NULL,
                SchemeName TEXT NOT NULL,
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
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT UNIQUE NOT NULL,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                PasswordHash TEXT NOT NULL
            );");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS UserPortfolio (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                AmfiCode INTEGER,
                StockSymbol TEXT,
                Units REAL NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id),
                FOREIGN KEY (AmfiCode) REFERENCES AvailableMutualFunds(AmfiCode)
            );");

       await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Stocks (
                Symbol TEXT PRIMARY KEY,
                CompanyName TEXT NOT NULL,
                LastPrice REAL NOT NULL,
                LastUpdated TEXT NOT NULL
            );");

        Console.WriteLine("Database Initialized Successfully.");
    }

    internal SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={_dbPath};");
    }
}
