using System;
using System.Collections.Generic;

namespace PortVault.Api.TempModels;

public partial class Asset
{
    public string Isin { get; set; } = null!;

    public int Type { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Nav { get; set; }

    public decimal? Inav { get; set; }

    public decimal MarketPrice { get; set; }

    public DateTime LastUpdated { get; set; }
}
