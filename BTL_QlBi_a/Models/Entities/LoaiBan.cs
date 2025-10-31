using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class LoaiBan
    {
        [Key]
        public int MaLoai { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenLoai { get; set; }

        [MaxLength(255)]
        public string MoTa { get; set; }

        [Column(TypeName = "decimal(10,0)")]
        public decimal GiaGio { get; set; }

        [MaxLength(20)]
        public string TrangThai { get; set; } = "Đang Áp Dụng";

        // Navigation Properties
        public ICollection<BanBi_a> BanBi_a { get; set; }
        public ICollection<GiaGioChoi> GiaGioChois { get; set; }
    }
}