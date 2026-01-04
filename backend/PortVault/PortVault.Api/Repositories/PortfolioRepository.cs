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

        public async Task<(int AddedCount, int DuplicatesSkipped, List<string> Errors)> AddTransactionsAsync(IEnumerable<Transaction> transactions, Guid userId)
        {
            var addedCount = 0;
            var duplicatesSkipped = 0;
            var errors = new List<string>();
            var processedIds = new HashSet<Guid>();
            
            foreach (var txn in transactions)
            {
                // Skip if we've already processed this ID in the current batch
                if (processedIds.Contains(txn.Id))
                {
                    duplicatesSkipped++;
                    continue;
                }

                try
                {
                    // Check if transaction already exists in DB
                    // FindAsync checks local cache first, then DB
                    var existing = await _db.Transactions.FindAsync(txn.Id);
                    if (existing != null)
                    {
                        duplicatesSkipped++;
                        // Detach the existing entity to keep context clean
                        _db.Entry(existing).State = EntityState.Detached;
                        processedIds.Add(txn.Id);
                        continue;
                    }

                    _db.Transactions.Add(txn);
                    await _db.SaveChangesAsync();
                    
                    // Mark as processed and detach to keep context clean
                    processedIds.Add(txn.Id);
                    _db.Entry(txn).State = EntityState.Detached;
                    
                    addedCount++;
                }
                catch (DbUpdateException ex) 
                    when (ex.InnerException is SqlException sqlEx && 
                          (sqlEx.Number == 2601 || sqlEx.Number == 2627)) // Unique constraint violation
                {
                    duplicatesSkipped++;
                    processedIds.Add(txn.Id);
                    // Detach the failed entity
                    try { _db.Entry(txn).State = EntityState.Detached; } catch {}
                    
                    // Log which transaction was skipped for debugging
                    Console.WriteLine($"Duplicate transaction skipped: {txn.Symbol} ({txn.ISIN}) on {txn.TradeDate:yyyy-MM-dd} - Qty: {txn.Quantity} @ {txn.Price}");
                }
                catch (Exception ex)
                {
                    errors.Add($"{txn.Symbol} on {txn.TradeDate:yyyy-MM-dd}: {ex.Message}");
                    try { _db.Entry(txn).State = EntityState.Detached; } catch {}
                }
            }
            
            return (addedCount, duplicatesSkipped, errors);
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
                if (g.Units < 0.1m) continue;

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
