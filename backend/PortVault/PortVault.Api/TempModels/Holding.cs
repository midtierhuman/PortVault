using System;
using System.Collections.Generic;

namespace PortVault.Api.TempModels;

public partial class Holding
{
    public Guid PortfolioId { get; set; }

    public string Isin { get; set; } = null!;

    public Guid Id { get; set; }

    public decimal Qty { get; set; }

    public decimal AvgPrice { get; set; }
}
