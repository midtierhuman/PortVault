using System.ComponentModel.DataAnnotations;

namespace PortVault.Api.Models.Dtos
{
    public sealed class AddInstrumentIdentifierRequest
    {
        [Required]
        public string Type { get; init; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Value { get; init; } = string.Empty;
        
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
    }
}
