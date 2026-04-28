using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Learnix.API.Data;
using Learnix.API.Models;
using Learnix.API.DTOs;
using BCrypt;
using System.Security.Cryptography.X509Certificates;

namespace Learnix.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context) 
        { 
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterUserDto dto) 
        { 
          var emailExist = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExist)
            {
                return BadRequest(new { error = "Пользователь с таким email уже существует" });
            }

          var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                PasswordHash = passwordHash,
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

       
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id) 
        {

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            { 
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost("login")]

        public async Task<ActionResult<User>> Login([FromBody] LoginUserDto dto) 
        { 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return Unauthorized(new { error = "Неверный email или пароль "});
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid) 
            {
                return Unauthorized(new { error = "Неверный логин или пароль" });
            }

            return Ok(user);



        }




    }
}
