using System.ComponentModel.DataAnnotations;

namespace PortVault.Api.Models.Dtos
{
    public sealed class CreateCorporateActionRequest
    {
        [Required]
        public string Type { get; init; } = string.Empty;
        
        [Required]
        public DateTime ExDate { get; init; }
        
        [Required]
        public long ParentInstrumentId { get; init; }
        
        public long? ChildInstrumentId { get; init; }
        
        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal RatioNumerator { get; init; }
        
        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal RatioDenominator { get; init; }
        
        [Range(0, 100)]
        public decimal CostPercentageAllocated { get; init; } = 0m;
    }
}
