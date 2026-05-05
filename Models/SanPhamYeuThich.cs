using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreWeb.Models
{
    public class SanPhamYeuThich
    {
        [Key]
        public int MaYeuThich { get; set; }

        [Required]
        [Display(Name = "Mã người dùng")]
        public int MaNguoiDung { get; set; }

        [Required]
        [Display(Name = "Mã sản phẩm")]
        public int MaSanPham { get; set; }

        [Display(Name = "Ngày thêm")]
        public DateTime NgayThem { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [ForeignKey("MaSanPham")]
        public virtual SanPham? SanPham { get; set; }
    }
}
