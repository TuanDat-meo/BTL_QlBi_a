using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class MatHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHang { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenHang { get; set; }

        public LoaiMatHang Loai { get; set; } = LoaiMatHang.Khac;

        [MaxLength(50)]
        public string DonVi { get; set; } = "cái";

        [Required]
        [Column(TypeName = "decimal(10,0)")]
        public decimal Gia { get; set; }

        public int SoLuongTon { get; set; } = 0;

        public int NguongCanhBao { get; set; } = 10;

        public int? MaNCCDefault { get; set; }

        public DateTime? NgayNhapGanNhat { get; set; }

        public TrangThaiMatHang TrangThai { get; set; } = TrangThaiMatHang.ConHang;

        [MaxLength(255)]
        public string? MoTa { get; set; }

        [MaxLength(255)]
        public string? HinhAnh { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("MaNCCDefault")]
        public virtual NhaCungCap? NhaCungCap { get; set; }

        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public virtual ICollection<DichVu> DichVus { get; set; }
    }
}