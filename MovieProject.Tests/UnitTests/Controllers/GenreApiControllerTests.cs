using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieProject.Controllers;
using MovieProject.Models;
using Xunit;

namespace MovieProject.Tests.UnitTests.ApiControllers
{
    public class GenreApiControllerTests
    {
        private MovieContext GetContext(string dbName)
        {
            var opts = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new MovieContext(opts);
            // seed
            ctx.Genres.AddRange(
                new Genre { GenreId = "A", Name = "Action" },
                new Genre { GenreId = "D", Name = "Drama" }
            );
            ctx.SaveChanges();
            return ctx;
        }

        [Fact]
        public async Task GetGenres_ReturnsAll()
        {
            var ctx = GetContext("g1");
            var ctrl = new GenreApiController(ctx);

            var result = await ctrl.GetGenres();
            var ok = Assert.IsType<ActionResult<IEnumerable<Genre>>>(result);
            var list = Assert.IsType<List<Genre>>(ok.Value);

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetGenre_ExistingId_ReturnsGenre()
        {
            var ctx = GetContext("g2");
            var ctrl = new GenreApiController(ctx);

            var result = await ctrl.GetGenre("A");
            var action = Assert.IsType<ActionResult<Genre>>(result);
            Assert.Equal("Action", action.Value.Name);
        }

        [Fact]
        public async Task GetGenre_NonexistentId_ReturnsNotFound()
        {
            var ctx = GetContext("g3");
            var ctrl = new GenreApiController(ctx);

            var result = await ctrl.GetGenre("X");
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddGenre_Valid_ReturnsCreated()
        {
            var ctx = GetContext("g4");
            var ctrl = new GenreApiController(ctx);

            var newGenre = new Genre { GenreId = "C", Name = "Comedy" };
            var result = await ctrl.AddGenre(newGenre);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<Genre>(created.Value);
            Assert.Equal("C", returned.GenreId);
            Assert.Equal(3, ctx.Genres.Count());
        }

        [Fact]
        public async Task AddGenre_DuplicateId_ReturnsConflict()
        {
            var ctx = GetContext("g5");
            var ctrl = new GenreApiController(ctx);

            var dup = new Genre { GenreId = "A", Name = "Another" };
            var result = await ctrl.AddGenre(dup);

            var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateGenre_IdMismatch_ReturnsBadRequest()
        {
            var ctx = GetContext("g7");
            var ctrl = new GenreApiController(ctx);

            var updated = new Genre { GenreId = "D", Name = "X" };
            var result = await ctrl.UpdateGenre("A", updated);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID uyuşmuyor", bad.Value);
        }

        [Fact]
        public async Task UpdateGenre_NotFound_ReturnsNotFound()
        {
            var ctx = GetContext("g8");
            var ctrl = new GenreApiController(ctx);

            var updated = new Genre { GenreId = "X", Name = "X" };
            var result = await ctrl.UpdateGenre("X", updated);

            var nf = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteGenre_Existing_ReturnsNoContent()
        {
            var ctx = GetContext("g9");
            var ctrl = new GenreApiController(ctx);

            var result = await ctrl.DeleteGenre("A");
            Assert.IsType<NoContentResult>(result);
            Assert.Null(ctx.Genres.Find("A"));
        }

        [Fact]
        public async Task DeleteGenre_NotFound_ReturnsNotFound()
        {
            var ctx = GetContext("g10");
            var ctrl = new GenreApiController(ctx);

            var result = await ctrl.DeleteGenre("Z");
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
