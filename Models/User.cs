using System;
using System.Collections.Generic;

namespace EduSync.API.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; } // Student or Instructor
        public required string PasswordHash { get; set; }

        // Navigation properties
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
} 