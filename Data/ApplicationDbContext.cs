using Microsoft.EntityFrameworkCore;
using BookStoreWeb.Models;

namespace BookStoreWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<PhanHoiBinhLuan> PhanHoiBinhLuans { get; set; }
        public DbSet<SanPhamYeuThich> SanPhamYeuThichs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình tên bảng (tùy chọn)
            modelBuilder.Entity<SanPham>().ToTable("SanPhams");
            modelBuilder.Entity<NguoiDung>().ToTable("NguoiDungs");
            modelBuilder.Entity<DonHang>().ToTable("DonHangs");
            modelBuilder.Entity<ChiTietDonHang>().ToTable("ChiTietDonHangs");
            modelBuilder.Entity<DanhGia>().ToTable("DanhGias");
            modelBuilder.Entity<PhanHoiBinhLuan>().ToTable("PhanHoiBinhLuans");
            modelBuilder.Entity<SanPhamYeuThich>().ToTable("SanPhamYeuThichs");

            // Cấu hình quan hệ
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.NguoiDung)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(c => c.MaDonHang)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.SanPham)
                .WithMany()
                .HasForeignKey(c => c.MaSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ cho DanhGia
            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.SanPham)
                .WithMany(s => s.DanhGias)
                .HasForeignKey(d => d.MaSanPham)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.NguoiDung)
                .WithMany(u => u.DanhGias)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ cho PhanHoiBinhLuan
            modelBuilder.Entity<PhanHoiBinhLuan>()
                .HasOne(p => p.DanhGia)
                .WithMany(d => d.PhanHois)
                .HasForeignKey(p => p.MaDanhGia)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PhanHoiBinhLuan>()
                .HasOne(p => p.NguoiDung)
                .WithMany(u => u.PhanHoiBinhLuans)
                .HasForeignKey(p => p.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình self-referencing cho nested replies
            modelBuilder.Entity<PhanHoiBinhLuan>()
                .HasOne(p => p.PhanHoiCha)
                .WithMany(p => p.PhanHoiCons)
                .HasForeignKey(p => p.MaPhanHoiCha)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình quan hệ cho SanPhamYeuThich
            modelBuilder.Entity<SanPhamYeuThich>()
                .HasOne(w => w.NguoiDung)
                .WithMany()
                .HasForeignKey(w => w.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SanPhamYeuThich>()
                .HasOne(w => w.SanPham)
                .WithMany()
                .HasForeignKey(w => w.MaSanPham)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: Mỗi user chỉ yêu thích 1 sách 1 lần
            modelBuilder.Entity<SanPhamYeuThich>()
                .HasIndex(w => new { w.MaNguoiDung, w.MaSanPham })
                .IsUnique();
        }
    }
}
