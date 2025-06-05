namespace EduSync.API.DTOs
{
    public class AssessmentSubmissionDto
    {
        public required string Answers { get; set; } // JSON string containing student's answers
        public DateTime SubmissionTime { get; set; } = DateTime.UtcNow;
    }
} 