// ViewModels/MovieFilterViewModel.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieProject.Models;

namespace MovieProject.ViewModels
{
    public class MovieFilterViewModel
    {
        public SelectList GenreList { get; set; } = default!;
        public List<Movie> Movies { get; set; } = new();
        public string SearchString { get; set; } = "";
        public string SelectedGenre { get; set; } = "";
    }
}
