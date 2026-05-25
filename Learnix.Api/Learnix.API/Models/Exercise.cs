using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnix.API.Models
{
    public class Exercise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("learning_level_id")]
        public int LearningLevelId { get; set; }

        [Required]
        [MaxLength(40)]
        [Column("type")]
        public string Type { get; set; } = "single_choice";

        [Required]
        [MaxLength(1000)]
        [Column("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [Required]
        [Column("options_json")]
        public string OptionsJson { get; set; } = "[]";

        [Required]
        [MaxLength(500)]
        [Column("correct_answer")]
        public string CorrectAnswer { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("explanation")]
        public string? Explanation { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("xp_reward")]
        public int XpReward { get; set; } = 5;

        public LearningLevel LearningLevel { get; set; } = null!;
        public ICollection<ExerciseAttempt> Attempts { get; set; } = new List<ExerciseAttempt>();
    }
}
