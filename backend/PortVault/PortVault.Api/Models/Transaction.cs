using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace PortVault.Api.Models
{
    public class Transaction
    {
        public long Id { get; init; } // Changed from Guid
        public Guid PortfolioId { get; init; }
        
        public long InstrumentId { get; set; }
        
        // Asset identifiers
        public string Symbol { get; init; } = string.Empty;
        
        // Trade details
        public DateTime TradeDate { get; init; }
        public DateTime? OrderExecutionTime { get; init; }
        public string Segment { get; init; } = string.Empty;
        public string Series { get; init; } = string.Empty;
        public TradeType TradeType { get; init; }
        public decimal Quantity { get; init; }
        public decimal Price { get; init; }
        public long? TradeID { get; init; }
        public long? OrderID { get; init; }

        [JsonIgnore]
        public Instrument? Instrument { get; set; }
    }
}

