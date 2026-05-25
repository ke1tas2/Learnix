using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnix.API.Models
{
    public class LearningLevel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Column("grade")]
        public int Grade { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Required]
        [MaxLength(160)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(700)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("xp_reward")]
        public int XpReward { get; set; } = 15;

        [MaxLength(250)]
        [Column("source_title")]
        public string SourceTitle { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("source_url")]
        public string SourceUrl { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public Subject Subject { get; set; } = null!;
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
        public ICollection<UserLevelProgress> UserProgresses { get; set; } = new List<UserLevelProgress>();
        public ICollection<LessonAttempt> Attempts { get; set; } = new List<LessonAttempt>();
    }
}
