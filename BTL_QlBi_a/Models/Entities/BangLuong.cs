using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BTL_QlBi_a.Models.Entities
{
    public class BangLuong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLuong { get; set; }

        [Required]
        public int MaNV { get; set; }

        [Required]
        public int Thang { get; set; }

        [Required]
        public int Nam { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal TongGio { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal LuongCoBan { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal PhuCap { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal Thuong { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal Phat { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal TongLuong { get; set; } = 0;

        public DateTime NgayTinh { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("MaNV")]
        public virtual NhanVien NhanVien { get; set; }
    }
}