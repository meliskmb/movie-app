using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MovieProject.ViewModels;
using Xunit;

namespace MovieProject.Tests.UnitTests.Models
{
    public class LoginModelValidationTests
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void LoginViewModel_WithValidData_PassesValidation()
        {
            var model = new LoginViewModel
            {
                Username = "testuser",
                Password = "Test123"
            };

            var isValid = ValidateModel(model, out var results);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void LoginViewModel_WithEmptyFields_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Username = "",
                Password = ""
            };

            var isValid = ValidateModel(model, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void LoginViewModel_WithTooLongUsername_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Username = new string('a', 51),
                Password = "Test123"
            };

            var isValid = ValidateModel(model, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void LoginViewModel_WithWhitespacePassword_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Username = "validuser",
                Password = "   " // sadece boşluk
            };

            var isValid = ValidateModel(model, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void LoginViewModel_WithWhitespaceUsername_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Username = "   ", // sadece boşluk
                Password = "Test123"
            };

            var isValid = ValidateModel(model, out var results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

    }
}
