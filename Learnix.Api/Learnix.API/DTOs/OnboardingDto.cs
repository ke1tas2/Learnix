using System.ComponentModel.DataAnnotations;

namespace Learnix.API.DTOs
{
    public class UpdateOnboardingDto
    {
        [Range(7, 11, ErrorMessage = "Learnix сейчас рассчитан на 7-11 классы")]
        public int? Grade { get; set; }

        [MaxLength(10, ErrorMessage = "Класс не должен превышать 10 символов")]
        public string? Class { get; set; }

        [Required]
        [MaxLength(32)]
        public string PreparednessLevel { get; set; } = "standard";

        [Range(5, 120, ErrorMessage = "Цель должна быть от 5 до 120 минут в день")]
        public int DailyGoalMinutes { get; set; } = 10;

        public List<int> SubjectIds { get; set; } = new();
    }
}
