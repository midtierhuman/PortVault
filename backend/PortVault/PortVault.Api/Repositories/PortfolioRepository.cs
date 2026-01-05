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

        public async Task<(Transaction[] Items, int TotalCount)> GetTransactionsAsync(Guid portfolioId, int page, int pageSize, DateTime? from, DateTime? to, string? search)
        {
            var query = _db.Transactions.Where(t => t.PortfolioId == portfolioId);

            if (from.HasValue)
                query = query.Where(t => t.TradeDate >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.TradeDate <= to.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t => t.Symbol.Contains(s) || t.ISIN.Contains(s));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.TradeDate)
                .ThenByDescending(t => t.OrderExecutionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();

            return (items, totalCount);
        }

        public async Task<AnalyticsResponse> GetPortfolioAnalyticsAsync(Guid portfolioId, DateTime? from, string frequency)
        {
            // 1. Fetch all transactions to calculate running balance correctly
            var transactions = await _db.Transactions
                .Where(t => t.PortfolioId == portfolioId)
                .OrderBy(t => t.TradeDate)
                .Select(t => new { t.TradeDate, t.TradeType, t.Quantity, t.Price, t.Segment, t.ISIN })
                .ToListAsync();

            // 2. Calculate daily running balance (sparse)
            var fullHistory = new SortedDictionary<DateTime, decimal>();
            decimal currentInvested = 0;
            
            var dailyTxns = transactions
                .GroupBy(t => t.TradeDate.Date)
                .OrderBy(g => g.Key);

            foreach (var day in dailyTxns)
            {
                foreach (var txn in day)
                {
                    var amount = txn.Quantity * txn.Price;
                    if (txn.TradeType == TradeType.Buy)
                        currentInvested += amount;
                    else
                        currentInvested -= amount;
                }
                fullHistory[day.Key] = currentInvested;
            }

            // 3. Resample based on Frequency and Filter by Duration
            var historyPoints = new List<TimePoint>();
            
            if (fullHistory.Any())
            {
                var startDate = from ?? fullHistory.Keys.First();
                var endDate = DateTime.UtcNow.Date;
                
                // Helper to get balance at specific date
                // Since fullHistory is sparse (only dates with txns), we need the last value <= date
                var historyKeys = fullHistory.Keys.ToList();
                
                decimal GetBalanceAt(DateTime d)
                {
                    var idx = historyKeys.BinarySearch(d);
                    if (idx >= 0) return fullHistory[historyKeys[idx]];
                    
                    var nextIdx = ~idx;
                    if (nextIdx == 0) return 0;
                    return fullHistory[historyKeys[nextIdx - 1]];
                }

                // Generate points based on frequency
                var freq = frequency?.ToLowerInvariant() ?? "daily";
                
                if (freq == "transaction")
                {
                    // Add start point
                    historyPoints.Add(new TimePoint { Date = startDate, Invested = GetBalanceAt(startDate) });
                    
                    // Add all transaction points in range
                    foreach (var kvp in fullHistory.Where(x => x.Key > startDate && x.Key <= endDate))
                    {
                        historyPoints.Add(new TimePoint { Date = kvp.Key, Invested = kvp.Value });
                    }
                    
                    // Add end point
                    if (historyPoints.Last().Date < endDate)
                        historyPoints.Add(new TimePoint { Date = endDate, Invested = GetBalanceAt(endDate) });
                }
                else
                {
                    // Periodic sampling
                    var iterator = startDate;
                    
                    // Align iterator if needed (e.g. end of month)
                    // For simplicity, we start at startDate and step forward.
                    // A more advanced implementation might align to calendar weeks/months.
                    
                    while (iterator <= endDate)
                    {
                        historyPoints.Add(new TimePoint { Date = iterator, Invested = GetBalanceAt(iterator) });
                        
                        if (freq == "monthly")
                            iterator = iterator.AddMonths(1);
                        else if (freq == "weekly")
                            iterator = iterator.AddDays(7);
                        else // daily
                            iterator = iterator.AddDays(1);
                    }
                    
                    // Ensure the final "today" point is included if not covered
                    if (historyPoints.Last().Date < endDate)
                        historyPoints.Add(new TimePoint { Date = endDate, Invested = GetBalanceAt(endDate) });
                }
            }

            // 4. Sector Allocation (based on current holdings)
            var holdings = await _db.Holdings
                .Where(h => h.PortfolioId == portfolioId)
                .ToListAsync();
                
            var segmentMap = transactions
                .GroupBy(t => t.ISIN)
                .ToDictionary(g => g.Key, g => g.Last().Segment);

            var allocation = new List<AllocationPoint>();
            decimal totalValue = 0;
            var groupedHoldings = new Dictionary<string, decimal>();

            foreach (var h in holdings)
            {
                var segment = segmentMap.ContainsKey(h.ISIN) && !string.IsNullOrWhiteSpace(segmentMap[h.ISIN]) 
                    ? segmentMap[h.ISIN] 
                    : "Other";
                
                var value = h.Qty * h.AvgPrice;
                
                if (!groupedHoldings.ContainsKey(segment))
                    groupedHoldings[segment] = 0;
                    
                groupedHoldings[segment] += value;
                totalValue += value;
            }

            foreach (var kvp in groupedHoldings)
            {
                allocation.Add(new AllocationPoint 
                { 
                    Segment = kvp.Key, 
                    Value = kvp.Value,
                    Percentage = totalValue == 0 ? 0 : Math.Round((kvp.Value / totalValue) * 100, 2)
                });
            }

            return new AnalyticsResponse 
            { 
                History = historyPoints, 
                SegmentAllocation = allocation 
            };
        }

        public async Task DeleteTransactionsByPortfolioIdAsync(Guid portfolioId)
        {
            var transactions = _db.Transactions.Where(t => t.PortfolioId == portfolioId);
            _db.Transactions.RemoveRange(transactions);
            
            var holdings = _db.Holdings.Where(h => h.PortfolioId == portfolioId);
            _db.Holdings.RemoveRange(holdings);

            var fileUploads = _db.FileUploads.Where(f => f.PortfolioId == portfolioId);
            _db.FileUploads.RemoveRange(fileUploads);

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

            // Update Portfolio Totals
            var totalInvested = holdings.Sum(h => h.Qty * h.AvgPrice);
            var portfolio = await _db.Portfolios.FirstOrDefaultAsync(p => p.Id == portfolioId);
            if (portfolio != null)
            {
                portfolio.Invested = totalInvested;
                portfolio.Current = totalInvested; // Default to invested value until market data is available
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
