using System.ComponentModel.DataAnnotations;

namespace Velora.Api.Models
{
    public class RegisterUserRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "At least 8 charactes.")]
        public string Password { get; set; } = string.Empty;
    }
}
