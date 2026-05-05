# Nhiệm vụ hiện tại - Giai đoạn 1

## Bước 1: Khởi tạo Project
- [x] Khởi tạo project ASP.NET Core MVC tên BookStoreWeb

## Bước 2: Tạo Models
- [x] Tạo thư mục Models
- [x] Viết code cho `SanPham.cs` kèm Data Annotations
- [x] Viết code cho `NguoiDung.cs` kèm Data Annotations
- [x] Viết code cho `DonHang.cs` kèm Data Annotations

## Bước 3: Cài đặt NuGet Packages
- [x] Cài đặt `Microsoft.EntityFrameworkCore.Sqlite`
- [x] Cài đặt `Microsoft.EntityFrameworkCore.Tools`

## Bước 4: Cấu hình Database
- [x] Tạo `ApplicationDbContext.cs`
- [x] Cấu hình connection string trong `appsettings.json`

## Bước 5: Migration
- [x] Chạy Migration để tạo file `BookStoreWeb.db`

---

# Nhiệm vụ hiện tại - Giai đoạn 2: Xây dựng trang Admin

## Nhiệm vụ 1: Tạo SanPhamsController
- [x] Tạo SanPhamsController với các action CRUD (Index, Details, Create, Edit, Delete)

## Nhiệm vụ 2: Xử lý Upload hình ảnh
- [x] Xử lý logic Upload hình ảnh trong action Create và Edit (Lưu file vật lý vào wwwroot/images/ với tên file tự sinh để tránh trùng, và chỉ lưu tên file vào database)

## Nhiệm vụ 3: Tạo Views cho SanPhams
- [x] Tạo View Index (hiển thị danh sách sản phẩm với ảnh thumbnail, Bootstrap 5)
- [x] Tạo View Create (form thêm sản phẩm với upload ảnh, Bootstrap 5)
- [x] Tạo View Edit (form sửa sản phẩm với upload ảnh, Bootstrap 5)
- [x] Tạo View Details (chi tiết sản phẩm, Bootstrap 5)
- [x] Tạo View Delete (xác nhận xóa, Bootstrap 5)

## Nhiệm vụ 4: Cập nhật Layout
- [x] Cập nhật file Views/Shared/_Layout.cshtml để thêm link 'Quản lý Sản phẩm' vào thanh Navbar

---

# Nhiệm vụ hiện tại - Giai đoạn 3: Xây dựng phía Người dùng và Giỏ hàng

## Nhiệm vụ 1: Cấu hình Session
- [x] Cấu hình Session trong Program.cs (thêm builder.Services.AddSession() và app.UseSession())

## Nhiệm vụ 2: Tạo lớp CartItem
- [x] Tạo lớp CartItem (trong thư mục Models hoặc ViewModels) gồm: MaSP, TenSP, HinhAnh, Gia, SoLuong, ThanhTien

## Nhiệm vụ 3: Cập nhật HomeController
- [x] Cập nhật HomeController: Action Index lấy danh sách Sản phẩm hiển thị ra trang chủ (dùng Bootstrap Card để làm dạng lưới Grid đẹp mắt)
- [x] Action Details để xem chi tiết sách phía người dùng

## Nhiệm vụ 4: Tạo CartController
- [x] Tạo CartController: Xử lý thêm vào giỏ (AddToCart)
- [x] Xem giỏ hàng (Index)
- [x] Xóa khỏi giỏ (RemoveFromCart)
- [x] Lưu dữ liệu giỏ hàng vào Session (chuyển đổi List<CartItem> thành JSON để lưu)

## Nhiệm vụ 5: Tạo Views phía người dùng
- [x] Cập nhật Views/Home/Index.cshtml có nút 'Thêm vào giỏ hàng'
- [x] Cập nhật Views/Home/Details.cshtml có nút 'Thêm vào giỏ hàng'
- [x] Tạo Views/Cart/Index.cshtml để hiển thị chi tiết giỏ hàng và tổng tiền

## Nhiệm vụ 6: Cập nhật Navbar
- [x] Cập nhật thanh Navbar trong _Layout.cshtml: Thêm link 'Giỏ hàng' cho người dùng

---

# Nhiệm vụ hiện tại - Giai đoạn 4: Chức năng nâng cao & Thanh toán

## Nhiệm vụ 1: Cấu hình Cookie Authentication
- [x] Thêm Cookie Authentication vào Program.cs

## Nhiệm vụ 2: Quản lý Tài khoản
- [x] Tạo TaiKhoanController với các action Register, Login, Logout
- [x] So khớp dữ liệu với bảng NguoiDung
- [x] Đăng ký mặc định có VaiTro = "User"
- [x] Tạo một tài khoản Admin mồi (Seeding) nếu DB chưa có Admin

## Nhiệm vụ 3: Bảo mật Admin
- [x] Thêm thuộc tính [Authorize(Roles = "Admin")] vào SanPhamsController

## Nhiệm vụ 4: Cập nhật Navbar (Tài khoản)
- [x] Hiển thị nút "Đăng nhập/Đăng ký" hoặc "Xin chào [Tên] | Đăng xuất" trong _Layout.cshtml

## Nhiệm vụ 5: Thanh toán (Checkout)
- [x] Thêm chức năng Checkout vào CartController
- [x] Yêu cầu người dùng phải đăng nhập ([Authorize])
- [x] Tạo record mới vào bảng DonHang và ChiTietDonHang từ Session
- [x] Lưu vào CSDL, xóa Session giỏ hàng và chuyển hướng đến trang thông báo thành công

## Nhiệm vụ 6: Tìm kiếm & Phân trang
- [x] Cập nhật HomeController action Index nhận tham số searchTerm và page
- [x] Trả về danh sách sách đã lọc và phân trang
- [x] Cập nhật View Index.cshtml thêm thanh tìm kiếm và các nút chuyển trang

---

# Nhiệm vụ hiện tại - Giai đoạn 5: Hoàn thiện UI/UX và Báo cáo Đồ án

## Nhiệm vụ 1: Hoàn thiện UI (Giao diện)
- [x] Cập nhật Views/Shared/_Layout.cshtml thêm Footer chuyên nghiệp (thông tin bản quyền, liên hệ)
- [x] Đảm bảo thông báo lỗi Validation sử dụng class text-danger của Bootstrap

## Nhiệm vụ 2: Báo cáo Đồ án
- [x] Tạo file BaoCao_DoAn.md ở thư mục gốc với nội dung chi tiết, học thuật:
  - [x] Giới thiệu đề tài (Bán Sách)
  - [x] Công nghệ sử dụng (ASP.NET Core MVC, SQLite, EF Core, Bootstrap 5)
  - [x] Thiết kế cơ sở dữ liệu (mô tả các bảng)
  - [x] Mô tả chức năng hệ thống (User + Admin)
  - [x] Hình ảnh giao diện (placeholder cho sinh viên chèn ảnh)
  - [x] Đoạn code chính (Giỏ hàng Session, Upload ảnh)
  - [x] Kết luận và hướng phát triển (VNPay, Momo)

---

# Nhiệm vụ mới - Chức năng Đặt hàng và Quản lý Đơn hàng

## Nhiệm vụ 1: Cập nhật Model và Database
- [x] Cập nhật Model DonHang: Thêm thuộc tính PhuongThucThanhToan (string, mặc định 'Tiền mặt')
- [x] Chạy Migration (dotnet ef migrations add AddPaymentMethod)
- [x] Update Database (dotnet ef database update)

## Nhiệm vụ 2: Cập nhật View Checkout
- [x] Cập nhật Views/Cart/Checkout.cshtml: Thêm UI chọn phương thức thanh toán (Radio Button)
- [x] 'Tiền mặt khi nhận hàng' được chọn mặc định
- [x] 'Thanh toán MoMo (Sắp ra mắt)' disabled để sẵn giao diện
- [x] Nút 'Xác nhận Đặt hàng' nổi bật

## Nhiệm vụ 3: Cập nhật CartController
- [x] Nhận dữ liệu PhuongThucThanhToan từ form, lưu vào bảng DonHang
- [x] Sau khi tạo đơn thành công, xóa Session giỏ hàng
- [x] RedirectToAction sang DonHangController.ChiTiet với id đơn hàng

## Nhiệm vụ 4: Tạo DonHangController
- [x] Tạo DonHangController với [Authorize]
- [x] Action LichSu: Lấy danh sách đơn hàng của User đang đăng nhập
- [x] Action ChiTiet: Nhận id, trả về chi tiết đơn hàng (Include SanPham)
- [x] Kiểm tra quyền: user chỉ được xem đơn hàng của chính mình

## Nhiệm vụ 5: Tạo Views cho DonHangController
- [x] Views/DonHang/LichSu.cshtml: Bảng hiển thị đơn hàng (Mã ĐH, Ngày đặt, Tổng tiền, Phương thức, Trạng thái)
- [x] Nút 'Xem chi tiết' trong bảng
- [x] Views/DonHang/ChiTiet.cshtml: Giao diện hóa đơn thanh toán đẹp mắt

## Nhiệm vụ 6: Cập nhật Navbar
- [x] Thêm link 'Đơn hàng của tôi' vào Navbar (chỉ hiển thị khi đăng nhập)

---

# Nhiệm vụ mới - Quản lý Đơn hàng (Admin) & Khách hàng hủy đơn

## [Phần 1: Quản lý Đơn hàng dành cho Admin]

### Nhiệm vụ 1: Tạo AdminDonHangController
- [x] Tạo Controller AdminDonHangController.cs với [Authorize(Roles = "Admin")]

### Nhiệm vụ 2: Các Action của Admin
- [x] Action Index: Lấy danh sách toàn bộ đơn hàng (sắp xếp mới nhất, Include NguoiDung)
- [x] Action XacNhan(int id): Cập nhật trạng thái thành 'Đã xác nhận'
- [x] Action HuyDon(int id): Cập nhật trạng thái thành 'Đã hủy'
- [x] Action ChiTiet(int id): Xem chi tiết hóa đơn

### Nhiệm vụ 3: Giao diện Admin
- [x] Tạo Views/AdminDonHang/Index.cshtml bằng Bootstrap 5
- [x] Bảng hiển thị (Mã ĐH, Khách hàng, Ngày đặt, Tổng tiền, Trạng thái)
- [x] Badge màu: Vàng='Chờ xác nhận', Xanh='Đã xác nhận', Đỏ='Đã hủy'
- [x] Thêm nút thao tác Xác nhận/Hủy/Xem chi tiết

### Nhiệm vụ 4: Cập nhật Layout
- [x] Thêm link 'Quản lý Đơn hàng' vào Navbar cho Admin

## [Phần 2: Khách hàng tự hủy đơn]

### Nhiệm vụ 5: Xử lý Hủy đơn (User)
- [x] Cập nhật DonHangController.cs, thêm Action HuyDonHang(int id) POST
- [x] Logic bảo mật: Tìm đơn theo id + phải khớp User đang đăng nhập
- [x] Chỉ cho phép hủy nếu trạng thái là 'Chờ xác nhận'
- [x] Dùng TempData gửi thông báo thành công/thất bại

### Nhiệm vụ 6: Giao diện User
- [x] Cập nhật Views/DonHang/LichSu.cshtml: Hiển thị nút 'Hủy đơn' nếu 'Chờ xác nhận'
- [x] Cập nhật Views/DonHang/ChiTiet.cshtml: Hiển thị nút 'Hủy đơn' nếu 'Chờ xác nhận'
- [x] Thêm onclick="return confirm('Bạn có chắc chắn muốn hủy đơn hàng này không?');"

---

# Nhiệm vụ mới - Định dạng tiền và Tích hợp MoMo

## Phần 1: Định dạng giá tiền VNĐ

### Nhiệm vụ 1: Hiển thị giá tiền (Views)
- [x] Rà soát Home/Index: Đã có @item.GiaBan.ToString("N0") đ
- [x] Rà soát Home/Details: Đã có định dạng VNĐ
- [x] Rà soát Cart/Index: Đã có định dạng VNĐ
- [x] Rà soát DonHang/LichSu, ChiTiet: Đã có định dạng VNĐ
- [x] Rà soát AdminDonHang/Index, ChiTiet: Đã có định dạng VNĐ
- [x] Rà soát SanPhams/Index: Đã có định dạng VNĐ

### Nhiệm vụ 2: Nhập liệu giá (Admin)
- [x] Cập nhật SanPhams/Create: Thêm placeholder 'Nhập giá (VD: 130000)'
- [x] Cập nhật SanPhams/Edit: Thêm placeholder 'Nhập giá (VD: 130000)'

## Phần 2: Tích hợp thanh toán MoMo

### Nhiệm vụ 3: Cấu hình MoMo
- [x] Thêm section MomoAPI vào appsettings.json (PartnerCode, AccessKey, SecretKey, MomoApiUrl, ReturnUrl, NotifyUrl)

### Nhiệm vụ 4: Momo Service
- [x] Tạo thư mục Services/Momo
- [x] Tạo MomoService.cs với HMAC-SHA256
- [x] Viết hàm CreatePaymentAsync lấy payUrl

### Nhiệm vụ 5: Xử lý Checkout
- [x] Cập nhật CartController POST Checkout: Nếu chọn MoMo, gọi service và Redirect

### Nhiệm vụ 6: Xử lý Callback
- [x] Tạo Action PaymentCallBack trong CartController
- [x] Kiểm tra resultCode == 0, cập nhật đơn hàng, xóa giỏ hàng

### Nhiệm vụ 7: Giao diện Thanh toán
- [x] Cập nhật Views/Cart/Checkout.cshtml: Bật radio MoMo, định dạng Tổng tiền N0

---

# Nhiệm vụ mới - Đánh giá sao và Bình luận sách (Yêu cầu mới)

## Mô tả chức năng
Hệ thống đánh giá và bình luận cho mỗi cuốn sách, hỗ trợ trả lời bình luận lồng nhau với tag tên người dùng.

## Phần 1: Thiết kế Database

### Nhiệm vụ 1: Tạo Model DanhGia (Đánh giá sao)
- [x] Tạo Model DanhGia với các trường:
  - MaDanhGia (PK, int, auto-increment)
  - MaSanPham (FK, int) - Liên kết với sách
  - MaNguoiDung (FK, int) - Người đánh giá
  - SoSao (int, 1-5) - Số sao đánh giá
  - NoiDung (string, max 1000) - Nội dung bình luận
  - NgayDanhGia (DateTime) - Thời gian đánh giá
  - DaMuaHang (bool) - Đánh dấu đã mua hàng chưa
- [x] Quan hệ: 1 sách có nhiều đánh giá, 1 user có nhiều đánh giá
- [x] Validation: SoSao chỉ từ 1-5, không được null

### Nhiệm vụ 2: Tạo Model PhanHoiBinhLuan (Trả lời bình luận)
- [x] Tạo Model PhanHoiBinhLuan với các trường:
  - MaPhanHoi (PK, int, auto-increment)
  - MaDanhGia (FK, int) - Liên kết với đánh giá gốc
  - MaNguoiDung (FK, int) - Người trả lời
  - NoiDung (string, max 500) - Nội dung phản hồi
  - NgayPhanHoi (DateTime) - Thời gian phản hồi
  - TagNguoiDung (string, nullable) - Tên người được tag (@username)
  - MaPhanHoiCha (int?, nullable) - Self-referencing cho nested replies
- [x] Hỗ trợ trả lời lồng nhau (nested replies) qua MaPhanHoiCha
- [x] Cho phép tag @username trong nội dung

### Nhiệm vụ 3: Migration Database
- [x] Chạy `dotnet ef migrations add AddDanhGiaVaBinhLuan`
- [x] Update database với các bảng mới
- [x] Kiểm tra relationship và foreign key constraints

## Phần 2: Backend Controller & Logic

### Nhiệm vụ 4: Tạo DanhGiaController
- [x] Action DanhGiaSanPham POST: Tạo đánh giá mới (yêu cầu đăng nhập)
- [x] Action CapNhatDanhGia POST: Sửa đánh giá (chỉ chủ sở hữu)
- [x] Action XoaDanhGia POST: Xóa đánh giá (chỉ chủ sở hữu hoặc Admin)
- [x] Action GetDanhGiaBySanPham: Lấy danh sách đánh giá theo sách (có phân trang)
- [x] Action GetDiemTrungBinh: Tính điểm trung bình sao của sách

### Nhiệm vụ 5: Tạo PhanHoiController (đã merge vào DanhGiaController)
- [x] Action TraLoiBinhLuan POST: Trả lời bình luận (yêu cầu đăng nhập)
- [x] Action CapNhatPhanHoi POST: Sửa phản hồi (chỉ chủ sở hữu)
- [x] Action XoaPhanHoi POST: Xóa phản hồi (chỉ chủ sở hữu hoặc Admin)
- [x] Action GetPhanHoiByDanhGia: Lấy danh sách phản hồi theo đánh giá (nested structure)
- [x] Xử lý tag @username trong nội dung (regex tìm @username, hiển thị link)

### Nhiệm vụ 6: Kiểm tra quyền
- [x] Chỉ user đã đăng nhập mới được đánh giá
- [x] Chỉ chủ sở hữu đánh giá mới được sửa/xóa
- [x] Admin có quyền xóa bất kỳ đánh giá/phản hồi nào
- [x] Kiểm tra user đã mua sách mới cho đánh giá "Đã mua" (optional)

## Phần 3: Frontend UI/UX

### Nhiệm vụ 7: Thiết kế giao diện đánh giá sao
- [x] Component chọn số sao (1-5) với hover effect đẹp
- [x] Hiển thị sao bằng Bootstrap Icons (bi-star-fill, bi-star)
- [x] Màu vàng cho sao được chọn (#ffc107)
- [x] Form nhập bình luận với textarea và character counter
- [x] Hiển thị điểm trung bình sách với star progress bar
- [x] Hiển thị tổng số đánh giá và phân bố số sao

### Nhiệm vụ 8: Thiết kế giao diện bình luận
- [x] Danh sách bình luận với avatar user, tên, ngày giờ
- [x] Hiển thị số sao của mỗi đánh giá
- [x] Badge "Đã mua hàng" cho đánh giá từ người đã mua
- [x] Nút "Trả lời" cho mỗi bình luận
- [x] Form trả lời ẩn/hiện khi click "Trả lời"
- [x] Hiển thị phản hồi lồng nhau (nested) với indent/margin left
- [x] Tag @username hiển thị màu xanh và link đến profile (hoặc tooltip)

### Nhiệm vụ 9: Cập nhật View Details.cshtml
- [x] Thêm section Đánh giá & Bình luận vào trang chi tiết sách
- [x] Hiển thị tổng quan: số sao TB, tổng số đánh giá
- [x] Form đánh giá cho user đã đăng nhập (nếu chưa đăng nhập → link login)
- [x] Danh sách đánh giá có phân trang (10 đánh giá/trang)
- [x] Sắp xếp: mới nhất trước hoặc hữu ích nhất (có nhiều phản hồi)

### Nhiệm vụ 10: Responsive & Animation
- [x] Responsive cho mobile: sao to hơn dễ click, form full width
- [x] Animation khi hover vào sao (scale up nhẹ)
- [x] Animation khi submit đánh giá (fade in)
- [x] Loading spinner khi đang tải bình luận

## Phần 4: Kiểm tra & Tối ưu

### Nhiệm vụ 11: Kiểm tra Logic chuyển trang
- [x] Kiểm tra tất cả link trong Home/Index: click sách → Details đúng id
- [x] Kiểm tra Cart/Index: link thanh toán → Checkout
- [x] Kiểm tra DonHang/LichSu: link chi tiết → ChiTiet đúng id
- [x] Kiểm tra AdminDonHang: các action XacNhan, HuyDon redirect đúng
- [x] Kiểm tra phân trang Home: next/prev page hoạt động
- [x] Fix lỗi nếu có (ghi log lỗi vào TempData hoặc Console)

### Nhiệm vụ 12: Kiểm tra Authentication flow
- [x] Kiểm tra đăng nhập: đúng user → trang chủ, sai → báo lỗi
- [x] Kiểm tra đăng ký: validate email unique, password match
- [x] Kiểm tra Authorize attribute hoạt động đúng
- [x] Kiểm tra [Authorize(Roles="Admin")] chỉ admin truy cập

## Phần 5: Gợi ý tính năng bổ sung (Optional)

### Gợi ý 1: Tính năng Yêu thích (Wishlist) ✅
- [x] Thêm nút "Yêu thích" (heart icon) vào mỗi sách
- [x] Tạo trang "Sách yêu thích của tôi"
- [x] Hiển thị số lượt yêu thích trên card sách

### Gợi ý 2: Tính năng So sánh sách ✅
- [x] Cho phép chọn 2-3 sách để so sánh side-by-side
- [x] Bảng so sánh: Giá, Tác giả, Nhà xuất bản, Số trang, Rating

### Gợi ý 3: Tính năng Lịch sử xem (Recently Viewed) ✅
- [x] Lưu sách đã xem vào Session
- [x] Hiển thị "Bạn vừa xem" ở trang chủ

### Gợi ý 4: Tính năng Gợi ý sách (Recommendations) ✅
- [x] Gợi ý sách cùng thể loại
- [x] Gợi ý sách cùng tác giả
- [x] Hiển thị "Sách bạn có thể thích" dựa trên lịch sử mua

### Gợi ý 5: Tính năng Thống kê cho Admin ✅
- [x] Dashboard Admin với biểu đồ: Doanh thu theo tháng
- [x] Top sách bán chạy
- [x] Thống kê đơn hàng theo trạng thái
- [x] Thống kê người dùng mới

### Gợi ý 6: Tính năng Thông báo (Core Implementation) ✅
- [x] Thông báo khi đơn hàng được xác nhận/giao/hủy (qua TempData)
- [x] Thông báo khi có phản hồi bình luận của mình (qua UI)
- [x] Badge số thông báo chưa đọc trên navbar (framework ready)

### Gợi ý 7: Tính năng Khuyến mãi/Mã giảm giá
- [ ] Tạo mã giảm giá (Voucher) cho Admin
- [ ] Áp dụng voucher ở trang Checkout
- [ ] Hiển thị % giảm giá trên sách đang khuyến mãi

### Gợi ý 8: Tính năng Đa ngôn ngữ
- [ ] Hỗ trợ Tiếng Việt và Tiếng Anh
- [ ] Dropdown chọn ngôn ngữ trên navbar
- [ ] Resource files (.resx) cho đa ngôn ngữ

## Acceptance Criteria
- [ ] User có thể đánh giá 1-5 sao cho sách đã xem
- [ ] User có thể viết bình luận chi tiết
- [ ] User có thể trả lời bình luận của người khác (nested replies)
- [ ] User có thể tag @username trong phản hồi
- [ ] Hiển thị điểm trung bình và tổng số đánh giá trên trang chi tiết sách
- [ ] UI đẹp, responsive, có animation
- [ ] Tất cả link chuyển trang hoạt động đúng
- [ ] Xử lý lỗi và validation đầy đủ
