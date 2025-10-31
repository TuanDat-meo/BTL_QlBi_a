using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class NhanVien
    {
        [Key]
        public int MaNV { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenNV { get; set; }

        [MaxLength(50)]
        public string MaVanTay { get; set; }

        [MaxLength(255)]
        public string FaceIDHash { get; set; }

        [MaxLength(255)]
        public string FaceIDAnh { get; set; }

        // Đã thay thế ChucVu? bằng string
        [MaxLength(20)]
        public string ChucVu { get; set; } = "Phục Vụ"; // Giá trị mặc định là chuỗi

        [Required]
        [MaxLength(15)]
        public string SDT { get; set; } 

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Column(TypeName = "decimal(12,0)")]
        public decimal LuongCoBan { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal PhuCap { get; set; } = 0;

        public DateTime NgayVaoLam { get; set; } = DateTime.Now; 

        [MaxLength(10)]
        public string CaMacDinh { get; set; } = "Sáng"; 

        [MaxLength(20)]
        public string TrangThai { get; set; } = "Đang Làm"; 

        [Required]
        [MaxLength(255)]
        public string MatKhau { get; set; }

        // Navigation Properties
        public ICollection<ChamCong> ChamCongs { get; set; }
        public ICollection<BangLuong> BangLuongs { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
        public ICollection<LichSuHoatDong> LichSuHoatDongs { get; set; }
    }
}