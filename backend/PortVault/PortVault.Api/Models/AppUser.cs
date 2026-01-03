using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PortVault.Api.Models
{
    public sealed class AppUser
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [Required]
        [MaxLength(64)]
        [JsonPropertyName("username")]
        public string Username { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; init; } = Array.Empty<byte>();

        [Required]
        public byte[] PasswordSalt { get; init; } = Array.Empty<byte>();

        public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    }
}
