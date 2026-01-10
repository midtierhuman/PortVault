using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PortVault.Api.Models
{
    public class Holding
    {
        public long Id { get; set; } // Changed from Guid
        public Guid PortfolioId { get; set; }
        public long InstrumentId { get; set; }
        
        [JsonIgnore]
        public Instrument? Instrument { get; set; }

        [NotMapped]
        public string Symbol => Instrument?.Name ?? string.Empty;

        [NotMapped]
        public string ISIN => Instrument?.Identifiers.FirstOrDefault(i => i.Type == IdentifierType.ISIN)?.Value ?? string.Empty;

        public decimal Qty { get; set; }
        public decimal AvgPrice { get; set; } // optional but normally needed
    }
}
