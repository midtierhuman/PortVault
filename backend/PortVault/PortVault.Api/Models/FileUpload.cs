using System;

namespace PortVault.Api.Models
{
    public class FileUpload
    {
        public long Id { get; set; } // Changed from Guid
        public Guid PortfolioId { get; set; }
        public Guid UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public int TransactionCount { get; set; }
    }
}
