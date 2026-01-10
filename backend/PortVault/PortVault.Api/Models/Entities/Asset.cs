using System.ComponentModel.DataAnnotations;
using PortVault.Api.Models;

namespace PortVault.Api.Models.Entities
{
    public class Asset
    {
        [Key]
        public string ISIN { get; init; } = string.Empty;
        public AssetType Type { get; init; }
        public string Name { get; init; } = string.Empty;

        public decimal? Nav { get; init; }
        public decimal? Inav { get; init; }
        public decimal MarketPrice { get; init; }
        public DateTime LastUpdated { get; init; }

    }
}
