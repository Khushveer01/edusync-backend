using EduSync.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace EduSync.API.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            try
            {
                // Use a transaction to ensure data consistency
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // Check if we already have data
                    if (context.Users.Any())
                    {
                        await transaction.CommitAsync();
                        return; // Database has been seeded
                    }

                    // Create test users
                    var instructor = new User
                    {
                        UserId = Guid.NewGuid(),
                        Name = "Test Instructor",
                        Email = "instructor@test.com",
                        Role = "Instructor",
                        PasswordHash = HashPassword("Test123!")
                    };

                    var student = new User
                    {
                        UserId = Guid.NewGuid(),
                        Name = "Test Student",
                        Email = "student@test.com",
                        Role = "Student",
                        PasswordHash = HashPassword("Test123!")
                    };

                    context.Users.AddRange(instructor, student);
                    await context.SaveChangesAsync();

                    // Create test courses
                    var courses = new List<Course>
                    {
                        new Course
                        {
                            CourseId = Guid.NewGuid(),
                            Title = "Introduction to Programming",
                            Description = "Learn the basics of programming",
                            MediaUrl = "https://example.com/course1",
                            InstructorId = instructor.UserId
                        },
                        new Course
                        {
                            CourseId = Guid.NewGuid(),
                            Title = "Web Development",
                            Description = "Learn modern web development",
                            MediaUrl = "https://example.com/course2",
                            InstructorId = instructor.UserId
                        }
                    };

                    context.Courses.AddRange(courses);
                    await context.SaveChangesAsync();

                    // Create test assessments
                    var assessments = new List<Assessment>
                    {
                        new Assessment
                        {
                            AssessmentId = Guid.NewGuid(),
                            Title = "Programming Basics Quiz",
                            Description = "Test your knowledge of programming basics",
                            CourseId = courses[0].CourseId,
                            DueDate = DateTime.UtcNow.AddDays(7),
                            MaxScore = 10,
                            Questions = new List<Question>
                            {
                                new Question
                                {
                                    QuestionId = Guid.NewGuid(),
                                    Text = "What is the output of 2 + 2?",
                                    Options = new List<string> { "3", "4", "5", "6" },
                                    CorrectOption = 1,
                                    Marks = 5
                                },
                                new Question
                                {
                                    QuestionId = Guid.NewGuid(),
                                    Text = "Which language is used for web apps?",
                                    Options = new List<string> { "Python", "JavaScript", "C++", "Java" },
                                    CorrectOption = 1,
                                    Marks = 5
                                }
                            }
                        },
                        new Assessment
                        {
                            AssessmentId = Guid.NewGuid(),
                            Title = "Web Development Quiz",
                            Description = "Test your knowledge of web development",
                            CourseId = courses[1].CourseId,
                            DueDate = DateTime.UtcNow.AddDays(7),
                            MaxScore = 10,
                            Questions = new List<Question>
                            {
                                new Question
                                {
                                    QuestionId = Guid.NewGuid(),
                                    Text = "What does HTML stand for?",
                                    Options = new List<string> { "Hyper Trainer Marking Language", "Hyper Text Markup Language", "Hyper Text Marketing Language", "Hyper Text Markup Leveler" },
                                    CorrectOption = 1,
                                    Marks = 5
                                },
                                new Question
                                {
                                    QuestionId = Guid.NewGuid(),
                                    Text = "Which is a JavaScript framework?",
                                    Options = new List<string> { "Django", "Flask", "React", "Laravel" },
                                    CorrectOption = 2,
                                    Marks = 5
                                }
                            }
                        }
                    };

                    context.Assessments.AddRange(assessments);
                    await context.SaveChangesAsync();

                    // Create test results
                    var results = new List<Result>
                    {
                        new Result
                        {
                            ResultId = Guid.NewGuid(),
                            AssessmentId = assessments[0].AssessmentId,
                            UserId = student.UserId,
                            Score = 85,
                            CompletedAt = DateTime.UtcNow
                        },
                        new Result
                        {
                            ResultId = Guid.NewGuid(),
                            AssessmentId = assessments[1].AssessmentId,
                            UserId = student.UserId,
                            Score = 90,
                            CompletedAt = DateTime.UtcNow
                        }
                    };

                    context.Results.AddRange(results);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw it
                // This allows the application to continue even if seeding fails
                Console.WriteLine($"Error during database seeding: {ex.Message}");
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 