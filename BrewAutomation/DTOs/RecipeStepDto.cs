using System.ComponentModel.DataAnnotations;

namespace BrewAutomation.DTOs
{
    public class RecipeStepDto
    {
        [Required]
        [Range(1, 100)] 
        public int StepOrder { get; set; }

        [Required]
        [Range(0, 100)]
        public float TargetTemperature { get; set; }

        [Required]
        [Range(1, 1440)]
        public int DurationMinutes { get; set; }
    }
}