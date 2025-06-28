using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;

namespace MovieProject.Tests.UITests
{
    public class GenreTests : SeleniumTestBase
    {
        [Fact]
        public void AddGenre_WithValidInputs_ShouldSucceed()
        {
            LoginAsTestUser();

            // Türler sayfasına git
            Driver.Navigate().GoToUrl($"{BaseUrl}/genre/index");

            // WebDriverWait ile Code input'unun görünmesini bekle
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.Id("Code"));
                    return element.Displayed && element.Enabled;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });

            // Eklenecek tür
            string code = $"X{DateTime.Now.Ticks % 1000}";
            string name = $"TestGenre{DateTime.Now.Ticks % 1000}";

            // Formu doldur
            Driver.FindElement(By.Id("Code")).SendKeys(code);
            Driver.FindElement(By.Id("Name")).SendKeys(name);
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Yeni türün tabloda yer alıp almadığını kontrol et
            var newRow = WaitAndFindElement(By.XPath($"//tr[td[text()='{code}'] and td[text()='{name}']]"));
            Assert.NotNull(newRow);
        }



        [Fact]
        public void AddGenre_WithEmptyInputs_ShouldShowValidationError()
        {
            LoginAsTestUser();

            // Türler sayfasına git
            Driver.Navigate().GoToUrl($"{BaseUrl}/genre/index");

            // Inputları boş bırak ve formu gönder
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // Hata mesajları olup olmadığını kontrol et
            var errors = Driver.FindElements(By.ClassName("text-danger"));
            Assert.Contains(errors, e => e.Displayed);
        }

        [Fact]
        public void AddGenre_WithDuplicateCode_ShouldShowModelError()
        {
            LoginAsTestUser();
            Driver.Navigate().GoToUrl($"{BaseUrl}/genre/index");

            string code = $"X{DateTime.Now.Ticks % 1000}";
            string name1 = "GenreOne";
            string name2 = "GenreTwo";

            // İlk giriş
            WaitAndFindElement(By.Id("Code")).SendKeys(code);
            WaitAndFindElement(By.Id("Name")).SendKeys(name1);
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // Sayfa yeniden yüklendiği için alanları taze bul ve doldur
            WaitAndFindElement(By.Id("Code")).Clear();
            WaitAndFindElement(By.Id("Code")).SendKeys(code);
            WaitAndFindElement(By.Id("Name")).Clear();
            WaitAndFindElement(By.Id("Name")).SendKeys(name2);
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // Alan bazlı hata kontrolü
            var codeError = WaitAndFindElement(By.CssSelector("span[data-valmsg-for='NewGenreId']"));
            Assert.True(codeError.Displayed);
            Assert.Contains("Bu kod zaten kullanılıyor", codeError.Text);
        }



        [Fact]
        public void AddGenre_WithWhitespaceInputs_ShouldShowValidationError()
        {
            LoginAsTestUser();
            Driver.Navigate().GoToUrl($"{BaseUrl}/genre/index");

            WaitAndFindElement(By.Id("Code")).SendKeys("   ");
            WaitAndFindElement(By.Id("Name")).SendKeys("   ");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // Alan bazlı hata kontrolü
            Assert.True(IsElementPresent(By.CssSelector("span[data-valmsg-for='NewGenreId']")));
            Assert.True(IsElementPresent(By.CssSelector("span[data-valmsg-for='NewGenreName']")));
        }



    }
}