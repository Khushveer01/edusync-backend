using System;
using System.Collections.Generic;
using EduSync.API.Models;

namespace EduSync.API.DTOs
{
    public class AssessmentDto
    {
        public Guid AssessmentId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public int MaxScore { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public Guid CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public DateTime DueDate { get; set; }
    }
} 