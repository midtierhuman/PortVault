using PortVault.Api.Models;

namespace PortVault.Api.Models.Dtos
{
    public class TransactionImportDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string ISIN { get; set; } = string.Empty;
        public DateTime TradeDate { get; set; }
        public DateTime? OrderExecutionTime { get; set; }
        public string Segment { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
        public TradeType TradeType { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string? TradeID { get; set; }
        public string? OrderID { get; set; }
    }
}
