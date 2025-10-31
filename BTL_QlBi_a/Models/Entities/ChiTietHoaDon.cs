using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class ChiTietHoaDon
    {
        [Key]
        public int ID { get; set; }

        public int? MaHD { get; set; }

        public int? MaDV { get; set; }

        public int SoLuong { get; set; } = 1;

        [Column(TypeName = "decimal(12,0)")]
        public decimal? ThanhTien { get; set; }

        [ForeignKey("MaHD")]
        public HoaDon HoaDon { get; set; }

        [ForeignKey("MaDV")]
        public DichVu DichVu { get; set; }
    }
}