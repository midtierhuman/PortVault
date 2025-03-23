using Dapper;
using PortVault.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Repositories.UserPortfolioRepository
{
    internal class UserPortfolioRepository : IUserPortfolioRepository
    {
        private readonly DBHelper _dbHelper;

        public UserPortfolioRepository(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }
        public async Task<List<UserPortfolio>> GetUserPortfoliosAsync(int userId)
        {
            using var connection = _dbHelper.GetConnection();
            await connection.OpenAsync();
            var portfolios = await connection.QueryAsync<UserPortfolio>(
                "SELECT * FROM UserPortfolio WHERE UserId = @UserId",
                new { UserId = userId });

            return portfolios.ToList();
        }

        public async Task AddPortfolioAsync(UserPortfolio portfolio)
        {
            using var connection = _dbHelper.GetConnection();
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "INSERT INTO UserPortfolio (UserId, AmfiCode, StockSymbol, Units) VALUES (@UserId, @AmfiCode, @StockSymbol, @Units)",
                portfolio);
        }

        public async Task DeletePortfolioAsync(int portfolioId)
        {
            using var connection = _dbHelper.GetConnection();
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "DELETE FROM UserPortfolio WHERE Id = @Id",
                new { Id = portfolioId });
        }
    }
}
