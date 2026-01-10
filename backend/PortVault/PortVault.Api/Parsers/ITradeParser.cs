using PortVault.Api.Models.Dtos;

namespace PortVault.Api.Parsers
{
    public interface ITradeParser
    {
        string Provider { get; }
        IEnumerable<TransactionImportDto> Parse(Stream file, Guid portfolioId, Guid userId, string? password = null);
        
    }
}
