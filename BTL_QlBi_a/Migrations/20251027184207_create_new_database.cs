using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_QlBi_a.Migrations
{
    /// <inheritdoc />
    public partial class create_new_database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "khach_hang",
                columns: table => new
                {
                    ma_kh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_kh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ngay_sinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    hang_tv = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Dong"),
                    diem_tich_luy = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    tong_chi_tieu = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    ngay_dang_ky = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    lan_den_cuoi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khach_hang", x => x.ma_kh);
                });

            migrationBuilder.CreateTable(
                name: "loai_ban",
                columns: table => new
                {
                    ma_loai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_loai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    gia_gio = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "DangApDung")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loai_ban", x => x.ma_loai);
                });

            migrationBuilder.CreateTable(
                name: "mat_hang",
                columns: table => new
                {
                    ma_hang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_hang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    loai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    don_vi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "cái"),
                    gia = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    so_luong_ton = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    nha_cung_cap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ngay_nhap_gan_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "ConHang"),
                    mo_ta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    hinh_anh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mat_hang", x => x.ma_hang);
                });

            migrationBuilder.CreateTable(
                name: "nhan_vien",
                columns: table => new
                {
                    ma_nv = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_nv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ma_van_tay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    faceid_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    faceid_anh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    chuc_vu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    luong_co_ban = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    phu_cap = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    ngay_vao_lam = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ca_mac_dinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "Sang"),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "DangLam"),
                    mat_khau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nhan_vien", x => x.ma_nv);
                });

            migrationBuilder.CreateTable(
                name: "ban_bia",
                columns: table => new
                {
                    ma_ban = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_ban = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ma_loai = table.Column<int>(type: "int", nullable: false),
                    khu_vuc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Tang1"),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Trong"),
                    gio_bat_dau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ma_kh = table.Column<int>(type: "int", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ban_bia", x => x.ma_ban);
                    table.ForeignKey(
                        name: "FK_ban_bia_khach_hang_ma_kh",
                        column: x => x.ma_kh,
                        principalTable: "khach_hang",
                        principalColumn: "ma_kh",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ban_bia_loai_ban_ma_loai",
                        column: x => x.ma_loai,
                        principalTable: "loai_ban",
                        principalColumn: "ma_loai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gia_gio_choi",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    khung_gio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    gio_bat_dau = table.Column<TimeSpan>(type: "time", nullable: false),
                    gio_ket_thuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    ma_loai = table.Column<int>(type: "int", nullable: true),
                    gia = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    ap_dung_tu_ngay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "DangApDung")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gia_gio_choi", x => x.id);
                    table.ForeignKey(
                        name: "FK_gia_gio_choi_loai_ban_ma_loai",
                        column: x => x.ma_loai,
                        principalTable: "loai_ban",
                        principalColumn: "ma_loai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dich_vu",
                columns: table => new
                {
                    ma_dv = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_dv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    loai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    gia = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    don_vi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "phần"),
                    so_luong_ton = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ma_hang = table.Column<int>(type: "int", nullable: true),
                    ti_le_loi_nhuan = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 30.00m),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "ConHang"),
                    mo_ta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    hinh_anh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dich_vu", x => x.ma_dv);
                    table.ForeignKey(
                        name: "FK_dich_vu_mat_hang_ma_hang",
                        column: x => x.ma_hang,
                        principalTable: "mat_hang",
                        principalColumn: "ma_hang",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "bang_luong",
                columns: table => new
                {
                    ma_luong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nv = table.Column<int>(type: "int", nullable: false),
                    thang = table.Column<int>(type: "int", nullable: false),
                    nam = table.Column<int>(type: "int", nullable: false),
                    tong_gio = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    luong_co_ban = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    phu_cap = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    thuong = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    phat = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    tong_luong = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    ngay_tinh = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bang_luong", x => x.ma_luong);
                    table.ForeignKey(
                        name: "FK_bang_luong_nhan_vien_ma_nv",
                        column: x => x.ma_nv,
                        principalTable: "nhan_vien",
                        principalColumn: "ma_nv",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cham_cong",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nv = table.Column<int>(type: "int", nullable: false),
                    ngay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    gio_vao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    gio_ra = table.Column<DateTime>(type: "datetime2", nullable: true),
                    hinh_anh_vao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    hinh_anh_ra = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    xac_thuc_bang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "ThuCong"),
                    so_gio_lam = table.Column<decimal>(type: "decimal(5,2)", nullable: true, computedColumnSql: "(CASE WHEN [gio_ra] IS NOT NULL THEN CAST(DATEDIFF(MINUTE, [gio_vao], [gio_ra]) AS DECIMAL(5,2)) / 60.0 ELSE 0 END)", stored: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "DungGio"),
                    ghi_chu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cham_cong", x => x.id);
                    table.ForeignKey(
                        name: "FK_cham_cong_nhan_vien_ma_nv",
                        column: x => x.ma_nv,
                        principalTable: "nhan_vien",
                        principalColumn: "ma_nv",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lich_su_hoat_dong",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    thoi_gian = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ma_nv = table.Column<int>(type: "int", nullable: true),
                    hanh_dong = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    chi_tiet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_su_hoat_dong", x => x.id);
                    table.ForeignKey(
                        name: "FK_lich_su_hoat_dong_nhan_vien_ma_nv",
                        column: x => x.ma_nv,
                        principalTable: "nhan_vien",
                        principalColumn: "ma_nv",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "dat_ban",
                columns: table => new
                {
                    ma_dat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ban = table.Column<int>(type: "int", nullable: true),
                    ma_kh = table.Column<int>(type: "int", nullable: true),
                    ten_khach = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    thoi_gian_dat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    so_nguoi = table.Column<int>(type: "int", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "DangCho"),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dat_ban", x => x.ma_dat);
                    table.ForeignKey(
                        name: "FK_dat_ban_ban_bia_ma_ban",
                        column: x => x.ma_ban,
                        principalTable: "ban_bia",
                        principalColumn: "ma_ban",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dat_ban_khach_hang_ma_kh",
                        column: x => x.ma_kh,
                        principalTable: "khach_hang",
                        principalColumn: "ma_kh",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "hoa_don",
                columns: table => new
                {
                    ma_hd = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ban = table.Column<int>(type: "int", nullable: true),
                    ma_kh = table.Column<int>(type: "int", nullable: true),
                    ma_nv = table.Column<int>(type: "int", nullable: true),
                    thoi_gian_bat_dau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    thoi_gian_ket_thuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    thoi_luong_phut = table.Column<int>(type: "int", nullable: true),
                    tien_ban = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    tien_dich_vu = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    giam_gia = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    tong_tien = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    phuong_thuc_thanh_toan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "TienMat"),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "DangChoi"),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hoa_don", x => x.ma_hd);
                    table.ForeignKey(
                        name: "FK_hoa_don_ban_bia_ma_ban",
                        column: x => x.ma_ban,
                        principalTable: "ban_bia",
                        principalColumn: "ma_ban",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hoa_don_khach_hang_ma_kh",
                        column: x => x.ma_kh,
                        principalTable: "khach_hang",
                        principalColumn: "ma_kh",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_hoa_don_nhan_vien_ma_nv",
                        column: x => x.ma_nv,
                        principalTable: "nhan_vien",
                        principalColumn: "ma_nv",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chi_tiet_hoa_don",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_hd = table.Column<int>(type: "int", nullable: true),
                    ma_dv = table.Column<int>(type: "int", nullable: true),
                    so_luong = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    thanh_tien = table.Column<decimal>(type: "decimal(12,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chi_tiet_hoa_don", x => x.id);
                    table.ForeignKey(
                        name: "FK_chi_tiet_hoa_don_dich_vu_ma_dv",
                        column: x => x.ma_dv,
                        principalTable: "dich_vu",
                        principalColumn: "ma_dv",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chi_tiet_hoa_don_hoa_don_ma_hd",
                        column: x => x.ma_hd,
                        principalTable: "hoa_don",
                        principalColumn: "ma_hd",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ban_bia_ma_kh",
                table: "ban_bia",
                column: "ma_kh");

            migrationBuilder.CreateIndex(
                name: "IX_ban_bia_ma_loai",
                table: "ban_bia",
                column: "ma_loai");

            migrationBuilder.CreateIndex(
                name: "IX_ban_bia_ten_ban",
                table: "ban_bia",
                column: "ten_ban",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bang_luong_ma_nv",
                table: "bang_luong",
                column: "ma_nv");

            migrationBuilder.CreateIndex(
                name: "IX_cham_cong_ma_nv",
                table: "cham_cong",
                column: "ma_nv");

            migrationBuilder.CreateIndex(
                name: "IX_chi_tiet_hoa_don_ma_dv",
                table: "chi_tiet_hoa_don",
                column: "ma_dv");

            migrationBuilder.CreateIndex(
                name: "IX_chi_tiet_hoa_don_ma_hd",
                table: "chi_tiet_hoa_don",
                column: "ma_hd");

            migrationBuilder.CreateIndex(
                name: "IX_dat_ban_ma_ban",
                table: "dat_ban",
                column: "ma_ban");

            migrationBuilder.CreateIndex(
                name: "IX_dat_ban_ma_kh",
                table: "dat_ban",
                column: "ma_kh");

            migrationBuilder.CreateIndex(
                name: "IX_dich_vu_ma_hang",
                table: "dich_vu",
                column: "ma_hang");

            migrationBuilder.CreateIndex(
                name: "IX_gia_gio_choi_ma_loai",
                table: "gia_gio_choi",
                column: "ma_loai");

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_ma_ban",
                table: "hoa_don",
                column: "ma_ban");

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_ma_kh",
                table: "hoa_don",
                column: "ma_kh");

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_ma_nv",
                table: "hoa_don",
                column: "ma_nv");

            migrationBuilder.CreateIndex(
                name: "IX_khach_hang_sdt",
                table: "khach_hang",
                column: "sdt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_hoat_dong_ma_nv",
                table: "lich_su_hoat_dong",
                column: "ma_nv");

            migrationBuilder.CreateIndex(
                name: "IX_loai_ban_ten_loai",
                table: "loai_ban",
                column: "ten_loai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nhan_vien_faceid_hash",
                table: "nhan_vien",
                column: "faceid_hash",
                unique: true,
                filter: "[faceid_hash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_nhan_vien_ma_van_tay",
                table: "nhan_vien",
                column: "ma_van_tay",
                unique: true,
                filter: "[ma_van_tay] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bang_luong");

            migrationBuilder.DropTable(
                name: "cham_cong");

            migrationBuilder.DropTable(
                name: "chi_tiet_hoa_don");

            migrationBuilder.DropTable(
                name: "dat_ban");

            migrationBuilder.DropTable(
                name: "gia_gio_choi");

            migrationBuilder.DropTable(
                name: "lich_su_hoat_dong");

            migrationBuilder.DropTable(
                name: "dich_vu");

            migrationBuilder.DropTable(
                name: "hoa_don");

            migrationBuilder.DropTable(
                name: "mat_hang");

            migrationBuilder.DropTable(
                name: "ban_bia");

            migrationBuilder.DropTable(
                name: "nhan_vien");

            migrationBuilder.DropTable(
                name: "khach_hang");

            migrationBuilder.DropTable(
                name: "loai_ban");
        }
    }
}
