using Learnix.API.Data;
using Learnix.API.DTOs;
using Learnix.API.Models;
using Learnix.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learnix.API.Controllers
{
    [Route("api/learning")]
    [ApiController]
    public class LearningController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;

        public LearningController(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet("levels/{levelId:int}")]
        public async Task<ActionResult<LessonDto>> GetLesson(int levelId)
        {
            var level = await _context.LearningLevels
                .AsNoTracking()
                .Include(l => l.Exercises.OrderBy(e => e.SortOrder))
                .FirstOrDefaultAsync(l => l.Id == levelId && l.IsActive);

            if (level == null)
            {
                return NotFound(new { error = "Уровень не найден" });
            }

            UserLevelProgress? progress = null;
            var userId = _currentUserService.GetUserId(Request);
            if (userId.HasValue)
            {
                progress = await _context.UserLevelProgresses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == userId.Value && p.LearningLevelId == levelId);
            }

            return Ok(new LessonDto
            {
                Level = LearningLevelDto.FromLevel(level, progress),
                Exercises = level.Exercises.OrderBy(e => e.SortOrder).Select(ExerciseDto.FromExercise).ToList()
            });
        }

        [HttpPost("levels/{levelId:int}/complete")]
        public async Task<ActionResult<LessonResultDto>> CompleteLesson(int levelId, [FromBody] SubmitLessonDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { error = "Требуется авторизация" });
            }

            var level = await _context.LearningLevels
                .Include(l => l.Exercises)
                .FirstOrDefaultAsync(l => l.Id == levelId && l.IsActive);

            if (level == null)
            {
                return NotFound(new { error = "Уровень не найден" });
            }

            var answersByExercise = dto.Answers
                .GroupBy(a => a.ExerciseId)
                .ToDictionary(g => g.Key, g => g.Last().Answer);

            var now = DateTime.UtcNow;
            var attempt = new LessonAttempt
            {
                UserId = user.Id,
                LearningLevelId = level.Id,
                StartedAt = now,
                CompletedAt = now,
                TotalQuestions = level.Exercises.Count
            };

            var answerResults = new List<LessonAnswerResultDto>();
            foreach (var exercise in level.Exercises.OrderBy(e => e.SortOrder))
            {
                answersByExercise.TryGetValue(exercise.Id, out var userAnswer);
                userAnswer ??= string.Empty;
                var isCorrect = NormalizeAnswer(userAnswer) == NormalizeAnswer(exercise.CorrectAnswer);

                if (isCorrect)
                {
                    attempt.CorrectAnswers++;
                }

                attempt.ExerciseAttempts.Add(new ExerciseAttempt
                {
                    ExerciseId = exercise.Id,
                    UserAnswer = userAnswer.Trim(),
                    IsCorrect = isCorrect,
                    CreatedAt = now
                });

                answerResults.Add(new LessonAnswerResultDto
                {
                    ExerciseId = exercise.Id,
                    Prompt = exercise.Prompt,
                    UserAnswer = userAnswer,
                    CorrectAnswer = exercise.CorrectAnswer,
                    IsCorrect = isCorrect,
                    Explanation = exercise.Explanation
                });
            }

            attempt.Mistakes = attempt.TotalQuestions - attempt.CorrectAnswers;
            attempt.ScorePercent = attempt.TotalQuestions == 0
                ? 0
                : (int)Math.Round((double)attempt.CorrectAnswers / attempt.TotalQuestions * 100);
            attempt.EarnedXp = (int)Math.Round(level.XpReward * (attempt.ScorePercent / 100.0));

            _context.LessonAttempts.Add(attempt);
            user.TotalXp += attempt.EarnedXp;
            UpdateStreak(user, now);

            var progress = await _context.UserLevelProgresses
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.LearningLevelId == level.Id);

            if (progress == null)
            {
                progress = new UserLevelProgress
                {
                    UserId = user.Id,
                    LearningLevelId = level.Id
                };
                _context.UserLevelProgresses.Add(progress);
            }

            progress.AttemptsCount++;
            progress.LastAttemptAt = now;
            progress.Mistakes += attempt.Mistakes;
            progress.BestScorePercent = Math.Max(progress.BestScorePercent, attempt.ScorePercent);
            progress.EarnedXp += attempt.EarnedXp;
            if (attempt.ScorePercent >= 70)
            {
                progress.Status = "completed";
                progress.CompletedAt ??= now;
            }
            else if (progress.Status == "not_started")
            {
                progress.Status = "in_progress";
            }

            await _context.SaveChangesAsync();

            return Ok(new LessonResultDto
            {
                AttemptId = attempt.Id,
                TotalQuestions = attempt.TotalQuestions,
                CorrectAnswers = attempt.CorrectAnswers,
                Mistakes = attempt.Mistakes,
                ScorePercent = attempt.ScorePercent,
                EarnedXp = attempt.EarnedXp,
                TotalXp = user.TotalXp,
                LevelStatus = progress.Status,
                Answers = answerResults
            });
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = _currentUserService.GetUserId(Request);
            return userId.HasValue
                ? await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value && u.IsActive)
                : null;
        }

        private static string NormalizeAnswer(string value)
        {
            return value.Trim().ToLowerInvariant().Replace('ё', 'е');
        }

        private static void UpdateStreak(User user, DateTime now)
        {
            var today = now.Date;
            var lastActivityDate = user.LastActivityDate?.Date;

            if (lastActivityDate == today)
            {
                return;
            }

            user.CurrentStreakDays = lastActivityDate == today.AddDays(-1)
                ? user.CurrentStreakDays + 1
                : 1;
            user.BestStreakDays = Math.Max(user.BestStreakDays, user.CurrentStreakDays);
            user.LastActivityDate = today;
        }
    }
}
