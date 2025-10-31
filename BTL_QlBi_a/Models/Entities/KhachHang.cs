using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class KhachHang
    {
        [Key]
        public int MaKH { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenKH { get; set; }

        [Required]
        [MaxLength(15)]
        public string SDT { get; set; } 

        [Required] 
        [MaxLength(100)]
        public string Email { get; set; }

        public DateTime? NgaySinh { get; set; }

        [MaxLength(20)]
        public string HangTV { get; set; } = "Đồng"; 

        public int DiemTichLuy { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal TongChiTieu { get; set; } = 0;

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public DateTime? LanDenCuoi { get; set; }

        // Navigation Properties
        public ICollection<BanBi_a> BanBias { get; set; }
        public ICollection<DatBan> DatBans { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
    }
}