using Learnix.API.Data;
using Learnix.API.DTOs;
using Learnix.API.Models;
using Learnix.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learnix.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto dto)
        {
            var email = NormalizeEmail(dto.Email);
            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailExists)
            {
                return BadRequest(new { error = "Пользователь с таким email уже существует" });
            }

            var user = new User
            {
                Email = email,
                Name = dto.Name.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Class = dto.Class?.Trim(),
                Grade = dto.Grade ?? TryExtractGrade(dto.Class)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(CreateAuthResponse(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserDto dto)
        {
            var email = NormalizeEmail(dto.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { error = "Неверный email или пароль" });
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { error = "Неверный email или пароль" });
            }

            return Ok(CreateAuthResponse(user));
        }

        private AuthResponseDto CreateAuthResponse(User user)
        {
            var token = _tokenService.CreateToken(user);
            return new AuthResponseDto
            {
                Token = token.Token,
                ExpiresAt = token.ExpiresAt,
                User = UserResponseDto.FromUser(user)
            };
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static int? TryExtractGrade(string? userClass)
        {
            if (string.IsNullOrWhiteSpace(userClass))
            {
                return null;
            }

            var digits = new string(userClass.TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(digits, out var grade) ? grade : null;
        }
    }
}
