using Learnix.API.Data;
using Learnix.API.DTOs;
using Learnix.API.Models;
using Learnix.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learnix.API.Controllers
{
    [Route("api/catalog")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;

        public CatalogController(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet("subjects")]
        public async Task<ActionResult<List<SubjectDto>>> GetSubjects([FromQuery] int? grade)
        {
            var subjects = await _context.Subjects
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            if (grade.HasValue)
            {
                subjects = subjects.Where(s => IsAvailableForGrade(s.Grades, grade.Value)).ToList();
            }

            return Ok(subjects.Select(SubjectDto.FromSubject).ToList());
        }

        [HttpGet("subjects/{subjectId:int}/levels")]
        public async Task<ActionResult<List<LearningLevelDto>>> GetLevels(int subjectId, [FromQuery] int? grade)
        {
            var userId = _currentUserService.GetUserId(Request);
            var progress = userId.HasValue
                ? await _context.UserLevelProgresses
                    .AsNoTracking()
                    .Where(p => p.UserId == userId.Value)
                    .ToDictionaryAsync(p => p.LearningLevelId)
                : new Dictionary<int, UserLevelProgress>();

            var levels = await _context.LearningLevels
                .AsNoTracking()
                .Include(l => l.Exercises)
                .Where(l => l.IsActive && l.SubjectId == subjectId)
                .Where(l => !grade.HasValue || l.Grade == grade.Value)
                .OrderBy(l => l.Grade)
                .ThenBy(l => l.Order)
                .ToListAsync();

            return Ok(levels.Select(l => LearningLevelDto.FromLevel(
                l,
                progress.TryGetValue(l.Id, out var levelProgress) ? levelProgress : null)).ToList());
        }

        private static bool IsAvailableForGrade(string grades, int grade)
        {
            foreach (var part in grades.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (part.Contains('-'))
                {
                    var range = part.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (range.Length == 2 &&
                        int.TryParse(range[0], out var start) &&
                        int.TryParse(range[1], out var end) &&
                        grade >= start &&
                        grade <= end)
                    {
                        return true;
                    }
                }
                else if (int.TryParse(part, out var singleGrade) && grade == singleGrade)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
