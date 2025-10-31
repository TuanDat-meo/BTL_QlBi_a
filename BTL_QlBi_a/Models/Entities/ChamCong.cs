using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BTL_QlBi_a.Models.Entities
{
    public class ChamCong
    {
        [Key]
        public int ID { get; set; }

        public int MaNV { get; set; }

        public DateTime Ngay { get; set; }

        public DateTime? GioVao { get; set; }

        public DateTime? GioRa { get; set; }

        [MaxLength(255)]
        public string HinhAnhVao { get; set; }

        [MaxLength(255)]
        public string HinhAnhRa { get; set; }

        [MaxLength(20)]
        public string XacThucBang { get; set; } = "Thủ Công";

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? SoGioLam { get; set; }

        // Đã thay thế TrangThaiChamCong bằng string
        [MaxLength(20)]
        public string TrangThai { get; set; } = "Đúng Giờ"; 

        [MaxLength(255)]
        public string GhiChu { get; set; }

        [ForeignKey("MaNV")]
        public NhanVien NhanVien { get; set; }
    }
}