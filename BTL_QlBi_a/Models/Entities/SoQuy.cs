using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class SoQuy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieu { get; set; }

        [Required]
        public int MaNV { get; set; }

        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Required]
        public LoaiPhieu LoaiPhieu { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,0)")]
        public decimal SoTien { get; set; }

        [Required]
        [MaxLength(500)]
        public string LyDo { get; set; }

        public int? MaHDLienQuan { get; set; }

        // Navigation properties
        [ForeignKey("MaNV")]
        public virtual NhanVien NhanVien { get; set; }

        [ForeignKey("MaHDLienQuan")]
        public virtual HoaDon? HoaDon { get; set; }
    }
}
