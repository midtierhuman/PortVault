using System.ComponentModel.DataAnnotations;

namespace PortVault.Api.Models.Auth
{
    public sealed class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(64)]
        public string Username { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(256)]
        public string Password { get; init; } = string.Empty;
    }
}
