using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using BookStoreWeb.Data;
using BookStoreWeb.Services.Momo;
using BookStoreWeb.Services.PayOS;
using dotenv.net;

// Load environment variables from .env file
DotEnv.Load();

// Build configuration with environment variables
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/Login";
        options.LogoutPath = "/TaiKhoan/Logout";
        options.AccessDeniedPath = "/Home/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Add Momo Payment Service
builder.Services.AddHttpClient<IMomoService, MomoService>();

// Add PayOS Payment Service
builder.Services.AddHttpClient<IPayOSService, PayOSService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Admin account if not exists
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Check if admin exists
    var adminExists = context.NguoiDungs.Any(u => u.VaiTro == "Admin");
    if (!adminExists)
    {
        // Create default admin
        var admin = new BookStoreWeb.Models.NguoiDung
        {
            HoTen = "Administrator",
            Email = "admin@bookstore.com",
            MatKhau = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("admin123"))),
            VaiTro = "Admin",
            SoDienThoai = "0901234567",
            DiaChi = "Hà Nội, Việt Nam",
            NgayTao = DateTime.Now,
            ConHoatDong = true
        };
        context.NguoiDungs.Add(admin);
        context.SaveChanges();
    }
}

app.Run();
