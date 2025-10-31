using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BTL_QlBi_a.Models.Entities
{
    public class BangLuong
    {
        [Key]
        public int MaLuong { get; set; }

        public int MaNV { get; set; }

        public int Thang { get; set; }

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

        [ForeignKey("MaNV")]
        public NhanVien NhanVien { get; set; }
    }
}