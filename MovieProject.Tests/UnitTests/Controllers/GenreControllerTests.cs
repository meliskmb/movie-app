using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieProject.Controllers;
using MovieProject.Models;
using MovieProject.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MovieProject.Tests.UnitTests.Controllers
{
    public class GenreControllerTests
    {
        // Her test için benzersiz, seedli bir InMemory context sağlar
        private MovieContext GetTestContext()
        {
            var opts = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase("GenreDb_" + System.Guid.NewGuid())
                .Options;
            var ctx = new MovieContext(opts);
            // Başlangıçta iki tür ekleyelim
            ctx.Genres.AddRange(
                new Genre { GenreId = "A", Name = "Action" },
                new Genre { GenreId = "D", Name = "Drama" }
            );
            ctx.SaveChanges();
            return ctx;
        }

        // GET Index çağrıldığında tüm türleri (Action, Drama) sıralı olarak ViewModel içinde döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_Get_ReturnsViewWithAllGenres()
        {
            // Arrange
            var ctx = GetTestContext();
            var ctrl = new GenreController(ctx);

            // Act
            var result = await ctrl.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GenreListViewModel>(result.Model);
            var vm = (GenreListViewModel)result.Model;
            Assert.Equal(2, vm.Genres.Count);
            // Sıralama: Action önce Drama
            Assert.Equal(new List<string> { "Action", "Drama" },
                         vm.Genres.Select(g => g.Name).ToList());
        }

        // Geçerli model ile POST Index yapıldığında yeni tür ekler ve Index’e yönlendirir.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_Post_ValidModel_AddsGenreAndRedirects()
        {
            // Arrange
            var ctx = GetTestContext();
            var ctrl = new GenreController(ctx);
            var vm = new GenreListViewModel { NewGenreId = "C", NewGenreName = "Comedy" };

            // Act
            IActionResult actionResult = await ctrl.Index(vm);

            // Assert: RedirectToActionResult olduğunu doğrula
            var redirect = Assert.IsType<RedirectToActionResult>(actionResult);
            Assert.Equal(nameof(GenreController.Index), redirect.ActionName);

            // DB gerçekten eklenmiş mi?
            Assert.Equal(3, ctx.Genres.Count());
            Assert.Equal("Comedy", ctx.Genres.Find("C")!.Name);
        }

        // ModelState geçersizse POST Index aynı View ile Model hatalarıyla geri döner.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_Post_InvalidModel_ReturnsViewWithModelErrors()
        {
            // Arrange
            var ctx = GetTestContext();
            var ctrl = new GenreController(ctx);
            var vm = new GenreListViewModel { NewGenreId = "", NewGenreName = "" };
            ctrl.ModelState.AddModelError("NewGenreId", "Required");

            // Act
            IActionResult actionResult = await ctrl.Index(vm);

            // Assert: Önce ViewResult olduğunu doğrula
            var viewResult = Assert.IsType<ViewResult>(actionResult);
            Assert.IsType<GenreListViewModel>(viewResult.Model);

            // View’a dönünce RedirectToActionResult olmamalı
            Assert.Null(actionResult as RedirectToActionResult);

            // VM.Genres her hâlde dolu olmalı
            var model = (GenreListViewModel)viewResult.Model;
            Assert.Equal(2, model.Genres.Count);

            // ModelState halen invalid
            Assert.False(ctrl.ModelState.IsValid);
            Assert.True(ctrl.ModelState.ContainsKey("NewGenreId"));
        }

        // Var olan bir GenreId ile POST Index yapıldığında ModelState’e hata ekler ve View döner.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_Post_DuplicateId_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var ctx = GetTestContext();
            var ctrl = new GenreController(ctx);

            var vm = new GenreListViewModel
            {
                NewGenreId = "A",       // zaten var
                NewGenreName = "Another"
            };

            // Act
            var result = await ctrl.Index(vm) as ViewResult;

            // Assert: View’e dönmeli
            Assert.NotNull(result);
            Assert.IsType<GenreListViewModel>(result.Model);

            // DB değişmemeli (hala 2)
            Assert.Equal(2, ctx.Genres.Count());

            // ModelState’e NewGenreId için hata eklenmiş
            Assert.False(ctrl.ModelState.IsValid);
            var errors = ctrl.ModelState["NewGenreId"].Errors;
            Assert.Single(errors);
            Assert.Contains("Bu kod zaten kullanılıyor", errors[0].ErrorMessage);
        }

        // Yeni tür adı boşsa POST Index’te ModelState hatası gösterir.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_Post_MissingName_ShowsModelError()
        {
            var ctx = GetTestContext();
            var ctrl = new GenreController(ctx);
            var vm = new GenreListViewModel { NewGenreId = "E", NewGenreName = "" };
            ctrl.ModelState.AddModelError("NewGenreName", "Required");

            var result = await ctrl.Index(vm) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<GenreListViewModel>(result.Model);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.True(ctrl.ModelState.ContainsKey("NewGenreName"));
        }

        // Yeni tür kimliği boşsa POST Index’te ModelState hatası gösterir.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_Post_MissingId_ShowsModelError()
        {
            var ctx = GetTestContext();
            var ctrl = new GenreController(ctx);
            var vm = new GenreListViewModel { NewGenreId = "", NewGenreName = "Fantasy" };
            ctrl.ModelState.AddModelError("NewGenreId", "Required");

            var result = await ctrl.Index(vm) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<GenreListViewModel>(result.Model);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.True(ctrl.ModelState.ContainsKey("NewGenreId"));
        }

        // Id ve Name sadece boşluk karakteriyse her iki alan için de hataları gösterir.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_Post_WhitespaceIdAndName_ShowsModelErrors()
        {
            var ctx = GetTestContext();
            var ctrl = new GenreController(ctx);
            var vm = new GenreListViewModel { NewGenreId = "   ", NewGenreName = "   " };
            ctrl.ModelState.AddModelError("NewGenreId", "Required");
            ctrl.ModelState.AddModelError("NewGenreName", "Required");

            var result = await ctrl.Index(vm) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<GenreListViewModel>(result.Model);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.True(ctrl.ModelState.ContainsKey("NewGenreId"));
            Assert.True(ctrl.ModelState.ContainsKey("NewGenreName"));
        }


        // Mevcut ID’nin farklı küçük/büyük harf varyasyonuyla POST Index eklemeye izin verir.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_Post_SameIdWithDifferentCase_AllowsInsertion()
        {
            // Arrange
            var ctx = GetTestContext();
            var ctrl = new GenreController(ctx);

            // Başlangıçta "A" var. Şimdi "a" eklemeye çalışalım.
            var vm = new GenreListViewModel
            {
                NewGenreId = "a",
                NewGenreName = "AltGenre"
            };

            // Act
            var result = await ctrl.Index(vm);

            // Assert: Redirect olmalı ⇒ eklenmiş
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(GenreController.Index), redirect.ActionName);

            // DB'de artık hem "A" hem "a" olmalı
            Assert.Equal(3, ctx.Genres.Count());
            Assert.NotNull(ctx.Genres.Find("a"));
        }


    }
}
