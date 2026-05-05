using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreWeb.Data;
using BookStoreWeb.Models;
using System.Text.Json;

namespace BookStoreWeb.Controllers
{
    public class SoSanhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string COMPARE_SESSION_KEY = "CompareList";

        public SoSanhController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SoSanh/Index
        public async Task<IActionResult> Index()
        {
            var compareIds = GetCompareList();
            var sachList = new List<SanPham>();

            if (compareIds.Any())
            {
                sachList = await _context.SanPhams
                    .Where(s => compareIds.Contains(s.MaSanPham) && s.ConHieuLuc)
                    .ToListAsync();
            }

            return View(sachList);
        }

        // POST: SoSanh/ThemVaoSoSanh
        [HttpPost]
        public IActionResult ThemVaoSoSanh(int maSanPham)
        {
            var compareList = GetCompareList();

            // Kiểm tra đã tồn tại chưa
            if (compareList.Contains(maSanPham))
            {
                return Json(new { success = false, message = "Sách đã có trong danh sách so sánh" });
            }

            // Kiểm tra tối đa 3 sách
            if (compareList.Count >= 3)
            {
                return Json(new { success = false, message = "Chỉ có thể so sánh tối đa 3 sách" });
            }

            compareList.Add(maSanPham);
            SaveCompareList(compareList);

            return Json(new 
            { 
                success = true, 
                message = "Đã thêm vào danh sách so sánh",
                count = compareList.Count 
            });
        }

        // POST: SoSanh/XoaKhoiSoSanh
        [HttpPost]
        public IActionResult XoaKhoiSoSanh(int maSanPham)
        {
            var compareList = GetCompareList();
            
            if (compareList.Remove(maSanPham))
            {
                SaveCompareList(compareList);
                return Json(new 
                { 
                    success = true, 
                    message = "Đã xóa khỏi danh sách so sánh",
                    count = compareList.Count 
                });
            }

            return Json(new { success = false, message = "Sách không có trong danh sách" });
        }

        // POST: SoSanh/XoaTatCa
        [HttpPost]
        public IActionResult XoaTatCa()
        {
            HttpContext.Session.Remove(COMPARE_SESSION_KEY);
            return Json(new { success = true, message = "Đã xóa tất cả" });
        }

        // GET: SoSanh/GetSoLuong
        [HttpGet]
        public IActionResult GetSoLuong()
        {
            var compareList = GetCompareList();
            return Json(new { count = compareList.Count });
        }

        // GET: SoSanh/GetDanhSach
        [HttpGet]
        public async Task<IActionResult> GetDanhSach()
        {
            var compareIds = GetCompareList();
            var sachList = await _context.SanPhams
                .Where(s => compareIds.Contains(s.MaSanPham) && s.ConHieuLuc)
                .Select(s => new { s.MaSanPham, s.TenSach, s.HinhAnh })
                .ToListAsync();

            return Json(sachList);
        }

        // Helper methods
        private List<int> GetCompareList()
        {
            var sessionData = HttpContext.Session.GetString(COMPARE_SESSION_KEY);
            if (string.IsNullOrEmpty(sessionData))
            {
                return new List<int>();
            }
            return JsonSerializer.Deserialize<List<int>>(sessionData) ?? new List<int>();
        }

        private void SaveCompareList(List<int> list)
        {
            HttpContext.Session.SetString(COMPARE_SESSION_KEY, JsonSerializer.Serialize(list));
        }
    }
}
