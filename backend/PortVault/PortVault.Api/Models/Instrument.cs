using System.Text.Json.Serialization;

namespace PortVault.Api.Models
{
    public enum InstrumentType
    {
        MF,
        EQ
    }

    public class Instrument
    {
        public long Id { get; set; }
        public InstrumentType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public ICollection<InstrumentIdentifier> Identifiers { get; set; } = new List<InstrumentIdentifier>();
    }
}
