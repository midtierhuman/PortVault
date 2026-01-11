using System.ComponentModel.DataAnnotations;
using PortVault.Api.Models;

namespace PortVault.Api.Models.Dtos
{
    public sealed class CreateTransactionRequest
    {
        /// <example>TATASTEEL</example>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Symbol { get; init; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string ISIN { get; init; } = string.Empty;
        
        [Required]
        public DateTime TradeDate { get; init; }
        
        public DateTime? OrderExecutionTime { get; init; }
        
        [Required]
        [StringLength(50)]
        public string Segment { get; init; } = string.Empty;
        
        [StringLength(50)]
        public string Series { get; init; } = string.Empty;
        
        [Required]
        public string TradeType { get; init; } = string.Empty;
        
        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Quantity { get; init; }
        
        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Price { get; init; }
        
        [StringLength(100)]
        public string? TradeID { get; init; }
        
        [StringLength(100)]
        public string? OrderID { get; init; }
    }
}
