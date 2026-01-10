using PortVault.Api.Models;
using PortVault.Api.Models.Entities;

namespace PortVault.Api.Repositories
{
    public interface IInstrumentRepository
    {
        Task<IEnumerable<Instrument>> GetAllAsync();
        Task<Instrument?> GetByIdAsync(long id);
        Task<Instrument> CreateAsync(Instrument instrument);
        Task<Instrument?> UpdateAsync(long id, Instrument instrument);
        Task<Instrument?> GetByIdentifierAsync(IdentifierType type, string value);
        Task<InstrumentIdentifier> AddIdentifierAsync(long instrumentId, InstrumentIdentifier identifier);
        Task<InstrumentIdentifier?> MoveIdentifierAsync(long instrumentId, long identifierId);
        Task DeleteIdentifierAsync(long identifierId);
        Task<IEnumerable<Instrument>> SearchAsync(string query);
        Task<(int TransactionCount, int HoldingCount, int IdentifierCount)> GetInstrumentDependenciesAsync(long instrumentId);
        Task<bool> DeleteInstrumentAsync(long instrumentId);
        Task<(int IdentifiersMoved, int TransactionsMigrated, int HoldingsMigrated)> MigrateInstrumentAsync(long sourceInstrumentId, long targetInstrumentId);
    }
}
