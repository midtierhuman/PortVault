using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Models
{
    public class UserPortfolio
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? SchemeCode { get; set; }
        public string? MutualFundName { get; set; }
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public double Units { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
