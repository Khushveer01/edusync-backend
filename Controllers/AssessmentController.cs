using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.API.Data;
using EduSync.API.Models;
using EduSync.API.DTOs;

namespace EduSync.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssessmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssessmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var assessments = await _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Questions)
                    .Select(a => new AssessmentDto
                    {
                        AssessmentId = a.AssessmentId,
                        Title = a.Title,
                        Description = a.Description,
                        CourseId = a.CourseId,
                        CourseTitle = a.Course.Title,
                        DueDate = a.DueDate,
                        Questions = a.Questions,
                        MaxScore = a.MaxScore
                    })
                    .ToListAsync();

                return Ok(assessments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching assessments", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var assessment = await _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Questions)
                    .FirstOrDefaultAsync(a => a.AssessmentId == id);

                if (assessment == null)
                    return NotFound(new { message = "Assessment not found" });

                var assessmentDto = new AssessmentDto
                {
                    AssessmentId = assessment.AssessmentId,
                    Title = assessment.Title,
                    Description = assessment.Description,
                    CourseId = assessment.CourseId,
                    CourseTitle = assessment.Course.Title,
                    DueDate = assessment.DueDate,
                    Questions = assessment.Questions,
                    MaxScore = assessment.MaxScore
                };

                return Ok(assessmentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the assessment", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(AssessmentDto assessmentDto)
        {
            try
            {
                var assessment = new Assessment
                {
                    AssessmentId = Guid.NewGuid(),
                    Title = assessmentDto.Title,
                    Description = assessmentDto.Description,
                    CourseId = assessmentDto.CourseId,
                    DueDate = assessmentDto.DueDate,
                    Questions = assessmentDto.Questions.Select(q => new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        Text = q.Text,
                        Options = q.Options,
                        CorrectOption = q.CorrectOption,
                        Marks = q.Marks,
                        AssessmentId = assessmentDto.AssessmentId
                    }).ToList(),
                    MaxScore = assessmentDto.MaxScore
                };

                _context.Assessments.Add(assessment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = assessment.AssessmentId }, assessment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the assessment", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update(Guid id, AssessmentDto assessmentDto)
        {
            try
            {
                var assessment = await _context.Assessments
                    .Include(a => a.Questions)
                    .FirstOrDefaultAsync(a => a.AssessmentId == id);

                if (assessment == null)
                    return NotFound(new { message = "Assessment not found" });

                assessment.Title = assessmentDto.Title;
                assessment.Description = assessmentDto.Description;
                assessment.CourseId = assessmentDto.CourseId;
                assessment.DueDate = assessmentDto.DueDate;
                assessment.MaxScore = assessmentDto.MaxScore;

                // Update questions
                _context.Questions.RemoveRange(assessment.Questions);
                assessment.Questions = assessmentDto.Questions.Select(q => new Question
                {
                    QuestionId = Guid.NewGuid(),
                    Text = q.Text,
                    Options = q.Options,
                    CorrectOption = q.CorrectOption,
                    Marks = q.Marks,
                    AssessmentId = id
                }).ToList();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the assessment", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var assessment = await _context.Assessments
                    .Include(a => a.Questions)
                    .FirstOrDefaultAsync(a => a.AssessmentId == id);

                if (assessment == null)
                    return NotFound(new { message = "Assessment not found" });

                _context.Questions.RemoveRange(assessment.Questions);
                _context.Assessments.Remove(assessment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the assessment", error = ex.Message });
            }
        }

        [HttpPost("{id}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(Guid id, [FromBody] AssessmentSubmissionDto submission)
        {
            try
            {
                var assessment = await _context.Assessments
                    .Include(a => a.Questions)
                    .FirstOrDefaultAsync(a => a.AssessmentId == id);

                if (assessment == null)
                    return NotFound(new { message = "Assessment not found" });

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                // Add submission logic here
                // This is a placeholder - you'll need to implement the actual submission logic

                return Ok(new { message = "Assessment submitted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while submitting the assessment", error = ex.Message });
            }
        }

        [HttpGet("instructor")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorAssessments()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                var assessments = await _context.Assessments
                    .Include(a => a.Course)
                    .Include(a => a.Questions)
                    .Where(a => a.Course != null && a.Course.InstructorId == userId)
                    .Select(a => new AssessmentDto
                    {
                        AssessmentId = a.AssessmentId,
                        Title = a.Title,
                        Description = a.Description,
                        CourseId = a.CourseId,
                        CourseTitle = a.Course != null ? a.Course.Title : "Unknown Course",
                        DueDate = a.DueDate,
                        Questions = a.Questions,
                        MaxScore = a.MaxScore
                    })
                    .ToListAsync();

                return Ok(assessments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching instructor assessments", error = ex.Message });
            }
        }
    }
} 