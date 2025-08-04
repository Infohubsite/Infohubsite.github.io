using System.Text.Json.Serialization;

namespace Frontend.Models.DTOs
{
    public class UpdateInstanceDto
    {
        [JsonPropertyName("data")] public Dictionary<string, object?> Data { get; set; } = [];
    }
}
