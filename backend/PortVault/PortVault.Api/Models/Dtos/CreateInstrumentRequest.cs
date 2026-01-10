using System.ComponentModel.DataAnnotations;

namespace PortVault.Api.Models.Dtos
{
    public sealed class CreateInstrumentRequest
    {
        [Required]
        public string Type { get; init; } = string.Empty;
        
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; init; } = string.Empty;
    }
}
