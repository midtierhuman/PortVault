using PortVault.Api.Models;
using PortVault.Api.Models.Entities;

namespace PortVault.Api.Services
{
    public interface ICorporateActionService
    {
        Task<IEnumerable<CorporateAction>> GetApplicableActionsAsync(long instrumentId, DateTime? beforeDate = null);
        
        (decimal AdjustedQuantity, decimal AdjustedPrice) AdjustForCorporateActions(
            decimal originalQuantity,
            decimal originalPrice,
            DateTime transactionDate,
            IEnumerable<CorporateAction> applicableActions);
    }
}
