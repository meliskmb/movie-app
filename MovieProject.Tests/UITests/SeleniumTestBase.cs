using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;

namespace MovieProject.Tests.UITests
{
    public class SeleniumTestBase : IDisposable
    {
        protected IWebDriver Driver { get; private set; }
        protected WebDriverWait Wait { get; private set; }
        protected string BaseUrl { get; private set; }

        public SeleniumTestBase()
        {
            var options = new ChromeOptions();
            // options.AddArgument("--headless"); // Run in headless mode
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--allow-insecure-localhost");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);

            Driver = new ChromeDriver(options);
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
            BaseUrl = "https://localhost:7288"; // Update this with your application's URL
        }

        public void Dispose()
        {
            Driver?.Quit();
            Driver?.Dispose();
        }

        protected void WaitAndClick(By by)
        {
            Wait.Until(d => d.FindElement(by)).Click();
        }

        protected IWebElement WaitAndFindElement(By by)
        {
            return Wait.Until(d => d.FindElement(by));
        }

        protected void WaitForUrl(string partialUrl)
        {
            Wait.Until(d => d.Url.Contains(partialUrl));
        }

        protected bool IsElementPresent(By by)
        {
            try
            {
                Driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        protected void LoginAsTestUser()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Admin/Login");

            var username = WaitAndFindElement(By.Id("Username"));
            var password = WaitAndFindElement(By.Id("Password"));
            var loginBtn = WaitAndFindElement(By.CssSelector("button[type='submit']"));

            username.SendKeys("testuser");         // Senin test kullanıcın neyse
            password.SendKeys("Test123");          // Şifresi neyse
            loginBtn.Click();

            WaitForUrl("/home/index");             // Giriş sonrası yönlendiği sayfa
        }

        // Yardımcı fonksiyon: Alanı temizle ve boş bırak
        protected void ClearAndEmpty(By selector)
        {
            var input = WaitAndFindElement(selector);
            input.Clear(); // Clear() bazen yeterlidir
            input.SendKeys(Keys.Control + "a");
            input.SendKeys(Keys.Delete);
        }

    }
}