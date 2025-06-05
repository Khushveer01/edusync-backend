using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EduSync.API.Models;
using EduSync.API.Data;
using EduSync.API.DTOs;

namespace EduSync.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Instructor)
                    .Select(c => new CourseDto
                    {
                        CourseId = c.CourseId,
                        Title = c.Title,
                        Description = c.Description,
                        MediaUrl = c.MediaUrl,
                        InstructorId = c.InstructorId,
                        InstructorName = c.Instructor.Name
                    })
                    .ToListAsync();

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching courses", error = ex.Message });
            }
        }

        [HttpGet("instructor")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorCourses()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var courses = await _context.Courses
                    .Include(c => c.Instructor)
                    .Where(c => c.InstructorId == userId)
                    .Select(c => new CourseDto
                    {
                        CourseId = c.CourseId,
                        Title = c.Title,
                        Description = c.Description,
                        MediaUrl = c.MediaUrl,
                        InstructorId = c.InstructorId,
                        InstructorName = c.Instructor.Name
                    })
                    .ToListAsync();

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching instructor courses", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                    return NotFound(new { message = "Course not found" });

                var courseDto = new CourseDto
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    MediaUrl = course.MediaUrl,
                    InstructorId = course.InstructorId,
                    InstructorName = course.Instructor.Name
                };

                return Ok(courseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the course", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(CourseDto courseDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var course = new Course
                {
                    CourseId = Guid.NewGuid(),
                    Title = courseDto.Title,
                    Description = courseDto.Description,
                    MediaUrl = courseDto.MediaUrl,
                    InstructorId = userId
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                // Return the created course with instructor details
                var createdCourse = await _context.Courses
                    .Include(c => c.Instructor)
                    .FirstOrDefaultAsync(c => c.CourseId == course.CourseId);

                var responseDto = new CourseDto
                {
                    CourseId = createdCourse.CourseId,
                    Title = createdCourse.Title,
                    Description = createdCourse.Description,
                    MediaUrl = createdCourse.MediaUrl,
                    InstructorId = createdCourse.InstructorId,
                    InstructorName = createdCourse.Instructor.Name
                };

                return CreatedAtAction(nameof(GetById), new { id = course.CourseId }, responseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the course", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update(Guid id, CourseDto courseDto)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound(new { message = "Course not found" });

                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                if (course.InstructorId != userId)
                    return Forbid();

                course.Title = courseDto.Title;
                course.Description = courseDto.Description;
                course.MediaUrl = courseDto.MediaUrl;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the course", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound(new { message = "Course not found" });

                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                if (course.InstructorId != userId)
                    return Forbid();

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the course", error = ex.Message });
            }
        }

        [HttpPost("{id}/enroll")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Enroll(Guid id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound(new { message = "Course not found" });

                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                // Add enrollment logic here
                // This is a placeholder - you'll need to implement the actual enrollment logic

                return Ok(new { message = "Successfully enrolled in the course" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while enrolling in the course", error = ex.Message });
            }
        }
    }
} 