using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlistAndFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SanPhamYeuThichs",
                columns: table => new
                {
                    MaYeuThich = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaNguoiDung = table.Column<int>(type: "INTEGER", nullable: false),
                    MaSanPham = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamYeuThichs", x => x.MaYeuThich);
                    table.ForeignKey(
                        name: "FK_SanPhamYeuThichs_NguoiDungs_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhamYeuThichs_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamYeuThichs_MaNguoiDung_MaSanPham",
                table: "SanPhamYeuThichs",
                columns: new[] { "MaNguoiDung", "MaSanPham" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamYeuThichs_MaSanPham",
                table: "SanPhamYeuThichs",
                column: "MaSanPham");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SanPhamYeuThichs");
        }
    }
}
