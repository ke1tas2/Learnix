using Microsoft.EntityFrameworkCore;
using Learnix.API.Models;
namespace Learnix.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
        
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<LearningLevel> LearningLevels { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<UserSubject> UserSubjects { get; set; } = null!;
        public DbSet<UserLevelProgress> UserLevelProgresses { get; set; } = null!;
        public DbSet<LessonAttempt> LessonAttempts { get; set; } = null!;
        public DbSet<ExerciseAttempt> ExerciseAttempts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("ix_users_email");

                entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

                entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

                entity.Property(u => u.Class)
                .IsRequired(false)
                .HasMaxLength(10);

                entity.Property(u => u.PreparednessLevel)
                .IsRequired(false)
                .HasMaxLength(32);

                entity.Property(u => u.DailyGoalMinutes)
                .IsRequired()
                .HasDefaultValue(10);

                entity.Property(u => u.CurrentStreakDays)
                .IsRequired()
                .HasDefaultValue(0);

                entity.Property(u => u.BestStreakDays)
                .IsRequired()
                .HasDefaultValue(0);

                entity.Property(u => u.TotalXp)
                .IsRequired()
                .HasDefaultValue(0);

                entity.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);



            });

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.ToTable("subjects");

                entity.HasIndex(s => s.Code)
                    .IsUnique()
                    .HasDatabaseName("ix_subjects_code");

                entity.Property(s => s.Code)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(120);

                entity.Property(s => s.Description)
                    .HasMaxLength(500);

                entity.Property(s => s.Grades)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(s => s.ColorHex)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(s => s.IconKey)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(s => s.SourceTitle)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(s => s.SourceUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(s => s.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<LearningLevel>(entity =>
            {
                entity.ToTable("learning_levels");

                entity.HasIndex(l => new { l.SubjectId, l.Grade, l.Order })
                    .IsUnique()
                    .HasDatabaseName("ix_learning_levels_subject_grade_order");

                entity.Property(l => l.Title)
                    .IsRequired()
                    .HasMaxLength(160);

                entity.Property(l => l.Description)
                    .HasMaxLength(700);

                entity.Property(l => l.SourceTitle)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(l => l.SourceUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(l => l.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.HasOne(l => l.Subject)
                    .WithMany(s => s.LearningLevels)
                    .HasForeignKey(l => l.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Exercise>(entity =>
            {
                entity.ToTable("exercises");

                entity.HasIndex(e => new { e.LearningLevelId, e.SortOrder })
                    .HasDatabaseName("ix_exercises_level_order");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.Prompt)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.OptionsJson)
                    .IsRequired()
                    .HasColumnType("jsonb");

                entity.Property(e => e.CorrectAnswer)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Explanation)
                    .HasMaxLength(1000);

                entity.HasOne(e => e.LearningLevel)
                    .WithMany(l => l.Exercises)
                    .HasForeignKey(e => e.LearningLevelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserSubject>(entity =>
            {
                entity.ToTable("user_subjects");

                entity.HasKey(us => new { us.UserId, us.SubjectId });

                entity.HasOne(us => us.User)
                    .WithMany(u => u.SelectedSubjects)
                    .HasForeignKey(us => us.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(us => us.Subject)
                    .WithMany(s => s.Users)
                    .HasForeignKey(us => us.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserLevelProgress>(entity =>
            {
                entity.ToTable("user_level_progresses");

                entity.HasKey(p => new { p.UserId, p.LearningLevelId });

                entity.Property(p => p.Status)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.HasOne(p => p.User)
                    .WithMany(u => u.LevelProgresses)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.LearningLevel)
                    .WithMany(l => l.UserProgresses)
                    .HasForeignKey(p => p.LearningLevelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LessonAttempt>(entity =>
            {
                entity.ToTable("lesson_attempts");

                entity.HasIndex(a => new { a.UserId, a.LearningLevelId, a.CompletedAt })
                    .HasDatabaseName("ix_lesson_attempts_user_level_completed");

                entity.HasOne(a => a.User)
                    .WithMany(u => u.LessonAttempts)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.LearningLevel)
                    .WithMany(l => l.Attempts)
                    .HasForeignKey(a => a.LearningLevelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ExerciseAttempt>(entity =>
            {
                entity.ToTable("exercise_attempts");

                entity.HasIndex(a => new { a.LessonAttemptId, a.ExerciseId })
                    .HasDatabaseName("ix_exercise_attempts_lesson_exercise");

                entity.Property(a => a.UserAnswer)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(a => a.LessonAttempt)
                    .WithMany(l => l.ExerciseAttempts)
                    .HasForeignKey(a => a.LessonAttemptId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Exercise)
                    .WithMany(e => e.Attempts)
                    .HasForeignKey(a => a.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
