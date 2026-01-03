using System;
using System.Collections.Generic;

namespace PortVault.Api.TempModels;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid PortfolioId { get; set; }

    public string Symbol { get; set; } = null!;

    public string Isin { get; set; } = null!;

    public DateTime TradeDate { get; set; }

    public DateTime? OrderExecutionTime { get; set; }

    public string Segment { get; set; } = null!;

    public string Series { get; set; } = null!;

    public int TradeType { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public long? OrderId { get; set; }

    public long? TradeId { get; set; }
}
