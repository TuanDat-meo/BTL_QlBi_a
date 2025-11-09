using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BTL_QlBi_a.Models.Entities
{
    public class ChamCong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int MaNV { get; set; }

        [Required]
        public DateTime Ngay { get; set; }

        public DateTime? GioVao { get; set; }

        public DateTime? GioRa { get; set; }

        [MaxLength(255)]
        public string? HinhAnhVao { get; set; }

        [MaxLength(255)]
        public string? HinhAnhRa { get; set; }

        public PhuongThucXacThuc XacThucBang { get; set; } = PhuongThucXacThuc.ThuCong;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? SoGioLam { get; set; }

        public TrangThaiChamCong TrangThai { get; set; } = TrangThaiChamCong.DungGio;

        [MaxLength(255)]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("MaNV")]
        public virtual NhanVien NhanVien { get; set; }
    }
}