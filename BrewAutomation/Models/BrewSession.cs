using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrewAutomation.API.Models
{
    public class BrewSession
    {
        [Key]
        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [Column("end_time")]
        public DateTime? EndTime { get; set; } 

        [Required]
        [Column("status")]
        public string Status { get; set; } = "в процесі";

        [Required]
        [Column("user_id")]
        public int UserId { get; set; } 

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }

        public ICollection<TelemetryData> TelemetryData { get; set; } = new List<TelemetryData>();
    }
}