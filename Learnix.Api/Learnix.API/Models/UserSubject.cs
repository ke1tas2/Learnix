using System.ComponentModel.DataAnnotations.Schema;

namespace Learnix.API.Models
{
    public class UserSubject
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Column("selected_at")]
        public DateTime SelectedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
    }
}
