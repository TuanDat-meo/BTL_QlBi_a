using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class HoaDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHD { get; set; }

        public int? MaBan { get; set; }

        public int? MaKH { get; set; }

        public int? MaNV { get; set; }

        public DateTime? ThoiGianBatDau { get; set; }

        public DateTime? ThoiGianKetThuc { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? ThoiLuongPhut { get; set; }

        [Column(TypeName = "decimal(12,0)")]
        public decimal TienBan { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal TienDichVu { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal GiamGia { get; set; } = 0;

        [MaxLength(255)]
        public string? GhiChuGiamGia { get; set; }

        [Column(TypeName = "decimal(12,0)")]
        public decimal TongTien { get; set; } = 0;

        public PhuongThucThanhToan PhuongThucThanhToan { get; set; } = PhuongThucThanhToan.TienMat;

        public TrangThaiHoaDon TrangThai { get; set; } = TrangThaiHoaDon.DangChoi;

        [MaxLength(100)]
        public string? MaGiaoDichQR { get; set; }

        [MaxLength(500)]
        public string? QRCodeUrl { get; set; }

        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("MaBan")]
        public virtual BanBia? BanBia { get; set; }

        [ForeignKey("MaKH")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MaNV")]
        public virtual NhanVien? NhanVien { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public virtual ICollection<SoQuy> SoQuys { get; set; }
    }
}