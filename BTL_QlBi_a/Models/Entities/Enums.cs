using System.ComponentModel.DataAnnotations;

namespace BTL_QlBi_a.Models.Entities
{
    public enum HangThanhVien
    {
        [Display(Name = "Đồng")]
        Dong,

        [Display(Name = "Bạc")]
        Bac,

        [Display(Name = "Vàng")]
        Vang,

        [Display(Name = "Bạch kim")]
        BachKim
    }

    // Enum cho Trạng thái loại bàn
    public enum TrangThaiLoaiBan
    {
        [Display(Name = "Đang áp dụng")]
        DangApDung,

        [Display(Name = "Ngừng áp dụng")]
        NgungApDung
    }

    // Enum cho Trạng thái bàn
    public enum TrangThaiBan
    {
        [Display(Name = "Trống")]
        Trong,

        [Display(Name = "Đang chơi")]
        DangChoi,

        [Display(Name = "Đã đặt")]
        DaDat,

        [Display(Name = "Bảo trì")]
        BaoTri
    }

    // Enum cho Trạng thái đặt bàn
    public enum TrangThaiDatBan
    {
        [Display(Name = "Đang chờ")]
        DangCho,

        [Display(Name = "Đã xác nhận")]
        DaXacNhan,

        [Display(Name = "Đã đến")]
        DaDen,

        [Display(Name = "Đã hủy")]
        DaHuy
    }

    // Enum cho Loại mặt hàng - ĐÃ SỬA: Đặt "Khác" lên đầu
    public enum LoaiMatHang
    {
        [Display(Name = "Khác")]
        Khac = 0,

        [Display(Name = "Đồ uống")]
        DoUong,

        [Display(Name = "Đồ ăn")]
        DoAn,

        [Display(Name = "Dụng cụ bi-a")]
        DungCuBia
    }

    // Enum cho Trạng thái mặt hàng
    public enum TrangThaiMatHang
    {
        [Display(Name = "Còn hàng")]
        ConHang,

        [Display(Name = "Hết hàng")]
        HetHang,

        [Display(Name = "Ngừng kinh doanh")]
        NgungKinhDoanh
    }

    // Enum cho Loại dịch vụ - ĐÃ SỬA: Đặt "Khác" lên đầu
    public enum LoaiDichVu
    {
        [Display(Name = "Khác")]
        Khac = 0,

        [Display(Name = "Đồ uống")]
        DoUong,

        [Display(Name = "Đồ ăn")]
        DoAn
    }

    // Enum cho Trạng thái dịch vụ
    public enum TrangThaiDichVu
    {
        [Display(Name = "Còn hàng")]
        ConHang,

        [Display(Name = "Hết hàng")]
        HetHang,

        [Display(Name = "Ngừng bán")]
        NgungBan
    }

    // Enum cho Ca làm việc
    public enum CaLamViec
    {
        [Display(Name = "Sáng")]
        Sang,

        [Display(Name = "Chiều")]
        Chieu,

        [Display(Name = "Tối")]
        Toi
    }
    // Enum cho Trạng thái nhân viên
    public enum TrangThaiNhanVien
    {
        [Display(Name = "Đang làm")]
        DangLam,

        [Display(Name = "Nghỉ")]
        Nghi
    }

    // Enum cho Phương thức xác thực
    public enum PhuongThucXacThuc
    {
        [Display(Name = "Thủ công")]
        ThuCong,

        [Display(Name = "Vân tay")]
        VanTay,

        [Display(Name = "FaceID")]
        FaceID
    }

    // Enum cho Trạng thái chấm công
    public enum TrangThaiChamCong
    {
        [Display(Name = "Đúng giờ")]
        DungGio,

        [Display(Name = "Đi trễ")]
        DiTre,

        [Display(Name = "Về sớm")]
        VeSom,

        [Display(Name = "Vắng")]
        Vang,

        [Display(Name = "Ca thêm")]
        CaThem
    }

    // Enum cho Phương thức thanh toán
    public enum PhuongThucThanhToan
    {
        [Display(Name = "Tiền mặt")]
        TienMat,

        [Display(Name = "Chuyển khoản")]
        ChuyenKhoan,

        [Display(Name = "Thẻ")]
        The,

        [Display(Name = "Ví điện tử")]
        ViDienTu,

        [Display(Name = "QR Tự động")]
        QRTuDong
    }

    // Enum cho Trạng thái hóa đơn
    public enum TrangThaiHoaDon
    {
        [Display(Name = "Đang chơi")]
        DangChoi,

        [Display(Name = "Đã thanh toán")]
        DaThanhToan,

        [Display(Name = "Đã hủy")]
        DaHuy
    }

    // Enum cho Loại phiếu sổ quỹ
    public enum LoaiPhieu
    {
        [Display(Name = "Thu")]
        Thu,

        [Display(Name = "Chi")]
        Chi
    }

    // Enum cho Trạng thái giá giờ chơi
    public enum TrangThaiGiaGio
    {
        [Display(Name = "Đang áp dụng")]
        DangApDung,

        [Display(Name = "Hết hiệu lực")]
        HetHieuLuc
    }
}