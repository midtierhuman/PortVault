using Microsoft.EntityFrameworkCore;
using PortVault.Api.Data;
using PortVault.Api.Models;
using System.Text;

namespace PortVault.Api.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly AppDb _db;
        public PortfolioRepository(AppDb db) => _db = db;


        public async Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync()
        {
            return await _db.Portfolios.ToListAsync();
        }

        public async Task<Portfolio?> GetPortfolioByIdAsync(Guid portfolioId)
        {
            return await _db.Portfolios.FirstOrDefaultAsync(x => x.Id == portfolioId);
        }

        public async Task<Portfolio> CreateAsync(Portfolio p)
        {
            var entity = new Portfolio
            {
                Id = Guid.NewGuid(),
                Name = p.Name,
                Invested = p.Invested,
                Current = p.Current
            };
            _db.Portfolios.Add(p);
            await _db.SaveChangesAsync();
            return p;
        }
        public async Task<Holding[]> GetHoldingsByPortfolioIdAsync(Guid portfolioId)
        {
            return await _db.Holdings
                .Where(h => h.PortfolioId == portfolioId)
                .ToArrayAsync();
        }
        public async Task<int> AddTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            try
            {
                var tradeIds = transactions.Select(x => x.TradeId).ToList();

                var existing = await _db.Transactions
                    .Where(x => tradeIds.Contains(x.TradeId))
                    .Select(x => x.TradeId)
                    .ToListAsync();

                var newOnes = transactions
                    .Where(x => !existing.Contains(x.TradeId))
                    .ToList();

                if (newOnes.Count == 0) return 0;

                _db.Transactions.AddRange(newOnes);
                return await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // optional log e
                return 0;
            }
        }

    }
}
