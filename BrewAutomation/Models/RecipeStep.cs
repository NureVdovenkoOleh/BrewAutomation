using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrewAutomation.API.Models
{
    public class RecipeStep
    {
        [Key]
        [Column("step_id")]
        public int StepId { get; set; }

        [Required]
        [Column("step_order")]
        public int StepOrder { get; set; }

        [Required]
        [Column("target_temperature")]
        public float TargetTemperature { get; set; }

        [Required]
        [Column("duration_minutes")]
        public int DurationMinutes { get; set; }

        [Required]
        [Column("recipe_id")]
        public int RecipeId { get; set; } 

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; } 
    }
}