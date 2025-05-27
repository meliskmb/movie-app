using Microsoft.EntityFrameworkCore;
namespace MovieProject.Models
{
    public class MovieContext : DbContext
    {
        // DbSet<Movie>, tüm Movie nesnelerinin koleksiyonunu temsil eden bir özelliktir.
        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; }
        public MovieContext(DbContextOptions<MovieContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data
            modelBuilder.Entity<Genre>().HasData(
                new Genre() { GenreId = "D", Name = "Drama" },
                new Genre() { GenreId = "C", Name = "Comedy" },
                new Genre() { GenreId = "A", Name = "Action" },
                new Genre() { GenreId = "H", Name = "Horror" },
                new Genre() { GenreId = "M", Name = "Musical" },
                new Genre() { GenreId = "R", Name = "RomCom" },
                new Genre() { GenreId = "S", Name = "SciFi" }
             );

            modelBuilder.Entity<Movie>().HasData(
                new Movie { MovieId = 1, Name = "Inception", Year = 2010, Rating = 8, GenreId = "D" },
                new Movie { MovieId = 2, Name = "The Matrix", Year = 1999, Rating = 9, GenreId = "D" },
                new Movie { MovieId = 3, Name = "Interstellar", Year = 2014, Rating = 8 , GenreId = "A" }
            );
        }
    }
}
