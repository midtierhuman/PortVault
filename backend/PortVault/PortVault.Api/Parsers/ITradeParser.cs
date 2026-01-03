using PortVault.Api.Models;

namespace PortVault.Api.Parsers
{
    public interface ITradeParser
    {
        string Provider { get; }
        IEnumerable<Transaction> Parse(Stream file, Guid portfolioId, Guid userId, string? password = null);
        
    }
}
