using PortVault.Api.Models;

namespace PortVault.Api.Services
{
    public interface IParserService
    {
        IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId);
    }
}
