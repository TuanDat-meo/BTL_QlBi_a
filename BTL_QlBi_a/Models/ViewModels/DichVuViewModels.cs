using System.ComponentModel.DataAnnotations;
using BTL_QlBi_a.Models.Entities;

namespace BTL_QlBi_a.Models.ViewModels
{
    public class DichVuViewModel
    {
        public int? MaDV { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
        [MaxLength(100)]
        public string TenDV { get; set; }

        [Required(ErrorMessage = "Loại dịch vụ là bắt buộc")]
        public LoaiDichVu Loai { get; set; } = LoaiDichVu.Khac;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Gia { get; set; }

        [MaxLength(50)]
        public string DonVi { get; set; } = "phần";

        public int? MaHang { get; set; }

        public TrangThaiDichVu TrangThai { get; set; } = TrangThaiDichVu.ConHang;

        [MaxLength(255)]
        public string MoTa { get; set; }

        public IFormFile HinhAnhFile { get; set; }
    }

    public class XoaDichVuMenuRequest
    {
        [Required(ErrorMessage = "Mã dịch vụ là bắt buộc")]
        public int MaDV { get; set; }
    }
}