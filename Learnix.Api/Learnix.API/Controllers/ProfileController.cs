using Learnix.API.Data;
using Learnix.API.DTOs;
using Learnix.API.Models;
using Learnix.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learnix.API.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;

        public ProfileController(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<ProfileStatsDto>> GetProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { error = "Требуется авторизация" });
            }

            var attempts = await _context.LessonAttempts
                .AsNoTracking()
                .Include(a => a.LearningLevel)
                .ThenInclude(l => l.Subject)
                .Where(a => a.UserId == user.Id && a.CompletedAt != null)
                .OrderByDescending(a => a.CompletedAt)
                .ToListAsync();

            var selectedSubjects = await _context.UserSubjects
                .AsNoTracking()
                .Include(us => us.Subject)
                .Where(us => us.UserId == user.Id)
                .Select(us => us.Subject)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            var completedLevelsCount = await _context.UserLevelProgresses
                .AsNoTracking()
                .CountAsync(p => p.UserId == user.Id && p.Status == "completed");

            return Ok(new ProfileStatsDto
            {
                User = UserResponseDto.FromUser(user),
                SelectedSubjectsCount = selectedSubjects.Count,
                CompletedLevelsCount = completedLevelsCount,
                AttemptsCount = attempts.Count,
                TotalMistakes = attempts.Sum(a => a.Mistakes),
                AverageScorePercent = attempts.Count == 0 ? 0 : (int)Math.Round(attempts.Average(a => a.ScorePercent)),
                SelectedSubjects = selectedSubjects.Select(SubjectDto.FromSubject).ToList(),
                RecentAttempts = attempts.Take(10).Select(a => new RecentAttemptDto
                {
                    AttemptId = a.Id,
                    SubjectName = a.LearningLevel.Subject.Name,
                    LevelTitle = a.LearningLevel.Title,
                    ScorePercent = a.ScorePercent,
                    Mistakes = a.Mistakes,
                    EarnedXp = a.EarnedXp,
                    CompletedAt = a.CompletedAt
                }).ToList()
            });
        }

        [HttpPut("onboarding")]
        public async Task<ActionResult<UserResponseDto>> UpdateOnboarding([FromBody] UpdateOnboardingDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { error = "Требуется авторизация" });
            }

            var subjectIds = dto.SubjectIds.Distinct().ToList();
            var existingSubjectIds = await _context.Subjects
                .Where(s => subjectIds.Contains(s.Id) && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync();

            if (existingSubjectIds.Count != subjectIds.Count)
            {
                return BadRequest(new { error = "Один или несколько выбранных предметов не найдены" });
            }

            user.Grade = dto.Grade ?? user.Grade;
            user.Class = string.IsNullOrWhiteSpace(dto.Class) ? user.Class : dto.Class.Trim();
            user.PreparednessLevel = dto.PreparednessLevel.Trim().ToLowerInvariant();
            user.DailyGoalMinutes = dto.DailyGoalMinutes;

            var currentSelections = await _context.UserSubjects
                .Where(us => us.UserId == user.Id)
                .ToListAsync();

            _context.UserSubjects.RemoveRange(currentSelections);
            _context.UserSubjects.AddRange(existingSubjectIds.Select(id => new UserSubject
            {
                UserId = user.Id,
                SubjectId = id
            }));

            await _context.SaveChangesAsync();
            return Ok(UserResponseDto.FromUser(user));
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = _currentUserService.GetUserId(Request);
            return userId.HasValue
                ? await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value && u.IsActive)
                : null;
        }
    }
}
