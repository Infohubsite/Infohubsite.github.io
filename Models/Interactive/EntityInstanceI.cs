using Frontend.Models.Converted;
using Shared.DTO.Client;
using Shared.Interface;

namespace Frontend.Models.Interactive
{
    public record EntityInstanceI : IConverterFrom<EntityInstanceI, EntityInstance>, IConverterTo<EntityInstanceI, CreateInstanceDto>, IConverterTo<EntityInstanceI, UpdateInstanceDto>
    {
        public Dictionary<string, object?> Data { get; set; } = [];
        private int Version { get; init; }

        public EntityInstanceI(EntityInstance from)
        {
            Data = from.Data;
            Version = from.Version;
        }
        public EntityInstanceI() { }

        static EntityInstanceI IConverterFrom<EntityInstanceI, EntityInstance>.Convert(EntityInstance from) => new(from);
        static CreateInstanceDto IConverterTo<EntityInstanceI, CreateInstanceDto>.Convert(EntityInstanceI from) => new()
        {
            Data = from.Data
        };
        static UpdateInstanceDto IConverterTo<EntityInstanceI, UpdateInstanceDto>.Convert(EntityInstanceI from) => new()
        {
            Data = from.Data,
            Version = from.Version
        };
    }
}
