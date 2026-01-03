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

        public async Task<Portfolio> CreateAsync(string name, Guid userId)
        {
            var portfolio = new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name.Trim(),
                Invested = 0,
                Current = 0
            };
            _db.Portfolios.Add(portfolio);
            await _db.SaveChangesAsync();
            return portfolio;
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
                var transactionHashes = transactions.Select(x => x.TransactionHash).ToList();

                var existing = await _db.Transactions
                    .Where(x => transactionHashes.Contains(x.TransactionHash))
                    .Select(x => x.TransactionHash)
                    .ToListAsync();

                var existingSet = existing.ToHashSet();

                var newOnes = transactions
                    .Where(x => !existingSet.Contains(x.TransactionHash))
                    .ToList();

                if (newOnes.Count == 0) return 0;

                _db.Transactions.AddRange(newOnes);
                return await _db.SaveChangesAsync();
            }
            catch (Exception e)
            { 
                var msg = e.Message;
                return 0;
            }
        }
        public async Task<bool> RecalculateHolding(Guid portfolioId)
        {
            var txns = await _db.Transactions
                .Where(x => x.PortfolioId == portfolioId)
                .ToListAsync();

            var grouped = txns
                .GroupBy(x => x.InstrumentId)
                .Select(g => new {
                    InstrumentId = g.Key,
                    Units = g.Sum(t => t.TradeType == TradeType.Buy ? t.Qty : -t.Qty)
                })
                .ToList();

            var holdings = new List<Holding>();

            foreach (var g in grouped)
            {
                if (g.Units <= 0) continue;

                var buys = txns
                    .Where(x => x.InstrumentId == g.InstrumentId && x.TradeType == TradeType.Buy)
                    .ToList();

                var totalBuyQty = buys.Sum(x => x.Qty);
                var totalBuyAmount = buys.Sum(x => x.Qty * x.Price);

                var avg = totalBuyQty == 0 ? 0 : totalBuyAmount / totalBuyQty;

                holdings.Add(new Holding
                {
                    Id = Guid.NewGuid(),
                    AvgPrice = avg,
                    PortfolioId = portfolioId,
                    InstrumentId = g.InstrumentId,
                    Qty = g.Units
                });
            }

            // nuke old holdings
            var old = _db.Holdings.Where(x => x.PortfolioId == portfolioId);
            _db.Holdings.RemoveRange(old);

            // insert fresh
            _db.Holdings.AddRange(holdings);

            await _db.SaveChangesAsync();
            return true;
        }

    }
}
