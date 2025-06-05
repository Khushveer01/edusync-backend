using System;

namespace EduSync.API.Models
{
    public class Result
    {
        public Guid ResultId { get; set; }
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public DateTime CompletedAt { get; set; }

        // Navigation properties
        public Assessment Assessment { get; set; } = null!;
        public User User { get; set; } = null!;
    }
} 