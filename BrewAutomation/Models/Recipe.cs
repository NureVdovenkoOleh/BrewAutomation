using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrewAutomation.API.Models
{
    [Table("Recipes")]
    public class Recipe
    {
        [Key]
        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; } 

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } 

        public ICollection<RecipeStep> RecipeSteps { get; set; } = new List<RecipeStep>();
    }
}