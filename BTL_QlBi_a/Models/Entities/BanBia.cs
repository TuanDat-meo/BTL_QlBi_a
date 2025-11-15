using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class BanBia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBan { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenBan { get; set; }

        [Required]
        public int MaLoai { get; set; }

        [Required]
        public int MaKhuVuc { get; set; }

        public TrangThaiBan TrangThai { get; set; } = TrangThaiBan.Trong;

        public DateTime? GioBatDau { get; set; }

        public int? MaKH { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ViTriX { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ViTriY { get; set; }

        [MaxLength(255)]
        public string? GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string? HinhAnh { get; set; }

        // Navigation properties
        [ForeignKey("MaLoai")]
        public virtual LoaiBan LoaiBan { get; set; }

        [ForeignKey("MaKH")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MaKhuVuc")]
        public virtual KhuVuc KhuVuc { get; set; }

        public virtual ICollection<DatBan> DatBans { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}