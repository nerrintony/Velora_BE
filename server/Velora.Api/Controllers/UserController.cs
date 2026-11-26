using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velora.Api.Models;
using Velora.Database;
using Velora.Database.Entities;

namespace Velora.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly VeloraDbContext _context;
        private readonly PasswordHasher<RegisterUserRequest> _password_hasher = new();

        public UserController(VeloraDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            var normalized_email = request.Email.Trim().ToLowerInvariant();

            bool email_exists = await _context.Users.AnyAsync
                (u => u.Email == normalized_email);

            if (email_exists)
                return Conflict(new { message = "email exists" });

            var now = DateTime.UtcNow;
            var user = new User
            {
                Name = request.Name.Trim(),
                Email = normalized_email,
                CreatedDttm = now,
                UpdatedDttm = now,
                IsActive = false
            };

            user.Password = _password_hasher.HashPassword(request, request.Password);

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "email exists" });
            }

            return Created($"/api/user/{user.Id}", new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                isActive = user.IsActive,
                createdDttm = user.CreatedDttm,
            });

        }
    }
}
