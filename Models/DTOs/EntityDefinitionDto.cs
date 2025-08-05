using System.Text.Json.Serialization;

namespace Frontend.Models.DTOs
{
    public class EntityDefinitionDto : IComparable<EntityDefinitionDto>
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("fields")] public List<FieldDefinitionDto> Fields { get; set; } = [];

        public int CompareTo(EntityDefinitionDto? other)
        {
            if (other == null)
                return 1;
            return string.Compare(this.Name, other.Name, StringComparison.Ordinal);
        }
    }
}
