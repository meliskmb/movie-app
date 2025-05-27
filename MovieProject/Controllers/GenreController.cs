using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieProject.Models;
using MovieProject.ViewModels;

namespace MovieProject.Controllers
{
    [Authorize]
    public class GenreController : Controller
    {
        private readonly MovieContext _context;
        public GenreController(MovieContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new GenreListViewModel
            {
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(GenreListViewModel vm)
        {
            vm.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            if (!ModelState.IsValid)
                return View(vm);

            if (await _context.Genres.AnyAsync(g => g.GenreId == vm.NewGenreId))
            {
                ModelState.AddModelError(nameof(vm.NewGenreId), "Bu kod zaten kullanılıyor.");
                return View(vm);
            }

            _context.Genres.Add(new Genre { GenreId = vm.NewGenreId, Name = vm.NewGenreName });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
