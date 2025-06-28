using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace MovieProject.Tests.UITests
{
    public class LoginTests : SeleniumTestBase
    {
        [Fact]
        public void Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            Driver.Navigate().GoToUrl($"{BaseUrl}/admin/login");

            // Act
            var usernameInput = WaitAndFindElement(By.Id("Username"));
            var passwordInput = WaitAndFindElement(By.Id("Password"));
            var loginButton = WaitAndFindElement(By.CssSelector("button[type='submit']"));

            usernameInput.SendKeys("testuser");
            passwordInput.SendKeys("Test123");
            loginButton.Click();

            // Assert
            WaitForUrl("/home/index");
            Assert.Contains("/home/index", Driver.Url);
            Assert.True(IsElementPresent(By.LinkText("Çıkış Yap")));
        }

        [Fact]
        public void Login_WithInvalidCredentials_ShouldShowError()
        {
            // Arrange
            Driver.Navigate().GoToUrl($"{BaseUrl}/admin/login");

            // Act
            var usernameInput = WaitAndFindElement(By.Id("Username"));
            var passwordInput = WaitAndFindElement(By.Id("Password"));
            var loginButton = WaitAndFindElement(By.CssSelector("button[type='submit']"));

            usernameInput.SendKeys("invaliduser");
            passwordInput.SendKeys("WrongPassword123!");
            loginButton.Click();

            // Assert
            var errorMessage = WaitAndFindElement(By.CssSelector(".validation-summary-errors"));
            Assert.Contains("Kullanıcı adı veya şifre hatalı", errorMessage.Text);
        }

        [Fact]
        public void Login_WithEmptyCredentials_ShouldShowValidationErrors()
        {
            // Arrange
            Driver.Navigate().GoToUrl($"{BaseUrl}/admin/login");

            // Act
            var loginButton = WaitAndFindElement(By.CssSelector("button[type='submit']"));
            loginButton.Click();

            // Assert
            Assert.True(IsElementPresent(By.CssSelector("[data-valmsg-for='Username']")));
            Assert.True(IsElementPresent(By.CssSelector("[data-valmsg-for='Password']")));
        }

        [Fact]
        public void Logout_ShouldRedirectToLoginPage()
        {
            // 1. Test kullanıcısı ile giriş yap
            LoginAsTestUser();

            // 2. Çıkış yap butonuna tıkla
            var logoutLink = WaitAndFindElement(By.LinkText("Çıkış Yap"));
            logoutLink.Click();

            // 3. Giriş sayfasına yönlendirildiğini doğrula
            Assert.Contains("/admin/login", Driver.Url);
        }

        [Fact]
        public void Login_WithSqlInjectionAttempt_ShouldFail()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/admin/login");

            var usernameInput = WaitAndFindElement(By.Id("Username"));
            var passwordInput = WaitAndFindElement(By.Id("Password"));
            var loginButton = WaitAndFindElement(By.CssSelector("button[type='submit']"));

            usernameInput.SendKeys("' OR '1'='1");
            passwordInput.SendKeys("anything");
            loginButton.Click();

            var errorMessage = WaitAndFindElement(By.CssSelector(".validation-summary-errors"));
            Assert.Contains("Kullanıcı adı veya şifre hatalı", errorMessage.Text);
        }

        [Fact]
        public void Login_FieldConstraints_ShouldLimitInputLength()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/admin/login");

            string longInput = new string('a', 200);
            var usernameInput = WaitAndFindElement(By.Id("Username"));
            var passwordInput = WaitAndFindElement(By.Id("Password"));

            usernameInput.SendKeys(longInput);
            passwordInput.SendKeys(longInput);

            var loginButton = WaitAndFindElement(By.CssSelector("button[type='submit']"));
            loginButton.Click();

            var errorMessage = WaitAndFindElement(By.CssSelector(".validation-summary-errors"));
            Assert.Contains("Kullanıcı adı veya şifre hatalı", errorMessage.Text);
        }



    }
}
