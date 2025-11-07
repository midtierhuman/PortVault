using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PortVault.Api.Models
{
    public sealed class Portfolio
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("invested")]
        public decimal Invested { get; init; }

        [JsonPropertyName("current")]
        public decimal Current { get; init; }

    }
    
}
