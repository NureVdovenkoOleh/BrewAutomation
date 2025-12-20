using BrewAutomation.API.DTOs;
using BrewAutomation.API.Services; 
using BrewAutomation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BrewAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly BrewingService _brewingService;

        public TelemetryController(BrewingService brewingService)
        {
            _brewingService = brewingService;
        }

        [HttpPost]
        public async Task<IActionResult> PostTelemetryData(TelemetryDto telemetryDto)
        {
            var response = await _brewingService.ProcessTelemetryAsync(telemetryDto);

            return Ok(response);
        }
    }
}