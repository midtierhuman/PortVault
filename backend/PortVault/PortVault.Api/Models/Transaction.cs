namespace PortVault.Api.Models
{
    public class Transaction
    {
        public Guid Id { get; init; }
        public int TradeId { get; init; } 
        public Guid PortfolioId { get; init; }
        public string InstrumentId { get; init; } = string.Empty;
        public TradeType TradeType { get; init; } 
        public DateTime Date { get; init; }
        public decimal Price { get; init; }
        public decimal Qty { get; init; }
    }
}
