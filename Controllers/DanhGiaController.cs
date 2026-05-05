using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookStoreWeb.Data;
using BookStoreWeb.Models;

namespace BookStoreWeb.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanhGiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DanhGia/GetDanhGiaBySanPham/5?page=1
        [HttpGet]
        public async Task<IActionResult> GetDanhGiaBySanPham(int id, int page = 1, int pageSize = 5)
        {
            var danhGias = await _context.DanhGias
                .Where(d => d.MaSanPham == id)
                .Include(d => d.NguoiDung)
                .Include(d => d.PhanHois)
                    .ThenInclude(p => p.NguoiDung)
                .OrderByDescending(d => d.NgayDanhGia)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalDanhGias = await _context.DanhGias
                .CountAsync(d => d.MaSanPham == id);

            var diemTrungBinh = await GetDiemTrungBinh(id);

            // Map sang DTO để tránh vòng lặp JSON
            var danhGiaDTOs = danhGias.Select(d => new
            {
                d.MaDanhGia,
                d.SoSao,
                d.NoiDung,
                d.NgayDanhGia,
                d.DaMuaHang,
                d.MaSanPham,
                d.MaNguoiDung,
                NguoiDung = d.NguoiDung == null ? null : new
                {
                    d.NguoiDung.MaNguoiDung,
                    d.NguoiDung.HoTen
                },
                PhanHois = d.PhanHois?.Select(p => new
                {
                    p.MaPhanHoi,
                    p.NoiDung,
                    p.NgayPhanHoi,
                    p.MaPhanHoiCha,
                    p.TagNguoiDung,
                    p.MaDanhGia,
                    p.MaNguoiDung,
                    NguoiDung = p.NguoiDung == null ? null : new
                    {
                        p.NguoiDung.MaNguoiDung,
                        p.NguoiDung.HoTen
                    }
                }).ToList()
            }).ToList();

            var viewModel = new
            {
                DanhGias = danhGiaDTOs,
                TotalCount = totalDanhGias,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalDanhGias / pageSize),
                DiemTrungBinh = diemTrungBinh
            };

            return Json(viewModel);
        }

        // GET: DanhGia/GetDiemTrungBinh/5
        [HttpGet]
        public async Task<double> GetDiemTrungBinh(int id)
        {
            var danhGias = await _context.DanhGias
                .Where(d => d.MaSanPham == id)
                .ToListAsync();

            if (!danhGias.Any())
                return 0;

            return Math.Round(danhGias.Average(d => d.SoSao), 1);
        }

        // POST: DanhGia/DanhGiaSanPham
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhGiaSanPham([FromBody] DanhGiaRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá" });
            }

            var maNguoiDung = int.Parse(userId);

            // Kiểm tra user đã đánh giá sách này chưa
            var existingDanhGia = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.MaSanPham == request.MaSanPham && d.MaNguoiDung == maNguoiDung);

            if (existingDanhGia != null)
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sách này rồi. Vui lòng cập nhật đánh giá cũ." });
            }

            // Kiểm tra user đã mua sách chưa
            var daMuaHang = await KiemTraDaMuaHang(maNguoiDung, request.MaSanPham);

            var danhGia = new DanhGia
            {
                MaSanPham = request.MaSanPham,
                MaNguoiDung = maNguoiDung,
                SoSao = request.SoSao,
                NoiDung = request.NoiDung,
                NgayDanhGia = DateTime.Now,
                DaMuaHang = daMuaHang
            };

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            // Load thông tin người dùng để trả về
            await _context.Entry(danhGia)
                .Reference(d => d.NguoiDung)
                .LoadAsync();

            return Json(new 
            { 
                success = true, 
                message = "Đánh giá thành công!",
                danhGia = new
                {
                    maDanhGia = danhGia.MaDanhGia,
                    soSao = danhGia.SoSao,
                    noiDung = danhGia.NoiDung,
                    ngayDanhGia = danhGia.NgayDanhGia.ToString("dd/MM/yyyy HH:mm"),
                    daMuaHang = danhGia.DaMuaHang,
                    hoTen = danhGia.NguoiDung?.HoTen ?? "Người dùng"
                }
            });
        }

        // POST: DanhGia/CapNhatDanhGia/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatDanhGia(int id, [FromBody] DanhGiaUpdateRequest request)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var maNguoiDung = int.Parse(userId);

            // Kiểm tra quyền: chỉ chủ sở hữu mới được sửa
            if (danhGia.MaNguoiDung != maNguoiDung && !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Bạn không có quyền sửa đánh giá này" });
            }

            danhGia.SoSao = request.SoSao;
            danhGia.NoiDung = request.NoiDung;
            danhGia.NgayDanhGia = DateTime.Now; // Cập nhật thời gian sửa

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật đánh giá thành công!" });
        }

        // POST: DanhGia/XoaDanhGia/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaDanhGia(int id)
        {
            var danhGia = await _context.DanhGias
                .Include(d => d.PhanHois)
                .FirstOrDefaultAsync(d => d.MaDanhGia == id);

            if (danhGia == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var maNguoiDung = int.Parse(userId);

            // Kiểm tra quyền: chủ sở hữu hoặc Admin mới được xóa
            if (danhGia.MaNguoiDung != maNguoiDung && !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa đánh giá này" });
            }

            // Xóa tất cả phản hồi trước
            if (danhGia.PhanHois != null && danhGia.PhanHois.Any())
            {
                _context.PhanHoiBinhLuans.RemoveRange(danhGia.PhanHois);
            }

            _context.DanhGias.Remove(danhGia);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa đánh giá thành công!" });
        }

        // POST: DanhGia/TraLoiBinhLuan
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraLoiBinhLuan([FromBody] TraLoiRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để trả lời" });
            }

            var maNguoiDung = int.Parse(userId);

            // Kiểm tra đánh giá tồn tại
            var danhGia = await _context.DanhGias.FindAsync(request.MaDanhGia);
            if (danhGia == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bình luận để trả lời" });
            }

            // Xử lý tag @username trong nội dung
            var noiDung = request.NoiDung;
            string? tagNguoiDung = null;
            
            // Tìm @username trong nội dung
            var match = System.Text.RegularExpressions.Regex.Match(noiDung, @"@(\w+)");
            if (match.Success)
            {
                tagNguoiDung = match.Groups[1].Value;
            }

            var phanHoi = new PhanHoiBinhLuan
            {
                MaDanhGia = request.MaDanhGia,
                MaNguoiDung = maNguoiDung,
                NoiDung = noiDung,
                NgayPhanHoi = DateTime.Now,
                TagNguoiDung = tagNguoiDung,
                MaPhanHoiCha = request.MaPhanHoiCha // Có thể null nếu trả lời đánh giá gốc
            };

            _context.PhanHoiBinhLuans.Add(phanHoi);
            await _context.SaveChangesAsync();

            // Load thông tin người dùng
            await _context.Entry(phanHoi)
                .Reference(p => p.NguoiDung)
                .LoadAsync();

            return Json(new 
            { 
                success = true, 
                message = "Trả lời thành công!",
                phanHoi = new
                {
                    maPhanHoi = phanHoi.MaPhanHoi,
                    maDanhGia = phanHoi.MaDanhGia,
                    maPhanHoiCha = phanHoi.MaPhanHoiCha,
                    noiDung = phanHoi.NoiDung,
                    ngayPhanHoi = phanHoi.NgayPhanHoi.ToString("dd/MM/yyyy HH:mm"),
                    tagNguoiDung = phanHoi.TagNguoiDung,
                    hoTen = phanHoi.NguoiDung?.HoTen ?? "Người dùng"
                }
            });
        }

        // POST: DanhGia/CapNhatPhanHoi/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatPhanHoi(int id, [FromBody] PhanHoiUpdateRequest request)
        {
            var phanHoi = await _context.PhanHoiBinhLuans.FindAsync(id);
            if (phanHoi == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phản hồi" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var maNguoiDung = int.Parse(userId);

            // Kiểm tra quyền: chỉ chủ sở hữu mới được sửa
            if (phanHoi.MaNguoiDung != maNguoiDung && !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Bạn không có quyền sửa phản hồi này" });
            }

            // Xử lý lại tag @username
            var noiDung = request.NoiDung;
            string? tagNguoiDung = null;
            var match = System.Text.RegularExpressions.Regex.Match(noiDung, @"@(\w+)");
            if (match.Success)
            {
                tagNguoiDung = match.Groups[1].Value;
            }

            phanHoi.NoiDung = noiDung;
            phanHoi.TagNguoiDung = tagNguoiDung;
            phanHoi.NgayPhanHoi = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật phản hồi thành công!" });
        }

        // POST: DanhGia/XoaPhanHoi/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaPhanHoi(int id)
        {
            var phanHoi = await _context.PhanHoiBinhLuans
                .Include(p => p.PhanHoiCons)
                .FirstOrDefaultAsync(p => p.MaPhanHoi == id);

            if (phanHoi == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phản hồi" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var maNguoiDung = int.Parse(userId);

            // Kiểm tra quyền: chủ sở hữu hoặc Admin mới được xóa
            if (phanHoi.MaNguoiDung != maNguoiDung && !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa phản hồi này" });
            }

            // Xóa tất cả phản hồi con (nested) trước
            if (phanHoi.PhanHoiCons != null && phanHoi.PhanHoiCons.Any())
            {
                await XoaPhanHoiConRecursive(phanHoi.PhanHoiCons);
            }

            _context.PhanHoiBinhLuans.Remove(phanHoi);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa phản hồi thành công!" });
        }

        // Helper method: Xóa phản hồi con đệ quy
        private async Task XoaPhanHoiConRecursive(ICollection<PhanHoiBinhLuan> phanHoiCons)
        {
            foreach (var con in phanHoiCons.ToList())
            {
                // Load phản hồi con của phản hồi này
                await _context.Entry(con)
                    .Collection(p => p.PhanHoiCons)
                    .LoadAsync();

                if (con.PhanHoiCons != null && con.PhanHoiCons.Any())
                {
                    await XoaPhanHoiConRecursive(con.PhanHoiCons);
                }

                _context.PhanHoiBinhLuans.Remove(con);
            }
        }

        // Helper method: Kiểm tra user đã mua sách chưa
        private async Task<bool> KiemTraDaMuaHang(int maNguoiDung, int maSanPham)
        {
            var daMua = await _context.DonHangs
                .Where(d => d.MaNguoiDung == maNguoiDung && d.TrangThai == "DaThanhToan")
                .Join(_context.ChiTietDonHangs,
                    d => d.MaDonHang,
                    c => c.MaDonHang,
                    (d, c) => c)
                .AnyAsync(c => c.MaSanPham == maSanPham);

            return daMua;
        }
    }

    // Request models
    public class DanhGiaRequest
    {
        public int MaSanPham { get; set; }
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
    }

    public class DanhGiaUpdateRequest
    {
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
    }

    public class TraLoiRequest
    {
        public int MaDanhGia { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public int? MaPhanHoiCha { get; set; }
    }

    public class PhanHoiUpdateRequest
    {
        public string NoiDung { get; set; } = string.Empty;
    }
}
