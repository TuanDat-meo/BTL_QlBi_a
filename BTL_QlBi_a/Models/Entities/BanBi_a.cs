using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BTL_QlBi_a.Models.Entities
{
    public class BanBi_a
    {
        [Key]
        public int MaBan { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenBan { get; set; }
        [MaxLength(255)]
        public string HinhAnh { get; set; }
        public int MaLoai { get; set; }

        [MaxLength(20)]
        public string KhuVuc { get; set; } = "Tầng 1"; 

        [MaxLength(20)]
        public string TrangThai { get; set; } = "Trống"; 

        public DateTime? GioBatDau { get; set; }

        public int? MaKH { get; set; }

        [MaxLength(255)]
        public string GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("MaLoai")]
        public LoaiBan LoaiBan { get; set; }

        [ForeignKey("MaKH")]
        public KhachHang KhachHang { get; set; }

        public ICollection<DatBan> DatBans { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
    }
}