using System.ComponentModel.DataAnnotations;

namespace PortVault.Api.Models.Dtos
{
    public sealed class LoginRequest
    {
        [MaxLength(256)]
        public string? Email { get; init; }

        [MaxLength(64)]
        public string? Username { get; init; }

        [Required]
        [MinLength(6)]
        [MaxLength(256)]
        public string Password { get; init; } = string.Empty;
    }
}
