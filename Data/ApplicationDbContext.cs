using Microsoft.EntityFrameworkCore;
using EduSync.API.Models;

namespace EduSync.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Question> Questions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Role).IsRequired();
            });

            // Configure Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.MediaUrl).IsRequired();
                entity.HasOne(e => e.Instructor)
                    .WithMany(u => u.Courses)
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Assessment
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.HasKey(e => e.AssessmentId);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.MaxScore).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Assessments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Question
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.QuestionId);
                entity.Property(e => e.Text).IsRequired();
                entity.Property(e => e.Options).IsRequired();
                entity.Property(e => e.CorrectOption).IsRequired();
                entity.Property(e => e.Marks).IsRequired();
                entity.HasOne(e => e.Assessment)
                    .WithMany(a => a.Questions)
                    .HasForeignKey(e => e.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Result
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(e => e.ResultId);
                entity.HasOne(e => e.Assessment)
                    .WithMany(a => a.Results)
                    .HasForeignKey(e => e.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Results)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
} 