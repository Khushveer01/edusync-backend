using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EduSync.API.Models;
using EduSync.API.Data;
using EduSync.API.Auth;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace EduSync.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Invalid input data" 
                    });
                }

                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    return BadRequest(new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Email already registered" 
                    });
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Name = dto.Name,
                    Email = dto.Email,
                    Role = dto.Role,
                    PasswordHash = HashPassword(dto.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                // Set token in cookie
                Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.Now.AddHours(3)
                });

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = token,
                    User = new UserDto
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new AuthResponseDto 
                { 
                    Success = false, 
                    Message = "An error occurred during registration. Please try again." 
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Invalid input data" 
                    });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    return Unauthorized(new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Invalid email or password" 
                    });
                }

                if (!VerifyPassword(dto.Password, user.PasswordHash))
                {
                    return Unauthorized(new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Invalid email or password" 
                    });
                }

                var token = GenerateJwtToken(user);

                // Set token in cookie
                Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.Now.AddHours(3)
                });

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = new UserDto
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new AuthResponseDto 
                { 
                    Success = false, 
                    Message = "An error occurred during login. Please try again." 
                });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "User not authenticated" 
                    });
                }

                var user = await _context.Users.FindAsync(Guid.Parse(userId));
                if (user == null)
                {
                    return NotFound(new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "User not found" 
                    });
                }

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    User = new UserDto
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, new AuthResponseDto 
                { 
                    Success = false, 
                    Message = "An error occurred while retrieving user data" 
                });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("JWT configuration is missing");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
} 