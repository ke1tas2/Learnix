using System.Text.Json;
using Learnix.API.Data;
using Learnix.API.DTOs;
using Learnix.API.Models;
using Learnix.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learnix.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private const string DefaultSourceTitle = "National Education Portal: electronic textbooks";
        private const string DefaultSourceUrl = "https://e-padruchnik.adu.by/";

        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;

        public AdminController(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var completedAttempts = _context.LessonAttempts
                .AsNoTracking()
                .Where(a => a.CompletedAt != null);

            var completedAttemptsCount = await completedAttempts.CountAsync();
            var totalMistakes = await completedAttempts.SumAsync(a => (int?)a.Mistakes) ?? 0;
            var averageScore = completedAttemptsCount == 0
                ? 0
                : (int)Math.Round(await completedAttempts.AverageAsync(a => a.ScorePercent));

            return Ok(new AdminStatsDto
            {
                UsersCount = await _context.Users.CountAsync(),
                ActiveUsersCount = await _context.Users.CountAsync(u => u.IsActive),
                AdminsCount = await _context.Users.CountAsync(u => u.Role == UserRoles.Admin),
                SubjectsCount = await _context.Subjects.CountAsync(),
                ActiveSubjectsCount = await _context.Subjects.CountAsync(s => s.IsActive),
                LevelsCount = await _context.LearningLevels.CountAsync(),
                ActiveLevelsCount = await _context.LearningLevels.CountAsync(l => l.IsActive),
                ExercisesCount = await _context.Exercises.CountAsync(),
                AttemptsCount = await _context.LessonAttempts.CountAsync(),
                CompletedAttemptsCount = completedAttemptsCount,
                TotalMistakes = totalMistakes,
                AverageScorePercent = averageScore
            });
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var users = await _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Name = u.Name,
                    Class = u.Class,
                    Grade = u.Grade,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    PreparednessLevel = u.PreparednessLevel,
                    DailyGoalMinutes = u.DailyGoalMinutes,
                    CurrentStreakDays = u.CurrentStreakDays,
                    BestStreakDays = u.BestStreakDays,
                    TotalXp = u.TotalXp,
                    CompletedLevelsCount = _context.UserLevelProgresses.Count(p =>
                        p.UserId == u.Id && p.Status == "completed"),
                    AttemptsCount = _context.LessonAttempts.Count(a => a.UserId == u.Id),
                    TotalMistakes = _context.LessonAttempts
                        .Where(a => a.UserId == u.Id)
                        .Sum(a => (int?)a.Mistakes) ?? 0,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("users/{id:int}/role")]
        public async Task<ActionResult<AdminUserDto>> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var normalizedRole = UserRoles.Normalize(dto.Role.Trim());
            if (normalizedRole == null)
            {
                return BadRequest(new { error = "Role must be User or Admin" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var currentUserId = _currentUserService.GetUserId(Request);
            if (user.Id == currentUserId &&
                user.Role == UserRoles.Admin &&
                normalizedRole != UserRoles.Admin &&
                await _context.Users.CountAsync(u => u.Role == UserRoles.Admin && u.IsActive) <= 1)
            {
                return BadRequest(new { error = "The last active administrator cannot be demoted" });
            }

            user.Role = normalizedRole;
            await _context.SaveChangesAsync();
            return Ok(await ToAdminUserDtoAsync(user.Id));
        }

        [HttpPut("users/{id:int}/active")]
        public async Task<ActionResult<AdminUserDto>> UpdateUserActive(int id, [FromBody] UpdateUserActiveDto dto)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var currentUserId = _currentUserService.GetUserId(Request);
            if (user.Id == currentUserId && !dto.IsActive)
            {
                return BadRequest(new { error = "Administrator cannot deactivate the current account" });
            }

            if (user.Role == UserRoles.Admin &&
                !dto.IsActive &&
                await _context.Users.CountAsync(u => u.Role == UserRoles.Admin && u.IsActive) <= 1)
            {
                return BadRequest(new { error = "The last active administrator cannot be deactivated" });
            }

            user.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return Ok(await ToAdminUserDtoAsync(user.Id));
        }

        [HttpPut("users/{id:int}/profile")]
        public async Task<ActionResult<AdminUserDto>> UpdateUserProfile(int id, [FromBody] UpdateAdminUserProfileDto dto)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (dto.Grade.HasValue && dto.Grade is < 7 or > 11)
            {
                return BadRequest(new { error = "Grade must be from 7 to 11" });
            }

            if (dto.DailyGoalMinutes is < 5 or > 120)
            {
                return BadRequest(new { error = "Daily goal must be from 5 to 120 minutes" });
            }

            user.Name = dto.Name.Trim();
            user.Class = TrimToNull(dto.Class);
            user.Grade = dto.Grade;
            user.PreparednessLevel = TrimToNull(dto.PreparednessLevel)?.ToLowerInvariant();
            user.DailyGoalMinutes = dto.DailyGoalMinutes;

            await _context.SaveChangesAsync();
            return Ok(await ToAdminUserDtoAsync(user.Id));
        }

        [HttpPost("users/{id:int}/reset-progress")]
        public async Task<ActionResult<AdminUserDto>> ResetUserProgress(int id)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var attempts = await _context.LessonAttempts
                .Where(a => a.UserId == id)
                .ToListAsync();
            var progress = await _context.UserLevelProgresses
                .Where(p => p.UserId == id)
                .ToListAsync();

            _context.LessonAttempts.RemoveRange(attempts);
            _context.UserLevelProgresses.RemoveRange(progress);

            user.TotalXp = 0;
            user.CurrentStreakDays = 0;
            user.BestStreakDays = 0;
            user.LastActivityDate = null;

            await _context.SaveChangesAsync();
            return Ok(await ToAdminUserDtoAsync(user.Id));
        }

        [HttpGet("subjects")]
        public async Task<ActionResult<List<AdminSubjectDto>>> GetSubjects()
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var subjects = await _context.Subjects
                .AsNoTracking()
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name)
                .Select(s => new AdminSubjectDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    Description = s.Description,
                    Grades = s.Grades,
                    ColorHex = s.ColorHex,
                    IconKey = s.IconKey,
                    SortOrder = s.SortOrder,
                    SourceTitle = s.SourceTitle,
                    SourceUrl = s.SourceUrl,
                    IsActive = s.IsActive,
                    LevelsCount = s.LearningLevels.Count
                })
                .ToListAsync();

            return Ok(subjects);
        }

        [HttpPost("subjects")]
        public async Task<ActionResult<AdminSubjectDto>> CreateSubject([FromBody] UpsertSubjectDto dto)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var code = NormalizeCode(dto.Code);
            if (await _context.Subjects.AnyAsync(s => s.Code == code))
            {
                return BadRequest(new { error = "Subject code already exists" });
            }

            var subject = new Subject();
            await ApplySubjectDtoAsync(subject, dto, code);

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubjects), AdminSubjectDto.FromSubject(subject, 0));
        }

        [HttpPut("subjects/{id:int}")]
        public async Task<ActionResult<AdminSubjectDto>> UpdateSubject(int id, [FromBody] UpsertSubjectDto dto)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null)
            {
                return NotFound(new { error = "Subject not found" });
            }

            var code = NormalizeCode(dto.Code);
            if (await _context.Subjects.AnyAsync(s => s.Code == code && s.Id != id))
            {
                return BadRequest(new { error = "Subject code already exists" });
            }

            await ApplySubjectDtoAsync(subject, dto, code);
            await _context.SaveChangesAsync();

            var levelsCount = await _context.LearningLevels.CountAsync(l => l.SubjectId == subject.Id);
            return Ok(AdminSubjectDto.FromSubject(subject, levelsCount));
        }

        [HttpGet("subjects/{subjectId:int}/levels")]
        public async Task<ActionResult<List<AdminLearningLevelDto>>> GetSubjectLevels(int subjectId)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var levels = await _context.LearningLevels
                .AsNoTracking()
                .Include(l => l.Exercises)
                .Where(l => l.SubjectId == subjectId)
                .OrderBy(l => l.Grade)
                .ThenBy(l => l.Order)
                .ToListAsync();

            return Ok(levels.Select(AdminLearningLevelDto.FromLevel).ToList());
        }

        [HttpPost("levels")]
        public async Task<ActionResult<AdminLearningLevelDto>> CreateLevel([FromBody] UpsertLearningLevelDto dto)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var validationError = await ValidateLevelDtoAsync(dto, null);
            if (validationError != null)
            {
                return BadRequest(new { error = validationError });
            }

            var level = new LearningLevel();
            ApplyLevelDto(level, dto);
            foreach (var exercise in CreateExercises(dto.Exercises))
            {
                level.Exercises.Add(exercise);
            }

            _context.LearningLevels.Add(level);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubjectLevels), new { subjectId = level.SubjectId }, AdminLearningLevelDto.FromLevel(level));
        }

        [HttpPut("levels/{id:int}")]
        public async Task<ActionResult<AdminLearningLevelDto>> UpdateLevel(int id, [FromBody] UpsertLearningLevelDto dto)
        {
            var adminCheck = await EnsureAdminAsync();
            if (adminCheck != null)
            {
                return adminCheck;
            }

            var level = await _context.LearningLevels
                .Include(l => l.Exercises)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (level == null)
            {
                return NotFound(new { error = "Level not found" });
            }

            var validationError = await ValidateLevelDtoAsync(dto, id);
            if (validationError != null)
            {
                return BadRequest(new { error = validationError });
            }

            ApplyLevelDto(level, dto);
            _context.Exercises.RemoveRange(level.Exercises);
            level.Exercises.Clear();
            foreach (var exercise in CreateExercises(dto.Exercises))
            {
                level.Exercises.Add(exercise);
            }

            await _context.SaveChangesAsync();
            return Ok(AdminLearningLevelDto.FromLevel(level));
        }

        private async Task<ActionResult?> EnsureAdminAsync()
        {
            var userId = _currentUserService.GetUserId(Request);
            if (!userId.HasValue)
            {
                return Unauthorized(new { error = "Authorization is required" });
            }

            var isAdmin = await _context.Users.AnyAsync(u =>
                u.Id == userId.Value &&
                u.IsActive &&
                u.Role == UserRoles.Admin);

            return isAdmin
                ? null
                : StatusCode(StatusCodes.Status403Forbidden, new { error = "Administrator role is required" });
        }

        private async Task<AdminUserDto> ToAdminUserDtoAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Name = u.Name,
                    Class = u.Class,
                    Grade = u.Grade,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    PreparednessLevel = u.PreparednessLevel,
                    DailyGoalMinutes = u.DailyGoalMinutes,
                    CurrentStreakDays = u.CurrentStreakDays,
                    BestStreakDays = u.BestStreakDays,
                    TotalXp = u.TotalXp,
                    CompletedLevelsCount = _context.UserLevelProgresses.Count(p =>
                        p.UserId == u.Id && p.Status == "completed"),
                    AttemptsCount = _context.LessonAttempts.Count(a => a.UserId == u.Id),
                    TotalMistakes = _context.LessonAttempts
                        .Where(a => a.UserId == u.Id)
                        .Sum(a => (int?)a.Mistakes) ?? 0,
                    CreatedAt = u.CreatedAt
                })
                .FirstAsync();
        }

        private async Task ApplySubjectDtoAsync(Subject subject, UpsertSubjectDto dto, string code)
        {
            subject.Code = code;
            subject.Name = dto.Name.Trim();
            subject.Description = TrimToNull(dto.Description);
            subject.Grades = dto.Grades.Trim();
            subject.ColorHex = string.IsNullOrWhiteSpace(dto.ColorHex) ? "#3AAAE0" : dto.ColorHex.Trim();
            subject.IconKey = string.IsNullOrWhiteSpace(dto.IconKey) ? "book" : dto.IconKey.Trim();
            subject.SortOrder = dto.SortOrder > 0
                ? dto.SortOrder
                : await GetNextSubjectSortOrderAsync();
            subject.SourceTitle = string.IsNullOrWhiteSpace(dto.SourceTitle)
                ? DefaultSourceTitle
                : dto.SourceTitle.Trim();
            subject.SourceUrl = string.IsNullOrWhiteSpace(dto.SourceUrl)
                ? DefaultSourceUrl
                : dto.SourceUrl.Trim();
            subject.IsActive = dto.IsActive;
        }

        private async Task<int> GetNextSubjectSortOrderAsync()
        {
            return await _context.Subjects.AnyAsync()
                ? await _context.Subjects.MaxAsync(s => s.SortOrder) + 1
                : 1;
        }

        private async Task<string?> ValidateLevelDtoAsync(UpsertLearningLevelDto dto, int? currentLevelId)
        {
            if (!await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
            {
                return "Subject not found";
            }

            if (dto.Grade is < 7 or > 11)
            {
                return "Grade must be from 7 to 11";
            }

            if (dto.Order <= 0)
            {
                return "Level order must be greater than zero";
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return "Level title is required";
            }

            var duplicateExists = await _context.LearningLevels.AnyAsync(l =>
                l.SubjectId == dto.SubjectId &&
                l.Grade == dto.Grade &&
                l.Order == dto.Order &&
                (!currentLevelId.HasValue || l.Id != currentLevelId.Value));

            if (duplicateExists)
            {
                return "Level with the same subject, grade and order already exists";
            }

            if (dto.Exercises.Count == 0)
            {
                return "At least one exercise is required";
            }

            foreach (var exercise in dto.Exercises)
            {
                var options = CleanOptions(exercise.Options);
                if (string.IsNullOrWhiteSpace(exercise.Prompt))
                {
                    return "Exercise prompt is required";
                }

                if (options.Count < 2)
                {
                    return "Exercise must have at least two options";
                }

                if (string.IsNullOrWhiteSpace(exercise.CorrectAnswer))
                {
                    return "Exercise correct answer is required";
                }

                if (!options.Contains(exercise.CorrectAnswer.Trim()))
                {
                    return "Exercise correct answer must be one of the options";
                }
            }

            return null;
        }

        private static void ApplyLevelDto(LearningLevel level, UpsertLearningLevelDto dto)
        {
            level.SubjectId = dto.SubjectId;
            level.Grade = dto.Grade;
            level.Order = dto.Order;
            level.Title = dto.Title.Trim();
            level.Description = TrimToNull(dto.Description);
            level.XpReward = dto.XpReward > 0 ? dto.XpReward : 15;
            level.SourceTitle = string.IsNullOrWhiteSpace(dto.SourceTitle)
                ? DefaultSourceTitle
                : dto.SourceTitle.Trim();
            level.SourceUrl = string.IsNullOrWhiteSpace(dto.SourceUrl)
                ? DefaultSourceUrl
                : dto.SourceUrl.Trim();
            level.IsActive = dto.IsActive;
        }

        private static List<Exercise> CreateExercises(IEnumerable<UpsertExerciseDto> dtos)
        {
            return dtos.Select((dto, index) => new Exercise
            {
                Type = string.IsNullOrWhiteSpace(dto.Type) ? "single_choice" : dto.Type.Trim(),
                Prompt = dto.Prompt.Trim(),
                OptionsJson = JsonSerializer.Serialize(CleanOptions(dto.Options)),
                CorrectAnswer = dto.CorrectAnswer.Trim(),
                Explanation = TrimToNull(dto.Explanation),
                SortOrder = dto.SortOrder > 0 ? dto.SortOrder : index + 1,
                XpReward = dto.XpReward > 0 ? dto.XpReward : 5
            }).ToList();
        }

        private static List<string> CleanOptions(IEnumerable<string> options)
        {
            return options
                .Select(option => option.Trim())
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToLowerInvariant();
        }

        private static string? TrimToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
