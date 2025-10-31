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

    public enum KhuVucBan
    {
        [Display(Name = "Tầng 1")]
        Tang1,

        [Display(Name = "Tầng 2")]
        Tang2,

        [Display(Name = "VIP")]
        VIP
    }

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

    public enum TrangThaiLoaiBan
    {
        [Display(Name = "Đang áp dụng")]
        DangApDung,

        [Display(Name = "Ngừng áp dụng")]
        NgungApDung
    }

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

    public enum LoaiMatHang
    {
        [Display(Name = "Đồ uống")]
        DoUong,

        [Display(Name = "Đồ ăn")]
        DoAn,

        [Display(Name = "Dụng cụ bi-a")]
        DungCuBia,

        [Display(Name = "Khác")]
        Khac
    }
    public enum TrangThaiMatHang
    {
        [Display(Name = "Còn hàng")]
        ConHang,

        [Display(Name = "Hết hàng")]
        HetHang,

        [Display(Name = "Ngừng kinh doanh")]
        NgungKinhDoanh
    }

    public enum LoaiDichVu
    {
        [Display(Name = "Đồ uống")]
        DoUong,

        [Display(Name = "Đồ ăn")]
        DoAn,

        [Display(Name = "Khác")]
        Khac
    }

    public enum TrangThaiDichVu
    {
        [Display(Name = "Còn hàng")]
        ConHang,

        [Display(Name = "Hết hàng")]
        HetHang,

        [Display(Name = "Ngừng bán")]
        NgungBan
    }

    public enum ChucVu
    {
        [Display(Name = "Quản lý")]
        QuanLy,

        [Display(Name = "Thu ngân")]
        ThuNgan,

        [Display(Name = "Phục vụ")]
        PhucVu
    }

    public enum CaLamViec
    {
        [Display(Name = "Sáng")]
        Sang,

        [Display(Name = "Chiều")]
        Chieu,

        [Display(Name = "Tối")]
        Toi
    }

    public enum TrangThaiNhanVien
    {
        [Display(Name = "Đang làm")]
        DangLam,

        [Display(Name = "Nghỉ")]
        Nghi
    }

    public enum PhuongThucXacThuc
    {
        [Display(Name = "Thủ công")]
        ThuCong,

        [Display(Name = "Vân tay")]
        VanTay,

        [Display(Name = "FaceID")]
        FaceID
    }

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

    public enum PhuongThucThanhToan
    {
        [Display(Name = "Tiền mặt")]
        TienMat,

        [Display(Name = "Chuyển khoản")]
        ChuyenKhoan,

        [Display(Name = "Thẻ")]
        The,

        [Display(Name = "Ví điện tử")]
        ViDienTu
    }

    public enum TrangThaiHoaDon
    {
        [Display(Name = "Đang chơi")]
        DangChoi,

        [Display(Name = "Đã thanh toán")]
        DaThanhToan,

        [Display(Name = "Đã hủy")]
        DaHuy
    }

    public enum TrangThaiGiaGio
    {
        [Display(Name = "Đang áp dụng")]
        DangApDung,

        [Display(Name = "Hết hiệu lực")]
        HetHieuLuc
    }
}