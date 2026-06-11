using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Learnix.Services
{
    public class LearnixApiClient
    {
        private const string TokenPreferenceKey = "learnix_auth_token";
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;

        public LearnixApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Preferences.Get(TokenPreferenceKey, null));

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var result = await PostAsync<AuthResponse>("api/auth/register", request);
            SaveToken(result.Token);
            return result;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var result = await PostAsync<AuthResponse>("api/auth/login", request);
            SaveToken(result.Token);
            return result;
        }

        public async Task<List<SubjectDto>> GetSubjectsAsync(int? grade = null)
        {
            ApplyAuthorizationHeader();
            var url = grade.HasValue ? $"api/catalog/subjects?grade={grade.Value}" : "api/catalog/subjects";
            var response = await SendAsync(() => _httpClient.GetAsync(url));
            return await ReadResponseAsync<List<SubjectDto>>(response);
        }

        public async Task<UserDto> UpdateOnboardingAsync(UpdateOnboardingRequest request)
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.PutAsJsonAsync("api/profile/onboarding", request, JsonOptions));
            return await ReadResponseAsync<UserDto>(response);
        }

        public async Task<ProfileStatsDto> GetProfileAsync()
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.GetAsync("api/profile"));
            return await ReadResponseAsync<ProfileStatsDto>(response);
        }

        public async Task<List<LearningLevelDto>> GetLevelsAsync(int subjectId, int? grade = null)
        {
            ApplyAuthorizationHeader();
            var url = grade.HasValue
                ? $"api/catalog/subjects/{subjectId}/levels?grade={grade.Value}"
                : $"api/catalog/subjects/{subjectId}/levels";
            var response = await SendAsync(() => _httpClient.GetAsync(url));
            return await ReadResponseAsync<List<LearningLevelDto>>(response);
        }

        public async Task<LessonDto> GetLessonAsync(int levelId)
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.GetAsync($"api/learning/levels/{levelId}"));
            return await ReadResponseAsync<LessonDto>(response);
        }

        public async Task<LessonResultDto> CompleteLessonAsync(int levelId, SubmitLessonRequest request)
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.PostAsJsonAsync($"api/learning/levels/{levelId}/complete", request, JsonOptions));
            return await ReadResponseAsync<LessonResultDto>(response);
        }

        public async Task<AdminStatsDto> GetAdminStatsAsync()
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.GetAsync("api/admin/stats"));
            return await ReadResponseAsync<AdminStatsDto>(response);
        }

        public async Task<List<AdminUserDto>> GetAdminUsersAsync()
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.GetAsync("api/admin/users"));
            return await ReadResponseAsync<List<AdminUserDto>>(response);
        }

        public async Task<AdminUserDto> UpdateAdminUserRoleAsync(int userId, string role)
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.PutAsJsonAsync(
                $"api/admin/users/{userId}/role",
                new UpdateUserRoleRequest { Role = role },
                JsonOptions));
            return await ReadResponseAsync<AdminUserDto>(response);
        }

        public async Task<AdminUserDto> UpdateAdminUserActiveAsync(int userId, bool isActive)
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.PutAsJsonAsync(
                $"api/admin/users/{userId}/active",
                new UpdateUserActiveRequest { IsActive = isActive },
                JsonOptions));
            return await ReadResponseAsync<AdminUserDto>(response);
        }

        public async Task<List<AdminSubjectDto>> GetAdminSubjectsAsync()
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.GetAsync("api/admin/subjects"));
            return await ReadResponseAsync<List<AdminSubjectDto>>(response);
        }

        public void Logout()
        {
            Preferences.Remove(TokenPreferenceKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private async Task<T> PostAsync<T>(string url, object body)
        {
            ApplyAuthorizationHeader();
            var response = await SendAsync(() => _httpClient.PostAsJsonAsync(url, body, JsonOptions));
            return await ReadResponseAsync<T>(response);
        }

        private static async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
        {
            try
            {
                return await send();
            }
            catch (HttpRequestException)
            {
                throw new LearnixApiException("API не запущен. Сначала запустите Learnix.API на http://localhost:5199, затем повторите действие.");
            }
            catch (TaskCanceledException)
            {
                throw new LearnixApiException("API не ответил вовремя. Проверьте, что Learnix.API запущен на http://localhost:5199.");
            }
        }

        private async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new LearnixApiException(ExtractError(content));
            }

            var result = JsonSerializer.Deserialize<T>(content, JsonOptions);
            return result ?? throw new LearnixApiException("Пустой ответ сервера");
        }

        private void SaveToken(string token)
        {
            Preferences.Set(TokenPreferenceKey, token);
            ApplyAuthorizationHeader();
        }

        private void ApplyAuthorizationHeader()
        {
            var token = Preferences.Get(TokenPreferenceKey, null);
            _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        private static string ExtractError(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "Сервер вернул ошибку";
            }

            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                if (root.TryGetProperty("error", out var error))
                {
                    return error.GetString() ?? "Сервер вернул ошибку";
                }

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (var message in field.Value.EnumerateArray())
                        {
                            var text = message.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                messages.Add(text);
                            }
                        }
                    }

                    if (messages.Count > 0)
                    {
                        return string.Join("\n", messages);
                    }
                }

                if (root.TryGetProperty("title", out var title))
                {
                    var titleText = title.GetString();
                    if (!string.IsNullOrWhiteSpace(titleText) &&
                        !titleText.Contains("validation errors", StringComparison.OrdinalIgnoreCase))
                    {
                        return titleText;
                    }
                }
            }
            catch
            {
                return content;
            }

            return "Проверьте введённые данные и попробуйте снова";
        }
    }

    public class LearnixApiException : Exception
    {
        public LearnixApiException(string message) : base(message)
        {
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Class { get; set; }
        public int? Grade { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new();
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Class { get; set; }
        public int? Grade { get; set; }
        public string? PreparednessLevel { get; set; }
        public int DailyGoalMinutes { get; set; }
        public int CurrentStreakDays { get; set; }
        public int BestStreakDays { get; set; }
        public int TotalXp { get; set; }
        public string Role { get; set; } = "User";
    }

    public class SubjectDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Grades { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#58CC02";
        public string IconKey { get; set; } = "book";
        public int SortOrder { get; set; }
    }

    public class UpdateOnboardingRequest
    {
        public int? Grade { get; set; }
        public string? Class { get; set; }
        public string PreparednessLevel { get; set; } = "standard";
        public int DailyGoalMinutes { get; set; } = 10;
        public List<int> SubjectIds { get; set; } = new();
    }

    public class ProfileStatsDto
    {
        public UserDto User { get; set; } = new();
        public int SelectedSubjectsCount { get; set; }
        public int CompletedLevelsCount { get; set; }
        public int AttemptsCount { get; set; }
        public int TotalMistakes { get; set; }
        public int AverageScorePercent { get; set; }
        public List<SubjectDto> SelectedSubjects { get; set; } = new();
        public List<RecentAttemptDto> RecentAttempts { get; set; } = new();
    }

    public class RecentAttemptDto
    {
        public int AttemptId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string LevelTitle { get; set; } = string.Empty;
        public int ScorePercent { get; set; }
        public int Mistakes { get; set; }
        public int EarnedXp { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class LearningLevelDto
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int Grade { get; set; }
        public int Order { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int XpReward { get; set; }
        public int ExerciseCount { get; set; }
        public string Status { get; set; } = "not_started";
        public int BestScorePercent { get; set; }
    }

    public class LessonDto
    {
        public LearningLevelDto Level { get; set; } = new();
        public List<ExerciseDto> Exercises { get; set; } = new();
    }

    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int SortOrder { get; set; }
        public int XpReward { get; set; }
    }

    public class SubmitLessonRequest
    {
        public List<SubmitExerciseAnswerRequest> Answers { get; set; } = new();
    }

    public class SubmitExerciseAnswerRequest
    {
        public int ExerciseId { get; set; }
        public string Answer { get; set; } = string.Empty;
    }

    public class LessonResultDto
    {
        public int AttemptId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Mistakes { get; set; }
        public int ScorePercent { get; set; }
        public int EarnedXp { get; set; }
        public int TotalXp { get; set; }
        public string LevelStatus { get; set; } = "not_started";
        public List<LessonAnswerResultDto> Answers { get; set; } = new();
    }

    public class LessonAnswerResultDto
    {
        public int ExerciseId { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
    }

    public class AdminStatsDto
    {
        public int UsersCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public int AdminsCount { get; set; }
        public int SubjectsCount { get; set; }
        public int ActiveSubjectsCount { get; set; }
        public int LevelsCount { get; set; }
        public int ActiveLevelsCount { get; set; }
        public int ExercisesCount { get; set; }
        public int AttemptsCount { get; set; }
        public int CompletedAttemptsCount { get; set; }
        public int TotalMistakes { get; set; }
        public int AverageScorePercent { get; set; }
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Class { get; set; }
        public int? Grade { get; set; }
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; }
        public int DailyGoalMinutes { get; set; }
        public int CurrentStreakDays { get; set; }
        public int BestStreakDays { get; set; }
        public int TotalXp { get; set; }
        public int CompletedLevelsCount { get; set; }
        public int AttemptsCount { get; set; }
        public int TotalMistakes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        public string Role { get; set; } = "User";
    }

    public class UpdateUserActiveRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class AdminSubjectDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Grades { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public string IconKey { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int LevelsCount { get; set; }
    }
}
