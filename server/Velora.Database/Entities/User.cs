using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Velora.Database.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [NotNull]
        public int Id { get; set; }
        [MaxLength(100), Required]
        public string Name { get; set; }
        [MaxLength(255), Required]
        public string Email { get; set; }
        [MaxLength(255), Required]
        public string Password { get; set; }
        public DateTime CreatedDttm { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDttm { get; set; }
        public Boolean IsActive { get; set; } = false;
    }
}
