namespace PortVault.Api.Models.Dtos
{
    public sealed class TransactionUploadResponse
    {
        public string Message { get; init; } = string.Empty;
        public int TotalProcessed { get; init; }
        public int NewTransactions { get; init; }
        public int AddedCount { get; init; }
        public List<string> Errors { get; init; } = new();
    }
}
