// fileName: DatBan.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class DatBan
    {
        [Key]
        public int MaDat { get; set; }

        public int? MaBan { get; set; }

        public int? MaKH { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenKhach { get; set; }

        [Required] 
        [MaxLength(15)]
        public string SDT { get; set; }

        [MaxLength(100)]
        public string Email { get; set; } 

        public DateTime ThoiGianDat { get; set; }

        public int? SoNguoi { get; set; }

        public string GhiChu { get; set; }

        [MaxLength(20)]
        public string TrangThai { get; set; } = "Đang Chờ"; 

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("MaBan")]
        public BanBi_a BanBia { get; set; }

        [ForeignKey("MaKH")]
        public KhachHang KhachHang { get; set; }
    }
}