namespace BrewAutomation.API.DTOs
{
    public class IoTResponseDto
    {
        public string Command { get; set; }
        public bool HeaterState { get; set; }

        public bool PumpState { get; set; }

        public float TargetTemperature { get; set; }
        public string CurrentStepName { get; set; }

        public string? ErrorMessage { get; set; }
    }
}