using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrewAutomation.API.Models
{
    public class TelemetryData
    {
        [Key]
        [Column("log_id")]
        public long LogId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("current_temperature")]
        public float CurrentTemperature { get; set; }

        [Column("is_heater_on")]
        public bool IsHeaterOn { get; set; }

        [Required]
        [Column("session_id")]
        public int SessionId { get; set; } 

        [ForeignKey("SessionId")]
        public BrewSession BrewSession { get; set; }
    }
}