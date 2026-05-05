using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookStoreWeb.Data;
using BookStoreWeb.Models;

namespace BookStoreWeb.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DonHang/LichSu - Xem lịch sử đơn hàng của user đang đăng nhập
        public async Task<IActionResult> LichSu()
        {
            // Lấy ID người dùng từ Claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            var maNguoiDung = int.Parse(userIdString);

            // Lấy danh sách đơn hàng của user
            var donHangs = await _context.DonHangs
                .Where(d => d.MaNguoiDung == maNguoiDung)
                .OrderByDescending(d => d.NgayDatHang)
                .ToListAsync();

            return View(donHangs);
        }

        // GET: DonHang/ChiTiet/5 - Xem chi tiết đơn hàng
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy ID người dùng từ Claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            var maNguoiDung = int.Parse(userIdString);

            // Lấy chi tiết đơn hàng kèm theo thông tin sản phẩm
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền: chỉ cho phép xem đơn hàng của chính mình
            // (Trừ khi là Admin)
            if (donHang.MaNguoiDung != maNguoiDung && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này";
                return RedirectToAction("LichSu");
            }

            return View(donHang);
        }

        // POST: DonHang/HuyDonHang/5 - Khách hàng tự hủy đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDonHang(int id)
        {
            // Lấy ID người dùng từ Claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            var maNguoiDung = int.Parse(userIdString);

            // Tìm đơn hàng theo id VÀ phải khớp với user đang đăng nhập
            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaNguoiDung == maNguoiDung);

            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction(nameof(LichSu));
            }

            // Chỉ cho phép hủy nếu trạng thái là 'Chờ xác nhận'
            if (donHang.TrangThai != "ChoXacNhan")
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng đang chờ xác nhận";
                return RedirectToAction(nameof(LichSu));
            }

            // Cập nhật trạng thái thành 'Huy'
            donHang.TrangThai = "Huy";
            donHang.NgayCapNhat = DateTime.Now;

            // Hoàn trả số lượng tồn kho
            var chiTiets = await _context.ChiTietDonHangs
                .Where(c => c.MaDonHang == id)
                .ToListAsync();

            foreach (var ct in chiTiets)
            {
                var sanPham = await _context.SanPhams.FindAsync(ct.MaSanPham);
                if (sanPham != null)
                {
                    sanPham.SoLuongTon += ct.SoLuong;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{id} thành công";

            return RedirectToAction(nameof(LichSu));
        }
    }
}
