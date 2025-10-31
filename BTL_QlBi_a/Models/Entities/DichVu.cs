using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class DichVu
    {
        [Key]
        public int MaDV { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenDV { get; set; }

        [MaxLength(20)]
        public string Loai { get; set; } = "Khác";

        [Column(TypeName = "decimal(10,0)")]
        public decimal Gia { get; set; }

        [MaxLength(50)]
        public string DonVi { get; set; } = "Phần";

        public int SoLuongTon { get; set; } = 0;

        public int? MaHang { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TiLeLoiNhuan { get; set; } = 30.00m;

        [MaxLength(20)]
        public string TrangThai { get; set; } = "Còn hàng"; 

        [MaxLength(255)]
        public string MoTa { get; set; }

        [MaxLength(255)]
        public string HinhAnh { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("MaHang")]
        public MatHang MatHang { get; set; }

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}