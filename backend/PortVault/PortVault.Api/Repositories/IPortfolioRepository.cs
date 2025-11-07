using PortVault.Api.Models;

namespace PortVault.Api.Repositories
{
    public interface IPortfolioRepository
    {
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync();
        Task<Portfolio?> GetPortfolioByIdAsync(string portfolioId);
    }
}
