using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EduSync.API.Models
{
    public class Course
    {
        public Guid CourseId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public Guid InstructorId { get; set; }
        public required string MediaUrl { get; set; }

        // Navigation properties
        [JsonIgnore]
        public User Instructor { get; set; } = null!;
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
} 