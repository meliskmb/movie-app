using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieProject.Models;
using MovieProject.ViewModels;
using System.Security.Claims;

namespace MovieProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly MovieContext _db;
        private readonly IPasswordHasher<Admin> _hasher;

        public AdminController(MovieContext db, IPasswordHasher<Admin> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // GET: /Admin/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Admin/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (_db.Admins.Any(a => a.Username == vm.Username))
            {
                ModelState.AddModelError(nameof(vm.Username), "Bu kullanıcı adı zaten kayıtlı.");
                return View(vm);
            }

            var admin = new Admin { Username = vm.Username };
            admin.PasswordHash = _hasher.HashPassword(admin, vm.Password);

            _db.Admins.Add(admin);
            await _db.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // GET: /Admin/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Admin/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var admin = _db.Admins.SingleOrDefault(a => a.Username == vm.Username);
            if (admin == null ||
                _hasher.VerifyHashedPassword(admin, admin.PasswordHash, vm.Password)
                    != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View(vm);
            }

            // 1) Claim’leri oluştur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim("AdminId", admin.Id.ToString())
            };

            // 2) Identity ve Principal
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 3) Cookie’ye yaz (login olmuş say)
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
