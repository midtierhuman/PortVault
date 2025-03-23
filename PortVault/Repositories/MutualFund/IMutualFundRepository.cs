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
        Task<List<MutualFundModel>> GetFundsAsync();
        Task InsertFundsAsync(List<MutualFundModel> funds);
        Task ClearFundsAsync();
        Task<List<MutualFundModel>> SearchMutualFundsAsync(string query);
    }
}
