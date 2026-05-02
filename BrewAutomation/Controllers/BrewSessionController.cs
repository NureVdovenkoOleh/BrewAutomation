using BrewAutomation.API.Data;
using BrewAutomation.API.DTOs;
using BrewAutomation.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BrewAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    [Authorize] 
    public class BrewSessionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BrewSessionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("start/{recipeId}")]
        public async Task<IActionResult> StartBrewSession(int recipeId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var subscription = User.FindFirst("Subscription")?.Value;

            if (subscription == "Free")
            {
                var hasActiveSession = await _context.BrewSessions
                    .AnyAsync(s => s.UserId == userId && s.Status == "в процесі");

                if (hasActiveSession)
                {
                    return StatusCode(403, new
                    {
                        message = "Тариф Free дозволяє лише одну активну варку одночасно. Завершіть попередню або купіть Pro."
                    });
                }
            }

            var recipeExists = await _context.Recipes
                .AnyAsync(r => r.RecipeId == recipeId && r.UserId == userId.Value);

            if (!recipeExists)
            {
                return NotFound("Рецепт не знайдено або він належить іншому користувачу.");
            }

            var newSession = new BrewSession
            {
                UserId = userId.Value,
                RecipeId = recipeId,
                StartTime = DateTime.UtcNow,
                Status = "в процесі" 
            };

            _context.BrewSessions.Add(newSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSessionById), new { id = newSession.SessionId }, newSession);
        }

        [HttpGet]
        public async Task<IActionResult> GetMySessions()
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var query = _context.BrewSessions
                .Include(s => s.Recipe) 
                .Where(s => s.UserId == userId.Value)
                .OrderByDescending(s => s.StartTime)
                .AsNoTracking();

            var subscription = User.FindFirst("Subscription")?.Value;

            if (subscription == "Free")
            {
                var sessions = await query.Take(5).ToListAsync();
                return Ok(sessions);
            }

            return Ok(await query.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSessionById(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var session = await _context.BrewSessions
                .Include(s => s.Recipe) 
                    .ThenInclude(r => r.RecipeSteps) 
                .Include(s => s.TelemetryData) 
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SessionId == id && s.UserId == userId.Value);

            if (session == null)
            {
                return NotFound("Сесію не знайдено або вона належить іншому користувачу.");
            }

            return Ok(session);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSession()
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var activeSession = await _context.BrewSessions
                .Where(s => s.UserId == userId.Value && s.Status == "в процесі")
                .FirstOrDefaultAsync();

            if (activeSession == null)
            {
                return NoContent(); 
            }

            return Ok(new { sessionId = activeSession.SessionId });
        }

        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopSession(int id)
        {
            var userId = GetUserIdFromToken();
            var session = await _context.BrewSessions
                .FirstOrDefaultAsync(s => s.SessionId == id && s.UserId == userId);

            if (session == null) return NotFound();

            session.Status = "завершено";
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetSessionStatus(int id)
        {
            var userId = GetUserIdFromToken();

            var session = await _context.BrewSessions
                .Include(s => s.Recipe)
                .ThenInclude(r => r.RecipeSteps)
                .Include(s => s.TelemetryData) 
                .FirstOrDefaultAsync(s => s.SessionId == id && s.UserId == userId);

            if (session == null) return NotFound();

            var lastLog = session.TelemetryData.OrderByDescending(t => t.Timestamp).FirstOrDefault();
            float currentTemp = lastLog?.CurrentTemperature ?? 0;
            bool heaterState = lastLog?.IsHeaterOn ?? false;

            var elapsedMinutes = 0.0;
            if (session.Status == "в процесі")
            {
                elapsedMinutes = (DateTime.UtcNow - session.StartTime).TotalMinutes;
            }

            string stepName = "Очікування / Завершено";
            float targetTemp = 0;
            double timeRemaining = 0;

            if (session.Status == "в процесі")
            {
                double accumulatedTime = 0;
                bool stepFound = false;

                foreach (var step in session.Recipe.RecipeSteps.OrderBy(s => s.StepOrder))
                {
                    accumulatedTime += step.DurationMinutes;

                    if (elapsedMinutes <= accumulatedTime)
                    {
                        stepName = $"Крок {step.StepOrder}";
                        targetTemp = step.TargetTemperature;
                        timeRemaining = accumulatedTime - elapsedMinutes; 
                        stepFound = true;
                        break;
                    }
                }

                if (!stepFound) stepName = "Завершення...";
            }

            var statusDto = new BrewStatusDto
            {
                Status = session.Status,
                CurrentStepName = stepName,
                TargetTemperature = targetTemp,
                CurrentTemperature = currentTemp,
                TimeElapsedMinutes = Math.Round(elapsedMinutes, 1),
                TimeRemainingInStep = Math.Round(timeRemaining, 1),
                IsHeaterOn = heaterState
            };

            return Ok(statusDto);
        }

        private int? GetUserIdFromToken()
        {
            var userIdString = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}