using Learnix.API.Models;

namespace Learnix.API.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Class { get; set; }
        public int? Grade { get; set; }
        public string? PreparednessLevel { get; set; }
        public int DailyGoalMinutes { get; set; }
        public int CurrentStreakDays { get; set; }
        public int BestStreakDays { get; set; }
        public int TotalXp { get; set; }
        public DateTime CreatedAt { get; set; }

        public static UserResponseDto FromUser(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Class = user.Class,
                Grade = user.Grade,
                PreparednessLevel = user.PreparednessLevel,
                DailyGoalMinutes = user.DailyGoalMinutes,
                CurrentStreakDays = user.CurrentStreakDays,
                BestStreakDays = user.BestStreakDays,
                TotalXp = user.TotalXp,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
