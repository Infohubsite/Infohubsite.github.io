using Frontend.Models.Converted;
using Shared.DTO.Client;
using Shared.Interface;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Interactive
{
    public record FieldDefinitionI : IConverterFrom<FieldDefinitionI, FieldDefinition>, IConverterTo<FieldDefinitionI, CreateFieldDefinitionDto>, IConverterTo<FieldDefinitionI, UpdateFieldDefinitionDto>
    {
        [Required(ErrorMessage = "Field name is required")][StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")] public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Data type is required")] public Shared.Enum.DataType DataType { get; set; }
        public bool IsRequired { get; set; } = false;
        public bool IsList { get; set; } = false;
        public Guid? ReferenceTargetEntityDefinitionId { get; set; }

        public FieldDefinitionI(FieldDefinition from)
        {
            Name = from.Name;
            DataType = from.DataType;
            IsRequired = from.IsRequired;
            IsList = from.IsList;
            ReferenceTargetEntityDefinitionId = from.ReferenceTargetEntityDefinitionId;
        }
        public FieldDefinitionI() { }

        static FieldDefinitionI IConverterFrom<FieldDefinitionI, FieldDefinition>.Convert(FieldDefinition from) => new(from);
        static CreateFieldDefinitionDto IConverterTo<FieldDefinitionI, CreateFieldDefinitionDto>.Convert(FieldDefinitionI from) => new()
        {
            Name = from.Name,
            DataType = from.DataType,
            IsRequired = from.IsRequired,
            IsList = from.IsList,
            ReferenceTargetEntityDefinitionId = from.ReferenceTargetEntityDefinitionId
        };
        static UpdateFieldDefinitionDto IConverterTo<FieldDefinitionI, UpdateFieldDefinitionDto>.Convert(FieldDefinitionI from) => new()
        {
            Name = from.Name
        };
    }
}
