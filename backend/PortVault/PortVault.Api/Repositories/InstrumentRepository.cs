using Microsoft.EntityFrameworkCore;
using PortVault.Api.Data;
using PortVault.Api.Models;
using PortVault.Api.Models.Entities;

namespace PortVault.Api.Repositories
{
    public class InstrumentRepository : IInstrumentRepository
    {
        private readonly AppDb _db;

        public InstrumentRepository(AppDb db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Instrument>> GetAllAsync()
        {
            return await _db.Instruments
                .Include(i => i.Identifiers)
                .ToListAsync();
        }

        public async Task<Instrument?> GetByIdAsync(long id)
        {
            return await _db.Instruments
                .Include(i => i.Identifiers)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Instrument> CreateAsync(Instrument instrument)
        {
            // EF Core will handle auto-increment ID generation
            _db.Instruments.Add(instrument);
            await _db.SaveChangesAsync();
            return instrument;
        }

        public async Task<Instrument?> UpdateAsync(long id, Instrument instrument)
        {
            var existing = await _db.Instruments.FindAsync(id);
            if (existing == null) return null;

            existing.Name = instrument.Name;
            existing.Type = instrument.Type;
            
            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<Instrument?> GetByIdentifierAsync(IdentifierType type, string value)
        {
            return await _db.Instruments
                .Include(i => i.Identifiers)
                .FirstOrDefaultAsync(i => i.Identifiers.Any(id => id.Type == type && id.Value == value));
        }

        public async Task<InstrumentIdentifier> AddIdentifierAsync(long instrumentId, InstrumentIdentifier identifier)
        {
            identifier.InstrumentId = instrumentId;
            
            _db.InstrumentIdentifiers.Add(identifier);
            await _db.SaveChangesAsync();
            return identifier;
        }

        public async Task<InstrumentIdentifier?> MoveIdentifierAsync(long instrumentId, long identifierId)
        {
            var identifier = await _db.InstrumentIdentifiers.FindAsync(identifierId);
            if (identifier == null) return null;

            var targetInstrument = await _db.Instruments.FindAsync(instrumentId);
            if (targetInstrument == null) return null; // Or throw exception

            identifier.InstrumentId = instrumentId;
            await _db.SaveChangesAsync();
            return identifier;
        }

        public async Task DeleteIdentifierAsync(long identifierId)
        {
            var identifier = await _db.InstrumentIdentifiers.FindAsync(identifierId);
            if (identifier != null)
            {
                _db.InstrumentIdentifiers.Remove(identifier);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Instrument>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<Instrument>();

            var q = query.Trim();
            
            // Search by Name or Identifier Value
            return await _db.Instruments
                .Include(i => i.Identifiers)
                .Where(i => i.Name.Contains(q) || i.Identifiers.Any(id => id.Value.Contains(q)))
                .ToListAsync();
        }

        public async Task<(int TransactionCount, int HoldingCount, int IdentifierCount)> GetInstrumentDependenciesAsync(long instrumentId)
        {
            var transactionCount = await _db.Transactions
                .CountAsync(t => t.InstrumentId == instrumentId);
            
            var holdingCount = await _db.Holdings
                .CountAsync(h => h.InstrumentId == instrumentId);
            
            var identifierCount = await _db.InstrumentIdentifiers
                .CountAsync(i => i.InstrumentId == instrumentId);
            
            return (transactionCount, holdingCount, identifierCount);
        }

        public async Task<bool> DeleteInstrumentAsync(long instrumentId)
        {
            var instrument = await _db.Instruments
                .Include(i => i.Identifiers)
                .FirstOrDefaultAsync(i => i.Id == instrumentId);
            
            if (instrument == null)
                return false;
            
            // Check for dependencies
            var transactionCount = await _db.Transactions.CountAsync(t => t.InstrumentId == instrumentId);
            var holdingCount = await _db.Holdings.CountAsync(h => h.InstrumentId == instrumentId);
            
            if (transactionCount > 0 || holdingCount > 0)
            {
                throw new InvalidOperationException("Cannot delete instrument with existing transactions or holdings");
            }
            
            // Delete all identifiers first (due to foreign key)
            if (instrument.Identifiers.Any())
            {
                _db.InstrumentIdentifiers.RemoveRange(instrument.Identifiers);
            }
            
            // Delete the instrument
            _db.Instruments.Remove(instrument);
            await _db.SaveChangesAsync();
            
            return true;
        }

        public async Task<(int IdentifiersMoved, int TransactionsMigrated, int HoldingsMigrated)> MigrateInstrumentAsync(long sourceInstrumentId, long targetInstrumentId)
        {
            // Validate both instruments exist
            var sourceInstrument = await _db.Instruments
                .Include(i => i.Identifiers)
                .FirstOrDefaultAsync(i => i.Id == sourceInstrumentId);
            
            if (sourceInstrument == null)
                throw new InvalidOperationException("Source instrument not found");
            
            var targetInstrument = await _db.Instruments.FindAsync(targetInstrumentId);
            if (targetInstrument == null)
                throw new InvalidOperationException("Target instrument not found");
            
            if (sourceInstrumentId == targetInstrumentId)
                throw new InvalidOperationException("Source and target instruments cannot be the same");
            
            int identifiersMoved = 0;
            int transactionsMigrated = 0;
            int holdingsMigrated = 0;
            
            // 1. Move all identifiers
            var identifiers = await _db.InstrumentIdentifiers
                .Where(i => i.InstrumentId == sourceInstrumentId)
                .ToListAsync();
            
            foreach (var identifier in identifiers)
            {
                // Check if target already has this identifier (to avoid duplicates)
                var existingIdentifier = await _db.InstrumentIdentifiers
                    .FirstOrDefaultAsync(i => i.InstrumentId == targetInstrumentId 
                                            && i.Type == identifier.Type 
                                            && i.Value == identifier.Value);
                
                if (existingIdentifier == null)
                {
                    identifier.InstrumentId = targetInstrumentId;
                    identifiersMoved++;
                }
                else
                {
                    // Target already has this identifier, delete the source one
                    _db.InstrumentIdentifiers.Remove(identifier);
                }
            }
            
            // 2. Migrate all transactions
            var transactions = await _db.Transactions
                .Where(t => t.InstrumentId == sourceInstrumentId)
                .ToListAsync();
            
            foreach (var transaction in transactions)
            {
                transaction.InstrumentId = targetInstrumentId;
                transactionsMigrated++;
            }
            
            // 3. Migrate all holdings (need to merge if target already has holdings for same portfolio)
            var sourceHoldings = await _db.Holdings
                .Where(h => h.InstrumentId == sourceInstrumentId)
                .ToListAsync();
            
            foreach (var sourceHolding in sourceHoldings)
            {
                var targetHolding = await _db.Holdings
                    .FirstOrDefaultAsync(h => h.PortfolioId == sourceHolding.PortfolioId 
                                            && h.InstrumentId == targetInstrumentId);
                
                if (targetHolding != null)
                {
                    // Merge holdings: calculate new weighted average price
                    var totalQty = targetHolding.Qty + sourceHolding.Qty;
                    var totalValue = (targetHolding.Qty * targetHolding.AvgPrice) + (sourceHolding.Qty * sourceHolding.AvgPrice);
                    targetHolding.AvgPrice = totalQty != 0 ? totalValue / totalQty : 0;
                    targetHolding.Qty = totalQty;
                    
                    // Remove source holding
                    _db.Holdings.Remove(sourceHolding);
                }
                else
                {
                    // No existing holding in target, just update the instrument id
                    sourceHolding.InstrumentId = targetInstrumentId;
                }
                
                holdingsMigrated++;
            }
            
            await _db.SaveChangesAsync();
            
            return (identifiersMoved, transactionsMigrated, holdingsMigrated);
        }
    }
}
