using PortVault.Api.Models;

namespace PortVault.Api.Repositories
{
    public interface IPortfolioRepository
    {
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync();
        Task<Portfolio?> GetPortfolioByIdAsync(Guid portfolioId);
        Task<Portfolio> CreateAsync(string name, Guid userId);
        Task<Holding[]> GetHoldingsByPortfolioIdAsync(Guid portfolioId);
        Task<int> AddTransactionsAsync(IEnumerable<Transaction> transactions);
        Task<bool> RecalculateHolding(Guid portfolioId);
    }
}
