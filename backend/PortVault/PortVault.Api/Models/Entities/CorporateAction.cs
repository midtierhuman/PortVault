using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PortVault.Api.Models;

namespace PortVault.Api.Models.Entities
{
    public class CorporateAction
    {
        [Key]
        public long Id { get; set; }

        public CorporateActionType Type { get; set; }

        public DateTime ExDate { get; set; }

        public long ParentInstrumentId { get; set; }
        
        [ForeignKey(nameof(ParentInstrumentId))]
        public Instrument ParentInstrument { get; set; } = null!;

        public long? ChildInstrumentId { get; set; }
        
        [ForeignKey(nameof(ChildInstrumentId))]
        public Instrument? ChildInstrument { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        public decimal RatioNumerator { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        public decimal RatioDenominator { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        public decimal CostPercentageAllocated { get; set; } = 0m;
    }
}
