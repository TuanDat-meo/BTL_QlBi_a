using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class GiaGioChoi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string KhungGio { get; set; }

        [Required]
        public TimeSpan GioBatDau { get; set; }

        [Required]
        public TimeSpan GioKetThuc { get; set; }

        public int? MaLoai { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,0)")]
        public decimal Gia { get; set; }

        public DateTime? ApDungTuNgay { get; set; }

        public TrangThaiGiaGio TrangThai { get; set; } = TrangThaiGiaGio.DangApDung;

        // Navigation properties
        [ForeignKey("MaLoai")]
        public virtual LoaiBan? LoaiBan { get; set; }
    }
}