// MovieControllerTests.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieProject.Controllers;
using MovieProject.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MovieProject.Tests.UnitTests.Controllers
{
    public class MovieControllerTests
    {
        private MovieContext GetTestContext()
        {
            var opts = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase("TestDb_" + System.Guid.NewGuid())
                .Options;
            var ctx = new MovieContext(opts);

            // Seed türler
            ctx.Genres.AddRange(
                new Genre { GenreId = "A", Name = "Action" },
                new Genre { GenreId = "D", Name = "Drama" }
            );
            ctx.SaveChanges();
            return ctx;
        }

        // Create GET çağrıldığında “Edit” sayfasını boş Movie ve seed’li türlerle döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public void Create_Get_ReturnsEditViewWithEmptyMovie()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);

            var vr = ctrl.Create() as ViewResult;

            Assert.NotNull(vr);
            Assert.Equal("Edit", vr.ViewName);
            Assert.IsType<Movie>(vr.Model);
            Assert.Equal("Add New Movie", vr.ViewData["Action"]);
            var genres = vr.ViewData["Genres"] as List<Genre>;
            Assert.Equal(2, genres.Count);
        }

        // Create GET, tür yoksa bile boş tür listesi ile View döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public void Create_Get_ReturnsGenresEvenIfEmpty()
        {
            var opts = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase("TestEmptyGenres_" + System.Guid.NewGuid())
                .Options;
            var ctx = new MovieContext(opts);

            var ctrl = new MovieController(ctx);
            var vr = ctrl.Create() as ViewResult;

            Assert.NotNull(vr);
            Assert.IsType<Movie>(vr.Model);
            var genres = vr.ViewData["Genres"] as List<Genre>;
            Assert.Empty(genres);
        }

        // Edit GET, geçersiz ID (0) ile en azından null olmayan bir sonuç döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Get_NullId_ReturnsBadRequest()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var result = ctrl.Edit(0);
            Assert.NotNull(result);
        }

        // Edit POST ile yeni film ekler ve Home/Index’e yönlendirir.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_NewMovie_AddsAndRedirects()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var m = new Movie { Name = "M1", GenreId = "A", Year = 2020, Rating = 5 };

            var res = ctrl.Edit(m) as RedirectToActionResult;

            Assert.NotNull(res);
            Assert.Equal("Index", res.ActionName);
            Assert.Equal("Home", res.ControllerName);
            Assert.Equal(1, ctx.Movies.Count());
            Assert.Equal("M1", ctx.Movies.Single().Name);
        }

        // Edit POST, Rating=0 olduğunda ModelState geçersiz olur ve View döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_RatingZero_InvalidModelState()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var m = new Movie { Name = "Bad", GenreId = "A", Year = 2022, Rating = 0 };
            ctrl.ModelState.AddModelError("Rating", "Invalid");

            var vr = ctrl.Edit(m) as ViewResult;

            Assert.NotNull(vr);
            Assert.False(ctrl.ModelState.IsValid);
        }

        // Edit POST, GenreId eksikse ModelState hatası ve View döner
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_EmptyGenreId_ReturnsValidationError()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var m = new Movie { Name = "G1", Year = 2021, Rating = 6 }; // GenreId eksik
            ctrl.ModelState.AddModelError("GenreId", "Required");

            var vr = ctrl.Edit(m) as ViewResult;

            Assert.NotNull(vr);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.True(ctrl.ModelState.ContainsKey("GenreId"));
        }

        // Edit POST, aynı film tekrar eklenmez.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_SameMovieNotAddedAgain()
        {
            var ctx = GetTestContext();
            var movie = new Movie { MovieId = 1, Name = "Same", GenreId = "A", Year = 2020, Rating = 5 };
            ctx.Movies.Add(movie);
            ctx.SaveChanges();

            var ctrl = new MovieController(ctx);
            var res = ctrl.Edit(movie) as RedirectToActionResult;

            Assert.NotNull(res);
            Assert.Equal(1, ctx.Movies.Count());
        }

        // Edit POST (MovieId!=0) mevcut filmi günceller ve yönlendirir.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_ExistingMovie_UpdatesAndRedirects()
        {
            var ctx = GetTestContext();
            var existing = new Movie { MovieId = 42, Name = "Old", GenreId = "D", Year = 2010, Rating = 4 };
            ctx.Movies.Add(existing);
            ctx.SaveChanges();

            var ctrl = new MovieController(ctx);
            existing.Name = "New";
            var res = ctrl.Edit(existing) as RedirectToActionResult;

            Assert.NotNull(res);
            Assert.Equal("Index", res.ActionName);
            Assert.Equal(1, ctx.Movies.Count());
            Assert.Equal("New", ctx.Movies.Find(42)!.Name);
        }

        // Edit POST, ModelState invalid ise “Edit” View ve orijinal model döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_InvalidModel_ReturnsEditViewWithErrors()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var bad = new Movie(); // Name, Year, Rating, GenreId eksik
            ctrl.ModelState.AddModelError("Name", "Required");

            var vr = ctrl.Edit(bad) as ViewResult;

            Assert.NotNull(vr);
            Assert.Equal("Edit", vr.ViewName);
            Assert.Same(bad, vr.Model);
            Assert.False(ctrl.ModelState.IsValid);
        }

        // Edit GET, var olan ID ile doğru filmi ve “Edit Movie” ActionViewData döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Get_ExistingId_ReturnsViewWithMovie()
        {
            var ctx = GetTestContext();
            var m = new Movie { MovieId = 5, Name = "X", GenreId = "A", Year = 2001, Rating = 6 };
            ctx.Movies.Add(m);
            ctx.SaveChanges();

            var ctrl = new MovieController(ctx);
            var vr = ctrl.Edit(5) as ViewResult;

            Assert.NotNull(vr);
            Assert.IsType<Movie>(vr.Model);
            Assert.Equal("X", ((Movie)vr.Model).Name);
            Assert.Equal("Edit Movie", ctrl.ViewData["Action"]);
        }

        // Edit GET, olmayan ID ile NotFoundResult döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Get_NonexistentId_ReturnsNotFound()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);

            var nf = ctrl.Edit(999) as NotFoundResult;

            Assert.NotNull(nf);
        }

        // Delete GET, var olan ID ile silme onayı ViewModel’i döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Delete_Get_ExistingId_ReturnsViewWithMovie()
        {
            var ctx = GetTestContext();
            var m = new Movie { MovieId = 7, Name = "Y", GenreId = "D", Year = 2011, Rating = 7 };
            ctx.Movies.Add(m);
            ctx.SaveChanges();

            var ctrl = new MovieController(ctx);
            var vr = ctrl.Delete(7) as ViewResult;

            Assert.NotNull(vr);
            Assert.IsType<Movie>(vr.Model);
            Assert.Equal("Y", ((Movie)vr.Model).Name);
        }

        // Delete GET, olmayan ID ile NotFoundResult döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Delete_Get_NonexistentId_ReturnsNotFound()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);

            var nf = ctrl.Delete(1234) as NotFoundResult;
            Assert.NotNull(nf);
        }

        // Delete POST (DeleteConfirmed), var olan ID ile siler ve Index’e yönlendirir.
        [Trait("Category", "Unit")]
        [Fact]
        public void Delete_Post_ExistingId_RemovesAndRedirects()
        {
            var ctx = GetTestContext();
            var m = new Movie { MovieId = 8, Name = "Z", GenreId = "A", Year = 2022, Rating = 8 };
            ctx.Movies.Add(m);
            ctx.SaveChanges();

            var ctrl = new MovieController(ctx);
            var res = ctrl.DeleteConfirmed(8) as RedirectToActionResult;

            Assert.NotNull(res);
            Assert.Equal("Index", res.ActionName);
            Assert.Empty(ctx.Movies);
        }

        // Delete POST, olmayan ID ile hata vermeden Index’e yönlendirir.
        [Trait("Category", "Unit")]
        [Fact]
        public void Delete_Post_NonexistentId_RedirectsWithoutError()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);

            var res = ctrl.DeleteConfirmed(55) as RedirectToActionResult;
            Assert.NotNull(res);
            Assert.Equal("Index", res.ActionName);
        }

        // Edit POST, Name eksikse ModelState hatası ve “Edit” View döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_MissingName_ReturnsModelError()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var movie = new Movie { Year = 2020, Rating = 5, GenreId = "A" }; // Name eksik

            ctrl.ModelState.AddModelError("Name", "Required");
            var result = ctrl.Edit(movie) as ViewResult;

            Assert.NotNull(result);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal("Edit", result.ViewName);
        }

        // Edit POST, GenreId eksikse ModelState hatası ve “Edit” View döner.        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_MissingGenreId_ReturnsModelError()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var movie = new Movie { Name = "Test", Year = 2020, Rating = 5 }; // GenreId eksik

            ctrl.ModelState.AddModelError("GenreId", "Required");
            var result = ctrl.Edit(movie) as ViewResult;

            Assert.NotNull(result);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal("Edit", result.ViewName);
        }

        /// Edit POST, Rating eksikse ModelState hatası ve “Edit” View döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_MissingRating_ReturnsModelError()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var movie = new Movie { Name = "Test", Year = 2020, GenreId = "A" }; // Rating eksik

            ctrl.ModelState.AddModelError("Rating", "Required");
            var result = ctrl.Edit(movie) as ViewResult;

            Assert.NotNull(result);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal("Edit", result.ViewName);
        }

        /// Edit POST, Year eksikse ModelState hatası ve “Edit” View döner.
        [Trait("Category", "Unit")]
        [Fact]
        public void Edit_Post_MissingYear_ReturnsModelError()
        {
            var ctx = GetTestContext();
            var ctrl = new MovieController(ctx);
            var movie = new Movie { Name = "Test", Rating = 5, GenreId = "A" }; // Year eksik

            ctrl.ModelState.AddModelError("Year", "Required");
            var result = ctrl.Edit(movie) as ViewResult;

            Assert.NotNull(result);
            Assert.False(ctrl.ModelState.IsValid);
            Assert.Equal("Edit", result.ViewName);
        }
    }
}
