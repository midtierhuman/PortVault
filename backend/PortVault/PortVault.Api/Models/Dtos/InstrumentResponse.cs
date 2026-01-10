namespace PortVault.Api.Models.Dtos
{
    public sealed class InstrumentResponse
    {
        public long Id { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public ICollection<InstrumentIdentifierResponse> Identifiers { get; init; } = new List<InstrumentIdentifierResponse>();
    }
}
