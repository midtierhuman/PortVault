using System;
using System.Collections.Generic;

namespace PortVault.Api.TempModels;

public partial class Portfolio
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Invested { get; set; }

    public decimal Current { get; set; }
}
