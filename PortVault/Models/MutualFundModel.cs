using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Models
{
    public class MutualFundModel
    {
            public int SchemeCode { get; set; }
            public string? ISINDivPayoutOrGrowth { get; set; } 
            public string? ISINDivReinvestment { get; set; }  
            public string SchemeName { get; set; } = string.Empty;
            public decimal NetAssetValue { get; set; } 
            public DateTime NAVDate { get; set; } 
       
    }
}
