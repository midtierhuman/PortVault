using PortVault.Api.Models;
using PortVault.Api.Models.Entities;
using PortVault.Api.Models.Dtos;

namespace PortVault.Api.Repositories
{
    public interface IPortfolioRepository
    {
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync(Guid userId);
        Task<Portfolio?> GetPortfolioByIdAsync(Guid portfolioId);
        Task<Portfolio?> GetByNameAsync(string name, Guid userId);
        Task<Portfolio> CreateAsync(string name, Guid userId);
        Task<Holding[]> GetHoldingsByPortfolioIdAsync(Guid portfolioId);
        Task<(Transaction[] Items, int TotalCount)> GetTransactionsAsync(Guid portfolioId, int page, int pageSize, DateTime? from, DateTime? to, string? search);
        Task<AnalyticsResponse> GetPortfolioAnalyticsAsync(Guid portfolioId, DateTime? from, string frequency);
        Task DeleteTransactionsByPortfolioIdAsync(Guid portfolioId);
        Task DeleteTransactionAsync(long transactionId, Guid portfolioId);
        Task<(int AddedCount, List<string> Errors)> AddTransactionsAsync(IEnumerable<TransactionImportDto> transactions, Guid portfolioId, Guid userId);
        Task<bool> RecalculateHolding(Guid portfolioId);
        Task<bool> IsFileUploadedAsync(Guid portfolioId, string fileHash);
        Task RecordFileUploadAsync(FileUpload fileUpload);
        Task<IEnumerable<FileUpload>> GetFileUploadsByPortfolioIdAsync(Guid portfolioId);
        Task DeleteFileUploadAsync(long fileId, Guid portfolioId);
    }
}
