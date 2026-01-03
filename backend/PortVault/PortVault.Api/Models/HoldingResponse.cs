namespace PortVault.Api.Models
{
    public sealed class HoldingResponse
    {

        public string ISIN { get; init; } = string.Empty;
        public string Symbol { get; init; } = string.Empty;
        public decimal Qty { get; init; }
        public decimal AvgPrice { get; init; }
    }
}
