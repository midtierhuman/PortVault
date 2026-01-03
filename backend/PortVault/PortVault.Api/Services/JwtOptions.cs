namespace PortVault.Api.Services
{
    public sealed class JwtOptions
    {
        public string Issuer { get; init; } = "PortVault";
        public string Audience { get; init; } = "PortVault";
        public string SigningKey { get; init; } = string.Empty;
        public int ExpirationMinutes { get; init; } = 60;
    }
}
