using Microsoft.EntityFrameworkCore;
using PortVault.Api.Data;
using PortVault.Api.Models;
using System.Text;
using Microsoft.Data.SqlClient;

namespace PortVault.Api.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly AppDb _db;
        public PortfolioRepository(AppDb db) => _db = db;


        public async Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync(Guid userId)
        {
            return await _db.Portfolios.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<Portfolio?> GetPortfolioByIdAsync(Guid portfolioId)
        {
            return await _db.Portfolios.FirstOrDefaultAsync(x => x.Id == portfolioId);
        }

        public async Task<Portfolio?> GetByNameAsync(string name, Guid userId)
        {
            return await _db.Portfolios.FirstOrDefaultAsync(x => x.Name == name && x.UserId == userId);
        }

        public async Task<Portfolio> CreateAsync(string name, Guid userId)
        {
            if (await _db.Portfolios.AnyAsync(x => x.Name == name && x.UserId == userId))
                throw new InvalidOperationException($"Portfolio '{name}' already exists.");

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
            var holdings = await _db.Holdings
                .Where(h => h.PortfolioId == portfolioId)
                .ToArrayAsync();

            if (holdings.Length == 0) return holdings;

            var isins = holdings.Select(h => h.ISIN).Distinct().ToList();

            var symbols = await _db.Transactions
                .Where(t => t.PortfolioId == portfolioId && isins.Contains(t.ISIN))
                .Select(t => new { ISIN = t.ISIN, t.Symbol })
                .Distinct()
                .ToListAsync();

            var symbolDict = symbols
                .GroupBy(x => x.ISIN)
                .ToDictionary(g => g.Key, g => g.First().Symbol);

            foreach (var h in holdings)
            {
                if (symbolDict.TryGetValue(h.ISIN, out var s))
                {
                    h.Symbol = s;
                }
            }

            return holdings;
        }

        public async Task<Transaction[]> GetTransactionsByPortfolioIdAsync(Guid portfolioId)
        {
            return await _db.Transactions
                .Where(t => t.PortfolioId == portfolioId)
                .OrderByDescending(t => t.TradeDate)
                .ThenByDescending(t => t.OrderExecutionTime)
                .ToArrayAsync();
        }

        public async Task DeleteTransactionsByPortfolioIdAsync(Guid portfolioId)
        {
            var transactions = _db.Transactions.Where(t => t.PortfolioId == portfolioId);
            _db.Transactions.RemoveRange(transactions);
            
            var holdings = _db.Holdings.Where(h => h.PortfolioId == portfolioId);
            _db.Holdings.RemoveRange(holdings);

            await _db.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(Guid transactionId, Guid portfolioId)
        {
            var transaction = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.PortfolioId == portfolioId);
            
            if (transaction != null)
            {
                _db.Transactions.Remove(transaction);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<(int AddedCount, List<string> Errors)> AddTransactionsAsync(IEnumerable<Transaction> transactions, Guid userId)
        {
            // Fast path: Bulk insert
            try
            {
                await _db.Transactions.AddRangeAsync(transactions);
                await _db.SaveChangesAsync();
                
                // Detach all to keep context clean
                foreach (var txn in transactions)
                {
                    _db.Entry(txn).State = EntityState.Detached;
                }

                return (transactions.Count(), new List<string>());
            }
            catch (Exception)
            {
                // Fallback: Slow path (one-by-one) to identify specific errors
                _db.ChangeTracker.Clear();
                
                var addedCount = 0;
                var errors = new List<string>();
                
                foreach (var txn in transactions)
                {
                    try
                    {
                        _db.Transactions.Add(txn);
                        await _db.SaveChangesAsync();
                        _db.Entry(txn).State = EntityState.Detached;
                        addedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{txn.Symbol} on {txn.TradeDate:yyyy-MM-dd}: {ex.Message}");
                        _db.ChangeTracker.Clear();
                    }
                }
                
                return (addedCount, errors);
            }
        }
        
        public async Task<bool> IsFileUploadedAsync(Guid portfolioId, string fileHash)
        {
            return await _db.FileUploads.AnyAsync(f => f.PortfolioId == portfolioId && f.FileHash == fileHash);
        }

        public async Task RecordFileUploadAsync(FileUpload fileUpload)
        {
            _db.FileUploads.Add(fileUpload);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> RecalculateHolding(Guid portfolioId)
        {
            var txns = await _db.Transactions
                .Where(x => x.PortfolioId == portfolioId)
                .ToListAsync();

            var grouped = txns
                .GroupBy(x => x.ISIN)
                .Select(g => new {
                    ISIN = g.Key,
                    Units = g.Sum(t => t.TradeType == TradeType.Buy ? t.Quantity : -t.Quantity)
                })
                .ToList();

            var holdings = new List<Holding>();

            foreach (var g in grouped)
            {
                // Skip if the position is effectively closed (within margin of error).
                // We use a threshold of 0.1 to account for fractional unit mismatches.
                // Anything outside this range (positive or negative) is considered significant.
                if (Math.Abs(g.Units) <= 0.1m) continue;

                var buys = txns
                    .Where(x => x.ISIN == g.ISIN && x.TradeType == TradeType.Buy)
                    .ToList();

                var totalBuyQty = buys.Sum(x => x.Quantity);
                var totalBuyAmount = buys.Sum(x => x.Quantity * x.Price);

                var avg = totalBuyQty == 0 ? 0 : totalBuyAmount / totalBuyQty;

                holdings.Add(new Holding
                {
                    Id = Guid.NewGuid(),
                    AvgPrice = avg,
                    PortfolioId = portfolioId,
                    ISIN = g.ISIN,
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
