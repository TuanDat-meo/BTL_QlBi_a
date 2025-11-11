using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BTL_QlBi_a.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ThongKe/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var endOfDay = today.AddDays(1).AddSeconds(-1);
                var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

                // Doanh thu hôm nay
                ViewBag.DoanhThuHomNay = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= today
                        && h.ThoiGianKetThuc <= endOfDay)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0m;

                // Doanh thu tháng
                ViewBag.DoanhThuThang = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startOfMonth
                        && h.ThoiGianKetThuc <= endOfMonth)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0m;

                // Số hóa đơn hôm nay
                ViewBag.SoHoaDon = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= today
                        && h.ThoiGianKetThuc <= endOfDay)
                    .CountAsync();

                // Khách mới tháng này
                ViewBag.KhachMoi = await _context.KhachHang
                    .Where(k => k.NgayDangKy >= startOfMonth && k.NgayDangKy <= endOfMonth)
                    .CountAsync();

                return View("~/Views/Home/Partials/ThongKe/ThongKe.cshtml");
            }
            catch (Exception ex)
            {
                // Set default values on error
                ViewBag.DoanhThuHomNay = 0m;
                ViewBag.DoanhThuThang = 0m;
                ViewBag.SoHoaDon = 0;
                ViewBag.KhachMoi = 0;

                // Log error
                Console.WriteLine($"Error in ThongKe Index: {ex.Message}");

                return View("~/Views/Home/Partials/ThongKe/ThongKe.cshtml");
            }
        }

        // API: Lấy tổng quan
        [HttpGet]
        public async Task<IActionResult> GetTongQuan(DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                var startDate = tuNgay ?? DateTime.Today;
                var endDate = denNgay ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                // Tổng doanh thu
                var tongDoanhThu = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate)
                    .SumAsync(h => h.TongTien);

                // Số hóa đơn
                var soHoaDon = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate)
                    .CountAsync();

                // Số khách hàng
                var soKhachHang = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate
                        && h.MaKH != null)
                    .Select(h => h.MaKH)
                    .Distinct()
                    .CountAsync();

                // Doanh thu trung bình
                var doanhThuTrungBinh = soHoaDon > 0 ? tongDoanhThu / soHoaDon : 0;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        tongDoanhThu,
                        soHoaDon,
                        soKhachHang,
                        doanhThuTrungBinh
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Doanh thu theo ngày (7 ngày gần nhất)
        [HttpGet]
        public async Task<IActionResult> GetDoanhThuTheoNgay()
        {
            try
            {
                var endDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                var startDate = DateTime.Today.AddDays(-6);

                var data = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate)
                    .GroupBy(h => h.ThoiGianKetThuc.Value.Date)
                    .Select(g => new
                    {
                        ngay = g.Key,
                        doanhThu = g.Sum(h => h.TongTien),
                        soHoaDon = g.Count()
                    })
                    .OrderBy(x => x.ngay)
                    .ToListAsync();

                // Fill missing days with 0
                var result = new List<object>();
                for (int i = 0; i < 7; i++)
                {
                    var date = startDate.AddDays(i);
                    var item = data.FirstOrDefault(d => d.ngay == date.Date);
                    result.Add(new
                    {
                        ngay = date.ToString("dd/MM"),
                        doanhThu = item?.doanhThu ?? 0,
                        soHoaDon = item?.soHoaDon ?? 0
                    });
                }

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Doanh thu theo tháng (12 tháng)
        [HttpGet]
        public async Task<IActionResult> GetDoanhThuTheoThang(int nam)
        {
            try
            {
                var startDate = new DateTime(nam, 1, 1);
                var endDate = new DateTime(nam, 12, 31, 23, 59, 59);

                var data = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate)
                    .GroupBy(h => h.ThoiGianKetThuc.Value.Month)
                    .Select(g => new
                    {
                        thang = g.Key,
                        doanhThu = g.Sum(h => h.TongTien),
                        soHoaDon = g.Count()
                    })
                    .OrderBy(x => x.thang)
                    .ToListAsync();

                // Fill missing months with 0
                var result = new List<object>();
                for (int i = 1; i <= 12; i++)
                {
                    var item = data.FirstOrDefault(d => d.thang == i);
                    result.Add(new
                    {
                        thang = $"T{i}",
                        doanhThu = item?.doanhThu ?? 0,
                        soHoaDon = item?.soHoaDon ?? 0
                    });
                }

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Top dịch vụ bán chạy
        [HttpGet]
        public async Task<IActionResult> GetTopDichVu(DateTime? tuNgay, DateTime? denNgay, int top = 10)
        {
            try
            {
                var startDate = tuNgay ?? DateTime.Today.AddDays(-30);
                var endDate = denNgay ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var data = await _context.ChiTietHoaDon
                    .Include(ct => ct.HoaDon)
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.HoaDon.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && ct.HoaDon.ThoiGianKetThuc >= startDate
                        && ct.HoaDon.ThoiGianKetThuc <= endDate)
                    .GroupBy(ct => new { ct.DichVu.MaDV, ct.DichVu.TenDV })
                    .Select(g => new
                    {
                        tenDV = g.Key.TenDV,
                        soLuong = g.Sum(ct => ct.SoLuong),
                        doanhThu = g.Sum(ct => ct.ThanhTien)
                    })
                    .OrderByDescending(x => x.soLuong)
                    .Take(top)
                    .ToListAsync();

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Doanh thu theo loại dịch vụ
        [HttpGet]
        public async Task<IActionResult> GetDoanhThuTheoLoaiDichVu(DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                var startDate = tuNgay ?? DateTime.Today.AddDays(-30);
                var endDate = denNgay ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var data = await _context.ChiTietHoaDon
                    .Include(ct => ct.HoaDon)
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.HoaDon.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && ct.HoaDon.ThoiGianKetThuc >= startDate
                        && ct.HoaDon.ThoiGianKetThuc <= endDate)
                    .GroupBy(ct => ct.DichVu.Loai)
                    .Select(g => new
                    {
                        loai = g.Key.ToString(),
                        doanhThu = g.Sum(ct => ct.ThanhTien),
                        soLuong = g.Sum(ct => ct.SoLuong)
                    })
                    .ToListAsync();

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Doanh thu theo loại bàn
        [HttpGet]
        public async Task<IActionResult> GetDoanhThuTheoLoaiBan(DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                var startDate = tuNgay ?? DateTime.Today.AddDays(-30);
                var endDate = denNgay ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var data = await _context.HoaDon
                    .Include(h => h.BanBia)
                    .ThenInclude(b => b.LoaiBan)
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate)
                    .GroupBy(h => h.BanBia.LoaiBan.TenLoai)
                    .Select(g => new
                    {
                        loaiBan = g.Key,
                        doanhThu = g.Sum(h => h.TongTien),
                        soHoaDon = g.Count()
                    })
                    .OrderByDescending(x => x.doanhThu)
                    .ToListAsync();

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Doanh thu theo khung giờ
        [HttpGet]
        public async Task<IActionResult> GetDoanhThuTheoGio(DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                var startDate = tuNgay ?? DateTime.Today.AddDays(-7);
                var endDate = denNgay ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var hoaDons = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate)
                    .ToListAsync();

                var data = hoaDons
                    .GroupBy(h =>
                    {
                        var hour = h.ThoiGianKetThuc.Value.Hour;
                        if (hour >= 6 && hour < 12) return "Sáng (6h-12h)";
                        if (hour >= 12 && hour < 17) return "Chiều (12h-17h)";
                        if (hour >= 17 && hour < 22) return "Tối (17h-22h)";
                        return "Khuya (22h-6h)";
                    })
                    .Select(g => new
                    {
                        khungGio = g.Key,
                        doanhThu = g.Sum(h => h.TongTien),
                        soHoaDon = g.Count()
                    })
                    .OrderBy(x => x.khungGio)
                    .ToList();

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Top khách hàng
        [HttpGet]
        public async Task<IActionResult> GetTopKhachHang(DateTime? tuNgay, DateTime? denNgay, int top = 10)
        {
            try
            {
                var startDate = tuNgay ?? DateTime.Today.AddDays(-30);
                var endDate = denNgay ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var data = await _context.HoaDon
                    .Include(h => h.KhachHang)
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate
                        && h.MaKH != null)
                    .GroupBy(h => new { h.KhachHang.MaKH, h.KhachHang.TenKH, h.KhachHang.SDT })
                    .Select(g => new
                    {
                        tenKH = g.Key.TenKH,
                        sdt = g.Key.SDT,
                        tongChiTieu = g.Sum(h => h.TongTien),
                        soLanDen = g.Count()
                    })
                    .OrderByDescending(x => x.tongChiTieu)
                    .Take(top)
                    .ToListAsync();

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Tỷ lệ phương thức thanh toán
        [HttpGet]
        public async Task<IActionResult> GetPhuongThucThanhToan(DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                var startDate = tuNgay ?? DateTime.Today.AddDays(-30);
                var endDate = denNgay ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var data = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startDate
                        && h.ThoiGianKetThuc <= endDate)
                    .GroupBy(h => h.PhuongThucThanhToan)
                    .Select(g => new
                    {
                        phuongThuc = g.Key,
                        soLuong = g.Count(),
                        tongTien = g.Sum(h => h.TongTien)
                    })
                    .ToListAsync();

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: So sánh doanh thu theo kỳ
        [HttpGet]
        public async Task<IActionResult> GetSoSanhDoanhThu(string loai)
        {
            try
            {
                var now = DateTime.Now;
                DateTime startCurrent, endCurrent, startPrevious, endPrevious;

                if (loai == "ngay")
                {
                    startCurrent = DateTime.Today;
                    endCurrent = DateTime.Today.AddDays(1).AddSeconds(-1);
                    startPrevious = DateTime.Today.AddDays(-1);
                    endPrevious = DateTime.Today.AddSeconds(-1);
                }
                else if (loai == "tuan")
                {
                    var dayOfWeek = (int)now.DayOfWeek;
                    startCurrent = now.Date.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
                    endCurrent = startCurrent.AddDays(7).AddSeconds(-1);
                    startPrevious = startCurrent.AddDays(-7);
                    endPrevious = startCurrent.AddSeconds(-1);
                }
                else // thang
                {
                    startCurrent = new DateTime(now.Year, now.Month, 1);
                    endCurrent = startCurrent.AddMonths(1).AddSeconds(-1);
                    startPrevious = startCurrent.AddMonths(-1);
                    endPrevious = startCurrent.AddSeconds(-1);
                }

                var doanhThuHienTai = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startCurrent
                        && h.ThoiGianKetThuc <= endCurrent)
                    .SumAsync(h => h.TongTien);

                var doanhThuTruoc = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan
                        && h.ThoiGianKetThuc >= startPrevious
                        && h.ThoiGianKetThuc <= endPrevious)
                    .SumAsync(h => h.TongTien);

                var tangTruong = doanhThuTruoc > 0
                    ? ((doanhThuHienTai - doanhThuTruoc) / doanhThuTruoc) * 100
                    : 0;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        hienTai = doanhThuHienTai,
                        truoc = doanhThuTruoc,
                        tangTruong = Math.Round(tangTruong, 2)
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}