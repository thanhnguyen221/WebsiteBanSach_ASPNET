using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreWeb.Data;
using BookStoreWeb.Models;

namespace BookStoreWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private const string RECENTLY_VIEWED_KEY = "RecentlyViewed";

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm, int page = 1, string? theLoai = null)
    {
        const int PAGE_SIZE = 8;

        var query = _context.SanPhams
            .Where(sp => sp.ConHieuLuc)
            .AsQueryable();

        // Tìm kiếm theo từ khóa
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(sp =>
                sp.TenSach.Contains(searchTerm) ||
                sp.TacGia.Contains(searchTerm) ||
                sp.NhaXuatBan.Contains(searchTerm) ||
                sp.TheLoai.Contains(searchTerm));
        }

        // Lọc theo thể loại
        if (!string.IsNullOrWhiteSpace(theLoai))
        {
            query = query.Where(sp => sp.TheLoai == theLoai);
        }

        // Đếm tổng số sản phẩm
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)PAGE_SIZE);

        // Phân trang
        var sanPhams = await query
            .OrderByDescending(sp => sp.NgayTao)
            .Skip((page - 1) * PAGE_SIZE)
            .Take(PAGE_SIZE)
            .ToListAsync();

        // Lấy danh sách thể loại để hiển thị filter
        var theLoais = await _context.SanPhams
            .Where(sp => sp.ConHieuLuc && sp.TheLoai != null)
            .Select(sp => sp.TheLoai)
            .Distinct()
            .ToListAsync();

        // Lấy lịch sử xem gần đây
        var recentlyViewed = await GetRecentlyViewed();

        // Truyền dữ liệu sang View
        ViewBag.TheLoais = theLoais;
        ViewBag.CurrentTheLoai = theLoai;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.RecentlyViewed = recentlyViewed;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(sanPhams);
    }

    // Helper: Lấy lịch sử xem gần đây
    private async Task<List<SanPham>> GetRecentlyViewed()
    {
        var viewedIds = HttpContext.Session.GetString(RECENTLY_VIEWED_KEY);
        if (string.IsNullOrEmpty(viewedIds))
        {
            return new List<SanPham>();
        }

        var ids = System.Text.Json.JsonSerializer.Deserialize<List<int>>(viewedIds);
        if (ids == null || !ids.Any())
        {
            return new List<SanPham>();
        }

        // Lấy sách theo thứ tự đã xem (mới nhất trước)
        var sachList = await _context.SanPhams
            .Where(s => ids.Contains(s.MaSanPham) && s.ConHieuLuc)
            .ToListAsync();

        // Sắp xếp theo thứ tự đã xem
        return ids
            .Join(sachList, id => id, s => s.MaSanPham, (id, s) => s)
            .Take(6)
            .ToList();
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sanPham = await _context.SanPhams
            .FirstOrDefaultAsync(sp => sp.MaSanPham == id && sp.ConHieuLuc);

        if (sanPham == null)
        {
            return NotFound();
        }

        // Thêm vào lịch sử xem
        if (id.HasValue)
        {
            AddToRecentlyViewed(id.Value);
        }

        // Gợi ý sách cùng thể loại và tác giả
        var goiYSanPham = await _context.SanPhams
            .Where(s => s.ConHieuLuc && s.MaSanPham != id)
            .Where(s => s.TheLoai == sanPham.TheLoai || s.TacGia == sanPham.TacGia)
            .OrderByDescending(s => s.NgayTao)
            .Take(4)
            .ToListAsync();

        ViewBag.GoiYSanPham = goiYSanPham;

        return View(sanPham);
    }

    // Helper: Thêm vào lịch sử xem
    private void AddToRecentlyViewed(int maSanPham)
    {
        var viewedIds = HttpContext.Session.GetString(RECENTLY_VIEWED_KEY);
        var ids = string.IsNullOrEmpty(viewedIds) 
            ? new List<int>() 
            : System.Text.Json.JsonSerializer.Deserialize<List<int>>(viewedIds) ?? new List<int>();

        // Xóa nếu đã tồn tại (để đưa lên đầu)
        ids.Remove(maSanPham);
        
        // Thêm vào đầu danh sách
        ids.Insert(0, maSanPham);

        // Giữ tối đa 10 sách
        if (ids.Count > 10)
        {
            ids = ids.Take(10).ToList();
        }

        HttpContext.Session.SetString(RECENTLY_VIEWED_KEY, 
            System.Text.Json.JsonSerializer.Serialize(ids));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
