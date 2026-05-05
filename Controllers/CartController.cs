using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using BookStoreWeb.Data;
using BookStoreWeb.Models;
using BookStoreWeb.Services.Momo;
using BookStoreWeb.Services.PayOS;

namespace BookStoreWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMomoService _momoService;
        private readonly IPayOSService _payOSService;
        private readonly ILogger<CartController> _logger;
        private const string CART_SESSION_KEY = "CartItems";

        public CartController(ApplicationDbContext context, IMomoService momoService, IPayOSService payOSService, ILogger<CartController> logger)
        {
            _context = context;
            _momoService = momoService;
            _payOSService = payOSService;
            _logger = logger;
        }

        // GET: Cart - Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cartItems = GetCartItems();
            return View(cartItems);
        }

        // POST: Cart/AddToCart - Thêm sản phẩm vào giỏ
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var sanPham = _context.SanPhams.Find(productId);
            if (sanPham == null || !sanPham.ConHieuLuc)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc đã ngừng bán" });
            }

            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
            }

            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(c => c.MaSP == productId);

            if (existingItem != null)
            {
                // Cập nhật số lượng nếu sản phẩm đã có trong giỏ
                existingItem.SoLuong += quantity;
            }
            else
            {
                // Thêm mới vào giỏ
                cartItems.Add(new CartItem
                {
                    MaSP = sanPham.MaSanPham,
                    TenSP = sanPham.TenSach,
                    HinhAnh = sanPham.HinhAnh,
                    Gia = sanPham.GiaBan,
                    SoLuong = quantity
                });
            }

            SaveCartItems(cartItems);

            var cartCount = cartItems.Sum(c => c.SoLuong);
            var cartTotal = cartItems.Sum(c => c.ThanhTien);

            return Json(new 
            { 
                success = true, 
                message = "Đã thêm vào giỏ hàng",
                cartCount = cartCount,
                cartTotal = cartTotal.ToString("N0")
            });
        }

        // POST: Cart/RemoveFromCart - Xóa sản phẩm khỏi giỏ
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cartItems = GetCartItems();
            var itemToRemove = cartItems.FirstOrDefault(c => c.MaSP == productId);

            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                SaveCartItems(cartItems);
            }

            var cartCount = cartItems.Sum(c => c.SoLuong);
            var cartTotal = cartItems.Sum(c => c.ThanhTien);

            return Json(new 
            { 
                success = true, 
                cartCount = cartCount,
                cartTotal = cartTotal.ToString("N0")
            });
        }

        // POST: Cart/UpdateQuantity - Cập nhật số lượng
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
            }

            var cartItems = GetCartItems();
            var item = cartItems.FirstOrDefault(c => c.MaSP == productId);

            if (item != null)
            {
                item.SoLuong = quantity;
                SaveCartItems(cartItems);
            }

            var cartCount = cartItems.Sum(c => c.SoLuong);
            var cartTotal = cartItems.Sum(c => c.ThanhTien);

            return Json(new 
            { 
                success = true, 
                itemTotal = item?.ThanhTien.ToString("N0"),
                cartCount = cartCount,
                cartTotal = cartTotal.ToString("N0")
            });
        }

        // POST: Cart/Clear - Xóa toàn bộ giỏ hàng
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_SESSION_KEY);
            return Json(new { success = true });
        }

        // Helper: Lấy danh sách giỏ hàng từ Session
        private List<CartItem> GetCartItems()
        {
            var sessionData = HttpContext.Session.GetString(CART_SESSION_KEY);
            if (string.IsNullOrEmpty(sessionData))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        // Helper: Lưu giỏ hàng vào Session
        private void SaveCartItems(List<CartItem> cartItems)
        {
            var sessionData = JsonSerializer.Serialize(cartItems);
            HttpContext.Session.SetString(CART_SESSION_KEY, sessionData);
        }

        // GET: Cart/GetCartCount - Lấy số lượng sản phẩm trong giỏ (dùng cho navbar)
        public IActionResult GetCartCount()
        {
            var cartItems = GetCartItems();
            var count = cartItems.Sum(c => c.SoLuong);
            var total = cartItems.Sum(c => c.ThanhTien);
            return Json(new { count = count, total = total.ToString("N0") });
        }

        // GET: Cart/Checkout - Hiển thị form thanh toán (Yêu cầu đăng nhập)
        [Authorize]
        public IActionResult Checkout()
        {
            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index");
            }

            // Lấy thông tin người dùng từ Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            var maNguoiDung = int.Parse(userId);
            var user = _context.NguoiDungs.Find(maNguoiDung);
            if (user == null)
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            ViewBag.User = user;
            return View(cartItems);
        }

        // POST: Cart/Checkout - Xử lý thanh toán (Yêu cầu đăng nhập)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string diaChiGiaoHang, string phuongThucThanhToan, string? ghiChu)
        {
            var cartItems = GetCartItems();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index");
            }

            // Lấy ID người dùng từ Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            var maNguoiDung = int.Parse(userId);
            var tongTien = cartItems.Sum(c => c.ThanhTien);

            // Tạo đơn hàng mới
            var donHang = new DonHang
            {
                MaNguoiDung = maNguoiDung,
                NgayDatHang = DateTime.Now,
                TongTien = tongTien,
                TrangThai = "ChoXacNhan",
                PhuongThucThanhToan = phuongThucThanhToan ?? "Tiền mặt",
                DiaChiGiaoHang = diaChiGiaoHang,
                GhiChu = ghiChu,
                NgayCapNhat = DateTime.Now
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            // Tạo chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                var chiTiet = new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSanPham = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia
                };
                _context.ChiTietDonHangs.Add(chiTiet);

                // Cập nhật số lượng tồn
                var sanPham = await _context.SanPhams.FindAsync(item.MaSP);
                if (sanPham != null)
                {
                    sanPham.SoLuongTon -= item.SoLuong;
                }
            }

            await _context.SaveChangesAsync();

            // Nếu thanh toán bằng MoMo, chuyển sang trang thanh toán MoMo
            if (phuongThucThanhToan == "MoMo")
            {
                var orderId = donHang.MaDonHang.ToString();
                var orderInfo = $"Thanh toan don hang #{donHang.MaDonHang} - BookStore";
                
                var momoResponse = await _momoService.CreatePaymentAsync(orderId, orderInfo, tongTien);
                
                if (momoResponse != null && momoResponse.ResultCode == 0 && !string.IsNullOrEmpty(momoResponse.PayUrl))
                {
                    // Lưu đơn hàng vào Session để xử lý sau khi thanh toán
                    HttpContext.Session.SetString("PendingOrderId", donHang.MaDonHang.ToString());
                    
                    return Redirect(momoResponse.PayUrl);
                }
                else
                {
                    // Thanh toán MoMo thất bại - Hoàn trả tồn kho
                    TempData["ErrorMessage"] = "Không thể kết nối đến MoMo. Vui lòng thử lại hoặc chọn phương thức thanh toán khác.";
                    donHang.TrangThai = "Huy";
                    donHang.GhiChu = (donHang.GhiChu ?? "") + " - Thanh toán MoMo thất bại";
                    donHang.NgayCapNhat = DateTime.Now;

                    // Hoàn trả số lượng tồn kho
                    var chiTiets = await _context.ChiTietDonHangs
                        .Where(c => c.MaDonHang == donHang.MaDonHang)
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
                    return RedirectToAction("Index");
                }
            }

            // Nếu thanh toán bằng PayOS, chuyển sang trang thanh toán PayOS
            if (phuongThucThanhToan == "PayOS")
            {
                var orderId = donHang.MaDonHang.ToString();
                var orderInfo = $"Thanh toan don hang #{donHang.MaDonHang}";
                var returnUrl = $"{Request.Scheme}://{Request.Host}/Cart/PayOSCallback";
                
                var paymentUrl = await _payOSService.CreatePaymentUrl(orderId, tongTien, orderInfo, returnUrl);
                
                if (!string.IsNullOrEmpty(paymentUrl))
                {
                    // Lưu đơn hàng vào Session để xử lý sau khi thanh toán
                    HttpContext.Session.SetString("PendingOrderId", donHang.MaDonHang.ToString());
                    HttpContext.Session.SetString("PaymentMethod", "PayOS");
                    
                    return Redirect(paymentUrl);
                }
                else
                {
                    // Thanh toán PayOS thất bại - Hoàn trả tồn kho
                    TempData["ErrorMessage"] = "Không thể kết nối đến PayOS. Vui lòng thử lại hoặc chọn phương thức thanh toán khác.";
                    donHang.TrangThai = "Huy";
                    donHang.GhiChu = (donHang.GhiChu ?? "") + " - Thanh toán PayOS thất bại";
                    donHang.NgayCapNhat = DateTime.Now;

                    // Hoàn trả số lượng tồn kho
                    var chiTiets = await _context.ChiTietDonHangs
                        .Where(c => c.MaDonHang == donHang.MaDonHang)
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
                    return RedirectToAction("Index");
                }
            }

            // Xóa giỏ hàng trong Session (chỉ cho thanh toán tiền mặt)
            HttpContext.Session.Remove(CART_SESSION_KEY);

            TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: {donHang.MaDonHang}";
            return RedirectToAction("ChiTiet", "DonHang", new { id = donHang.MaDonHang });
        }

        // GET: Cart/PayOSCallback - Xử lý callback từ PayOS
        public async Task<IActionResult> PayOSCallback(string code, string id, bool cancel, string status, long orderCode)
        {
            // Lấy mã đơn hàng từ Session
            var pendingOrderId = HttpContext.Session.GetString("PendingOrderId");
            
            if (int.TryParse(pendingOrderId, out var maDonHang) && maDonHang > 0)
            {
                var donHang = await _context.DonHangs.FindAsync(maDonHang);
                
                if (donHang != null)
                {
                    // Kiểm tra trạng thái thanh toán
                    if (status == "PAID" && !cancel)
                    {
                        // Thanh toán thành công
                        donHang.TrangThai = "DaThanhToan"; // Đã thanh toán qua PayOS
                        donHang.GhiChu = (donHang.GhiChu ?? "") + $" - Đã thanh toán PayOS (Mã GD: {id})";
                        donHang.NgayCapNhat = DateTime.Now;
                        await _context.SaveChangesAsync();

                        // Xóa giỏ hàng
                        HttpContext.Session.Remove(CART_SESSION_KEY);
                        HttpContext.Session.Remove("PendingOrderId");
                        HttpContext.Session.Remove("PaymentMethod");

                        TempData["SuccessMessage"] = $"Thanh toán PayOS thành công! Mã đơn hàng: {maDonHang}";
                        return RedirectToAction("ChiTiet", "DonHang", new { id = maDonHang });
                    }
                    else if (cancel || status == "CANCELLED")
                    {
                        // Người dùng hủy thanh toán
                        if (donHang.TrangThai == "ChoXacNhan")
                        {
                            donHang.TrangThai = "Huy";
                            donHang.GhiChu = (donHang.GhiChu ?? "") + " - Người dùng hủy thanh toán PayOS";
                            donHang.NgayCapNhat = DateTime.Now;

                            // Hoàn trả số lượng tồn kho
                            var chiTiets = await _context.ChiTietDonHangs
                                .Where(c => c.MaDonHang == donHang.MaDonHang)
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
                        }

                        TempData["ErrorMessage"] = "Bạn đã hủy thanh toán PayOS. Đơn hàng đã bị hủy.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Thanh toán thất bại
                        if (donHang.TrangThai == "ChoXacNhan")
                        {
                            donHang.TrangThai = "Huy";
                            donHang.GhiChu = (donHang.GhiChu ?? "") + $" - Thanh toán PayOS thất bại: {status}";
                            donHang.NgayCapNhat = DateTime.Now;

                            // Hoàn trả số lượng tồn kho
                            var chiTiets = await _context.ChiTietDonHangs
                                .Where(c => c.MaDonHang == donHang.MaDonHang)
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
                        }

                        TempData["ErrorMessage"] = $"Thanh toán PayOS không thành công (Trạng thái: {status})";
                        return RedirectToAction("Index");
                    }
                }
            }

            TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng";
            return RedirectToAction("Index");
        }

        // GET: Cart/PaymentCallBack - Xử lý callback từ MoMo
        public async Task<IActionResult> PaymentCallBack(string orderId, string resultCode, string message)
        {
            // Lấy orderId từ Session nếu có
            var pendingOrderId = HttpContext.Session.GetString("PendingOrderId");
            
            // Parse orderId (format: id_timestamp)
            int? maDonHang = null;
            if (!string.IsNullOrEmpty(orderId))
            {
                var orderIdParts = orderId.Split('_');
                if (orderIdParts.Length > 0 && int.TryParse(orderIdParts[0], out var parsedId))
                {
                    maDonHang = parsedId;
                }
            }
            
            // Kiểm tra kết quả thanh toán từ MoMo
            if (resultCode == "0" && maDonHang.HasValue)
            {
                // Thanh toán thành công
                var donHang = await _context.DonHangs.FindAsync(maDonHang.Value);
                if (donHang != null)
                {
                    donHang.TrangThai = "DaThanhToan"; // Đã thanh toán qua MoMo
                    donHang.GhiChu = (donHang.GhiChu ?? "") + " - Đã thanh toán MoMo";
                    donHang.NgayCapNhat = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // Xóa giỏ hàng
                    HttpContext.Session.Remove(CART_SESSION_KEY);
                    HttpContext.Session.Remove("PendingOrderId");

                    TempData["SuccessMessage"] = $"Thanh toán MoMo thành công! Mã đơn hàng: {maDonHang}";
                    return RedirectToAction("ChiTiet", "DonHang", new { id = maDonHang });
                }
            }
            else
            {
                // Thanh toán thất bại hoặc bị hủy
                if (maDonHang.HasValue)
                {
                    var donHang = await _context.DonHangs.FindAsync(maDonHang.Value);
                    if (donHang != null && donHang.TrangThai == "ChoXacNhan")
                    {
                        // Hủy đơn hàng và hoàn trả tồn kho
                        donHang.TrangThai = "Huy";
                        donHang.GhiChu = (donHang.GhiChu ?? "") + $" - Hủy thanh toán MoMo: {message}";
                        donHang.NgayCapNhat = DateTime.Now;

                        // Hoàn trả số lượng tồn kho
                        var chiTiets = await _context.ChiTietDonHangs
                            .Where(c => c.MaDonHang == maDonHang)
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
                    }
                }

                HttpContext.Session.Remove("PendingOrderId");
                TempData["ErrorMessage"] = $"Thanh toán MoMo thất bại: {message}";
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Cart/OrderSuccess - Trang thông báo đặt hàng thành công
        public IActionResult OrderSuccess(int id)
        {
            var donHang = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(c => c.SanPham)
                .FirstOrDefault(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: Cart/PayOSWebhook - Nhận webhook từ PayOS khi thanh toán thành công
        [HttpPost]
        public async Task<IActionResult> PayOSWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var jsonBody = await reader.ReadToEndAsync();

                _logger.LogInformation("PayOS Webhook received: {Body}", jsonBody);

                // Verify signature (optional but recommended)
                var signature = Request.Headers["X-PayOS-Signature"].ToString();
                if (!string.IsNullOrEmpty(signature) && !_payOSService.VerifyWebhookSignature(jsonBody, signature))
                {
                    _logger.LogWarning("Invalid PayOS webhook signature");
                    return BadRequest(new { code = "01", desc = "Invalid signature" });
                }

                var webhookData = System.Text.Json.JsonSerializer.Deserialize<PayOSWebhookRequest>(jsonBody, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData?.Data != null && webhookData.Code == "00")
                {
                    // Tìm đơn hàng dựa trên mã đơn hàng hoặc thông tin trong description
                    var orderCode = webhookData.Data.OrderCode;
                    var donHang = await _context.DonHangs
                        .FirstOrDefaultAsync(d => d.TrangThai == "ChoXacNhan" && d.GhiChu != null && d.GhiChu.Contains(orderCode.ToString()));

                    if (donHang != null)
                    {
                        donHang.TrangThai = "DaThanhToan";
                        donHang.GhiChu = (donHang.GhiChu ?? "") + $" - PayOS Webhook: Đã thanh toán (Ref: {webhookData.Data.RefId})";
                        donHang.NgayCapNhat = DateTime.Now;
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Order {OrderId} updated to DaThanhToan via PayOS webhook", donHang.MaDonHang);
                    }
                }

                return Ok(new { code = "00", desc = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook");
                return StatusCode(500, new { code = "99", desc = "Internal error" });
            }
        }
    }
}
