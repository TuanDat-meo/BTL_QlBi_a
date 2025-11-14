using BTL_QlBi_a.Models.Entities;

namespace BTL_QlBi_a.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public KhachHang KhachHang { get; set; }
        public List<DatBan> LichSuDatBan { get; set; }
        public List<HoaDon> LichSuHoaDon { get; set; }
    }
}