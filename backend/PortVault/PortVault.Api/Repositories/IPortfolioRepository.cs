using PortVault.Api.Models;

namespace PortVault.Api.Repositories
{
    public interface IPortfolioRepository
    {
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync();
        Task<Portfolio?> GetPortfolioByIdAsync(Guid portfolioId);
        Task<Portfolio> CreateAsync(Portfolio p);
        Task<Holding[]> GetHoldingsByPortfolioIdAsync(Guid portfolioId);
    }
}
