using PortVault.Api.Models;

namespace PortVault.Api.Repositories
{
    public interface IPortfolioRepository
    {
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync(Guid userId);
        Task<Portfolio?> GetPortfolioByIdAsync(Guid portfolioId);
        Task<Portfolio?> GetByNameAsync(string name, Guid userId);
        Task<Portfolio> CreateAsync(string name, Guid userId);
        Task<Holding[]> GetHoldingsByPortfolioIdAsync(Guid portfolioId);
        Task<Transaction[]> GetTransactionsByPortfolioIdAsync(Guid portfolioId);
        Task<(int AddedCount, int DuplicatesSkipped, List<string> Errors)> AddTransactionsAsync(IEnumerable<Transaction> transactions, Guid userId);
        Task<bool> RecalculateHolding(Guid portfolioId);
    }
}
