using Shared.DTO.Server;
using Shared.Enum;
using Shared.Interface;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Frontend.Models.Converted
{
    public record FieldDefinition : IConverterBi<FieldDefinition, FieldDefinitionDto>
    {
        [JsonPropertyName("id")] public required Guid Id { get; init; }
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("dataType")] public required DataType DataType { get; init; }
        [JsonPropertyName("isRequired")] public required bool IsRequired { get; set; }
        [JsonPropertyName("isList")] public required bool IsList { get; set; }
        [JsonPropertyName("entityDefinitionId")] public required Guid EntityDefinitionId { get; init; }
        [JsonPropertyName("referenceTargetEntityDefinitionId")] public Guid? ReferenceTargetEntityDefinitionId { get; set; }

        [SetsRequiredMembers]
        public FieldDefinition(FieldDefinitionDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            DataType = dto.DataType;
            IsRequired = dto.IsRequired;
            IsList = dto.IsList;
            EntityDefinitionId = dto.EntityDefinitionId;
            ReferenceTargetEntityDefinitionId = dto.ReferenceTargetEntityDefinitionId;
        }
        public FieldDefinition() { }

        public static FieldDefinition Convert(FieldDefinitionDto from) => new(from);
        public static FieldDefinitionDto Convert(FieldDefinition from) => new()
        {
            Id = from.Id,
            Name = from.Name,
            DataType = from.DataType,
            IsRequired = from.IsRequired,
            IsList = from.IsList,
            EntityDefinitionId = from.EntityDefinitionId,
            ReferenceTargetEntityDefinitionId = from.ReferenceTargetEntityDefinitionId
        };
    }
}
