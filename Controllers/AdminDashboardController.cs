using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreWeb.Data;
using BookStoreWeb.Models;

namespace BookStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            var totalProducts = await _context.SanPhams.CountAsync();
            var totalOrders = await _context.DonHangs.CountAsync();
            var totalUsers = await _context.NguoiDungs.CountAsync();
            var totalRevenue = (decimal)(await _context.DonHangs
                .Where(d => d.TrangThai == "DaThanhToan" || d.TrangThai == "DaGiao")
                .Select(d => (double?)d.TongTien)
                .SumAsync() ?? 0);

            // Thống kê đơn hàng theo trạng thái
            var ordersByStatus = await _context.DonHangs
                .GroupBy(d => d.TrangThai)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Top sách bán chạy (tháng này)
            var today = DateTime.Now;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            
            var topSellingBooks = await _context.ChiTietDonHangs
                .Include(c => c.SanPham)
                .Include(c => c.DonHang)
                .Where(c => c.DonHang != null && c.DonHang.NgayDatHang >= firstDayOfMonth)
                .Where(c => c.SanPham != null)
                .GroupBy(c => new { c.MaSanPham, c.SanPham.TenSach, c.SanPham.HinhAnh })
                .Select(g => new TopSellingBook
                {
                    MaSanPham = g.Key.MaSanPham,
                    TenSach = g.Key.TenSach ?? "Không có tên",
                    HinhAnh = g.Key.HinhAnh,
                    SoLuongBan = g.Sum(c => c.SoLuong),
                    DoanhThu = (decimal)g.Sum(c => (double)c.SoLuong * (double)c.DonGia)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .ToListAsync();

            // Doanh thu theo tháng (6 tháng gần nhất)
            var revenueByMonth = await _context.DonHangs
                .Where(d => d.NgayDatHang >= today.AddMonths(-6))
                .Where(d => d.TrangThai == "DaThanhToan" || d.TrangThai == "DaGiao")
                .GroupBy(d => new { d.NgayDatHang.Year, d.NgayDatHang.Month })
                .Select(g => new MonthlyRevenue
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = (decimal)g.Sum(d => (double?)d.TongTien ?? 0)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Người dùng mới (tháng này)
            var newUsersThisMonth = await _context.NguoiDungs
                .CountAsync(u => u.NgayTao >= firstDayOfMonth);

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.OrdersByStatus = ordersByStatus;
            ViewBag.TopSellingBooks = topSellingBooks;
            ViewBag.RevenueByMonth = revenueByMonth;
            ViewBag.NewUsersThisMonth = newUsersThisMonth;

            return View();
        }
    }

    public class TopSellingBook
    {
        public int MaSanPham { get; set; }
        public string TenSach { get; set; } = "";
        public string? HinhAnh { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }
}
