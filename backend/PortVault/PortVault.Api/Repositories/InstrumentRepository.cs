using Microsoft.EntityFrameworkCore;
using PortVault.Api.Data;
using PortVault.Api.Models;

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
    }
}
