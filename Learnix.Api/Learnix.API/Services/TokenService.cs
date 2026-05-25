using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Learnix.API.Models;

namespace Learnix.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime ExpiresAt) CreateToken(User user)
        {
            var expiresAt = DateTime.UtcNow.AddDays(GetLifetimeDays());
            var header = new Dictionary<string, object>
            {
                ["alg"] = "HS256",
                ["typ"] = "JWT"
            };
            var payload = new Dictionary<string, object>
            {
                ["sub"] = user.Id,
                ["email"] = user.Email,
                ["name"] = user.Name,
                ["exp"] = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
            };

            var headerPart = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
            var payloadPart = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
            var unsignedToken = $"{headerPart}.{payloadPart}";
            var signature = Sign(unsignedToken);

            return ($"{unsignedToken}.{signature}", expiresAt);
        }

        public TokenClaims? ValidateToken(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return null;
            }

            var unsignedToken = $"{parts[0]}.{parts[1]}";
            var expectedSignature = Sign(unsignedToken);
            if (!FixedTimeEquals(parts[2], expectedSignature))
            {
                return null;
            }

            try
            {
                var payloadBytes = Base64UrlDecode(parts[1]);
                var claims = JsonSerializer.Deserialize<TokenClaims>(payloadBytes);
                if (claims == null)
                {
                    return null;
                }

                var expiresAt = DateTimeOffset.FromUnixTimeSeconds(claims.ExpiresAtUnix);
                return expiresAt <= DateTimeOffset.UtcNow ? null : claims;
            }
            catch
            {
                return null;
            }
        }

        private string Sign(string value)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(GetSigningKey()));
            return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        private string GetSigningKey()
        {
            return _configuration["Auth:SigningKey"]
                ?? "learnix-development-signing-key-change-before-production";
        }

        private int GetLifetimeDays()
        {
            return int.TryParse(_configuration["Auth:TokenLifetimeDays"], out var days) && days > 0
                ? days
                : 14;
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);
            return leftBytes.Length == rightBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
            return Convert.FromBase64String(padded);
        }
    }

    public class TokenClaims
    {
        [JsonPropertyName("sub")]
        public int UserId { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("exp")]
        public long ExpiresAtUnix { get; set; }
    }
}
