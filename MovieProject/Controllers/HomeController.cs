using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using MovieProject.Models;
using MovieProject.ViewModels;
using System.Diagnostics;

namespace MovieProject.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private MovieContext Context { get; set; }

        // Yap�c� metod, ba��ml�l�k enjeksiyonu yoluyla bir MovieContext �rne�ini kabul eder.
        public HomeController(MovieContext ctx)
        {
            Context = ctx;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index(string searchString, string genre)
        {
            // T�r listesini �ekiyoruz
            var genreQuery = Context.Movies
                                 .Select(m => m.Genre.Name)
                                 .Distinct()
                                 .OrderBy(x => x);

            // Ba�lang��ta t�m filmler
            var moviesQuery = Context.Movies
                                 .Include(m => m.Genre)
                                 .AsQueryable();

            // �sim filtreleme (prefix her kelime ba��na bakar)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                // SQL LIKE i�in:
                //   1) Ba�lang�ca bakan: "s%"
                //   2) Herhangi bir bo�luktan sonraki kelimeye bakan: "% s%"
                var prefix = searchString + "%";
                var wordPrefix = "% " + searchString + "%";

                moviesQuery = moviesQuery.Where(m =>
                    EF.Functions.Like(m.Name, prefix)      // �lk kelime
                 || EF.Functions.Like(m.Name, wordPrefix) // �kinci, ���nc�� kelime
                );
            }

            // T�r filtreleme
            if (!string.IsNullOrWhiteSpace(genre))
            {
                moviesQuery = moviesQuery
                    .Where(m => m.Genre.Name == genre);
            }

            var vm = new MovieFilterViewModel
            {
                GenreList = new SelectList(await genreQuery.ToListAsync()),
                Movies = await moviesQuery.ToListAsync(),
                SearchString = searchString,
                SelectedGenre = genre
            };

            return View(vm);
        }

    }
}
