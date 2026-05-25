namespace Learnix.API.DTOs
{
    public class ProfileStatsDto
    {
        public UserResponseDto User { get; set; } = new();
        public int SelectedSubjectsCount { get; set; }
        public int CompletedLevelsCount { get; set; }
        public int AttemptsCount { get; set; }
        public int TotalMistakes { get; set; }
        public int AverageScorePercent { get; set; }
        public List<SubjectDto> SelectedSubjects { get; set; } = new();
        public List<RecentAttemptDto> RecentAttempts { get; set; } = new();
    }

    public class RecentAttemptDto
    {
        public int AttemptId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string LevelTitle { get; set; } = string.Empty;
        public int ScorePercent { get; set; }
        public int Mistakes { get; set; }
        public int EarnedXp { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
