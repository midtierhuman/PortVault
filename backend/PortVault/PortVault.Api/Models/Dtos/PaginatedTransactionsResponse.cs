namespace PortVault.Api.Models.Dtos
{
    public sealed class PaginatedTransactionsResponse
    {
        public IEnumerable<TransactionResponse> Data { get; init; } = new List<TransactionResponse>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
    }
}
