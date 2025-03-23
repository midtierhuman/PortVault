using PortVault.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Services.MutualFund
{
    public interface IMutualFundService
    {
        Task EnsureFundsExistAsync();
        Task<List<MutualFundModel>> GetFundsAsync();
        Task<List<MutualFundModel>> SearchFundsAsync(string query);
    }
}
