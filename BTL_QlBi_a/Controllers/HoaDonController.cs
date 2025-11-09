using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QlBi_a.Controllers
{
    public class HoaDonController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Hàm Phụ
        private async Task LoadHeaderStats()
        {
            var danhSachBan = await _context.BanBia.ToListAsync();
            ViewBag.BanTrong = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.Trong);
            ViewBag.BanDangChoi = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DangChoi);
            ViewBag.BanDaDat = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DaDat);

            var today = DateTime.Today;
            var doanhThuHomNay = await _context.HoaDon
                .Where(h => h.ThoiGianKetThuc.HasValue &&
                           h.ThoiGianKetThuc.Value.Date == today &&
                           h.TrangThai == TrangThaiHoaDon.DaThanhToan)
                .SumAsync(h => h.TongTien);
            ViewBag.DoanhThuHomNay = doanhThuHomNay.ToString("N0") + "đ";

            ViewBag.TongKhachHang = await _context.KhachHang.CountAsync();
        }


        #endregion
        public async Task<IActionResult> HoaDon(
            DateTime? fromDate, DateTime? toDate, string status = "All")
        {
            await LoadHeaderStats();

            var query = _context.HoaDon
                .Include(h => h.BanBia)
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .AsQueryable();


            TrangThaiHoaDon? statusEnum = null;
            if (status != "All" && !string.IsNullOrEmpty(status))
            {
                if (status == "Đang chơi")
                    statusEnum = TrangThaiHoaDon.DangChoi;
                else if (status == "Đã thanh toán")
                    statusEnum = TrangThaiHoaDon.DaThanhToan;
                else if (status == "Đã hủy")
                    statusEnum = TrangThaiHoaDon.DaHuy;
            }

            if (statusEnum.HasValue)
            {
                query = query.Where(h => h.TrangThai == statusEnum.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.ThoiGianBatDau >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(h => h.ThoiGianBatDau < toDate.Value.AddDays(1));
            }

            var danhSachHD = await query
                .OrderByDescending(h => h.ThoiGianBatDau)
                .ToListAsync();

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Status = status;

            return View("~/Views/Home/HoaDon.cshtml", danhSachHD);
        }

        public async Task<IActionResult> ChiTietHoaDon(int maHD)
        {
            Console.WriteLine($"=== TaiChiTietHoaDon called for MaHD: {maHD} ===");

            var hoaDon = await _context.HoaDon
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.BanBia) // Include bàn
                .FirstOrDefaultAsync(h => h.MaHD == maHD);

            if (hoaDon == null)
            {
                Console.WriteLine($"Hóa đơn {maHD} không tìm thấy!");
                return NotFound(new { message = "Không tìm thấy hóa đơn" });
            }

            var chiTiet = await _context.ChiTietHoaDon
                .Include(ct => ct.DichVu)
                .Where(ct => ct.MaHD == hoaDon.MaHD)
                .ToListAsync();

            ViewBag.ChiTietDichVu = chiTiet;
            Console.WriteLine($"Chi tiết dịch vụ: {chiTiet.Count} items");

            return PartialView("~/Views/Home/Partials/HoaDon/_ChiTietHoaDon.cshtml", hoaDon);
        }
        public async Task<IActionResult> InHoaDon(int maHD)
        {
            Console.WriteLine($"=== InHoaDon called for MaHD: {maHD} ===");

            var hoaDon = await _context.HoaDon
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.BanBia)
                    .ThenInclude(b => b.LoaiBan)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.DichVu)
                .FirstOrDefaultAsync(h => h.MaHD == maHD);

            if (hoaDon == null)
            {
                return NotFound("Không tìm thấy hóa đơn.");
            }

            return PartialView("~/Views/Home/Partials/HoaDon/_InHoaDon.cshtml", hoaDon);
        }
    }
}
