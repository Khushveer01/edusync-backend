namespace EduSync.API.DTOs
{
    public class ResultDto
    {
        public Guid ResultId { get; set; }
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public DateTime CompletedAt { get; set; }
        public string? UserName { get; set; }
        public string? AssessmentTitle { get; set; }
    }
} 