using BrewAutomation.API.Data;
using BrewAutomation.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BrewAutomation.DTOs;

namespace BrewAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    [Authorize]
    public class RecipeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecipeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe(RecipeDto createDto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var subscription = User.FindFirst("Subscription")?.Value;
            if (subscription == "Free")
            {
                var recipesCount = await _context.Recipes.CountAsync(r => r.UserId == userId);
                if (recipesCount >= 3)
                {
                    return StatusCode(403, new { message = "У вас тариф Free. Ліміт: 3 рецепти." });
                }
            }

            var recipe = new Recipe
            {
                Name = createDto.Name,
                Description = createDto.Description,
                UserId = userId.Value,
                RecipeSteps = createDto.Steps?.Select(s => new RecipeStep
                {
                    StepOrder = s.StepOrder,
                    TargetTemperature = s.TargetTemperature,
                    DurationMinutes = s.DurationMinutes
                }).ToList() ?? new List<RecipeStep>()
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecipeById), new { id = recipe.RecipeId }, recipe);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            var userId = GetUserIdFromToken();

            var recipe = await _context.Recipes
                .Include(r => r.RecipeSteps) 
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == userId); 

            if (recipe == null)
            {
                return NotFound();
            }
            return Ok(recipe);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyRecipes()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var recipes = await _context.Recipes
                .Where(r => r.UserId == userId.Value) 
                .AsNoTracking()
                .ToListAsync();

            return Ok(recipes);
        }

        [HttpPost("{recipeId}/steps")]
        public async Task<IActionResult> AddStepToRecipe(int recipeId, RecipeStepDto stepDto)
        {
            var userId = GetUserIdFromToken();

            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == userId);

            if (recipe == null)
            {
                return NotFound("Рецепт не знайдено або він належить іншому користувачу.");
            }

            var newStep = new RecipeStep
            {
                RecipeId = recipeId,
                StepOrder = stepDto.StepOrder,
                TargetTemperature = stepDto.TargetTemperature,
                DurationMinutes = stepDto.DurationMinutes
            };

            _context.RecipeSteps.Add(newStep);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecipeById), new { id = recipeId }, newStep);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRecipe(int id, RecipeDto updateDto)
        {
            var userId = GetUserIdFromToken();

            var recipe = await _context.Recipes
                .Include(r => r.RecipeSteps)
                .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == userId);

            if (recipe == null) return NotFound("Рецепт не знайдено або ви не власник.");

            recipe.Name = updateDto.Name;
            recipe.Description = updateDto.Description;

            if (updateDto.Steps != null && updateDto.Steps.Any())
            {
                _context.RecipeSteps.RemoveRange(recipe.RecipeSteps);

                var newSteps = updateDto.Steps.Select(s => new RecipeStep
                {
                    StepOrder = s.StepOrder,
                    TargetTemperature = s.TargetTemperature,
                    DurationMinutes = s.DurationMinutes,
                    RecipeId = recipe.RecipeId 
                }).ToList();

                _context.RecipeSteps.AddRange(newSteps);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var userId = GetUserIdFromToken();

            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == userId);

            if (recipe == null)
            {
                return NotFound("Рецепт не знайдено або він належить іншому користувачу.");
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
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
