using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnix.API.Models
{
    public class LessonAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("learning_level_id")]
        public int LearningLevelId { get; set; }

        [Column("started_at")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("total_questions")]
        public int TotalQuestions { get; set; }

        [Column("correct_answers")]
        public int CorrectAnswers { get; set; }

        [Column("mistakes")]
        public int Mistakes { get; set; }

        [Column("score_percent")]
        public int ScorePercent { get; set; }

        [Column("earned_xp")]
        public int EarnedXp { get; set; }

        public User User { get; set; } = null!;
        public LearningLevel LearningLevel { get; set; } = null!;
        public ICollection<ExerciseAttempt> ExerciseAttempts { get; set; } = new List<ExerciseAttempt>();
    }
}
