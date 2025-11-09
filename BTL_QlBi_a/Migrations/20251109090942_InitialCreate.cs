using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_QlBi_a.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chuc_nang",
                columns: table => new
                {
                    ma_cn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_cn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chuc_nang", x => x.ma_cn);
                });

            migrationBuilder.CreateTable(
                name: "khach_hang",
                columns: table => new
                {
                    ma_kh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_kh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    mat_khau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ngay_sinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    hang_tv = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Đồng"),
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
                name: "khu_vuc",
                columns: table => new
                {
                    ma_khu_vuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_khu_vuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khu_vuc", x => x.ma_khu_vuc);
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
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Đang áp dụng")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loai_ban", x => x.ma_loai);
                });

            migrationBuilder.CreateTable(
                name: "nha_cung_cap",
                columns: table => new
                {
                    ma_ncc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_ncc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    dia_chi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nha_cung_cap", x => x.ma_ncc);
                });

            migrationBuilder.CreateTable(
                name: "nhom_quyen",
                columns: table => new
                {
                    ma_nhom = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_nhom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nhom_quyen", x => x.ma_nhom);
                });

            migrationBuilder.CreateTable(
                name: "ban_bia",
                columns: table => new
                {
                    ma_ban = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_ban = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ma_loai = table.Column<int>(type: "int", nullable: false),
                    ma_khu_vuc = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Trống"),
                    gio_bat_dau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ma_kh = table.Column<int>(type: "int", nullable: true),
                    vi_tri_x = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    vi_tri_y = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    hinh_anh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
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
                        name: "FK_ban_bia_khu_vuc_ma_khu_vuc",
                        column: x => x.ma_khu_vuc,
                        principalTable: "khu_vuc",
                        principalColumn: "ma_khu_vuc",
                        onDelete: ReferentialAction.Restrict);
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
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Đang áp dụng")
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
                name: "mat_hang",
                columns: table => new
                {
                    ma_hang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_hang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    loai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Khác"),
                    don_vi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "cái"),
                    gia = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    so_luong_ton = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    nguong_canh_bao = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    ma_ncc_default = table.Column<int>(type: "int", nullable: true),
                    ngay_nhap_gan_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Còn hàng"),
                    mo_ta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    hinh_anh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mat_hang", x => x.ma_hang);
                    table.ForeignKey(
                        name: "FK_mat_hang_nha_cung_cap_ma_ncc_default",
                        column: x => x.ma_ncc_default,
                        principalTable: "nha_cung_cap",
                        principalColumn: "ma_ncc",
                        onDelete: ReferentialAction.SetNull);
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
                    ma_nhom = table.Column<int>(type: "int", nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    luong_co_ban = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    phu_cap = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    ca_mac_dinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Sáng"),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Đang làm"),
                    mat_khau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nhan_vien", x => x.ma_nv);
                    table.ForeignKey(
                        name: "FK_nhan_vien_nhom_quyen_ma_nhom",
                        column: x => x.ma_nhom,
                        principalTable: "nhom_quyen",
                        principalColumn: "ma_nhom",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "phan_quyen",
                columns: table => new
                {
                    ma_nhom = table.Column<int>(type: "int", nullable: false),
                    ma_cn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phan_quyen", x => new { x.ma_nhom, x.ma_cn });
                    table.ForeignKey(
                        name: "FK_phan_quyen_chuc_nang_ma_cn",
                        column: x => x.ma_cn,
                        principalTable: "chuc_nang",
                        principalColumn: "ma_cn",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_phan_quyen_nhom_quyen_ma_nhom",
                        column: x => x.ma_nhom,
                        principalTable: "nhom_quyen",
                        principalColumn: "ma_nhom",
                        onDelete: ReferentialAction.Cascade);
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
                    thoi_gian_dat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    so_nguoi = table.Column<int>(type: "int", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Đang chờ"),
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
                name: "dich_vu",
                columns: table => new
                {
                    ma_dv = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_dv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    loai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Khác"),
                    gia = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    don_vi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "phần"),
                    ma_hang = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Còn hàng"),
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
                    xac_thuc_bang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Thủ công"),
                    so_gio_lam = table.Column<decimal>(type: "decimal(5,2)", nullable: true, computedColumnSql: "(CASE WHEN [gio_ra] IS NOT NULL THEN CAST(DATEDIFF(MINUTE, [gio_vao], [gio_ra]) AS DECIMAL(5,2)) / 60.0 ELSE 0 END)", stored: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Đúng giờ"),
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
                    thoi_luong_phut = table.Column<int>(type: "int", nullable: true, computedColumnSql: "(CASE WHEN [thoi_gian_ket_thuc] IS NOT NULL THEN DATEDIFF(MINUTE, [thoi_gian_bat_dau], [thoi_gian_ket_thuc]) ELSE 0 END)", stored: true),
                    tien_ban = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    tien_dich_vu = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    giam_gia = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    ghi_chu_giam_gia = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    tong_tien = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    phuong_thuc_thanh_toan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Tiền mặt"),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Đang chơi"),
                    ma_giao_dich_qr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    qr_code_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                name: "phieu_nhap",
                columns: table => new
                {
                    ma_pn = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nv = table.Column<int>(type: "int", nullable: false),
                    ma_ncc = table.Column<int>(type: "int", nullable: false),
                    ngay_nhap = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    tong_tien = table.Column<decimal>(type: "decimal(12,0)", nullable: false, defaultValue: 0m),
                    ghi_chu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phieu_nhap", x => x.ma_pn);
                    table.ForeignKey(
                        name: "FK_phieu_nhap_nha_cung_cap_ma_ncc",
                        column: x => x.ma_ncc,
                        principalTable: "nha_cung_cap",
                        principalColumn: "ma_ncc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_phieu_nhap_nhan_vien_ma_nv",
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

            migrationBuilder.CreateTable(
                name: "so_quy",
                columns: table => new
                {
                    ma_phieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nv = table.Column<int>(type: "int", nullable: false),
                    ngay_lap = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    loai_phieu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_tien = table.Column<decimal>(type: "decimal(12,0)", nullable: false),
                    ly_do = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ma_hd_lien_quan = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_so_quy", x => x.ma_phieu);
                    table.ForeignKey(
                        name: "FK_so_quy_hoa_don_ma_hd_lien_quan",
                        column: x => x.ma_hd_lien_quan,
                        principalTable: "hoa_don",
                        principalColumn: "ma_hd",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_so_quy_nhan_vien_ma_nv",
                        column: x => x.ma_nv,
                        principalTable: "nhan_vien",
                        principalColumn: "ma_nv",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chi_tiet_phieu_nhap",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_pn = table.Column<int>(type: "int", nullable: false),
                    ma_hang = table.Column<int>(type: "int", nullable: false),
                    so_luong_nhap = table.Column<int>(type: "int", nullable: false),
                    don_gia_nhap = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    thanh_tien = table.Column<decimal>(type: "decimal(10,0)", nullable: true, computedColumnSql: "[so_luong_nhap] * [don_gia_nhap]", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chi_tiet_phieu_nhap", x => x.id);
                    table.ForeignKey(
                        name: "FK_chi_tiet_phieu_nhap_mat_hang_ma_hang",
                        column: x => x.ma_hang,
                        principalTable: "mat_hang",
                        principalColumn: "ma_hang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chi_tiet_phieu_nhap_phieu_nhap_ma_pn",
                        column: x => x.ma_pn,
                        principalTable: "phieu_nhap",
                        principalColumn: "ma_pn",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ban_bia_ma_kh",
                table: "ban_bia",
                column: "ma_kh");

            migrationBuilder.CreateIndex(
                name: "IX_ban_bia_ma_khu_vuc",
                table: "ban_bia",
                column: "ma_khu_vuc");

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
                name: "IX_chi_tiet_phieu_nhap_ma_hang",
                table: "chi_tiet_phieu_nhap",
                column: "ma_hang");

            migrationBuilder.CreateIndex(
                name: "IX_chi_tiet_phieu_nhap_ma_pn",
                table: "chi_tiet_phieu_nhap",
                column: "ma_pn");

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
                unique: true,
                filter: "[sdt] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_khu_vuc_ten_khu_vuc",
                table: "khu_vuc",
                column: "ten_khu_vuc",
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
                name: "IX_mat_hang_ma_ncc_default",
                table: "mat_hang",
                column: "ma_ncc_default");

            migrationBuilder.CreateIndex(
                name: "IX_nhan_vien_faceid_hash",
                table: "nhan_vien",
                column: "faceid_hash",
                unique: true,
                filter: "[faceid_hash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_nhan_vien_ma_nhom",
                table: "nhan_vien",
                column: "ma_nhom");

            migrationBuilder.CreateIndex(
                name: "IX_nhan_vien_ma_van_tay",
                table: "nhan_vien",
                column: "ma_van_tay",
                unique: true,
                filter: "[ma_van_tay] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_nhom_quyen_ten_nhom",
                table: "nhom_quyen",
                column: "ten_nhom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phan_quyen_ma_cn",
                table: "phan_quyen",
                column: "ma_cn");

            migrationBuilder.CreateIndex(
                name: "IX_phieu_nhap_ma_ncc",
                table: "phieu_nhap",
                column: "ma_ncc");

            migrationBuilder.CreateIndex(
                name: "IX_phieu_nhap_ma_nv",
                table: "phieu_nhap",
                column: "ma_nv");

            migrationBuilder.CreateIndex(
                name: "IX_so_quy_ma_hd_lien_quan",
                table: "so_quy",
                column: "ma_hd_lien_quan");

            migrationBuilder.CreateIndex(
                name: "IX_so_quy_ma_nv",
                table: "so_quy",
                column: "ma_nv");
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
                name: "chi_tiet_phieu_nhap");

            migrationBuilder.DropTable(
                name: "dat_ban");

            migrationBuilder.DropTable(
                name: "gia_gio_choi");

            migrationBuilder.DropTable(
                name: "lich_su_hoat_dong");

            migrationBuilder.DropTable(
                name: "phan_quyen");

            migrationBuilder.DropTable(
                name: "so_quy");

            migrationBuilder.DropTable(
                name: "dich_vu");

            migrationBuilder.DropTable(
                name: "phieu_nhap");

            migrationBuilder.DropTable(
                name: "chuc_nang");

            migrationBuilder.DropTable(
                name: "hoa_don");

            migrationBuilder.DropTable(
                name: "mat_hang");

            migrationBuilder.DropTable(
                name: "ban_bia");

            migrationBuilder.DropTable(
                name: "nhan_vien");

            migrationBuilder.DropTable(
                name: "nha_cung_cap");

            migrationBuilder.DropTable(
                name: "khach_hang");

            migrationBuilder.DropTable(
                name: "khu_vuc");

            migrationBuilder.DropTable(
                name: "loai_ban");

            migrationBuilder.DropTable(
                name: "nhom_quyen");
        }
    }
}
