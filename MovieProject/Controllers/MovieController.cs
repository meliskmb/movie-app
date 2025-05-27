using Microsoft.AspNetCore.Mvc;
using MovieProject.Models;
using System.Linq;

namespace MovieProject.Controllers
{
    public class MovieController : Controller
    {
        private readonly MovieContext _context;
        public MovieController(MovieContext context)
        {
            _context = context;
        }

        // GET: /Movie/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Action = "Add New Movie";
            ViewBag.Genres = _context.Genres
                                     .OrderBy(g => g.Name)
                                     .ToList();
            // reuse the Edit.cshtml view for both create & edit
            return View("Edit", new Movie());
        }

        // POST: /Movie/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Movie movie)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Action = movie.MovieId == 0
                    ? "Add New Movie"
                    : "Edit Movie";
                ViewBag.Genres = _context.Genres
                                         .OrderBy(g => g.Name)
                                         .ToList();
                return View("Edit", movie);
            }

            if (movie.MovieId == 0)
                _context.Movies.Add(movie);
            else
                _context.Movies.Update(movie);

            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Movie/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null) return NotFound();

            ViewBag.Action = "Edit Movie";
            ViewBag.Genres = _context.Genres
                                     .OrderBy(g => g.Name)
                                     .ToList();
            return View(movie);
        }

        // GET: /Movie/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        // POST: /Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
