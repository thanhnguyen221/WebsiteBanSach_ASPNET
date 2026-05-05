using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddDanhGiaVaBinhLuan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhGias",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoSao = table.Column<int>(type: "INTEGER", nullable: false),
                    NoiDung = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DaMuaHang = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaSanPham = table.Column<int>(type: "INTEGER", nullable: false),
                    MaNguoiDung = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGias", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGias_NguoiDungs_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGias_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhanHoiBinhLuans",
                columns: table => new
                {
                    MaPhanHoi = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NoiDung = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    NgayPhanHoi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TagNguoiDung = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MaDanhGia = table.Column<int>(type: "INTEGER", nullable: false),
                    MaNguoiDung = table.Column<int>(type: "INTEGER", nullable: false),
                    MaPhanHoiCha = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanHoiBinhLuans", x => x.MaPhanHoi);
                    table.ForeignKey(
                        name: "FK_PhanHoiBinhLuans_DanhGias_MaDanhGia",
                        column: x => x.MaDanhGia,
                        principalTable: "DanhGias",
                        principalColumn: "MaDanhGia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhanHoiBinhLuans_NguoiDungs_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhanHoiBinhLuans_PhanHoiBinhLuans_MaPhanHoiCha",
                        column: x => x.MaPhanHoiCha,
                        principalTable: "PhanHoiBinhLuans",
                        principalColumn: "MaPhanHoi");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaNguoiDung",
                table: "DanhGias",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaSanPham",
                table: "DanhGias",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_PhanHoiBinhLuans_MaDanhGia",
                table: "PhanHoiBinhLuans",
                column: "MaDanhGia");

            migrationBuilder.CreateIndex(
                name: "IX_PhanHoiBinhLuans_MaNguoiDung",
                table: "PhanHoiBinhLuans",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_PhanHoiBinhLuans_MaPhanHoiCha",
                table: "PhanHoiBinhLuans",
                column: "MaPhanHoiCha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhanHoiBinhLuans");

            migrationBuilder.DropTable(
                name: "DanhGias");
        }
    }
}
