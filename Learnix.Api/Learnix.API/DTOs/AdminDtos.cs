using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Learnix.API.Models;

namespace Learnix.API.DTOs
{
    public class AdminStatsDto
    {
        public int UsersCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public int AdminsCount { get; set; }
        public int SubjectsCount { get; set; }
        public int ActiveSubjectsCount { get; set; }
        public int LevelsCount { get; set; }
        public int ActiveLevelsCount { get; set; }
        public int ExercisesCount { get; set; }
        public int AttemptsCount { get; set; }
        public int CompletedAttemptsCount { get; set; }
        public int TotalMistakes { get; set; }
        public int AverageScorePercent { get; set; }
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Class { get; set; }
        public int? Grade { get; set; }
        public string Role { get; set; } = UserRoles.User;
        public bool IsActive { get; set; }
        public int DailyGoalMinutes { get; set; }
        public int CurrentStreakDays { get; set; }
        public int BestStreakDays { get; set; }
        public int TotalXp { get; set; }
        public int CompletedLevelsCount { get; set; }
        public int AttemptsCount { get; set; }
        public int TotalMistakes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateUserRoleDto
    {
        [Required]
        [MaxLength(32)]
        public string Role { get; set; } = UserRoles.User;
    }

    public class UpdateUserActiveDto
    {
        public bool IsActive { get; set; } = true;
    }

    public class AdminSubjectDto
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
        public bool IsActive { get; set; }
        public int LevelsCount { get; set; }

        public static AdminSubjectDto FromSubject(Subject subject, int levelsCount)
        {
            return new AdminSubjectDto
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
                SourceUrl = subject.SourceUrl,
                IsActive = subject.IsActive,
                LevelsCount = levelsCount
            };
        }
    }

    public class UpsertSubjectDto
    {
        [Required]
        [MaxLength(40)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(40)]
        public string Grades { get; set; } = "7-11";

        [MaxLength(16)]
        public string ColorHex { get; set; } = "#58CC02";

        [MaxLength(40)]
        public string IconKey { get; set; } = "book";

        public int SortOrder { get; set; }

        [MaxLength(250)]
        public string? SourceTitle { get; set; }

        [MaxLength(500)]
        public string? SourceUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class AdminLearningLevelDto
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
        public bool IsActive { get; set; }
        public List<AdminExerciseDto> Exercises { get; set; } = new();

        public static AdminLearningLevelDto FromLevel(LearningLevel level)
        {
            return new AdminLearningLevelDto
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
                IsActive = level.IsActive,
                Exercises = level.Exercises
                    .OrderBy(e => e.SortOrder)
                    .Select(AdminExerciseDto.FromExercise)
                    .ToList()
            };
        }
    }

    public class UpsertLearningLevelDto
    {
        public int SubjectId { get; set; }
        public int Grade { get; set; } = 7;
        public int Order { get; set; } = 1;

        [Required]
        [MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(700)]
        public string? Description { get; set; }

        public int XpReward { get; set; } = 15;

        [MaxLength(250)]
        public string? SourceTitle { get; set; }

        [MaxLength(500)]
        public string? SourceUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public List<UpsertExerciseDto> Exercises { get; set; } = new();
    }

    public class AdminExerciseDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "single_choice";
        public string Prompt { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int SortOrder { get; set; }
        public int XpReward { get; set; }

        public static AdminExerciseDto FromExercise(Exercise exercise)
        {
            return new AdminExerciseDto
            {
                Id = exercise.Id,
                Type = exercise.Type,
                Prompt = exercise.Prompt,
                Options = JsonSerializer.Deserialize<List<string>>(exercise.OptionsJson) ?? new List<string>(),
                CorrectAnswer = exercise.CorrectAnswer,
                Explanation = exercise.Explanation,
                SortOrder = exercise.SortOrder,
                XpReward = exercise.XpReward
            };
        }
    }

    public class UpsertExerciseDto
    {
        [Required]
        [MaxLength(40)]
        public string Type { get; set; } = "single_choice";

        [Required]
        [MaxLength(1000)]
        public string Prompt { get; set; } = string.Empty;

        public List<string> Options { get; set; } = new();

        [Required]
        [MaxLength(500)]
        public string CorrectAnswer { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Explanation { get; set; }

        public int SortOrder { get; set; }
        public int XpReward { get; set; } = 5;
    }
}
