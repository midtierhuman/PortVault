using PortVault.Api.Models;
using PortVault.Api.Utils;

namespace PortVault.Api.Parsers
{
    public interface ITradeParser
    {
        string Provider { get; }
        IEnumerable<TransactionImportDto> Parse(Stream file, Guid portfolioId, Guid userId, string? password = null);
        
    }
}
