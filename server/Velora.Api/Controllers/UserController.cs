using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly PasswordHasher<User> _password_hasher = new();
        private readonly IConfiguration _config;

        public UserController(VeloraDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

            user.Password = _password_hasher.HashPassword(user, request.Password);

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

        // POST: api/user/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var passwordResult = _password_hasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            if (!user.IsActive)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Account is inactive."
                });
            }

            var token = GenerateJwtToken(user, out var expiresAt);

            return Ok(new
            {
                token,
                expiresAt,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email
                }
            });
        }

        private string GenerateJwtToken(User user, out DateTime expiresAt)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryMinutes = double.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");
            expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}

