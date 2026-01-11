namespace PortVault.Api.Models.Dtos
{
    public sealed class AdjustedHoldingResponse
    {
        public string ISIN { get; init; } = string.Empty;
        public string Symbol { get; init; } = string.Empty;
        public decimal OriginalQty { get; init; }
        public decimal AdjustedQty { get; init; }
        public decimal OriginalAvgPrice { get; init; }
        public decimal AdjustedAvgPrice { get; init; }
        public int CorporateActionsApplied { get; init; }
        public List<string> ActionsSummary { get; init; } = new();
    }
}
