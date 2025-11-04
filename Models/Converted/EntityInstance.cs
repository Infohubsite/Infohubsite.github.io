using Shared.DTO.Server;
using Shared.Interface;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Frontend.Models.Converted
{
    public record EntityInstance : IConverterBi<EntityInstance, EntityInstanceDto>
    {
        [JsonPropertyName("id")] public required Guid Id { get; init; }
        [JsonPropertyName("entityDefinitionId")] public required Guid EntityDefinitionId { get; init; }
        [JsonPropertyName("data")] public Dictionary<string, object?> Data { get; set; } = [];

        [SetsRequiredMembers]
        public EntityInstance(EntityInstanceDto dto)
        {
            Id = dto.Id;
            EntityDefinitionId = dto.EntityDefinitionId;
            Data = dto.Data;
        }
        public EntityInstance() { }

        public static EntityInstance Convert(EntityInstanceDto from) => new(from);
        public static EntityInstanceDto Convert(EntityInstance from) => new()
        {
            Id = from.Id,
            EntityDefinitionId = from.EntityDefinitionId,
            Data = from.Data
        };
    }
}
