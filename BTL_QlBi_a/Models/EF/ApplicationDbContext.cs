using BTL_QlBi_a.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_QlBi_a.Models.EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<LoaiBan> LoaiBan { get; set; }
        public DbSet<BanBi_a> BanBi_a { get; set; }
        public DbSet<DatBan> DatBan { get; set; }
        public DbSet<MatHang> MatHang { get; set; }
        public DbSet<DichVu> DichVu { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<ChamCong> ChamCong { get; set; }
        public DbSet<BangLuong> BangLuong { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDon { get; set; }
        public DbSet<GiaGioChoi> GiaGioChoi { get; set; }
        public DbSet<LichSuHoatDong> LichSuHoatDong { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KhachHang>(entity =>
            {
                entity.ToTable("khach_hang");
                entity.HasKey(e => e.MaKH);

                entity.Property(e => e.MaKH).HasColumnName("ma_kh");
                entity.Property(e => e.TenKH).HasColumnName("ten_kh").HasMaxLength(100).IsRequired();
                entity.Property(e => e.SDT).HasColumnName("sdt").HasMaxLength(15);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
                entity.Property(e => e.NgaySinh).HasColumnName("ngay_sinh");
                entity.Property(e => e.HangTV).HasColumnName("hang_tv").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(HangThanhVien.Dong);
                entity.Property(e => e.DiemTichLuy).HasColumnName("diem_tich_luy").HasDefaultValue(0);
                entity.Property(e => e.TongChiTieu).HasColumnName("tong_chi_tieu").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.NgayDangKy).HasColumnName("ngay_dang_ky").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.LanDenCuoi).HasColumnName("lan_den_cuoi");

                entity.HasIndex(e => e.SDT).IsUnique();
            });

            modelBuilder.Entity<LoaiBan>(entity =>
            {
                entity.ToTable("loai_ban");
                entity.HasKey(e => e.MaLoai);

                entity.Property(e => e.MaLoai).HasColumnName("ma_loai");
                entity.Property(e => e.TenLoai).HasColumnName("ten_loai").HasMaxLength(50).IsRequired();
                entity.Property(e => e.MoTa).HasColumnName("mo_ta").HasMaxLength(255);
                entity.Property(e => e.GiaGio).HasColumnName("gia_gio").HasColumnType("decimal(10,0)").IsRequired();
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiLoaiBan.DangApDung);

                entity.HasIndex(e => e.TenLoai).IsUnique();
            });

            modelBuilder.Entity<BanBi_a>(entity =>
            {
                entity.ToTable("ban_bia");
                entity.HasKey(e => e.MaBan);

                entity.Property(e => e.MaBan).HasColumnName("ma_ban");
                entity.Property(e => e.TenBan).HasColumnName("ten_ban").HasMaxLength(50).IsRequired();
                entity.Property(e => e.HinhAnh)
                    .HasColumnName("hinh_anh")
                    .HasMaxLength(255);
                entity.Property(e => e.MaLoai).HasColumnName("ma_loai").IsRequired();
                entity.Property(e => e.KhuVuc).HasColumnName("khu_vuc").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(KhuVucBan.Tang1);
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiBan.Trong);
                entity.Property(e => e.GioBatDau).HasColumnName("gio_bat_dau");
                entity.Property(e => e.MaKH).HasColumnName("ma_kh");
                entity.Property(e => e.GhiChu).HasColumnName("ghi_chu").HasMaxLength(255);
                entity.Property(e => e.NgayTao).HasColumnName("ngay_tao").HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.TenBan).IsUnique();

                entity.HasOne(e => e.LoaiBan)
                    .WithMany(l => l.BanBi_a)
                    .HasForeignKey(e => e.MaLoai)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.KhachHang)
                    .WithMany(k => k.BanBias)
                    .HasForeignKey(e => e.MaKH)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DatBan>(entity =>
            {
                entity.ToTable("dat_ban");
                entity.HasKey(e => e.MaDat);

                entity.Property(e => e.MaDat).HasColumnName("ma_dat");
                entity.Property(e => e.MaBan).HasColumnName("ma_ban");
                entity.Property(e => e.MaKH).HasColumnName("ma_kh");
                entity.Property(e => e.TenKhach).HasColumnName("ten_khach").HasMaxLength(100).IsRequired();
                entity.Property(e => e.SDT).HasColumnName("sdt").HasMaxLength(15).IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100); 
                entity.Property(e => e.ThoiGianDat).HasColumnName("thoi_gian_dat").IsRequired();
                entity.Property(e => e.SoNguoi).HasColumnName("so_nguoi");
                entity.Property(e => e.GhiChu).HasColumnName("ghi_chu");
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiDatBan.DangCho);
                entity.Property(e => e.NgayTao).HasColumnName("ngay_tao").HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.BanBia)
                    .WithMany(b => b.DatBans)
                    .HasForeignKey(e => e.MaBan)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.KhachHang)
                    .WithMany(k => k.DatBans)
                    .HasForeignKey(e => e.MaKH)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MatHang>(entity =>
            {
                entity.ToTable("mat_hang");
                entity.HasKey(e => e.MaHang);

                entity.Property(e => e.MaHang).HasColumnName("ma_hang");
                entity.Property(e => e.TenHang).HasColumnName("ten_hang").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Loai).HasColumnName("loai").HasMaxLength(20)
                    .HasConversion<string>(); 
                entity.Property(e => e.DonVi).HasColumnName("don_vi").HasMaxLength(50).HasDefaultValue("cái");
                entity.Property(e => e.Gia).HasColumnName("gia").HasColumnType("decimal(10,0)").IsRequired();
                entity.Property(e => e.SoLuongTon).HasColumnName("so_luong_ton").HasDefaultValue(0);
                entity.Property(e => e.NhaCungCap).HasColumnName("nha_cung_cap").HasMaxLength(100);
                entity.Property(e => e.NgayNhapGanNhat).HasColumnName("ngay_nhap_gan_nhat");
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiMatHang.ConHang);
                entity.Property(e => e.MoTa).HasColumnName("mo_ta").HasMaxLength(255);
                entity.Property(e => e.HinhAnh).HasColumnName("hinh_anh").HasMaxLength(255);
                entity.Property(e => e.NgayTao).HasColumnName("ngay_tao").HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<DichVu>(entity =>
            {
                entity.ToTable("dich_vu");
                entity.HasKey(e => e.MaDV);

                entity.Property(e => e.MaDV).HasColumnName("ma_dv");
                entity.Property(e => e.TenDV).HasColumnName("ten_dv").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Loai).HasColumnName("loai").HasMaxLength(20)
                    .HasConversion<string>(); 
                entity.Property(e => e.Gia).HasColumnName("gia").HasColumnType("decimal(10,0)").IsRequired();
                entity.Property(e => e.DonVi).HasColumnName("don_vi").HasMaxLength(50).HasDefaultValue("phần");
                entity.Property(e => e.SoLuongTon).HasColumnName("so_luong_ton").HasDefaultValue(0);
                entity.Property(e => e.MaHang).HasColumnName("ma_hang");
                entity.Property(e => e.TiLeLoiNhuan).HasColumnName("ti_le_loi_nhuan").HasColumnType("decimal(5,2)").HasDefaultValue(30.00m);
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiDichVu.ConHang);
                entity.Property(e => e.MoTa).HasColumnName("mo_ta").HasMaxLength(255);
                entity.Property(e => e.HinhAnh).HasColumnName("hinh_anh").HasMaxLength(255);
                entity.Property(e => e.NgayTao).HasColumnName("ngay_tao").HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.MatHang)
                    .WithMany(m => m.DichVus)
                    .HasForeignKey(e => e.MaHang)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<NhanVien>(entity =>
            {
                entity.ToTable("nhan_vien");
                entity.HasKey(e => e.MaNV);

                entity.Property(e => e.MaNV).HasColumnName("ma_nv");
                entity.Property(e => e.TenNV).HasColumnName("ten_nv").HasMaxLength(100).IsRequired();
                entity.Property(e => e.MaVanTay).HasColumnName("ma_van_tay").HasMaxLength(50);
                entity.Property(e => e.FaceIDHash).HasColumnName("faceid_hash").HasMaxLength(255);
                entity.Property(e => e.FaceIDAnh).HasColumnName("faceid_anh").HasMaxLength(255);
                entity.Property(e => e.ChucVu).HasColumnName("chuc_vu").HasMaxLength(20)
                    .HasConversion<string>();
                entity.Property(e => e.SDT).HasColumnName("sdt").HasMaxLength(15);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
                entity.Property(e => e.LuongCoBan).HasColumnName("luong_co_ban").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.PhuCap).HasColumnName("phu_cap").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.NgayVaoLam).HasColumnName("ngay_vao_lam").HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CaMacDinh).HasColumnName("ca_mac_dinh").HasMaxLength(10)
                    .HasConversion<string>()
                    .HasDefaultValue(CaLamViec.Sang);
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiNhanVien.DangLam);
                entity.Property(e => e.MatKhau).HasColumnName("mat_khau").HasMaxLength(255).IsRequired();

                entity.HasIndex(e => e.MaVanTay).IsUnique();
                entity.HasIndex(e => e.FaceIDHash).IsUnique();
            });

            modelBuilder.Entity<ChamCong>(entity =>
            {
                entity.ToTable("cham_cong");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).HasColumnName("id");
                entity.Property(e => e.MaNV).HasColumnName("ma_nv").IsRequired();
                entity.Property(e => e.Ngay).HasColumnName("ngay").IsRequired();
                entity.Property(e => e.GioVao).HasColumnName("gio_vao");
                entity.Property(e => e.GioRa).HasColumnName("gio_ra");
                entity.Property(e => e.HinhAnhVao).HasColumnName("hinh_anh_vao").HasMaxLength(255);
                entity.Property(e => e.HinhAnhRa).HasColumnName("hinh_anh_ra").HasMaxLength(255);
                entity.Property(e => e.XacThucBang).HasColumnName("xac_thuc_bang").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(PhuongThucXacThuc.ThuCong);

                entity.Property(e => e.SoGioLam)
                    .HasColumnName("so_gio_lam")
                    .HasColumnType("decimal(5,2)")
                    .HasComputedColumnSql(
                        "(CASE WHEN [gio_ra] IS NOT NULL THEN CAST(DATEDIFF(MINUTE, [gio_vao], [gio_ra]) AS DECIMAL(5,2)) / 60.0 ELSE 0 END)",
                        stored: true
                    );

                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiChamCong.DungGio);
                entity.Property(e => e.GhiChu).HasColumnName("ghi_chu").HasMaxLength(255);

                entity.HasOne(e => e.NhanVien)
                    .WithMany(n => n.ChamCongs)
                    .HasForeignKey(e => e.MaNV)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BangLuong>(entity =>
            {
                entity.ToTable("bang_luong");
                entity.HasKey(e => e.MaLuong);

                entity.Property(e => e.MaLuong).HasColumnName("ma_luong");
                entity.Property(e => e.MaNV).HasColumnName("ma_nv").IsRequired();
                entity.Property(e => e.Thang).HasColumnName("thang").IsRequired();
                entity.Property(e => e.Nam).HasColumnName("nam").IsRequired();
                entity.Property(e => e.TongGio).HasColumnName("tong_gio").HasColumnType("decimal(8,2)").HasDefaultValue(0);
                entity.Property(e => e.LuongCoBan).HasColumnName("luong_co_ban").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.PhuCap).HasColumnName("phu_cap").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.Thuong).HasColumnName("thuong").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.Phat).HasColumnName("phat").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.TongLuong).HasColumnName("tong_luong").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.NgayTinh).HasColumnName("ngay_tinh").HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.NhanVien)
                    .WithMany(n => n.BangLuongs)
                    .HasForeignKey(e => e.MaNV)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<HoaDon>(entity =>
            {
                entity.ToTable("hoa_don");
                entity.HasKey(e => e.MaHD);

                entity.Property(e => e.MaHD).HasColumnName("ma_hd");
                entity.Property(e => e.MaBan).HasColumnName("ma_ban");
                entity.Property(e => e.MaKH).HasColumnName("ma_kh");
                entity.Property(e => e.MaNV).HasColumnName("ma_nv");
                entity.Property(e => e.ThoiGianBatDau).HasColumnName("thoi_gian_bat_dau");
                entity.Property(e => e.ThoiGianKetThuc).HasColumnName("thoi_gian_ket_thuc");
                entity.Property(e => e.ThoiLuongPhut).HasColumnName("thoi_luong_phut");
                entity.Property(e => e.TienBan).HasColumnName("tien_ban").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.TienDichVu).HasColumnName("tien_dich_vu").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.GiamGia).HasColumnName("giam_gia").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.TongTien).HasColumnName("tong_tien").HasColumnType("decimal(12,0)").HasDefaultValue(0);
                entity.Property(e => e.PhuongThucThanhToan).HasColumnName("phuong_thuc_thanh_toan").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(PhuongThucThanhToan.TienMat);
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiHoaDon.DangChoi);
                entity.Property(e => e.GhiChu).HasColumnName("ghi_chu");

                entity.HasOne(e => e.BanBia)
                    .WithMany(b => b.HoaDons)
                    .HasForeignKey(e => e.MaBan)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.KhachHang)
                    .WithMany(k => k.HoaDons)
                    .HasForeignKey(e => e.MaKH)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.NhanVien)
                    .WithMany(n => n.HoaDons)
                    .HasForeignKey(e => e.MaNV)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChiTietHoaDon>(entity =>
            {
                entity.ToTable("chi_tiet_hoa_don");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).HasColumnName("id");
                entity.Property(e => e.MaHD).HasColumnName("ma_hd");
                entity.Property(e => e.MaDV).HasColumnName("ma_dv");
                entity.Property(e => e.SoLuong).HasColumnName("so_luong").HasDefaultValue(1);
                entity.Property(e => e.ThanhTien).HasColumnName("thanh_tien").HasColumnType("decimal(12,0)");

                entity.HasOne(e => e.HoaDon)
                    .WithMany(h => h.ChiTietHoaDons)
                    .HasForeignKey(e => e.MaHD)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.DichVu)
                    .WithMany(d => d.ChiTietHoaDons)
                    .HasForeignKey(e => e.MaDV)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GiaGioChoi>(entity =>
            {
                entity.ToTable("gia_gio_choi");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).HasColumnName("id");
                entity.Property(e => e.KhungGio).HasColumnName("khung_gio").HasMaxLength(50).IsRequired();
                entity.Property(e => e.GioBatDau).HasColumnName("gio_bat_dau").IsRequired();
                entity.Property(e => e.GioKetThuc).HasColumnName("gio_ket_thuc").IsRequired();
                entity.Property(e => e.MaLoai).HasColumnName("ma_loai");
                entity.Property(e => e.Gia).HasColumnName("gia").HasColumnType("decimal(10,0)").IsRequired();
                entity.Property(e => e.ApDungTuNgay).HasColumnName("ap_dung_tu_ngay");
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(20)
                    .HasConversion<string>()
                    .HasDefaultValue(TrangThaiGiaGio.DangApDung);

                entity.HasOne(e => e.LoaiBan)
                    .WithMany(l => l.GiaGioChois)
                    .HasForeignKey(e => e.MaLoai)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LichSuHoatDong>(entity =>
            {
                entity.ToTable("lich_su_hoat_dong");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).HasColumnName("id");
                entity.Property(e => e.ThoiGian).HasColumnName("thoi_gian").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.MaNV).HasColumnName("ma_nv");
                entity.Property(e => e.HanhDong).HasColumnName("hanh_dong").HasMaxLength(255);
                entity.Property(e => e.ChiTiet).HasColumnName("chi_tiet");

                entity.HasOne(e => e.NhanVien)
                    .WithMany(n => n.LichSuHoatDongs)
                    .HasForeignKey(e => e.MaNV)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}