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
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var results = await _context.Results
                    .Include(r => r.Assessment)
                    .Include(r => r.User)
                    .Select(r => new ResultDto
                    {
                        ResultId = r.ResultId,
                        AssessmentId = r.AssessmentId,
                        AssessmentTitle = r.Assessment.Title,
                        UserId = r.UserId,
                        UserName = r.User.Name,
                        Score = r.Score,
                        CompletedAt = r.CompletedAt
                    })
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching results", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _context.Results
                    .Include(r => r.Assessment)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ResultId == id);

                if (result == null)
                    return NotFound(new { message = "Result not found" });

                var resultDto = new ResultDto
                {
                    ResultId = result.ResultId,
                    AssessmentId = result.AssessmentId,
                    AssessmentTitle = result.Assessment.Title,
                    UserId = result.UserId,
                    UserName = result.User.Name,
                    Score = result.Score,
                    CompletedAt = result.CompletedAt
                };

                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the result", error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var isInstructor = User.IsInRole("Instructor");

                if (!isInstructor && currentUserId != userId)
                    return Forbid();

                var results = await _context.Results
                    .Include(r => r.Assessment)
                    .Include(r => r.User)
                    .Where(r => r.UserId == userId)
                    .Select(r => new ResultDto
                    {
                        ResultId = r.ResultId,
                        AssessmentId = r.AssessmentId,
                        AssessmentTitle = r.Assessment.Title,
                        UserId = r.UserId,
                        UserName = r.User.Name,
                        Score = r.Score,
                        CompletedAt = r.CompletedAt
                    })
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching user results", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(ResultDto resultDto)
        {
            try
            {
                var result = new Result
                {
                    ResultId = Guid.NewGuid(),
                    AssessmentId = resultDto.AssessmentId,
                    UserId = resultDto.UserId,
                    Score = resultDto.Score,
                    CompletedAt = DateTime.UtcNow
                };

                _context.Results.Add(result);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = result.ResultId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the result", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update(Guid id, ResultDto resultDto)
        {
            try
            {
                var result = await _context.Results.FindAsync(id);
                if (result == null)
                    return NotFound(new { message = "Result not found" });

                result.Score = resultDto.Score;
                result.CompletedAt = resultDto.CompletedAt;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the result", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _context.Results.FindAsync(id);
                if (result == null)
                    return NotFound(new { message = "Result not found" });

                _context.Results.Remove(result);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the result", error = ex.Message });
            }
        }
    }
} 