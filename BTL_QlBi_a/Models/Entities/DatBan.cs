// fileName: DatBan.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QlBi_a.Models.Entities
{
    public class DatBan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDat { get; set; }

        public int? MaBan { get; set; }

        public int? MaKH { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenKhach { get; set; }

        [Required]
        [MaxLength(15)]
        public string SDT { get; set; }

        [Required]
        public DateTime ThoiGianDat { get; set; }

        public int? SoNguoi { get; set; }

        public int SoGio { get; set; } = 1; 

        public string? GhiChu { get; set; }

        public TrangThaiDatBan TrangThai { get; set; } = TrangThaiDatBan.DangCho;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("MaBan")]
        public virtual BanBia? BanBia { get; set; }

        [ForeignKey("MaKH")]
        public virtual KhachHang? KhachHang { get; set; }
    }
}