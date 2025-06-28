using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using MovieProject.Controllers;
using MovieProject.Models;
using MovieProject.ViewModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MovieProject.Tests.UnitTests.Controllers
{
    public class AdminControllerTests
    {
        private MovieContext GetTestContext()
        {
            var options = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase("AdminDb_" + System.Guid.NewGuid())
                .Options;

            var context = new MovieContext(options);
            context.Admins.Add(new Admin
            {
                Id = 1,
                Username = "existing",
                PasswordHash = new PasswordHasher<Admin>().HashPassword(null, "password")
            });
            context.SaveChanges();
            return context;
        }

        // GET Register çağrıldığında boş View döner
        [Trait("Category", "Unit")]
        [Fact]
        public void Register_Get_ReturnsView()
        {
            var controller = new AdminController(null, null);
            var result = controller.Register();
            Assert.IsType<ViewResult>(result);
        }

        // Geçerli model ile Register POST yapıldığında yeni kullanıcı eklenir ve Login sayfasına yönlendirir
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Register_Post_ValidModel_AddsUserAndRedirects()
        {
            var context = GetTestContext();
            var hasher = new PasswordHasher<Admin>();
            var controller = new AdminController(context, hasher);

            var vm = new RegisterViewModel { Username = "newuser", Password = "123" };
            var result = await controller.Register(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.NotNull(await context.Admins.SingleOrDefaultAsync(a => a.Username == "newuser"));
        }

        // Varolan kullanıcı adı ile Register POST yapıldığında ModelState hatası eklenir ve View döner
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Register_Post_DuplicateUsername_AddsModelError()
        {
            var context = GetTestContext();
            var hasher = new PasswordHasher<Admin>();
            var controller = new AdminController(context, hasher);

            var vm = new RegisterViewModel { Username = "existing", Password = "123" };
            var result = await controller.Register(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Username"));
        }

        // GET Login çağrıldığında boş View döner
        [Trait("Category", "Unit")]
        [Fact]
        public void Login_Get_ReturnsView()
        {
            var controller = new AdminController(null, null);
            var result = controller.Login();
            Assert.IsType<ViewResult>(result);
        }

        // Geçersiz model (ModelState invalid) ile Login POST yapıldığında View döner
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Login_Post_InvalidModel_ReturnsView()
        {
            var controller = new AdminController(null, null);
            controller.ModelState.AddModelError("Username", "Required");

            var vm = new LoginViewModel();
            var result = await controller.Login(vm);
            Assert.IsType<ViewResult>(result);
        }

        // Yanlış şifre girildiğinde Login POST hata verir ve View döner
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Login_Post_WrongPassword_ReturnsViewWithError()
        {
            var context = GetTestContext();
            var hasher = new PasswordHasher<Admin>();
            var controller = new AdminController(context, hasher);

            var vm = new LoginViewModel { Username = "existing", Password = "wrong" };
            var result = await controller.Login(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        // Doğru kimlik bilgileri ile Login POST yapıldığında oturum açılır ve Home/Index’e yönlendirilir
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Login_Post_ValidCredentials_SignsInAndRedirects()
        {
            var context = GetTestContext();
            var hasher = new PasswordHasher<Admin>();
            var controller = new AdminController(context, hasher);

            var mockUrlHelper = new Mock<IUrlHelper>();
            controller.Url = mockUrlHelper.Object;

            // mock SignInAsync
            var mockHttpContext = new DefaultHttpContext();
            var mockAuth = new Mock<IAuthenticationService>();
            mockAuth.Setup(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), null))
                    .Returns(Task.CompletedTask);

            mockHttpContext.RequestServices = new ServiceCollection()
                .AddSingleton(mockAuth.Object)
                .BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            var vm = new LoginViewModel { Username = "existing", Password = "password" };
            var result = await controller.Login(vm);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        // Geçersiz model (Register) ile POST yapıldığında hata mesajlarıyla View döner
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Register_Post_InvalidModel_ReturnsViewWithErrors()
        {
            var context = GetTestContext();
            var hasher = new PasswordHasher<Admin>();
            var controller = new AdminController(context, hasher);
            controller.ModelState.AddModelError("Username", "Required");

            var vm = new RegisterViewModel { Username = "", Password = "" };
            var result = await controller.Register(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal(vm, viewResult.Model);
        }

        // Bulunamayan kullanıcı adı ile Login POST yapıldığında hata mesajı eklenir ve View döner
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Login_Post_UnknownUsername_ReturnsViewWithError()
        {
            var context = GetTestContext();
            var hasher = new PasswordHasher<Admin>();
            var controller = new AdminController(context, hasher);

            var vm = new LoginViewModel { Username = "notfound", Password = "123" };
            var result = await controller.Login(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains("", controller.ModelState.Keys); // genel hata
        }

        // Logout çağrıldığında oturum kapatılır ve Login sayfasına yönlendirilir
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Logout_ClearsAuthenticationAndRedirectsToLogin()
        {
            var context = GetTestContext();
            var hasher = new PasswordHasher<Admin>();
            var controller = new AdminController(context, hasher);

            // Mock IAuthenticationService
            var mockHttpContext = new DefaultHttpContext();
            var mockAuth = new Mock<IAuthenticationService>();
            mockAuth.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), null))
                    .Returns(Task.CompletedTask);

            // Mock IUrlHelper
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/admin/login");
            controller.Url = mockUrlHelper.Object;

            // Set required services
            mockHttpContext.RequestServices = new ServiceCollection()
                .AddSingleton(mockAuth.Object)
                .BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await controller.Logout();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
        }


    }
}