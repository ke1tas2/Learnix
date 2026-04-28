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

                entity.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);



            });
        }
    }
}
