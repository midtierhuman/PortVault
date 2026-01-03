using System.ComponentModel.DataAnnotations.Schema;

namespace PortVault.Api.Models
{
    public class Holding
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public string ISIN { get; set; } = string.Empty;

        [NotMapped]
        public string Symbol { get; set; } = string.Empty;

        public decimal Qty { get; set; }
        public decimal AvgPrice { get; set; } // optional but normally needed
    }
}
