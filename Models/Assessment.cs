using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EduSync.API.Models
{
    public class Assessment
    {
        public Guid AssessmentId { get; set; }
        public Guid CourseId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public int MaxScore { get; set; }
        public DateTime DueDate { get; set; }

        // Navigation properties
        [JsonIgnore]
        public Course Course { get; set; } = null!;
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
} 