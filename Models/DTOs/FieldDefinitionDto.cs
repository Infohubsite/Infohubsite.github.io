using System.Text.Json.Serialization;

namespace Frontend.Models.DTOs
{
    public class FieldDefinitionDto
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("dataType")] public Enums.DataType DataType { get; set; }
        [JsonPropertyName("entityDefinitionId")] public Guid EntityDefinitionId { get; set; }
        [JsonPropertyName("referenceTargetEntityDefinitionId")] public Guid? ReferenceTargetEntityDefinitionId { get; set; }
    }
}