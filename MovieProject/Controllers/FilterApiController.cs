using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieProject.Models;

namespace MovieProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilterApiController : ControllerBase
    {
        private readonly MovieContext _context;

        public FilterApiController(MovieContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredMovies([FromQuery] string? searchString, [FromQuery] string? genre)
        {
            var moviesQuery = _context.Movies
                .Include(m => m.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var prefix = searchString + "%";
                var wordPrefix = "% " + searchString + "%";

                moviesQuery = moviesQuery.Where(m =>
                    EF.Functions.Like(m.Name, prefix) ||
                    EF.Functions.Like(m.Name, wordPrefix));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                moviesQuery = moviesQuery.Where(m => m.Genre.Name == genre);
            }

            var result = await moviesQuery
                .Select(m => new
                {
                    m.MovieId,
                    m.Name,
                    m.Year,
                    m.Rating,
                    Genre = m.Genre.Name
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}
