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
            var response = await _httpClient.GetAsync(url);
            return await ReadResponseAsync<List<SubjectDto>>(response);
        }

        public async Task<UserDto> UpdateOnboardingAsync(UpdateOnboardingRequest request)
        {
            ApplyAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync("api/profile/onboarding", request, JsonOptions);
            return await ReadResponseAsync<UserDto>(response);
        }

        public void Logout()
        {
            Preferences.Remove(TokenPreferenceKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private async Task<T> PostAsync<T>(string url, object body)
        {
            ApplyAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync(url, body, JsonOptions);
            return await ReadResponseAsync<T>(response);
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
                if (document.RootElement.TryGetProperty("error", out var error))
                {
                    return error.GetString() ?? "Сервер вернул ошибку";
                }

                if (document.RootElement.TryGetProperty("title", out var title))
                {
                    return title.GetString() ?? "Сервер вернул ошибку";
                }
            }
            catch
            {
                return content;
            }

            return content;
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
}
