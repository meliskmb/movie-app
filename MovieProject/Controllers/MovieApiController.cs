using Microsoft.AspNetCore.Mvc;
using MovieProject.Models;
using Microsoft.EntityFrameworkCore;

namespace MovieProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieApiController : ControllerBase
    {
        private readonly MovieContext _context;

        public MovieApiController(MovieContext context)
        {
            _context = context;
        }

        // GET: api/movieapi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> GetAll()
        {
            return await _context.Movies.Include(m => m.Genre).ToListAsync();
        }

        // GET: api/movieapi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> GetById(int id)
        {
            var movie = await _context.Movies.Include(m => m.Genre).FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null) return NotFound();
            return movie;
        }

        // POST: api/movieapi
        [HttpPost]
        public async Task<ActionResult<Movie>> Create(Movie movie)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = movie.MovieId }, movie);
        }

        // PUT: api/movieapi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Movie movie)
        {
            if (id != movie.MovieId) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(movie).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Movies.Any(e => e.MovieId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/movieapi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
