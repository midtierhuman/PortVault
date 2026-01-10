namespace PortVault.Api.Models.Dtos
{
    public sealed class PortfolioResponse
    {
        public string Name { get; init; } = string.Empty;
        public decimal Invested { get; init; }
        public decimal Current { get; init; }
    }
}
