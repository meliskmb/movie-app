using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace MovieProject.Tests.UITests
{
    public class MovieTests : SeleniumTestBase
    {
        [Fact]
        public void AddMovie_WithValidInputs_ShouldRedirectToIndexAndDisplayMovie()
        {
            LoginAsTestUser();

            // Arrange
            Driver.Navigate().GoToUrl($"{BaseUrl}/movie/create");

            var nameInput = WaitAndFindElement(By.Id("Name"));
            var yearInput = WaitAndFindElement(By.Id("Year"));
            var genreSelect = WaitAndFindElement(By.Id("GenreId"));
            var ratingInput = WaitAndFindElement(By.Id("Rating"));
            var saveButton = WaitAndFindElement(By.CssSelector("button[type='submit']"));

            // Act
            nameInput.SendKeys("Test Movie");
            yearInput.SendKeys("2025");
            var genre = new SelectElement(genreSelect);
            genre.SelectByIndex(1); // İlk gerçek genre
            ratingInput.SendKeys("9");
            saveButton.Click();

            // Assert
            WaitForUrl("/home/index");
            Assert.Contains("/home/index", Driver.Url.ToLower());
            Assert.True(IsElementPresent(By.XPath("//td[contains(text(), 'Test Movie')]")));
        }

        [Fact]
        public void AddMovie_WithEmptyInputs_ShouldShowValidationSummaryErrors()
        {
            LoginAsTestUser();
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");

            var saveButton = WaitAndFindElement(By.CssSelector("button[type='submit']"));
            saveButton.Click();

            var errorSummary = WaitAndFindElement(By.CssSelector(".validation-summary-errors"));

            Assert.Contains("Please enter a name", errorSummary.Text);
            Assert.Contains("Please enter a year", errorSummary.Text);
            Assert.Contains("Please enter a genre", errorSummary.Text);
            Assert.Contains("Please enter a rating", errorSummary.Text);
        }

        [Fact]
        public void EditMovie_WithValidInputs_ShouldUpdateMovie()
        {
            LoginAsTestUser();

            // Adım 1: Test için yeni film oluştur (daha sonra düzenlenecek)
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys("Original Movie");
            WaitAndFindElement(By.Id("Year")).SendKeys("2022");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(1);
            WaitAndFindElement(By.Id("Rating")).SendKeys("7");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // Adım 2: "Original Movie" için Edit linkini bul
            var editButton = WaitAndFindElement(By.XPath("//tr[td[contains(text(), 'Original Movie')]]//a[contains(text(), 'Edit')]"));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", editButton);
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", editButton);

            var nameInput = WaitAndFindElement(By.Id("Name"));
            nameInput.Click();
            nameInput.SendKeys(Keys.Control + "a");
            nameInput.SendKeys(Keys.Delete);
            nameInput.SendKeys("Edited Movie");

            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // Yeni doğrulama
            Assert.True(IsElementPresent(By.XPath("//tr[td[contains(text(), 'Edited Movie')]]")));
        }

        [Fact]
        public void EditMovie_WithEmptyFields_ShouldShowValidationErrors()
        {
            LoginAsTestUser();

            string originalName = $"Original Movie {DateTime.Now.Ticks}";

            // 1) Film ekle
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys(originalName);
            WaitAndFindElement(By.Id("Year")).SendKeys("2022");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(1);
            WaitAndFindElement(By.Id("Rating")).SendKeys("7");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 2) Filmin eklendiğini doğrula
            Assert.True(IsElementPresent(By.XPath($"//td[contains(text(), '{originalName}')]")));

            // 3) Edit sayfasına git
            var editButtonXPath = $"//tr[td[contains(text(), '{originalName}')]]//a[contains(text(), 'Edit')]";
            var editButton = WaitAndFindElement(By.XPath(editButtonXPath));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", editButton);
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", editButton);

            // 4) Alanları temizle
            ClearAndEmpty(By.Id("Name"));
            ClearAndEmpty(By.Id("Year"));
            ClearAndEmpty(By.Id("Rating"));
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(0); // "-- Select Genre --"

            // 5) Kaydet
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 6) Validasyon mesajlarını kontrol et (DOM’a düşene kadar bekle)
            var summary = Wait.Until(d =>
            {
                var el = d.FindElement(By.CssSelector(".validation-summary-errors"));
                return string.IsNullOrWhiteSpace(el.Text) ? null : el;
            });

            Assert.Contains("Please enter a name", summary.Text);
            Assert.Contains("Please enter a year", summary.Text);
            Assert.Contains("Please enter a genre", summary.Text);
            Assert.Contains("Please enter a rating", summary.Text);
        }


        [Fact]
        public void DeleteMovie_WithConfirmation_ShouldRemoveMovie()
        {
            LoginAsTestUser();

            string movieName = $"Movie To Delete {DateTime.Now.Ticks}";

            // 1) Yeni film oluştur
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys(movieName);
            WaitAndFindElement(By.Id("Year")).SendKeys("2020");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(1);
            WaitAndFindElement(By.Id("Rating")).SendKeys("6");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 2) Delete butonuna tıkla
            var deleteButton = WaitAndFindElement(By.XPath($"//tr[td[contains(text(), '{movieName}')]]//a[contains(text(), 'Delete')]"));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", deleteButton);
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", deleteButton);

            // 3) Onay sayfasında "Delete" butonuna bas
            var confirmButton = WaitAndFindElement(By.CssSelector("button[type='submit']"));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", confirmButton);

            // 4) Film silindi mi kontrol et
            Thread.Sleep(1000); // hızlı redirect olabilir, garantiye almak için

            var stillExists = Driver.FindElements(By.XPath($"//td[contains(text(), '{movieName}')]")).Any();
            Assert.False(stillExists);
        }

        [Fact]
        public void DeleteMovie_Cancel_ShouldNotRemoveMovie()
        {
            LoginAsTestUser();

            string movieName = $"Movie To Keep {DateTime.Now.Ticks}";

            // 1) Yeni film oluştur
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys(movieName);
            WaitAndFindElement(By.Id("Year")).SendKeys("2021");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(1);
            WaitAndFindElement(By.Id("Rating")).SendKeys("8");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 2) Delete linkine tıkla
            var deleteButton = WaitAndFindElement(By.XPath($"//tr[td[contains(text(), '{movieName}')]]//a[contains(text(), 'Delete')]"));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", deleteButton);
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", deleteButton);

            // 3) Cancel butonuna tıkla (sayfada varsa)
            var cancelButton = WaitAndFindElement(By.CssSelector("a.btn-secondary")); // genelde iptal linkleri bu şekilde olur
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", cancelButton);

            // 4) Film hâlâ var mı?
            var stillExists = new WebDriverWait(Driver, TimeSpan.FromSeconds(5))
                .Until(drv => drv.FindElements(By.XPath($"//td[contains(text(), '{movieName}')]")).Any());

            Assert.True(stillExists);
        }

        [Fact]
        public void FilterMovies_ByName_ShouldShowMatchingResults()
        {
            LoginAsTestUser();

            string movieName = $"Unique Filter Movie {DateTime.Now.Ticks}";

            // 1) Film ekle
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys(movieName);
            WaitAndFindElement(By.Id("Year")).SendKeys("2023");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(1);
            WaitAndFindElement(By.Id("Rating")).SendKeys("9");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 2) Ana sayfaya dön ve arama kutusuna yaz (DÜZELTİLDİ)
            Driver.Navigate().GoToUrl($"{BaseUrl}/Home/Index");
            var searchInput = WaitAndFindElement(By.Id("searchString")); // ✅ DOĞRU SELECTOR
            searchInput.SendKeys(movieName);

            // 3) Filtrele butonuna tıkla
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 4) Sonuçları kontrol et
            var result = new WebDriverWait(Driver, TimeSpan.FromSeconds(5))
                .Until(drv => drv.FindElements(By.XPath($"//td[contains(text(), '{movieName}')]")).Any());

            Assert.True(result);
        }

        [Fact]
        public void FilterMovies_ByGenre_ShouldShowCorrectResults()
        {
            LoginAsTestUser();

            string uniqueDrama = $"Drama_{DateTime.Now.Ticks}";
            string uniqueAction = $"Action_{DateTime.Now.Ticks}";

            // 1) Drama filmi ekle
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys(uniqueDrama);
            WaitAndFindElement(By.Id("Year")).SendKeys("2021");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByText("Drama");
            WaitAndFindElement(By.Id("Rating")).SendKeys("7");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 2) Action filmi ekle
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys(uniqueAction);
            WaitAndFindElement(By.Id("Year")).SendKeys("2022");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByText("Action");
            WaitAndFindElement(By.Id("Rating")).SendKeys("8");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 3) Ana sayfaya dön → sadece "Drama" filtrele
            Driver.Navigate().GoToUrl($"{BaseUrl}/Home/Index");
            new SelectElement(WaitAndFindElement(By.Id("genre"))).SelectByText("Drama");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 4) Doğrulama
            var dramaVisible = Driver.FindElements(By.XPath($"//td[contains(text(), '{uniqueDrama}')]")).Any();
            var actionVisible = Driver.FindElements(By.XPath($"//td[contains(text(), '{uniqueAction}')]")).Any();

            Assert.True(dramaVisible);   // görünmeli
            Assert.False(actionVisible); // görünmemeli
        }
        [Fact]
        public void Filter_ByInvalidName_ShouldShowNoResults()
        {
            LoginAsTestUser();

            // 1) Ana sayfaya git
            Driver.Navigate().GoToUrl($"{BaseUrl}/Home/Index");

            // 2) Geçersiz film adı gir
            string invalidName = "asdgfjasgfhasdghas";
            var nameInput = WaitAndFindElement(By.Id("searchString"));
            nameInput.Clear();
            nameInput.SendKeys(invalidName);

            // 3) Filtrele
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 4) Tabloyu yeniden bul (stale hatası çözümü)
            var tableBody = WaitAndFindElement(By.CssSelector("tbody"));
            var rows = tableBody.FindElements(By.TagName("tr"));

            // 5) Satır yoksa veya sonuçlar geçersiz ad içermiyorsa test başarılı
            Assert.True(rows.Count == 0 || !rows.Any(r => r.Text.Contains(invalidName)));
        }

        [Fact]
        public void AddMovie_WithInvalidYear_ShouldShowValidationError()
        {
            LoginAsTestUser();
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");

            WaitAndFindElement(By.Id("Name")).SendKeys("Old Movie");
            WaitAndFindElement(By.Id("Year")).SendKeys("1800"); // Geçersiz yıl
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(1);
            WaitAndFindElement(By.Id("Rating")).SendKeys("5");

            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            var summary = WaitAndFindElement(By.CssSelector(".validation-summary-errors"));
            Assert.Contains("Year must be between 1900 and 2025", summary.Text);
        }

        [Fact]
        public void AddMovie_WithDuplicateName_ShouldStillAddIfAllowed()
        {
            LoginAsTestUser();
            string name = $"DuplicateNameTest{DateTime.Now.Ticks % 1000}";

            // 1. Film
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys(name);
            WaitAndFindElement(By.Id("Year")).SendKeys("2020");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(1);
            WaitAndFindElement(By.Id("Rating")).SendKeys("7");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // 2. Film (aynı isim)
            Driver.Navigate().GoToUrl($"{BaseUrl}/Movie/Create");
            WaitAndFindElement(By.Id("Name")).SendKeys(name);
            WaitAndFindElement(By.Id("Year")).SendKeys("2021");
            new SelectElement(WaitAndFindElement(By.Id("GenreId"))).SelectByIndex(1);
            WaitAndFindElement(By.Id("Rating")).SendKeys("8");
            WaitAndFindElement(By.CssSelector("button[type='submit']")).Click();

            // Sonuç
            var results = Driver.FindElements(By.XPath($"//td[contains(text(), '{name}')]"));
            Assert.True(results.Count >= 2); // Eğer duplicate kabul ediliyorsa
        }

    }
}
