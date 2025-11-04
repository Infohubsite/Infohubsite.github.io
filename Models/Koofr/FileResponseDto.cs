using System.Text.Json.Serialization;

namespace Frontend.Models.Koofr
{
    public record FileResponseDto
    {
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("type")] public required string Type { get; init; }
    }
}
