using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Frontend.Models.DTOs
{
    public class CreateEntityWithFieldsDto
    {
        [Required(ErrorMessage = "Entity name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("fields")]
        public List<CreateFieldDefinitionDto> Fields { get; set; } = [];
    }
}
