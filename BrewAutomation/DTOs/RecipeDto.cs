using System.ComponentModel.DataAnnotations;

namespace BrewAutomation.DTOs
{
    public class RecipeDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }

        public List<RecipeStepDto>? Steps { get; set; }
    }
}