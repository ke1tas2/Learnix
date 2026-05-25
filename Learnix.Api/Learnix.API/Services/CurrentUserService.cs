namespace Learnix.API.Services
{
    public class CurrentUserService
    {
        private readonly TokenService _tokenService;

        public CurrentUserService(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public int? GetUserId(HttpRequest request)
        {
            var authorization = request.Headers.Authorization.ToString();
            const string prefix = "Bearer ";
            if (!authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var token = authorization[prefix.Length..].Trim();
            return _tokenService.ValidateToken(token)?.UserId;
        }
    }
}
