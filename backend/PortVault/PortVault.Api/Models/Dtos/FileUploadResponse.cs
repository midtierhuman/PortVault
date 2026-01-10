namespace PortVault.Api.Models.Dtos
{
    public sealed class FileUploadResponse
    {
        public long Id { get; init; }
        public string FileName { get; init; } = string.Empty;
        public DateTime UploadedAt { get; init; }
        public int TransactionCount { get; init; }
    }
}
