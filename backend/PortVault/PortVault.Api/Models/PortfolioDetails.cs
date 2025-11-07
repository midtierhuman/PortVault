namespace PortVault.Api.Models
{
    public class PortfolioDetails
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;

        public ICollection<Asset> Holdings { get; init; } = new List<Asset>();
    }
}
