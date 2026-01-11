namespace PortVault.Api.Models.Dtos
{
    public sealed class CorporateActionResponse
    {
        public long Id { get; init; }
        public string Type { get; init; } = string.Empty;
        public DateTime ExDate { get; init; }
        public long ParentInstrumentId { get; init; }
        public string ParentInstrumentName { get; init; } = string.Empty;
        public long? ChildInstrumentId { get; init; }
        public string? ChildInstrumentName { get; init; }
        public decimal RatioNumerator { get; init; }
        public decimal RatioDenominator { get; init; }
        public decimal CostPercentageAllocated { get; init; }
    }
}
