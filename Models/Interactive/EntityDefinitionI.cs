using Frontend.Models.Converted;
using Shared.DTO.Client;
using Shared.Extensions;
using Shared.Interface;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Interactive
{
    public record EntityDefinitionI : IConverterFrom<EntityDefinitionI, EntityDefinition>, IConverterTo<EntityDefinitionI, CreateEntityDefinitionDto>, IConverterTo<EntityDefinitionI, UpdateEntityDefinitionDto>
    {
        [Required(ErrorMessage = "Entity name is required")][StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")] public string Name { get; set; } = string.Empty;
        public List<FieldDefinitionI> Fields { get; set; } = [];

        public EntityDefinitionI(EntityDefinition from)
        {
            Name = from.Name;
            Fields = ListConverter.ConvertFrom<FieldDefinitionI, FieldDefinition>(from.Fields);
        }
        public EntityDefinitionI() { }

        static EntityDefinitionI IConverterFrom<EntityDefinitionI, EntityDefinition>.Convert(EntityDefinition from) => new(from);
        static CreateEntityDefinitionDto IConverterTo<EntityDefinitionI, CreateEntityDefinitionDto>.Convert(EntityDefinitionI from) => new()
        {
            Name = from.Name,
            Fields = ListConverter.ConvertTo<CreateFieldDefinitionDto, FieldDefinitionI>(from.Fields)
        };
        static UpdateEntityDefinitionDto IConverterTo<EntityDefinitionI, UpdateEntityDefinitionDto>.Convert(EntityDefinitionI from) => new()
        {
            Name = from.Name
        };
    }
}