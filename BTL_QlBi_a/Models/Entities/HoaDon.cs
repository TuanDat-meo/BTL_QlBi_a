using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class HoaDon
    {
        [Key]
        public int MaHD { get; set; }

        public int? MaBan { get; set; }

        public int? MaKH { get; set; }

        public int? MaNV { get; set; }

        public DateTime? ThoiGianBatDau { get; set; }

        public DateTime? ThoiGianKetThuc { get; set; }

        public int? ThoiLuongPhut { get; set; }

        [Column(TypeName = "decimal(12,0)")]
        public decimal TienBan { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal TienDichVu { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal GiamGia { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal TongTien { get; set; } = 0;

        [MaxLength(20)]
        public string PhuongThucThanhToan { get; set; } = "Tiền Mặt"; 

        [MaxLength(20)]
        public string TrangThai { get; set; } = "Đang Chơi"; 

        public string GhiChu { get; set; }

        // Navigation Properties
        [ForeignKey("MaBan")]
        public BanBi_a BanBia { get; set; }

        [ForeignKey("MaKH")]
        public KhachHang KhachHang { get; set; }

        [ForeignKey("MaNV")]
        public NhanVien NhanVien { get; set; }

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}