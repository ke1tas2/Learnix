using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnix.API.Models
{
    public class Subject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [MaxLength(40)]
        [Column("grades")]
        public string Grades { get; set; } = "7-11";

        [MaxLength(16)]
        [Column("color_hex")]
        public string ColorHex { get; set; } = "#58CC02";

        [MaxLength(40)]
        [Column("icon_key")]
        public string IconKey { get; set; } = "book";

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [MaxLength(250)]
        [Column("source_title")]
        public string SourceTitle { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("source_url")]
        public string SourceUrl { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public ICollection<LearningLevel> LearningLevels { get; set; } = new List<LearningLevel>();
        public ICollection<UserSubject> Users { get; set; } = new List<UserSubject>();
    }
}
