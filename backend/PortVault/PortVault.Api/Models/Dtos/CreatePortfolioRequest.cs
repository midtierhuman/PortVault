using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PortVault.Api.Models.Dtos
{
    public sealed class CreatePortfolioRequest
    {
        [Required(ErrorMessage = "Portfolio name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Portfolio name must be between 1 and 100 characters")]
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;
    }
}
