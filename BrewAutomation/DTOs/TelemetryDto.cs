using System.ComponentModel.DataAnnotations;

namespace BrewAutomation.DTOs
{
    public class TelemetryDto
    {
        [Required]
        public int SessionId { get; set; }

        [Required]
        public float CurrentTemperature { get; set; }

        public bool IsHeaterOn { get; set; }
    }
}