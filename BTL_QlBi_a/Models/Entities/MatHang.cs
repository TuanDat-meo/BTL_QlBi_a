using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class MatHang
    {
        [Key]
        public int MaHang { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenHang { get; set; }

        [MaxLength(20)]
        public string Loai { get; set; } = "Khác"; // Giá trị mặc định là chuỗi

        [MaxLength(50)]
        public string DonVi { get; set; } = "Cái";

        [Column(TypeName = "decimal(10,0)")]
        public decimal Gia { get; set; }

        public int SoLuongTon { get; set; } = 0;

        [MaxLength(100)]
        public string NhaCungCap { get; set; }

        public DateTime? NgayNhapGanNhat { get; set; }

        [MaxLength(20)]
        public string TrangThai { get; set; } = "Còn Hàng"; // Giá trị mặc định là chuỗi

        [MaxLength(255)]
        public string MoTa { get; set; }

        [MaxLength(255)]
        public string HinhAnh { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties
        public ICollection<DichVu> DichVus { get; set; }
    }
}