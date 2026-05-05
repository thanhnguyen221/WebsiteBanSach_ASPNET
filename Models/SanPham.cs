using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreWeb.Models
{
    public class SanPham
    {
        [Key]
        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sách không được vượt quá 200 ký tự")]
        [Display(Name = "Tên sách")]
        public string TenSach { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Tác giả không được vượt quá 100 ký tự")]
        [Display(Name = "Tác giả")]
        public string? TacGia { get; set; }

        [StringLength(100, ErrorMessage = "Nhà xuất bản không được vượt quá 100 ký tự")]
        [Display(Name = "Nhà xuất bản")]
        public string? NhaXuatBan { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Năm xuất bản không hợp lệ")]
        [Display(Name = "Năm xuất bản")]
        public int? NamXuatBan { get; set; }

        [StringLength(50, ErrorMessage = "Thể loại không được vượt quá 50 ký tự")]
        [Display(Name = "Thể loại")]
        public string? TheLoai { get; set; }

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá bán")]
        public decimal GiaBan { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng tồn")]
        public int SoLuongTon { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [StringLength(255, ErrorMessage = "Đường dẫn ảnh không được vượt quá 255 ký tự")]
        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Còn hiệu lực")]
        public bool ConHieuLuc { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<DanhGia>? DanhGias { get; set; }
    }
}
