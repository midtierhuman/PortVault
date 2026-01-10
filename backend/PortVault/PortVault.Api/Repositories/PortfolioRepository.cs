using Microsoft.EntityFrameworkCore;
using PortVault.Api.Data;
using PortVault.Api.Models;
using PortVault.Api.Utils;
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
                .Include(h => h.Instrument)
                .ThenInclude(i => i.Identifiers)
                .Where(h => h.PortfolioId == portfolioId)
                .ToArrayAsync();
                
            return holdings;
        }

        public async Task<(Transaction[] Items, int TotalCount)> GetTransactionsAsync(Guid portfolioId, int page, int pageSize, DateTime? from, DateTime? to, string? search)
        {
            var query = _db.Transactions
                .Include(t => t.Instrument)
                .ThenInclude(i => i.Identifiers)
                .Where(t => t.PortfolioId == portfolioId);

            if (from.HasValue)
                query = query.Where(t => t.TradeDate >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.TradeDate <= to.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t => t.Instrument.Name.Contains(s) || 
                                         t.Instrument.Identifiers.Any(id => id.Value.Contains(s)));
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
                .Include(t => t.Instrument)
                .Where(t => t.PortfolioId == portfolioId)
                .OrderBy(t => t.TradeDate)
                .Select(t => new { t.TradeDate, t.TradeType, t.Quantity, t.Price, t.Segment, t.InstrumentId })
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
                
                var historyKeys = fullHistory.Keys.ToList();
                
                decimal GetBalanceAt(DateTime d)
                {
                    var idx = historyKeys.BinarySearch(d);
                    if (idx >= 0) return fullHistory[historyKeys[idx]];
                    
                    var nextIdx = ~idx;
                    if (nextIdx == 0) return 0;
                    return fullHistory[historyKeys[nextIdx - 1]];
                }

                var freq = frequency?.ToLowerInvariant() ?? "daily";
                
                if (freq == "transaction")
                {
                    historyPoints.Add(new TimePoint { Date = startDate, Invested = GetBalanceAt(startDate) });
                    foreach (var kvp in fullHistory.Where(x => x.Key > startDate && x.Key <= endDate))
                    {
                        historyPoints.Add(new TimePoint { Date = kvp.Key, Invested = kvp.Value });
                    }
                    if (historyPoints.Last().Date < endDate)
                        historyPoints.Add(new TimePoint { Date = endDate, Invested = GetBalanceAt(endDate) });
                }
                else
                {
                     var iterator = startDate;
                    while (iterator <= endDate)
                    {
                        historyPoints.Add(new TimePoint { Date = iterator, Invested = GetBalanceAt(iterator) });
                        if (freq == "monthly") iterator = iterator.AddMonths(1);
                        else if (freq == "weekly") iterator = iterator.AddDays(7);
                        else iterator = iterator.AddDays(1);
                    }
                    if (historyPoints.Last().Date < endDate)
                        historyPoints.Add(new TimePoint { Date = endDate, Invested = GetBalanceAt(endDate) });
                }
            }

            // 4. Sector Allocation (based on current holdings)
            var holdings = await _db.Holdings
                .Where(h => h.PortfolioId == portfolioId)
                .ToListAsync();
                
            var segmentMap = transactions
                .GroupBy(t => t.InstrumentId)
                .ToDictionary(g => g.Key, g => g.Last().Segment);

            var allocation = new List<AllocationPoint>();
            decimal totalValue = 0;
            var groupedHoldings = new Dictionary<string, decimal>();

            foreach (var h in holdings)
            {
                var segment = segmentMap.ContainsKey(h.InstrumentId) && !string.IsNullOrWhiteSpace(segmentMap[h.InstrumentId]) 
                    ? segmentMap[h.InstrumentId] 
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

        public async Task DeleteTransactionAsync(long transactionId, Guid portfolioId)
        {
            var transaction = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.PortfolioId == portfolioId);
            
            if (transaction != null)
            {
                _db.Transactions.Remove(transaction);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<(int AddedCount, List<string> Errors)> AddTransactionsAsync(IEnumerable<TransactionImportDto> parsedTransactions, Guid portfolioId, Guid userId)
        {
            var transactions = parsedTransactions.ToList();
            if (!transactions.Any()) return (0, new List<string>());

            var distinctIsins = transactions.Select(t => t.ISIN).Distinct().ToList();

            // 1. Find existing instruments by ISIN
            var existingIdentifiers = await _db.InstrumentIdentifiers
                .Where(x => x.Type == IdentifierType.ISIN && distinctIsins.Contains(x.Value))
                .Select(x => new { x.Value, x.InstrumentId })
                .ToListAsync();

            var isinToInstrumentId = existingIdentifiers.ToDictionary(x => x.Value, x => x.InstrumentId);

            // 2. Identify new instruments
            var newIsins = distinctIsins.Except(isinToInstrumentId.Keys).ToList();
            
            if (newIsins.Any())
            {
                var newInstrumentsMap = new Dictionary<string, Instrument>(); // ISIN -> Instrument

                foreach (var isin in newIsins)
                {
                    var firstTxn = transactions.First(t => t.ISIN == isin);
                    var instrument = new Instrument
                    {
                        Type = firstTxn.Segment.Equals("MF", StringComparison.OrdinalIgnoreCase) ? InstrumentType.MF : InstrumentType.EQ,
                        Name = firstTxn.Symbol // Use Symbol as Name initially
                    };
                    newInstrumentsMap[isin] = instrument;
                    _db.Instruments.Add(instrument);
                }

                await _db.SaveChangesAsync();

                var newIdentifiers = new List<InstrumentIdentifier>();
                foreach (var kvp in newInstrumentsMap)
                {
                    newIdentifiers.Add(new InstrumentIdentifier
                    {
                        InstrumentId = kvp.Value.Id,
                        Type = IdentifierType.ISIN,
                        Value = kvp.Key
                    });
                    
                    // Update map
                    isinToInstrumentId[kvp.Key] = kvp.Value.Id;
                }
                
                _db.InstrumentIdentifiers.AddRange(newIdentifiers);
                await _db.SaveChangesAsync();
            }

            // 3. Create Transactions
            var entitiesToAdd = new List<Transaction>();
            var errors = new List<string>();

            foreach(var dto in transactions)
            {
                if(isinToInstrumentId.TryGetValue(dto.ISIN, out var instrumentId))
                {
                    entitiesToAdd.Add(new Transaction
                    {
                        // Id not set (auto-increment)
                        PortfolioId = portfolioId,
                        InstrumentId = instrumentId,
                        Symbol = dto.Symbol,
                        TradeDate = dto.TradeDate,
                        OrderExecutionTime = dto.OrderExecutionTime,
                        Segment = dto.Segment,
                        Series = dto.Series,
                        TradeType = dto.TradeType,
                        Quantity = dto.Quantity,
                        Price = dto.Price,
                        TradeID = dto.TradeID,
                        OrderID = dto.OrderID
                    });
                }
                else
                {
                    errors.Add($"Could not resolve Instrument for ISIN {dto.ISIN}");
                }
            }

            try
            {
                await _db.Transactions.AddRangeAsync(entitiesToAdd);
                await _db.SaveChangesAsync();
                return (entitiesToAdd.Count, errors);
            }
            catch (Exception ex)
            {
                 errors.Add($"Bulk insert failed: {ex.Message}");
                 return (0, errors);
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

        public async Task<IEnumerable<FileUpload>> GetFileUploadsByPortfolioIdAsync(Guid portfolioId)
        {
            return await _db.FileUploads
                .Where(f => f.PortfolioId == portfolioId)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task DeleteFileUploadAsync(long fileId, Guid portfolioId)
        {
            var file = await _db.FileUploads.FirstOrDefaultAsync(f => f.Id == fileId && f.PortfolioId == portfolioId);
            if (file != null)
            {
                _db.FileUploads.Remove(file);
                await _db.SaveChangesAsync();
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
                    Units = g.Sum(t => t.TradeType == TradeType.Buy ? t.Quantity : -t.Quantity)
                })
                .ToList();

            var holdings = new List<Holding>();

            foreach (var g in grouped)
            {
                if (Math.Abs(g.Units) <= 0.1m) continue;

                var buys = txns
                    .Where(x => x.InstrumentId == g.InstrumentId && x.TradeType == TradeType.Buy)
                    .ToList();

                var totalBuyQty = buys.Sum(x => x.Quantity);
                var totalBuyAmount = buys.Sum(x => x.Quantity * x.Price);

                var avg = totalBuyQty == 0 ? 0 : totalBuyAmount / totalBuyQty;

                holdings.Add(new Holding
                {
                    // Id not set (auto-increment)
                    AvgPrice = avg,
                    PortfolioId = portfolioId,
                    InstrumentId = g.InstrumentId,
                    Qty = g.Units
                });
            }

            // Update Portfolio Totals
            var totalInvested = holdings.Sum(h => h.Qty * h.AvgPrice);
            var portfolio = await _db.Portfolios.FirstOrDefaultAsync(p => p.Id == portfolioId);
            if (portfolio != null)
            {
                portfolio.Invested = totalInvested;
                portfolio.Current = totalInvested; 
            }

            var old = _db.Holdings.Where(x => x.PortfolioId == portfolioId);
            _db.Holdings.RemoveRange(old);

            _db.Holdings.AddRange(holdings);

            await _db.SaveChangesAsync();
            return true;
        }

    }
}
