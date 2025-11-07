using PortVault.Api.Models;

namespace PortVault.Api.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        Task<IEnumerable<Portfolio>> IPortfolioRepository.GetAllPortfoliosAsync()
        {
            throw new NotImplementedException();
        }

        Task<Portfolio?> IPortfolioRepository.GetPortfolioByIdAsync(string portfolioId)
        {
            throw new NotImplementedException();
        }
    }
}
