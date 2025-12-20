using BrewAutomation.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrewAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserId,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.SubscriptionType,
                })
                .ToListAsync();

            return Ok(users);
        }


        [HttpPost("users/{id}/toggle-ban")]
        public async Task<IActionResult> ToggleUserBan(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            user.IsActive = !user.IsActive;

            await _context.SaveChangesAsync();

            string status = user.IsActive ? "активирован" : "заблокирован";
            return Ok(new { message = $"Пользователь {user.Email} был {status}." });
        }
    }
}