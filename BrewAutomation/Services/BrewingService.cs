using BrewAutomation.API.Data;
using BrewAutomation.API.DTOs;
using BrewAutomation.API.Models;
using BrewAutomation.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BrewAutomation.API.Services
{
    public class BrewingService
    {
        private readonly AppDbContext _context;

        public BrewingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IoTResponseDto> ProcessTelemetryAsync(TelemetryDto data)
        {
            var session = await _context.BrewSessions
                .Include(s => s.Recipe)
                .ThenInclude(r => r.RecipeSteps)
                .FirstOrDefaultAsync(s => s.SessionId == data.SessionId);

            if (session == null || session.Status != "в процесі")
            {
                return new IoTResponseDto { Command = "STOP", HeaterState = false, PumpState = false };
            }


            if (data.CurrentTemperature > 105.0)
            {
                session.Status = "АВАРІЯ"; 
                session.EndTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new IoTResponseDto
                {
                    Command = "STOP",
                    HeaterState = false,
                    PumpState = false,
                    ErrorMessage = "КРИТИЧНИЙ ПЕРЕГРІВ! СИСТЕМУ ЗУПИНЕНО."
                };
            }

            var log = new TelemetryData
            {
                SessionId = session.SessionId,
                CurrentTemperature = data.CurrentTemperature,
                IsHeaterOn = data.IsHeaterOn,
                Timestamp = DateTime.UtcNow
            };
            _context.TelemetryData.Add(log);
            await _context.SaveChangesAsync();

            var currentStep = DetermineCurrentStep(session);

            if (currentStep == null)
            {
                session.Status = "завершено";
                session.EndTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new IoTResponseDto { Command = "FINISH", HeaterState = false, PumpState = false };
            }

            bool shouldHeat = false;
            bool shouldPump = true; 

            if (data.CurrentTemperature < currentStep.TargetTemperature - 0.5f)
            {
                shouldHeat = true;
            }
            else if (data.CurrentTemperature >= currentStep.TargetTemperature)
            {
                shouldHeat = false;
            }


            if (currentStep.TargetTemperature > 90)
            {
                shouldPump = false;
            }

            return new IoTResponseDto
            {
                Command = "WORK",
                HeaterState = shouldHeat,
                PumpState = shouldPump, 
                TargetTemperature = currentStep.TargetTemperature,
                CurrentStepName = $"Крок {currentStep.StepOrder}"
            };
        }

        private RecipeStep? DetermineCurrentStep(BrewSession session)
        {
            var elapsedMinutes = (DateTime.UtcNow - session.StartTime).TotalMinutes;
            double accumulatedTime = 0;

            foreach (var step in session.Recipe.RecipeSteps.OrderBy(s => s.StepOrder))
            {
                accumulatedTime += step.DurationMinutes;
                if (elapsedMinutes <= accumulatedTime)
                {
                    return step;
                }
            }
            return null; 
        }
    }
}