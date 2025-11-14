using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class NhanVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNV { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenNV { get; set; }

        [MaxLength(50)]
        public string? MaVanTay { get; set; }

        [MaxLength(255)]
        public string? FaceIDHash { get; set; }

        [MaxLength(255)]
        public string? FaceIDAnh { get; set; }

        [Required]
        public int MaNhom { get; set; }

        [MaxLength(15)]
        public string? SDT { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [Column(TypeName = "decimal(12,0)")]
        public decimal LuongCoBan { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal PhuCap { get; set; } = 0;

        public CaLamViec CaMacDinh { get; set; } = CaLamViec.Sang;

        public TrangThaiNhanVien TrangThai { get; set; } = TrangThaiNhanVien.DangLam;

        [Required]
        [MaxLength(255)]
        public string MatKhau { get; set; }

        // Navigation properties
        [ForeignKey("MaNhom")]
        public virtual NhomQuyen NhomQuyen { get; set; }

        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; }
        public virtual ICollection<ChamCong> ChamCongs { get; set; }
        public virtual ICollection<BangLuong> BangLuongs { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ICollection<SoQuy> SoQuys { get; set; }
        public virtual ICollection<LichSuHoatDong> LichSuHoatDongs { get; set; }
    }
}