namespace PortVault.Api.Models
{
    public sealed class TransactionResponse
    {
        public long Id { get; init; } // Changed from Guid
        public string Symbol { get; init; } = string.Empty;
        public string ISIN { get; init; } = string.Empty;
        public DateTime TradeDate { get; init; }
        public DateTime? OrderExecutionTime { get; init; }
        public string Segment { get; init; } = string.Empty;
        public string Series { get; init; } = string.Empty;
        public string TradeType { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
        public decimal Price { get; init; }
        public long TradeID { get; init; }
        public long OrderID { get; init; }
    }
}
