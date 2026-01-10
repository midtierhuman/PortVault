namespace PortVault.Api.Models.Dtos
{
    public sealed class InstrumentDependenciesResponse
    {
        public long InstrumentId { get; init; }
        public string InstrumentName { get; init; } = string.Empty;
        public bool CanDelete { get; init; }
        public int TransactionCount { get; init; }
        public int HoldingCount { get; init; }
        public int IdentifierCount { get; init; }
        public ICollection<InstrumentIdentifierResponse> Identifiers { get; init; } = new List<InstrumentIdentifierResponse>();
        public string Message { get; init; } = string.Empty;
    }
}
