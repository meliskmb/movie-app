using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieProject.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core Baðýmlýlýk Enjeksiyonunu (Dependency Injection) ekle
builder.Services.AddDbContext<MovieContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MovieContext")));

// Password hasher for Admin
builder.Services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();

builder.Services
  .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
      options.LoginPath = "/Admin/Login";        // yetkisizse buraya yönlensin
      options.AccessDeniedPath = "/Admin/Login"; // eriþim reddedilirse de
      options.ExpireTimeSpan = TimeSpan.FromHours(1);
  });


builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = true;
});

builder.Services.AddHostedService<MovieProject.Services.TestRunnerHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Register}/{id?}/{slug?}");

app.Run();
