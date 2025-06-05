namespace EduSync.API.DTOs
{
    public class CourseDto
    {
        public Guid CourseId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string MediaUrl { get; set; }
        public Guid InstructorId { get; set; }
        public string? InstructorName { get; set; }
    }
} 