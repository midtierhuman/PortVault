using PortVault.Api.Models.Entities;

namespace PortVault.Api.Repositories
{
    public interface ICorporateActionRepository
    {
        Task<IEnumerable<CorporateAction>> GetAllAsync();
        Task<CorporateAction?> GetByIdAsync(long id);
        Task<IEnumerable<CorporateAction>> GetByInstrumentIdAsync(long instrumentId);
        Task<CorporateAction> CreateAsync(CorporateAction corporateAction);
        Task<CorporateAction?> UpdateAsync(long id, CorporateAction corporateAction);
        Task<bool> DeleteAsync(long id);
    }
}
