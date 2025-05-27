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

        // Yapýcý metod, baðýmlýlýk enjeksiyonu yoluyla bir MovieContext örneðini kabul eder.
        public HomeController(MovieContext ctx)
        {
            Context = ctx;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index(string searchString, string genre)
        {
            // Tür listesini çekiyoruz
            var genreQuery = Context.Movies
                                 .Select(m => m.Genre.Name)
                                 .Distinct()
                                 .OrderBy(x => x);

            // Baþlangýçta tüm filmler
            var moviesQuery = Context.Movies
                                 .Include(m => m.Genre)
                                 .AsQueryable();

            // Ýsim filtreleme (prefix her kelime baþýna bakar)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                // SQL LIKE için:
                //   1) Baþlangýca bakan: "s%"
                //   2) Herhangi bir boþluktan sonraki kelimeye bakan: "% s%"
                var prefix = searchString + "%";
                var wordPrefix = "% " + searchString + "%";

                moviesQuery = moviesQuery.Where(m =>
                    EF.Functions.Like(m.Name, prefix)      // Ýlk kelime
                 || EF.Functions.Like(m.Name, wordPrefix) // Ýkinci, üçüncü… kelime
                );
            }

            // Tür filtreleme
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
