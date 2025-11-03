using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class ChiTietPhieuNhap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int MaPN { get; set; }

        [Required]
        public int MaHang { get; set; }

        [Required]
        public int SoLuongNhap { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,0)")]
        public decimal DonGiaNhap { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(10,0)")]
        public decimal? ThanhTien { get; set; }

        // Navigation properties
        [ForeignKey("MaPN")]
        public virtual PhieuNhap PhieuNhap { get; set; }

        [ForeignKey("MaHang")]
        public virtual MatHang MatHang { get; set; }
    }
}
