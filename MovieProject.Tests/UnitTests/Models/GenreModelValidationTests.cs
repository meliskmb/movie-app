using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MovieProject.Models;
using Xunit;

namespace MovieProject.Tests.UnitTests.Models
{
    public class GenreModelValidationTests
    {
        [Trait("Category", "Unit")]
        [Fact]
        public void Genre_WithValidData_PassesValidation()
        {
            var genre = new Genre
            {
                GenreId = "A",
                Name = "Action"
            };

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(genre, new ValidationContext(genre), validationResults, true);

            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Genre_WithEmptyGenreId_FailsValidation()
        {
            var genre = new Genre
            {
                GenreId = "",
                Name = "Action"
            };

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(genre, new ValidationContext(genre), validationResults, true);

            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("GenreId"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Genre_WithEmptyName_FailsValidation()
        {
            var genre = new Genre
            {
                GenreId = "A",
                Name = ""
            };

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(genre, new ValidationContext(genre), validationResults, true);

            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("Name"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Genre_WithTooLongName_FailsValidation()
        {
            var genre = new Genre
            {
                GenreId = "A",
                Name = new string('X', 21)
            };

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(genre, new ValidationContext(genre), validationResults, true);

            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("Name"));
        }
    }
}
