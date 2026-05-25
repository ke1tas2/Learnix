using System.ComponentModel.DataAnnotations;

namespace Learnix.API.DTOs
{
    public class SubmitLessonDto
    {
        [Required]
        public List<SubmitExerciseAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitExerciseAnswerDto
    {
        [Required]
        public int ExerciseId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Answer { get; set; } = string.Empty;
    }

    public class LessonResultDto
    {
        public int AttemptId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Mistakes { get; set; }
        public int ScorePercent { get; set; }
        public int EarnedXp { get; set; }
        public int TotalXp { get; set; }
        public string LevelStatus { get; set; } = "not_started";
        public List<LessonAnswerResultDto> Answers { get; set; } = new();
    }

    public class LessonAnswerResultDto
    {
        public int ExerciseId { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
    }
}
