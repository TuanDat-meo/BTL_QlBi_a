using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class GiaGioChoi
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string KhungGio { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public int? MaLoai { get; set; }

        [Column(TypeName = "decimal(10,0)")]
        public decimal Gia { get; set; }

        public DateTime? ApDungTuNgay { get; set; }

        // Đã thay thế TrangThaiGiaGio bằng string
        [MaxLength(20)]
        public string TrangThai { get; set; } = "Đang Áp Dụng"; // Giá trị mặc định là chuỗi

        [ForeignKey("MaLoai")]
        public LoaiBan LoaiBan { get; set; }
    }
}