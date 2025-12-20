namespace BrewAutomation.API.DTOs
{
    public class BrewStatusDto
    {
        public string Status { get; set; }        
        public string CurrentStepName { get; set; }
        public float TargetTemperature { get; set; } 
        public float CurrentTemperature { get; set; } 
        public double TimeElapsedMinutes { get; set; } 
        public double TimeRemainingInStep { get; set; } 
        public bool IsHeaterOn { get; set; }    
    }
}