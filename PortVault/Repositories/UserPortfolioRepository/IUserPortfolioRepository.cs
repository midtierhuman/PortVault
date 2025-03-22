using PortVault.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Repositories.UserPortfolioRepository
{
    internal interface IUserPortfolioRepository
    {
            Task<List<UserPortfolio>> GetUserPortfoliosAsync(int userId);
            Task AddPortfolioAsync(UserPortfolio portfolio);
            Task DeletePortfolioAsync(int portfolioId);
    }
}
