using Microsoft.EntityFrameworkCore;
using PortVault.Api.Data;
using PortVault.Api.Models;
using PortVault.Api.Models.Entities;

namespace PortVault.Api.Services
{
    public class CorporateActionService : ICorporateActionService
    {
        private readonly AppDb _db;

        public CorporateActionService(AppDb db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CorporateAction>> GetApplicableActionsAsync(long instrumentId, DateTime? beforeDate = null)
        {
            var query = _db.CorporateActions
                .Where(ca => ca.ParentInstrumentId == instrumentId);

            if (beforeDate.HasValue)
            {
                query = query.Where(ca => ca.ExDate <= beforeDate.Value);
            }

            return await query
                .OrderBy(ca => ca.ExDate)
                .ToListAsync();
        }

        public (decimal AdjustedQuantity, decimal AdjustedPrice) AdjustForCorporateActions(
            decimal originalQuantity,
            decimal originalPrice,
            DateTime transactionDate,
            IEnumerable<CorporateAction> applicableActions)
        {
            var adjustedQty = originalQuantity;
            var adjustedPrice = originalPrice;

            foreach (var action in applicableActions.Where(ca => ca.ExDate > transactionDate).OrderBy(ca => ca.ExDate))
            {
                switch (action.Type)
                {
                    case CorporateActionType.Split:
                    case CorporateActionType.Bonus:
                        var multiplier = action.RatioNumerator / action.RatioDenominator;
                        adjustedQty *= multiplier;
                        adjustedPrice /= multiplier;
                        break;

                    case CorporateActionType.Merger:
                    case CorporateActionType.Demerger:
                        break;

                    case CorporateActionType.NameChange:
                        break;
                }
            }

            return (adjustedQty, adjustedPrice);
        }
    }
}
