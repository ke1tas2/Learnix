using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnix.API.Models
{
    public class UserLevelProgress
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("learning_level_id")]
        public int LearningLevelId { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "not_started";

        [Column("best_score_percent")]
        public int BestScorePercent { get; set; }

        [Column("mistakes")]
        public int Mistakes { get; set; }

        [Column("attempts_count")]
        public int AttemptsCount { get; set; }

        [Column("earned_xp")]
        public int EarnedXp { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("last_attempt_at")]
        public DateTime? LastAttemptAt { get; set; }

        public User User { get; set; } = null!;
        public LearningLevel LearningLevel { get; set; } = null!;
    }
}
