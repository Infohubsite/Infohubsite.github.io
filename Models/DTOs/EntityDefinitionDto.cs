using System.Text.Json.Serialization;

namespace Frontend.Models.DTOs
{
    public class EntityDefinitionDto
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("fields")] public List<FieldDefinitionDto> Fields { get; set; } = [];
    }
}
