using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Frontend.Models.DTOs
{
    public class CreateFieldDefinitionDto
    {
        [Required(ErrorMessage = "Field name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("dataType")]
        public Enums.DataType DataType { get; set; }

        [JsonPropertyName("referenceTargetEntityDefinitionId")]
        public Guid? ReferenceTargetEntityDefinitionId { get; set; }
    }
}
