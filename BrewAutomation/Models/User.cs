using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrewAutomation.API.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Required]
        [Column("role")]
        public string Role { get; set; } = "User";

        // 2. Статус (Чи забанений)
        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("subscription_type")]
        public string SubscriptionType { get; set; } = "Free";

        public ICollection<Recipe> Recipes { get; set; }
        public ICollection<BrewSession> BrewSessions { get; set; }
    }
}