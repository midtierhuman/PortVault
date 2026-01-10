using System.Text.Json.Serialization;
using PortVault.Api.Models;

namespace PortVault.Api.Models.Entities
{
    public class InstrumentIdentifier
    {
        public long Id { get; set; }
        public long InstrumentId { get; set; }
        public IdentifierType Type { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        [JsonIgnore]
        public Instrument? Instrument { get; set; }
    }
}
