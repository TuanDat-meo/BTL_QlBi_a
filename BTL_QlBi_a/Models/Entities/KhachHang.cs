using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class KhachHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKH { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenKH { get; set; }

        [MaxLength(15)]
        [RegularExpression(@"^[0-9]{9,11}$", ErrorMessage = "Số điện thoại phải từ 9-11 chữ số")]
        public string? SDT { get; set; }

        [MaxLength(255)]
        public string? MatKhau { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        public DateTime? NgaySinh { get; set; }

        public HangThanhVien HangTV { get; set; } = HangThanhVien.Dong;

        public int DiemTichLuy { get; set; } = 0;

        [Column(TypeName = "decimal(12,0)")]
        public decimal TongChiTieu { get; set; } = 0;

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public DateTime? LanDenCuoi { get; set; }

        public bool HoatDong { get; set; } = true;

        [StringLength(255)]
        public string? Avatar { get; set; }


        // Navigation properties
        public virtual ICollection<BanBia> BanBias { get; set; }
        public virtual ICollection<DatBan> DatBans { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}