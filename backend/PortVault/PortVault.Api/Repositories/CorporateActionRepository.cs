using Microsoft.EntityFrameworkCore;
using PortVault.Api.Data;
using PortVault.Api.Models.Entities;

namespace PortVault.Api.Repositories
{
    public class CorporateActionRepository : ICorporateActionRepository
    {
        private readonly AppDb _db;

        public CorporateActionRepository(AppDb db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CorporateAction>> GetAllAsync()
        {
            return await _db.CorporateActions
                .Include(ca => ca.ParentInstrument)
                .Include(ca => ca.ChildInstrument)
                .OrderByDescending(ca => ca.ExDate)
                .ToListAsync();
        }

        public async Task<CorporateAction?> GetByIdAsync(long id)
        {
            return await _db.CorporateActions
                .Include(ca => ca.ParentInstrument)
                .Include(ca => ca.ChildInstrument)
                .FirstOrDefaultAsync(ca => ca.Id == id);
        }

        public async Task<IEnumerable<CorporateAction>> GetByInstrumentIdAsync(long instrumentId)
        {
            return await _db.CorporateActions
                .Include(ca => ca.ParentInstrument)
                .Include(ca => ca.ChildInstrument)
                .Where(ca => ca.ParentInstrumentId == instrumentId || ca.ChildInstrumentId == instrumentId)
                .OrderByDescending(ca => ca.ExDate)
                .ToListAsync();
        }

        public async Task<CorporateAction> CreateAsync(CorporateAction corporateAction)
        {
            _db.CorporateActions.Add(corporateAction);
            await _db.SaveChangesAsync();
            
            return await GetByIdAsync(corporateAction.Id) ?? corporateAction;
        }

        public async Task<CorporateAction?> UpdateAsync(long id, CorporateAction corporateAction)
        {
            var existing = await _db.CorporateActions.FindAsync(id);
            if (existing == null)
                return null;

            existing.Type = corporateAction.Type;
            existing.ExDate = corporateAction.ExDate;
            existing.ParentInstrumentId = corporateAction.ParentInstrumentId;
            existing.ChildInstrumentId = corporateAction.ChildInstrumentId;
            existing.RatioNumerator = corporateAction.RatioNumerator;
            existing.RatioDenominator = corporateAction.RatioDenominator;
            existing.CostPercentageAllocated = corporateAction.CostPercentageAllocated;

            await _db.SaveChangesAsync();
            
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var corporateAction = await _db.CorporateActions.FindAsync(id);
            if (corporateAction == null)
                return false;

            _db.CorporateActions.Remove(corporateAction);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
