using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Frontend.Models.DTOs
{
    public class CreateFieldDefinitionDto
    {
        [JsonPropertyName("name"), Required(ErrorMessage = "Field name is required"), StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("dataType")] public Enums.DataType DataType { get; set; }
        [JsonPropertyName("isRequired")] public bool IsRequired { get; set; }
        [JsonPropertyName("isList")] public bool IsList {  get; set; }
        [JsonPropertyName("referenceTargetEntityDefinitionId")] public Guid? ReferenceTargetEntityDefinitionId { get; set; }
    }
}
