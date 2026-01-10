namespace PortVault.Api.Models.Dtos
{
    public sealed class InstrumentIdentifierResponse
    {
        public long Id { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
    }
}
