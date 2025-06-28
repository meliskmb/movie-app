using MovieProject.Models;
using MovieProject.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MovieProject.Tests.UnitTests.Models
{
    public class AdminModelValidationTests
    {
        private IList<ValidationResult> ValidateModel(Admin model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, true);
            return results;
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Admin_WithValidData_PassesValidation()
        {
            var admin = new Admin
            {
                Username = "testuser",
                PasswordHash = "hashedpassword"
            };

            var results = ValidateModel(admin);

            Assert.Empty(results);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Admin_WithEmptyUsername_FailsValidation()
        {
            var admin = new Admin
            {
                Username = "",
                PasswordHash = "hashedpassword"
            };

            var results = ValidateModel(admin);

            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }
        [Trait("Category", "Unit")]
        [Fact]
        public void Admin_WithTooLongUsername_FailsValidation()
        {
            var admin = new Admin
            {
                Username = new string('x', 51), // 51 karakter
                PasswordHash = "hashedpassword"
            };

            var results = ValidateModel(admin);

            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void Admin_WithEmptyPasswordHash_FailsValidation()
        {
            var admin = new Admin
            {
                Username = "testuser",
                PasswordHash = ""
            };

            var results = ValidateModel(admin);

            Assert.Contains(results, r => r.MemberNames.Contains("PasswordHash"));
        }
    }
}
