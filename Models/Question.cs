using System;
using System.Collections.Generic;

namespace EduSync.API.Models
{
    public class Question
    {
        public Guid QuestionId { get; set; }
        public required string Text { get; set; }
        public required List<string> Options { get; set; }
        public int CorrectOption { get; set; }
        public int Marks { get; set; }
        public Guid AssessmentId { get; set; }
        
        // Navigation property
        public Assessment Assessment { get; set; } = null!;
    }
} 