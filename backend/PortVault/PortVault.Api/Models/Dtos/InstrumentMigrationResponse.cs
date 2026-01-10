namespace PortVault.Api.Models.Dtos
{
    public sealed class InstrumentMigrationResponse
    {
        public long SourceInstrumentId { get; init; }
        public string SourceInstrumentName { get; init; } = string.Empty;
        public long TargetInstrumentId { get; init; }
        public string TargetInstrumentName { get; init; } = string.Empty;
        public int IdentifiersMoved { get; init; }
        public int TransactionsMigrated { get; init; }
        public int HoldingsMigrated { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
