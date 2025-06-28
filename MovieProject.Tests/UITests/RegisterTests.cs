using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;

namespace MovieProject.Tests.UITests
{
    public class RegisterTests : SeleniumTestBase
    {
        [Fact]
        public void Register_WithValidCredentials_ShouldRedirectToLogin()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Admin/Register");

            string uniqueUsername = "test" + DateTime.Now.Ticks;

            WaitAndFindElement(By.Id("Username")).SendKeys(uniqueUsername);
            WaitAndFindElement(By.Id("Password")).SendKeys("Test123");
            WaitAndFindElement(By.Id("ConfirmPassword")).SendKeys("Test123");

            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            WaitForUrl("/admin/login");
            Assert.Contains("/admin/login", Driver.Url);
        }

        [Fact]
        public void Register_WithEmptyInputs_ShouldShowValidationMessages()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Admin/Register");

            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            Assert.True(IsElementPresent(By.CssSelector("[data-valmsg-for='Username']")));
            Assert.True(IsElementPresent(By.CssSelector("[data-valmsg-for='Password']")));
            Assert.True(IsElementPresent(By.CssSelector("[data-valmsg-for='ConfirmPassword']")));
        }

        [Fact]
        public void Register_WithMismatchedPasswords_ShouldShowError()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Admin/Register");

            string username = "test" + DateTime.Now.Ticks;

            WaitAndFindElement(By.Id("Username")).SendKeys(username);
            WaitAndFindElement(By.Id("Password")).SendKeys("Test123");
            WaitAndFindElement(By.Id("ConfirmPassword")).SendKeys("Wrong123");

            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            var errorMessages = Driver.FindElements(By.ClassName("text-danger"));
            Assert.Contains(errorMessages, e => e.Text.ToLower().Contains("şifre"));
        }


        [Fact]
        public void Register_WithDuplicateUsername_ShouldShowError()
        {
            string duplicateUsername = "duplicate" + DateTime.Now.Ticks;
            RegisterUser(duplicateUsername, "Test123");

            Driver.Navigate().GoToUrl($"{BaseUrl}/Admin/Register");

            WaitAndFindElement(By.Id("Username")).SendKeys(duplicateUsername);
            WaitAndFindElement(By.Id("Password")).SendKeys("Test123");
            WaitAndFindElement(By.Id("ConfirmPassword")).SendKeys("Test123");

            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            var errorMessages = Driver.FindElements(By.ClassName("text-danger"));
            Assert.Contains(errorMessages, e => e.Text.ToLower().Contains("kayıtlı"));
        }


        private void RegisterUser(string username, string password)
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Admin/Register");

            WaitAndFindElement(By.Id("Username")).SendKeys(username);
            WaitAndFindElement(By.Id("Password")).SendKeys(password);
            WaitAndFindElement(By.Id("ConfirmPassword")).SendKeys(password);

            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            WaitForUrl("/admin/login");
        }
    }
}
