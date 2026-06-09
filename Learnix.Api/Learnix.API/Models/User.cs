using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnix.API.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя обязательно")]
        [MaxLength(100)]
        [Column("name")]

        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        [Column("class")]
        public string? Class { get; set; }

        [Column("grade")]
        public int? Grade { get; set; }

        [MaxLength(32)]
        [Column("preparedness_level")]
        public string? PreparednessLevel { get; set; }

        [Column("daily_goal_minutes")]
        public int DailyGoalMinutes { get; set; } = 10;

        [Column("current_streak_days")]
        public int CurrentStreakDays { get; set; }

        [Column("best_streak_days")]
        public int BestStreakDays { get; set; }

        [Column("total_xp")]
        public int TotalXp { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("role")]
        public string Role { get; set; } = "User";

        [Column("last_activity_date")]
        public DateTime? LastActivityDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public ICollection<UserSubject> SelectedSubjects { get; set; } = new List<UserSubject>();
        public ICollection<UserLevelProgress> LevelProgresses { get; set; } = new List<UserLevelProgress>();
        public ICollection<LessonAttempt> LessonAttempts { get; set; } = new List<LessonAttempt>();
    }
}
