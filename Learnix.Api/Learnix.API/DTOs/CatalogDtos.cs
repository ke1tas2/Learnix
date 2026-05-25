using System.Text.Json;
using Learnix.API.Models;

namespace Learnix.API.DTOs
{
    public class SubjectDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Grades { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public string IconKey { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string SourceTitle { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;

        public static SubjectDto FromSubject(Subject subject)
        {
            return new SubjectDto
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Description = subject.Description,
                Grades = subject.Grades,
                ColorHex = subject.ColorHex,
                IconKey = subject.IconKey,
                SortOrder = subject.SortOrder,
                SourceTitle = subject.SourceTitle,
                SourceUrl = subject.SourceUrl
            };
        }
    }

    public class LearningLevelDto
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int Grade { get; set; }
        public int Order { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int XpReward { get; set; }
        public string SourceTitle { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public int ExerciseCount { get; set; }
        public string Status { get; set; } = "not_started";
        public int BestScorePercent { get; set; }

        public static LearningLevelDto FromLevel(LearningLevel level, UserLevelProgress? progress = null)
        {
            return new LearningLevelDto
            {
                Id = level.Id,
                SubjectId = level.SubjectId,
                Grade = level.Grade,
                Order = level.Order,
                Title = level.Title,
                Description = level.Description,
                XpReward = level.XpReward,
                SourceTitle = level.SourceTitle,
                SourceUrl = level.SourceUrl,
                ExerciseCount = level.Exercises.Count,
                Status = progress?.Status ?? "not_started",
                BestScorePercent = progress?.BestScorePercent ?? 0
            };
        }
    }

    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int SortOrder { get; set; }
        public int XpReward { get; set; }

        public static ExerciseDto FromExercise(Exercise exercise)
        {
            return new ExerciseDto
            {
                Id = exercise.Id,
                Type = exercise.Type,
                Prompt = exercise.Prompt,
                Options = JsonSerializer.Deserialize<List<string>>(exercise.OptionsJson) ?? new List<string>(),
                SortOrder = exercise.SortOrder,
                XpReward = exercise.XpReward
            };
        }
    }

    public class LessonDto
    {
        public LearningLevelDto Level { get; set; } = new();
        public List<ExerciseDto> Exercises { get; set; } = new();
    }
}
