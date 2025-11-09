using OfficeOpenXml;
using PortVault.Api.Models;

namespace PortVault.Api.Parsers
{
    public class CamsKfinCasParser : ITradeParser
    {
        public string Provider => "CamsKfin";
    
        public IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId, string? password = null)
        {

            return;
        }
    }
}

