using Microsoft.AspNetCore.Mvc;
using EduSync.API.Models;
using EduSync.API.DTOs;
using EduSync.API.Auth;
using System;
using System.Collections.Generic;

namespace EduSync.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        // Dummy in-memory store for demonstration
        private static List<User> users = new List<User>();

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = dto.Password // Hash in real app
            };
            users.Add(user);
            return Ok(new { user.UserId, user.Name, user.Email, user.Role });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = users.Find(u => u.Email == dto.Email && u.PasswordHash == dto.Password);
            if (user == null) return Unauthorized();
            // Return dummy token for now
            return Ok(new { token = "dummy-jwt-token", user.UserId, user.Name, user.Email, user.Role });
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(Guid id)
        {
            var user = users.Find(u => u.UserId == id);
            if (user == null) return NotFound();
            return Ok(new EduSync.API.Auth.UserDto 
            { 
                UserId = user.UserId, 
                Name = user.Name, 
                Email = user.Email, 
                Role = user.Role 
            });
        }
    }
} 