using System.ComponentModel.DataAnnotations;

namespace MovieProject.Models
{
    public class Genre
    {
        [Required]
        [StringLength(1)]
        public string GenreId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Name { get; set; } = string.Empty;
    }
}
