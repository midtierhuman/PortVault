using System.Text.Json.Serialization;

namespace PortVault.Api.Models
{
    public enum IdentifierType
    {
        ISIN,
        TICKER,
        NSE_SYMBOL,
        BSE_CODE,
        SCHEME_CODE
    }

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
