namespace PortVault.Api.Models.Dtos
{
    public sealed class AuthResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public DateTime ExpiresUtc { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }
}
