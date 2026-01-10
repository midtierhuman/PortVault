using System.ComponentModel.DataAnnotations;

namespace PortVault.Api.Models.Dtos
{
    public sealed class MigrateInstrumentRequest
    {
        [Required]
        public long TargetInstrumentId { get; init; }
    }
}
