using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class DichVu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDV { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenDV { get; set; }

        public LoaiDichVu Loai { get; set; } = LoaiDichVu.Khac;

        [Required]
        [Column(TypeName = "decimal(10,0)")]
        public decimal Gia { get; set; }

        [MaxLength(50)]
        public string DonVi { get; set; } = "phần";

        public int? MaHang { get; set; }

        public TrangThaiDichVu TrangThai { get; set; } = TrangThaiDichVu.ConHang;

        [MaxLength(255)]
        public string? MoTa { get; set; }

        [MaxLength(255)]
        public string? HinhAnh { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("MaHang")]
        public virtual MatHang? MatHang { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}