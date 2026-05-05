using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreWeb.Models
{
    public class PhanHoiBinhLuan
    {
        [Key]
        public int MaPhanHoi { get; set; }

        [Required(ErrorMessage = "Nội dung phản hồi không được để trống")]
        [StringLength(500, ErrorMessage = "Phản hồi không được vượt quá 500 ký tự")]
        [Display(Name = "Nội dung phản hồi")]
        public string NoiDung { get; set; } = string.Empty;

        [Display(Name = "Ngày phản hồi")]
        public DateTime NgayPhanHoi { get; set; } = DateTime.Now;

        // Tag người dùng được nhắc đến (ví dụ: @username)
        [StringLength(50, ErrorMessage = "Tên tag không được vượt quá 50 ký tự")]
        [Display(Name = "Tag người dùng")]
        public string? TagNguoiDung { get; set; }

        // Foreign Keys
        [Required]
        [Display(Name = "Mã đánh giá")]
        public int MaDanhGia { get; set; }

        [Required]
        [Display(Name = "Mã người dùng")]
        public int MaNguoiDung { get; set; }

        // Self-referencing cho nested replies (trả lời lồng nhau)
        [Display(Name = "Mã phản hồi cha")]
        public int? MaPhanHoiCha { get; set; }

        // Navigation Properties
        [ForeignKey("MaDanhGia")]
        public virtual DanhGia? DanhGia { get; set; }

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }

        // Phản hồi cha
        [ForeignKey("MaPhanHoiCha")]
        public virtual PhanHoiBinhLuan? PhanHoiCha { get; set; }

        // Danh sách phản hồi con (nested replies)
        public virtual ICollection<PhanHoiBinhLuan>? PhanHoiCons { get; set; }
    }
}
