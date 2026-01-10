using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using PortVault.Api.Models;

namespace PortVault.Api.Models.Entities
{
    public class Transaction
    {
        public long Id { get; init; }
        public Guid PortfolioId { get; init; }
        
        public long InstrumentId { get; set; }
        
        public string Symbol { get; init; } = string.Empty;
        
        public DateTime TradeDate { get; init; }
        public DateTime? OrderExecutionTime { get; init; }
        public string Segment { get; init; } = string.Empty;
        public string Series { get; init; } = string.Empty;
        public TradeType TradeType { get; init; }
        public decimal Quantity { get; init; }
        public decimal Price { get; init; }
        public string? TradeID { get; init; }
        public string? OrderID { get; init; }

        [JsonIgnore]
        public Instrument? Instrument { get; set; }
    }
}
