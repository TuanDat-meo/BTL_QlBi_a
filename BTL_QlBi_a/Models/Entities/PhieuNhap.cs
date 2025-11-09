using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class PhieuNhap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPN { get; set; }

        [Required]
        public int MaNV { get; set; }

        [Required]
        public int MaNCC { get; set; }

        public DateTime NgayNhap { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(12,0)")]
        public decimal TongTien { get; set; } = 0;

        [MaxLength(255)]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("MaNV")]
        public virtual NhanVien NhanVien { get; set; }

        [ForeignKey("MaNCC")]
        public virtual NhaCungCap NhaCungCap { get; set; }

        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
    }
}
