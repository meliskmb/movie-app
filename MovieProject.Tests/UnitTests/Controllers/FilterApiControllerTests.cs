using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieProject.Controllers;
using MovieProject.Models;
using Xunit;

namespace MovieProject.Tests.UnitTests.ApiControllers
{
    public class FilterApiControllerTests
    {
        private MovieContext GetTestContext()
        {
            var opts = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase("FilterDb_" + Guid.NewGuid())
                .Options;
            var ctx = new MovieContext(opts);

            // Türleri oluştur
            ctx.Genres.AddRange(
                new Genre { GenreId = "A", Name = "Action" },
                new Genre { GenreId = "D", Name = "Drama" }
            );

            // Filmleri ekle
            ctx.Movies.AddRange(
                new Movie { MovieId = 1, Name = "Star Wars", Year = 1977, Rating = 9, GenreId = "A" },
                new Movie { MovieId = 2, Name = "Star Trek", Year = 1979, Rating = 8, GenreId = "A" },
                new Movie { MovieId = 3, Name = "Dramatic Story", Year = 2020, Rating = 7, GenreId = "D" }
            );
            ctx.SaveChanges();
            return ctx;
        }

        [Trait("Category", "Unit")]
        [Fact]
        public async Task GetFilteredMovies_NoFilters_ReturnsAllMovies()
        {
            // Arrange
            var ctx = GetTestContext();
            var ctrl = new FilterApiController(ctx);

            // Act
            IActionResult action;
            try
            {
                action = await ctrl.GetFilteredMovies(null, null);
            }
            catch (Exception ex)
            {
                // Eğer exception fırlatırsa testin başarısız olmasını sağla
                Assert.False(true, $"Beklenmeyen exception: {ex}");
                return;
            }

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            var list = Assert.IsAssignableFrom<IEnumerable<dynamic>>(ok.Value);
            Assert.Equal(3, list.Count());
        }

        [Trait("Category", "Unit")]
        [Fact]
        public async Task GetFilteredMovies_FilterByNamePrefix_ReturnsMatching()
        {
            // Arrange
            var ctx = GetTestContext();
            var ctrl = new FilterApiController(ctx);

            // Act
            var actionResult = await ctrl.GetFilteredMovies("Star", null);
            var ok = Assert.IsType<OkObjectResult>(actionResult);

            // OkObjectResult.Value bir IEnumerable<object> olarak geliyor
            var items = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);

            // Her item'in Name property'sini refleksiyonla çekelim
            var names = items
                .Select(item => item.GetType().GetProperty("Name")!.GetValue(item) as string)
                .ToList();

            // Assert
            Assert.Equal(2, names.Count);
            Assert.Contains("Star Wars", names);
            Assert.Contains("Star Trek", names);
        }


        // Genre sorgusu olduğunda yalnızca o türdeki filmleri döndürür
        [Trait("Category", "Unit")]
        [Fact]
        public async Task GetFilteredMovies_FilterByGenre_ReturnsOnlyThatGenre()
        {
            var ctx = GetTestContext();
            var ctrl = new FilterApiController(ctx);

            var result = await ctrl.GetFilteredMovies(null, "Drama");
            var ok = Assert.IsType<OkObjectResult>(result);

            // anonim tipleri object olarak al
            var items = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);

            // her item için Name property’sine yansıtma (reflection) ile eriş
            var names = items
                .Select(item => item.GetType().GetProperty("Name")!.GetValue(item) as string)
                .ToList();

            Assert.Single(names);
            Assert.Equal("Dramatic Story", names[0]);
        }


        [Trait("Category", "Unit")]
        [Fact]
        public async Task GetFilteredMovies_FilterByNameAndGenre_ReturnsIntersection()
        {
            var ctx = GetTestContext();
            var ctrl = new FilterApiController(ctx);

            var result = await ctrl.GetFilteredMovies("Star", "Action");
            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<dynamic>>(ok.Value);

            Assert.Equal(2, list.Count());
        }

        [Trait("Category", "Unit")]
        [Fact]
        public async Task GetFilteredMovies_NoMatches_ReturnsEmpty()
        {
            var ctx = GetTestContext();
            var ctrl = new FilterApiController(ctx);

            var result = await ctrl.GetFilteredMovies("XYZ", "Comedy");
            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<dynamic>>(ok.Value);

            Assert.Empty(list);
        }
    }
}
