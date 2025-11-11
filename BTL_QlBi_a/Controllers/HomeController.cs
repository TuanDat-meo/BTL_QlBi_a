#nullable enable
using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BTL_QlBi_a.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
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
        public IActionResult BanBia()
        {
            // Redirect to QLBan controller
            return RedirectToAction("BanBia", "QLBan");
        }
        public async Task<IActionResult> DichVu()
        {
            await LoadHeaderStats();
            var danhSachDV = await _context.DichVus.ToListAsync();
            return View(danhSachDV);
        }

  

        public async Task<IActionResult> ThongKe()
        {
            await LoadHeaderStats();

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            ViewBag.DoanhThuThang = await _context.HoaDon
                .Where(h => h.ThoiGianKetThuc.HasValue &&
                           h.ThoiGianKetThuc.Value >= firstDayOfMonth &&
                           h.TrangThai == TrangThaiHoaDon.DaThanhToan)
                .SumAsync(h => h.TongTien);

            return View("~/Views/Home/Partials/ThongKe/ThongKe.cshtml");
        }

        public async Task<IActionResult> BangGia()
        {
            await LoadHeaderStats();
            var loaiBan = await _context.LoaiBan.ToListAsync();
            ViewBag.LoaiBan = loaiBan;
            return View();
        }

        public async Task<IActionResult> NhanVien()
        {
            if (HttpContext.Session.GetInt32("MaNV") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get current user role
            var currentRole = HttpContext.Session.GetString("ChucVu");

            // Load employees based on role
            IQueryable<NhanVien> query = _context.NhanVien
                .Include(n => n.NhomQuyen);

            // If not admin/manager, only show active employees
            if (currentRole != "Admin" && currentRole != "Quản lý")
            {
                query = query.Where(n => n.TrangThai == TrangThaiNhanVien.DangLam);
            }

            var employees = await query
                .OrderBy(n => n.TrangThai)
                .ThenBy(n => n.MaNhom)
                .ThenBy(n => n.TenNV)
                .ToListAsync();

            // Calculate stats for header
            ViewBag.TongNhanVien = employees.Count(n => n.TrangThai == TrangThaiNhanVien.DangLam);
            ViewBag.NhanVienNghi = employees.Count(n => n.TrangThai == TrangThaiNhanVien.Nghi);

            // Today's attendance
            var today = DateTime.Today;
            var todayAttendance = await _context.ChamCong
                .Where(c => c.Ngay == today)
                .CountAsync();
            ViewBag.ChamCongHomNay = todayAttendance;

            return View(employees);
        }

        public async Task<IActionResult> CaiDat()
        {
            await LoadHeaderStats();
            return View();
        }
    }
}