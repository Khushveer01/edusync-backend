namespace EduSync.API.Auth
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
} 