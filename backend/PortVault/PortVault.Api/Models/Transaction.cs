namespace PortVault.Api.Models
{
    public class Transaction
    {
        public Guid Id { get; init; }
        public Guid PortfolioId { get; init; }
        public string InstrumentId { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty; // 'buy' | 'sell'
        public DateTime Date { get; init; }
        public decimal Price { get; init; }
        public decimal Qty { get; init; }
    }
}
