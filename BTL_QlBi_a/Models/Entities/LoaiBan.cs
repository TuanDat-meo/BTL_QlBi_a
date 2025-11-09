using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class LoaiBan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLoai { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenLoai { get; set; }

        [MaxLength(255)]
        public string? MoTa { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,0)")]
        public decimal GiaGio { get; set; }

        public TrangThaiLoaiBan TrangThai { get; set; } = TrangThaiLoaiBan.DangApDung;

        // Navigation properties
        public virtual ICollection<BanBia> BanBias { get; set; }
        public virtual ICollection<GiaGioChoi> GiaGioChois { get; set; }
    }
}