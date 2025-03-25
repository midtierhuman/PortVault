using PortVault.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Repositories.MutualFund
{
    public interface IMutualFundRepository
    {
        Task<int> GetFundCountAsync();
        Task<List<MutualFundModel>> GetAllFundsAsync();
        Task InsertFundsAsync(List<MutualFundModel> funds);
        Task ClearFundsAsync();
        Task<List<MutualFundModel>> SearchMutualFundsAsync(string query);
        Task UpdateNAVTimeAsync();
        Task<DateTime?> GetLastNAVTimeAsync();
        Task BulkUpdateNAVsAsync(List<MutualFundModel> funds);
        Task BulkInsertNewFundsAsync(List<MutualFundModel> newFunds);
    }
}
