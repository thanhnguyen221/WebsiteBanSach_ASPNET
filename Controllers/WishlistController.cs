using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookStoreWeb.Data;
using BookStoreWeb.Models;

namespace BookStoreWeb.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Wishlist/Index - Hiển thị danh sách yêu thích
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            var maNguoiDung = int.Parse(userId);

            var wishlist = await _context.SanPhamYeuThichs
                .Where(w => w.MaNguoiDung == maNguoiDung)
                .Include(w => w.SanPham)
                .OrderByDescending(w => w.NgayThem)
                .ToListAsync();

            return View(wishlist);
        }

        // POST: Wishlist/ThemVaoYeuThich
        [HttpPost]
        public async Task<IActionResult> ThemVaoYeuThich(int maSanPham)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var maNguoiDung = int.Parse(userId);

            // Kiểm tra sản phẩm tồn tại
            var sanPham = await _context.SanPhams.FindAsync(maSanPham);
            if (sanPham == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            // Kiểm tra đã yêu thích chưa
            var existing = await _context.SanPhamYeuThichs
                .FirstOrDefaultAsync(w => w.MaNguoiDung == maNguoiDung && w.MaSanPham == maSanPham);

            if (existing != null)
            {
                return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích" });
            }

            var yeuThich = new SanPhamYeuThich
            {
                MaNguoiDung = maNguoiDung,
                MaSanPham = maSanPham,
                NgayThem = DateTime.Now
            };

            _context.SanPhamYeuThichs.Add(yeuThich);
            await _context.SaveChangesAsync();

            // Đếm tổng số yêu thích
            var totalWishlist = await _context.SanPhamYeuThichs
                .CountAsync(w => w.MaNguoiDung == maNguoiDung);

            return Json(new 
            { 
                success = true, 
                message = "Đã thêm vào danh sách yêu thích",
                totalWishlist = totalWishlist
            });
        }

        // POST: Wishlist/XoaKhoiYeuThich/5
        [HttpPost]
        public async Task<IActionResult> XoaKhoiYeuThich(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var maNguoiDung = int.Parse(userId);

            var yeuThich = await _context.SanPhamYeuThichs
                .FirstOrDefaultAsync(w => w.MaYeuThich == id && w.MaNguoiDung == maNguoiDung);

            if (yeuThich == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong danh sách yêu thích" });
            }

            _context.SanPhamYeuThichs.Remove(yeuThich);
            await _context.SaveChangesAsync();

            // Đếm tổng số yêu thích còn lại
            var totalWishlist = await _context.SanPhamYeuThichs
                .CountAsync(w => w.MaNguoiDung == maNguoiDung);

            return Json(new 
            { 
                success = true, 
                message = "Đã xóa khỏi danh sách yêu thích",
                totalWishlist = totalWishlist
            });
        }

        // GET: Wishlist/KiemTraYeuThich/5
        [HttpGet]
        public async Task<IActionResult> KiemTraYeuThich(int maSanPham)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { isWishlisted = false });
            }

            var maNguoiDung = int.Parse(userId);

            var isWishlisted = await _context.SanPhamYeuThichs
                .AnyAsync(w => w.MaNguoiDung == maNguoiDung && w.MaSanPham == maSanPham);

            return Json(new { isWishlisted });
        }

        // GET: Wishlist/DemSoLuong
        [HttpGet]
        public async Task<IActionResult> DemSoLuong()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var maNguoiDung = int.Parse(userId);

            var count = await _context.SanPhamYeuThichs
                .CountAsync(w => w.MaNguoiDung == maNguoiDung);

            return Json(new { count });
        }
    }
}
