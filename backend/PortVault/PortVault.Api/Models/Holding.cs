namespace PortVault.Api.Models
{
    public class Holding
    {
        public Guid PortfolioId { get; set; }
        public string InstrumentId { get; set; } = string.Empty;

        public decimal Qty { get; set; }
        public decimal AvgPrice { get; set; } // optional but normally needed
    }
}
