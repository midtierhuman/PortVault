using PortVault.Api.Models;

namespace PortVault.Api.Services
{
    public class ParserService : IParserService
    {
        public IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId)
        {
            throw new NotImplementedException();
        }
    }
}
