using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieProject.Controllers;
using MovieProject.Models;
using MovieProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MovieProject.Tests.UnitTests.Controllers
{
    public class HomeControllerTests
    {
        private MovieContext GetTestContext()
        {
            var opts = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase("HomeDb_" + Guid.NewGuid())
                .Options;
            var ctx = new MovieContext(opts);

            ctx.Genres.AddRange(
                new Genre { GenreId = "T", Name = "Thriller" },
                new Genre { GenreId = "D", Name = "Drama" }
            );

            ctx.Movies.AddRange(
                new Movie { MovieId = 1, Name = "Semaaa", Year = 2002, Rating = 3, GenreId = "T" },
                new Movie { MovieId = 2, Name = "melisss", Year = 2003, Rating = 10, GenreId = "T" },
                new Movie { MovieId = 3, Name = "melis zeynep sema", Year = 2004, Rating = 7, GenreId = "D" }
            );
            ctx.SaveChanges();
            return ctx;
        }

        // Filtre uygulanmadan tüm filmleri ve türleri döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_NoFilters_ReturnsAllMoviesAndGenres()
        {
            var ctx = GetTestContext();
            var ctrl = new HomeController(ctx);

            IActionResult actionResult = await ctrl.Index(null, null);
            var vr = Assert.IsType<ViewResult>(actionResult);
            var vm = Assert.IsType<MovieFilterViewModel>(vr.Model);

            Assert.Equal(3, vm.Movies.Count);

            var genres = ((SelectList)vm.GenreList).Select(x => x.Text).ToList();
            Assert.Equal(new List<string> { "Drama", "Thriller" }, genres);

            Assert.Null(vm.SearchString);
            Assert.Null(vm.SelectedGenre);
        }

        // İsme göre önek filtre uygulandığında eşleşen filmleri döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_FilterByNamePrefix_ReturnsMatchingMovies()
        {
            var ctx = GetTestContext();
            var ctrl = new HomeController(ctx);

            IActionResult actionResult = await ctrl.Index("s", null);
            var vr = Assert.IsType<ViewResult>(actionResult);
            var vm = Assert.IsType<MovieFilterViewModel>(vr.Model);

            var names = vm.Movies.Select(m => m.Name).ToList();
            Assert.Equal(2, names.Count);
            Assert.Contains("Semaaa", names);
            Assert.Contains("melis zeynep sema", names);

            Assert.Null(vm.SelectedGenre);
            Assert.Equal("s", vm.SearchString);
        }

        // Tür filtresi uygulandığında yalnızca o türe ait filmleri döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_FilterByGenre_ReturnsMoviesOfThatGenre()
        {
            var ctx = GetTestContext();
            var ctrl = new HomeController(ctx);

            IActionResult actionResult = await ctrl.Index(null, "Drama");
            var vr = Assert.IsType<ViewResult>(actionResult);
            var vm = Assert.IsType<MovieFilterViewModel>(vr.Model);

            Assert.Single(vm.Movies);
            Assert.Equal("melis zeynep sema", vm.Movies[0].Name);

            Assert.Null(vm.SearchString);
            Assert.Equal("Drama", vm.SelectedGenre);
        }

        // Hem isim hem tür filtresi uygulanınca kesişim kümesini döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_FilterByNameAndGenre_ReturnsIntersection()
        {
            var ctx = GetTestContext();
            var ctrl = new HomeController(ctx);

            IActionResult actionResult = await ctrl.Index("m", "Thriller");
            var vr = Assert.IsType<ViewResult>(actionResult);
            var vm = Assert.IsType<MovieFilterViewModel>(vr.Model);

            Assert.Single(vm.Movies);
            Assert.Equal("melisss", vm.Movies[0].Name);

            Assert.Equal("m", vm.SearchString);
            Assert.Equal("Thriller", vm.SelectedGenre);
        }

        // Hiçbir eşleşme olmadığında boş liste döner.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_FilterNoMatches_ReturnsEmptyList()
        {
            var ctx = GetTestContext();
            var ctrl = new HomeController(ctx);

            IActionResult actionResult = await ctrl.Index("xyz", "NonexistentGenre");
            var vr = Assert.IsType<ViewResult>(actionResult);
            var vm = Assert.IsType<MovieFilterViewModel>(vr.Model);

            Assert.Empty(vm.Movies);
            Assert.Equal("xyz", vm.SearchString);
            Assert.Equal("NonexistentGenre", vm.SelectedGenre);
        }

        // Yeni eklenen tür için film yoksa boş liste döner.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_FilterByGenreWithNoMovies_ReturnsEmptyList()
        {
            var ctx = GetTestContext();
            var ctrl = new HomeController(ctx);

            ctx.Genres.Add(new Genre { GenreId = "X", Name = "Horror" });
            ctx.SaveChanges();

            IActionResult actionResult = await ctrl.Index(null, "Horror");
            var vr = Assert.IsType<ViewResult>(actionResult);
            var vm = Assert.IsType<MovieFilterViewModel>(vr.Model);

            Assert.Empty(vm.Movies);
            Assert.Equal("Horror", vm.SelectedGenre);
        }

        // İsme göre arama büyük/küçük harf duyarsızdır.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_NameSearch_CaseInsensitive()
        {
            var ctx = GetTestContext();
            var ctrl = new HomeController(ctx);

            var result = await ctrl.Index("MELİS", null);
            var vr = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<MovieFilterViewModel>(vr.Model);

            Assert.Equal(2, vm.Movies.Count); // melisss + melis zeynep sema
        }

        // Boşluk içeren aramalarda doğru sonuçları döndürür.
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_NameSearch_WithWhitespace_ReturnsCorrectResults()
        {
            var ctx = GetTestContext();
            var ctrl = new HomeController(ctx);

            var result = await ctrl.Index("melis zeynep", null);
            var vr = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<MovieFilterViewModel>(vr.Model);

            Assert.Single(vm.Movies);
            Assert.Equal("melis zeynep sema", vm.Movies[0].Name);
        }

    }
}
