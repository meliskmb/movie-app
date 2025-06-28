using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieProject.Models;

namespace MovieProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreApiController : ControllerBase
    {
        private readonly MovieContext _context;

        public GenreApiController(MovieContext context)
        {
            _context = context;
        }

        // GET: api/genreapi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
        {
            return await _context.Genres.ToListAsync();
        }

        // GET: api/genreapi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Genre>> GetGenre(string id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return NotFound();
            return genre;
        }

        // POST: api/genreapi
        [HttpPost]
        public async Task<ActionResult<Genre>> AddGenre(Genre genre)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (_context.Genres.Any(g => g.GenreId == genre.GenreId))
                return Conflict(new { message = "Bu ID zaten kayıtlı." });

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGenre), new { id = genre.GenreId }, genre);
        }

        // PUT: api/genreapi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(string id, Genre updatedGenre)
        {
            if (id != updatedGenre.GenreId) return BadRequest("ID uyuşmuyor");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_context.Genres.Any(g => g.GenreId == id))
                return NotFound(new { message = "Tür bulunamadı." });

            _context.Entry(updatedGenre).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/genreapi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(string id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return NotFound();

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
