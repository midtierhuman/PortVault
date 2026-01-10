using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PortVault.Api.Models;

namespace PortVault.Api.Models.Entities
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

        [Required]
        public AppRole Role { get; init; } = AppRole.User;

        public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    }
}
