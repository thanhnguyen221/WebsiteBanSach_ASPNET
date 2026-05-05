using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreWeb.Models
{
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        [Required(ErrorMessage = "Mã người dùng không được để trống")]
        [Display(Name = "Mã người dùng")]
        public int MaNguoiDung { get; set; }

        [ForeignKey("MaNguoiDung")]
        [Display(Name = "Người dùng")]
        public NguoiDung? NguoiDung { get; set; }

        [Required(ErrorMessage = "Ngày đặt hàng không được để trống")]
        [Display(Name = "Ngày đặt hàng")]
        public DateTime NgayDatHang { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Tổng tiền không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "ChoXacNhan";

        [StringLength(50, ErrorMessage = "Phương thức thanh toán không được vượt quá 50 ký tự")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = "Tiền mặt";

        [StringLength(200, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 200 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string? DiaChiGiaoHang { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }

        [Display(Name = "Chi tiết đơn hàng")]
        public List<ChiTietDonHang>? ChiTietDonHangs { get; set; }
    }

    public class ChiTietDonHang
    {
        [Key]
        public int MaChiTiet { get; set; }

        [Required(ErrorMessage = "Mã đơn hàng không được để trống")]
        [Display(Name = "Mã đơn hàng")]
        public int MaDonHang { get; set; }

        [ForeignKey("MaDonHang")]
        [Display(Name = "Đơn hàng")]
        public DonHang? DonHang { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [Display(Name = "Mã sản phẩm")]
        public int MaSanPham { get; set; }

        [ForeignKey("MaSanPham")]
        [Display(Name = "Sản phẩm")]
        public SanPham? SanPham { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
