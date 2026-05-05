using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookStoreWeb.Data;
using BookStoreWeb.Models;
using System.Text;
using System.Security.Cryptography;

namespace BookStoreWeb.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TaiKhoan/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /TaiKhoan/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string matKhau, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                ModelState.AddModelError("", "Vui lòng nhập email và mật khẩu");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            // Tìm người dùng trong database
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == email && u.ConHoatDong);

            if (nguoiDung == null || !VerifyPassword(matKhau, nguoiDung.MatKhau))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            // Tạo Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                new Claim(ClaimTypes.Email, nguoiDung.Email),
                new Claim(ClaimTypes.Role, nguoiDung.VaiTro)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Chuyển hướng sau đăng nhập
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Chuyển hướng theo vai trò
            if (nguoiDung.VaiTro == "Admin")
            {
                return RedirectToAction("Index", "SanPhams");
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoan/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /TaiKhoan/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("HoTen,Email,MatKhau,SoDienThoai,DiaChi")] NguoiDung nguoiDung, string xacNhanMatKhau)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.Email == nguoiDung.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký");
                    return View(nguoiDung);
                }

                // Kiểm tra xác nhận mật khẩu
                if (nguoiDung.MatKhau != xacNhanMatKhau)
                {
                    ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                    return View(nguoiDung);
                }

                // Mã hóa mật khẩu
                nguoiDung.MatKhau = HashPassword(nguoiDung.MatKhau);

                // Gán vai trò mặc định là User
                nguoiDung.VaiTro = "User";
                nguoiDung.NgayTao = DateTime.Now;
                nguoiDung.ConHoatDong = true;

                _context.Add(nguoiDung);
                await _context.SaveChangesAsync();

                // Tự động đăng nhập sau khi đăng ký
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
                    new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                    new Claim(ClaimTypes.Email, nguoiDung.Email),
                    new Claim(ClaimTypes.Role, nguoiDung.VaiTro)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                TempData["SuccessMessage"] = "Đăng ký thành công! Chào mừng bạn.";
                return RedirectToAction("Index", "Home");
            }

            return View(nguoiDung);
        }

        // POST: /TaiKhoan/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoan/Index (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var nguoiDungs = await _context.NguoiDungs
                .OrderByDescending(u => u.NgayTao)
                .ToListAsync();
            return View(nguoiDungs);
        }

        // GET: /TaiKhoan/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper: Hash mật khẩu (SHA256)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Helper: Verify mật khẩu
        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            return HashPassword(inputPassword) == hashedPassword;
        }
    }
}
