using MovieProject.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MovieProject.Tests.UnitTests.Models
{
    public class MovieModelValidationTests
    {
        private bool ValidateModel(Movie movie, out List<ValidationResult> results)
        {
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(movie, new ValidationContext(movie), results, true);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Movie_WithValidData_PassesValidation()
        {
            var movie = new Movie
            {
                Name = "Test Movie",
                Year = 2022,
                Rating = 8,
                GenreId = "A"
            };

            var isValid = ValidateModel(movie, out var results);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Movie_WithMissingName_FailsValidation()
        {
            var movie = new Movie
            {
                Name = "",
                Year = 2022,
                Rating = 8,
                GenreId = "A"
            };

            var isValid = ValidateModel(movie, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Movie_WithYearOutOfRange_FailsValidation()
        {
            var movie = new Movie
            {
                Name = "Old Movie",
                Year = 1800,  // Geçersiz yıl
                Rating = 7,
                GenreId = "A"
            };

            var isValid = ValidateModel(movie, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Year"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Movie_WithNullYear_FailsValidation()
        {
            var movie = new Movie
            {
                Name = "Incomplete",
                Year = null,
                Rating = 5,
                GenreId = "A"
            };

            var isValid = ValidateModel(movie, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Year"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Movie_WithInvalidRating_FailsValidation()
        {
            var movie = new Movie
            {
                Name = "Invalid Rating",
                Year = 2020,
                Rating = -1,  // Geçersiz
                GenreId = "A"
            };

            var isValid = ValidateModel(movie, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Rating"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Movie_WithNullRating_FailsValidation()
        {
            var movie = new Movie
            {
                Name = "No Rating",
                Year = 2020,
                Rating = null,
                GenreId = "A"
            };

            var isValid = ValidateModel(movie, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Rating"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Movie_WithEmptyGenreId_FailsValidation()
        {
            var movie = new Movie
            {
                Name = "No Genre",
                Year = 2022,
                Rating = 6,
                GenreId = ""
            };

            var isValid = ValidateModel(movie, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("GenreId"));
        }
    }
}
