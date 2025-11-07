using System.ComponentModel.DataAnnotations;

namespace PortVault.Api.Models
{
    public class Asset
    {
        [Key]
        public string InstrumentId { get; init; } = string.Empty; // ISIN or SYMBOL
        public AssetType Type { get; init; }
        public string Name { get; init; } = string.Empty;

        public decimal? Nav { get; init; }
        public decimal? Inav { get; init; }
        public decimal MarketPrice { get; init; } // for mfs marketprice = nav, but for etfs marketprice != nav or inav
        public DateTime LastUpdated { get; init; }

    }
}
