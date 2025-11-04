using Shared.DTO.Server;
using Shared.Extensions;
using Shared.Interface;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Frontend.Models.Converted
{
    public record EntityDefinition : IComparable<EntityDefinition>, IConverterBi<EntityDefinition, EntityDefinitionDto>
    {
        [JsonPropertyName("id")] public required Guid Id { get; init; }
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("fields")] public List<FieldDefinition> Fields { get; set; } = [];

        [SetsRequiredMembers]
        public EntityDefinition(EntityDefinitionDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Fields = [.. dto.Fields.ConvertFrom<FieldDefinition, FieldDefinitionDto>()];
        }
        public EntityDefinition() { }

        public int CompareTo(EntityDefinition? other)
        {
            if (other is null) return 1;
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public static EntityDefinition Convert(EntityDefinitionDto from) => new(from);
        public static EntityDefinitionDto Convert(EntityDefinition from) => new()
        {
            Id = from.Id,
            Name = from.Name,
            Fields = [.. from.Fields.ConvertTo<FieldDefinitionDto, FieldDefinition>()]
        };
    }
}
