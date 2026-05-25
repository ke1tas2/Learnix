using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnix.API.Models
{
    public class ExerciseAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("lesson_attempt_id")]
        public int LessonAttemptId { get; set; }

        [Column("exercise_id")]
        public int ExerciseId { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("user_answer")]
        public string UserAnswer { get; set; } = string.Empty;

        [Column("is_correct")]
        public bool IsCorrect { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public LessonAttempt LessonAttempt { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;
    }
}
