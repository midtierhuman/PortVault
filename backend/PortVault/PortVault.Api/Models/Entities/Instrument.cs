using System.Text.Json.Serialization;
using PortVault.Api.Models;

namespace PortVault.Api.Models.Entities
{
    public class Instrument
    {
        public long Id { get; set; }
        public InstrumentType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public ICollection<InstrumentIdentifier> Identifiers { get; set; } = new List<InstrumentIdentifier>();
    }
}
