using Xunit;
using MovieProject.Controllers;
using MovieProject.Models;
using MovieProject.ApiDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;

namespace MovieProject.Tests.UnitTests.ApiControllers
{
    public class AdminApiControllerTests
    {
        private readonly MovieContext _context;
        private readonly Mock<IPasswordHasher<Admin>> _mockHasher;
        private readonly AdminApiController _controller;

        public AdminApiControllerTests()
        {
            var options = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase("TestDb_" + System.Guid.NewGuid())
                .Options;

            _context = new MovieContext(options);
            _mockHasher = new Mock<IPasswordHasher<Admin>>();
            _controller = new AdminApiController(_context, _mockHasher.Object);
        }

        /// Geçerli RegisterDto ile Register çağrıldığında 201 Created döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Register_ValidInput_ReturnsCreated()
        {
            var dto = new RegisterDto { Username = "user1", Password = "Secret123" };
            _mockHasher.Setup(h => h.HashPassword(It.IsAny<Admin>(), It.IsAny<string>())).Returns("hashed");

            var result = await _controller.Register(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
        }

        /// Aynı kullanıcı adı zaten var ise Register çağrısında 409 Conflict döner.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Register_DuplicateUsername_ReturnsConflict()
        {
            _context.Admins.Add(new Admin { Username = "user2", PasswordHash = "hash" });
            await _context.SaveChangesAsync();

            var dto = new RegisterDto { Username = "user2", Password = "Secret123" };

            var result = await _controller.Register(dto);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflict.StatusCode);
        }

        /// ModelState geçersizse Register çağrısında 400 Bad Request döner.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequest()
        {
            var dto = new RegisterDto { Username = "", Password = "" };
            _controller.ModelState.AddModelError("Username", "Required");

            var result = await _controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// Doğru kimlik bilgileri ile Login çağrıldığında 200 OK döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Login_ValidCredentials_ReturnsOk()
        {
            var admin = new Admin { Username = "user3", PasswordHash = "hashed" };
            _context.Admins.Add(admin);
            _context.SaveChanges();

            var dto = new LoginDto { Username = "user3", Password = "Secret123" };

            _mockHasher.Setup(h => h.VerifyHashedPassword(admin, "hashed", "Secret123"))
                       .Returns(PasswordVerificationResult.Success);

            var result = _controller.Login(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        /// Yanlış kimlik bilgileri ile Login çağrıldığında 401 Unauthorized döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var dto = new LoginDto { Username = "ghost", Password = "bad" };

            var result = _controller.Login(dto);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorized.StatusCode);
        }

        /// ModelState geçersizse Login çağrısında 400 Bad Request döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Login_InvalidModel_ReturnsBadRequest()
        {
            var dto = new LoginDto(); // empty

            _controller.ModelState.AddModelError("Username", "Required");

            var result = _controller.Login(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
