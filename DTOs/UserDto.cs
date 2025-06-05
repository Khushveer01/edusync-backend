namespace EduSync.API.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
} 