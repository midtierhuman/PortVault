using System;
using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

public static class DBHelper
{
    private static readonly string DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortVault.db");

    public static void InitializeDatabase()
    {
        if (!File.Exists(DbPath))
        {
            Console.WriteLine("Database not found! Creating new database...");
            File.Create(DbPath).Close(); // Creates an empty database file
        }

        using var connection = GetConnection();
        connection.Open();

        // Create Tables if they do not exist
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS AvailableMutualFunds (
                AmfiCode INTEGER PRIMARY KEY,
                ISIN1 TEXT,
                ISIN2 TEXT,
                FundName TEXT NOT NULL,
                NAV REAL NOT NULL,
                LastUpdated TEXT NOT NULL
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT UNIQUE NOT NULL,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                PasswordHash TEXT NOT NULL
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS UserPortfolio (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                AmfiCode INTEGER,
                StockSymbol TEXT,
                Units REAL NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id),
                FOREIGN KEY (AmfiCode) REFERENCES AvailableMutualFunds(AmfiCode)
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Stocks (
                Symbol TEXT PRIMARY KEY,
                CompanyName TEXT NOT NULL,
                LastPrice REAL NOT NULL,
                LastUpdated TEXT NOT NULL
            );");

        Console.WriteLine("Database Initialized Successfully.");
    }

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={DbPath};");
    }
}
