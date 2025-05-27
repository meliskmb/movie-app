using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MovieProject.Models;

namespace MovieProject.ViewModels
{
    public class GenreListViewModel
    {
        // Mevcut tüm türleri buraya yüklüyoruz
        public List<Genre> Genres { get; set; } = new List<Genre>();

        // Formdan gelecek yeni türün ID'si
        [Required, StringLength(50)]
        [Display(Name = "Tür Kodu")]
        public string NewGenreId { get; set; } = "";

        // Formdan gelecek yeni türün adı
        [Required, StringLength(100)]
        [Display(Name = "Tür Adı")]
        public string NewGenreName { get; set; } = "";
    }
}
