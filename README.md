# MovieProject

.NET 8 tabanlı bir film yönetim uygulamasıdır. Kullanıcılar kimlik doğrulaması ile giriş yapabilir, film ve tür işlemlerini gerçekleştirebilir, filtreleme yapabilirler. Uygulama ayrıca RESTful API uç noktaları ve geniş kapsamlı testlerle desteklenmektedir.


##  Özellikler

- ASP.NET Core MVC mimarisi
- Entity Framework Core ile veri yönetimi
- Login / Register (Admin) işlemleri
- Film ve Tür CRUD işlemleri
- Arama ve tür filtreleme (UI ve API üzerinden)
- API uç noktaları: `AdminApi`, `MovieApi`, `GenreApi`, `FilterApi`
- xUnit ile Unit testler
- Selenium ile UI testleri
- Postman ile API testleri


## Kullanılan Teknolojiler

| Alan              | Teknoloji / Araç                         |
|-------------------|------------------------------------------|
| Backend           | ASP.NET Core MVC (net8.0)                |
| ORM               | Entity Framework Core                    |
| Veritabanı        | SQL Server / InMemory (test için)        |
| Unit Test         | xUnit, Moq, coverlet                     |
| UI Test           | Selenium WebDriver + ChromeDriver        |
| API Test          | Postman                                  |
| Paket Yönetimi    | NuGet                                    |


## Proje Yapısı
```css
MovieSolution/
│
├── MovieProject/ # MVC Web Uygulaması
│ ├── Controllers/
│ ├── Models/
│ ├── Views/
│ └── ViewModels/
│
├── MovieProject.Tests/ # Test Projesi
│ ├── UnitTests/
│ ├── UITests/
│ ├── Models/
│ └── ApiControllerTests/
│
└── MovieProject.postman_collection.json # Postman API test koleksiyonu
```

## Testler

### Unit Testler
- `xUnit` + `Moq` + `EFCore.InMemory`
- Controller, ViewModel ve Model validasyon testleri içerir.
- `dotnet test` komutu ile çalıştırılır.

### UI Testler
- Selenium WebDriver (Chrome)
- `Login`, `Register`, `Genre`, `Movie` sayfalarında pozitif ve negatif senaryolar test edilir.

### API Testleri
- Postman ile `AdminApiController`, `MovieApiController`, `GenreApiController`, `FilterApiController` uç noktaları test edilir.
- Başarılı ve başarısız istek senaryoları mevcuttur.


## Uygulama Nasıl Çalıştırılır?

1. **Veritabanını oluşturun:**

 ```bash
 dotnet ef database update
```

2. **Projeyi Çalıştırın:**

```bash
dotnet run --project MovieProject
```

3. **Unit testleri manuel çalıştırmak için:**

```bash
dotnet test
```

## Test Kullanıcı Bilgisi

```text
Kullanıcı Adı: testuser
Şifre: Test123
```
Bu kullanıcı testlerde ve elle denemelerde kullanılabilir.

## Lisans
Bu proje kişisel/akademik amaçlı geliştirilmiştir. Açık kaynak olarak kullanılabilir.