namespace PortVault.Api.Models
{
    public sealed class HoldingResponse
    {
        public string InstrumentId { get; init; } = string.Empty;
        public decimal Qty { get; init; }
        public decimal AvgPrice { get; init; }
    }
}
