using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MovieProject.Models;
using MovieProject.ApiDto;
using System.ComponentModel.DataAnnotations;

namespace MovieProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApiController : ControllerBase
    {
        private readonly MovieContext _db;
        private readonly IPasswordHasher<Admin> _hasher;

        public AdminApiController(MovieContext db, IPasswordHasher<Admin> hasher)
        {
            _db = db; _hasher = hasher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (_db.Admins.Any(a => a.Username == dto.Username))
                return Conflict(new { message = "Kullanıcı adı mevcut." });

            var admin = new Admin { Username = dto.Username };
            admin.PasswordHash = _hasher.HashPassword(admin, dto.Password);
            _db.Admins.Add(admin);
            await _db.SaveChangesAsync();
            return CreatedAtAction(null, new { admin.Id, admin.Username });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var admin = _db.Admins.SingleOrDefault(a => a.Username == dto.Username);
            if (admin == null ||
                _hasher.VerifyHashedPassword(admin, admin.PasswordHash, dto.Password)
                    != PasswordVerificationResult.Success)
                return Unauthorized(new { message = "Geçersiz kimlik bilgileri." });
            return Ok(new { message = "Başarılı giriş." });
        }
    }
}
