using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BTL_QlBi_a.Models.Entities
{
    public enum HangThanhVien
    {
        [Display(Name = "Đồng")]
        [EnumMember(Value = "Đồng")]
        Dong,

        [Display(Name = "Bạc")]
        [EnumMember(Value = "Bạc")]
        Bac,

        [Display(Name = "Vàng")]
        [EnumMember(Value = "Vàng")]
        Vang,

        [Display(Name = "Bạch kim")]
        [EnumMember(Value = "Bạch kim")]
        BachKim
    }

    public enum TrangThaiLoaiBan
    {
        [Display(Name = "Đang áp dụng")]
        [EnumMember(Value = "Đang áp dụng")]
        DangApDung,

        [Display(Name = "Ngừng áp dụng")]
        [EnumMember(Value = "Ngừng áp dụng")]
        NgungApDung
    }

    public enum TrangThaiBan
    {
        [Display(Name = "Trống")]
        [EnumMember(Value = "Trống")]
        Trong,

        [Display(Name = "Đang chơi")]
        [EnumMember(Value = "Đang chơi")]
        DangChoi,

        [Display(Name = "Đã đặt")]
        [EnumMember(Value = "Đã đặt")]
        DaDat,

        [Display(Name = "Bảo trì")]
        [EnumMember(Value = "Bảo trì")]
        BaoTri
    }

    public enum TrangThaiDatBan
    {
        [Display(Name = "Đang chờ")]
        [EnumMember(Value = "Đang chờ")]
        DangCho,

        [Display(Name = "Đã xác nhận")]
        [EnumMember(Value = "Đã xác nhận")]
        DaXacNhan,

        [Display(Name = "Đã đến")]
        [EnumMember(Value = "Đã đến")]
        DaDen,

        [Display(Name = "Đã hủy")]
        [EnumMember(Value = "Đã hủy")]
        DaHuy
    }

    public enum LoaiMatHang
    {
        [Display(Name = "Khác")]
        [EnumMember(Value = "Khác")]
        Khac = 0,

        [Display(Name = "Đồ uống")]
        [EnumMember(Value = "Đồ uống")]
        DoUong,

        [Display(Name = "Đồ ăn")]
        [EnumMember(Value = "Đồ ăn")]
        DoAn,

        [Display(Name = "Dụng cụ bi-a")]
        [EnumMember(Value = "Dụng cụ bi-a")]
        DungCuBia
    }

    public enum TrangThaiMatHang
    {
        [Display(Name = "Còn hàng")]
        [EnumMember(Value = "Còn hàng")]
        ConHang,

        [Display(Name = "Hết hàng")]
        [EnumMember(Value = "Hết hàng")]
        HetHang,

        [Display(Name = "Ngừng kinh doanh")]
        [EnumMember(Value = "Ngừng kinh doanh")]
        NgungKinhDoanh
    }

    public enum LoaiDichVu
    {
        [Display(Name = "Khác")]
        [EnumMember(Value = "Khác")]
        Khac = 0,

        [Display(Name = "Đồ uống")]
        [EnumMember(Value = "Đồ uống")]
        DoUong,

        [Display(Name = "Đồ ăn")]
        [EnumMember(Value = "Đồ ăn")]
        DoAn
    }

    public enum TrangThaiDichVu
    {
        [Display(Name = "Còn hàng")]
        [EnumMember(Value = "Còn hàng")]
        ConHang,

        [Display(Name = "Hết hàng")]
        [EnumMember(Value = "Hết hàng")]
        HetHang,

        [Display(Name = "Ngừng bán")]
        [EnumMember(Value = "Ngừng bán")]
        NgungBan
    }

    // FIX: Ca làm việc - Map với giá trị có dấu trong DB
    public enum CaLamViec
    {
        [Display(Name = "Sáng")]
        [EnumMember(Value = "Sáng")]
        Sang,

        [Display(Name = "Chiều")]
        [EnumMember(Value = "Chiều")]
        Chieu,

        [Display(Name = "Tối")]
        [EnumMember(Value = "Tối")]
        Toi
    }

    public enum TrangThaiNhanVien
    {
        [Display(Name = "Đang làm")]
        [EnumMember(Value = "Đang làm")]
        DangLam,

        [Display(Name = "Nghỉ")]
        [EnumMember(Value = "Nghỉ")]
        Nghi
    }

    public enum PhuongThucXacThuc
    {
        [Display(Name = "Thủ công")]
        [EnumMember(Value = "Thủ công")]
        ThuCong,

        [Display(Name = "Vân tay")]
        [EnumMember(Value = "Vân tay")]
        VanTay,

        [Display(Name = "FaceID")]
        [EnumMember(Value = "FaceID")]
        FaceID
    }

    public enum TrangThaiChamCong
    {
        [Display(Name = "Đúng giờ")]
        [EnumMember(Value = "Đúng giờ")]
        DungGio,

        [Display(Name = "Đi trễ")]
        [EnumMember(Value = "Đi trễ")]
        DiTre,

        [Display(Name = "Về sớm")]
        [EnumMember(Value = "Về sớm")]
        VeSom,

        [Display(Name = "Vắng")]
        [EnumMember(Value = "Vắng")]
        Vang,

        [Display(Name = "Ca thêm")]
        [EnumMember(Value = "Ca thêm")]
        CaThem
    }

    public enum PhuongThucThanhToan
    {
        [Display(Name = "Tiền mặt")]
        [EnumMember(Value = "Tiền mặt")]
        TienMat,

        [Display(Name = "Chuyển khoản")]
        [EnumMember(Value = "Chuyển khoản")]
        ChuyenKhoan,

        [Display(Name = "Thẻ")]
        [EnumMember(Value = "Thẻ")]
        The,

        [Display(Name = "Ví điện tử")]
        [EnumMember(Value = "Ví điện tử")]
        ViDienTu,

        [Display(Name = "QR Tự động")]
        [EnumMember(Value = "QR Tự động")]
        QRTuDong
    }

    public enum TrangThaiHoaDon
    {
        [Display(Name = "Đang chơi")]
        [EnumMember(Value = "Đang chơi")]
        DangChoi,

        [Display(Name = "Đã thanh toán")]
        [EnumMember(Value = "Đã thanh toán")]
        DaThanhToan,

        [Display(Name = "Đã hủy")]
        [EnumMember(Value = "Đã hủy")]
        DaHuy
    }

    public enum LoaiPhieu
    {
        [Display(Name = "Thu")]
        [EnumMember(Value = "Thu")]
        Thu,

        [Display(Name = "Chi")]
        [EnumMember(Value = "Chi")]
        Chi
    }

    public enum TrangThaiGiaGio
    {
        [Display(Name = "Đang áp dụng")]
        [EnumMember(Value = "Đang áp dụng")]
        DangApDung,

        [Display(Name = "Hết hiệu lực")]
        [EnumMember(Value = "Hết hiệu lực")]
        HetHieuLuc
    }
}