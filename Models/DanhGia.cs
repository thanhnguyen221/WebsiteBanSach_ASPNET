using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreWeb.Models
{
    public class DanhGia
    {
        [Key]
        public int MaDanhGia { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        [Display(Name = "Số sao")]
        public int SoSao { get; set; }

        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
        [Display(Name = "Nội dung bình luận")]
        public string? NoiDung { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        [Display(Name = "Đã mua hàng")]
        public bool DaMuaHang { get; set; } = false;

        // Foreign Keys
        [Required]
        [Display(Name = "Mã sản phẩm")]
        public int MaSanPham { get; set; }

        [Required]
        [Display(Name = "Mã người dùng")]
        public int MaNguoiDung { get; set; }

        // Navigation Properties
        [ForeignKey("MaSanPham")]
        public virtual SanPham? SanPham { get; set; }

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }

        // Danh sách phản hồi cho bình luận này
        public virtual ICollection<PhanHoiBinhLuan>? PhanHois { get; set; }
    }
}
