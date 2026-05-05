using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookStoreWeb.Data;
using BookStoreWeb.Models;

namespace BookStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminDonHang/Index - Danh sách tất cả đơn hàng
        public async Task<IActionResult> Index()
        {
            var donHangs = await _context.DonHangs
                .Include(d => d.NguoiDung)
                .OrderByDescending(d => d.NgayDatHang)
                .ToListAsync();

            return View(donHangs);
        }

        // GET: AdminDonHang/ChiTiet/5 - Xem chi tiết đơn hàng
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.NguoiDung)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: AdminDonHang/XacNhan/5 - Xác nhận đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            
            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ xác nhận nếu đang ở trạng thái Chờ xác nhận
            if (donHang.TrangThai == "ChoXacNhan")
            {
                donHang.TrangThai = "DaXacNhan";
                donHang.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xác nhận đơn hàng #{id}";
            }
            else
            {
                TempData["ErrorMessage"] = "Đơn hàng không ở trạng thái chờ xác nhận";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: AdminDonHang/HuyDon/5 - Hủy đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            
            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ hủy nếu đang ở trạng thái Chờ xác nhận
            if (donHang.TrangThai == "ChoXacNhan")
            {
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
                TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{id}";
            }
            else
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng ở trạng thái chờ xác nhận";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: AdminDonHang/XoaDon/5 - Xóa đơn hàng (chỉ cho phép xóa đơn đã hủy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaDon(int id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);
            
            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ cho phép xóa đơn hàng đã hủy
            if (donHang.TrangThai != "Huy")
            {
                TempData["ErrorMessage"] = "Chỉ có thể xóa đơn hàng đã hủy";
                return RedirectToAction(nameof(Index));
            }

            // Hoàn trả số lượng tồn kho (phòng trường hợp chưa hoàn trả)
            foreach (var ct in donHang.ChiTietDonHangs!)
            {
                var sanPham = await _context.SanPhams.FindAsync(ct.MaSanPham);
                if (sanPham != null)
                {
                    sanPham.SoLuongTon += ct.SoLuong;
                }
            }

            // Xóa chi tiết đơn hàng trước
            _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs!);
            
            // Xóa đơn hàng
            _context.DonHangs.Remove(donHang);
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa đơn hàng #{id} thành công";

            return RedirectToAction(nameof(Index));
        }
    }
}
