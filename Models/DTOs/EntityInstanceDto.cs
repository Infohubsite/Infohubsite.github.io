using System.Text.Json.Serialization;

namespace Frontend.Models.DTOs
{
    public class EntityInstanceDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("entityDefinitionId")]
        public Guid EntityDefinitionId { get; set; }

        [JsonPropertyName("entityDefinitionName")]
        public string EntityDefinitionName { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
