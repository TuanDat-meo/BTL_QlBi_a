using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class LichSuHoatDong
    {
        [Key]
        public int ID { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        public int? MaNV { get; set; }

        [MaxLength(255)]
        public string HanhDong { get; set; }

        public string ChiTiet { get; set; }

        [ForeignKey("MaNV")]
        public NhanVien NhanVien { get; set; }
    }
}
