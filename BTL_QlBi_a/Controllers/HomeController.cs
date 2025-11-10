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

            return View();
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
            await LoadHeaderStats();
            var danhSachNV = await _context.NhanVien
                .Include(nv => nv.NhomQuyen)
                .ToListAsync();

            return View(danhSachNV);
        }

        public async Task<IActionResult> CaiDat()
        {
            await LoadHeaderStats();
            return View();
        }
    }
}