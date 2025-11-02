using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using BTL_QlBi_a.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BTL_QlBi_a.Controllers
{
    public class HomeController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IActionResult> Index()
        {
            // Lấy MaNV từ Session
            int? maNV = HttpContext.Session.GetInt32("MaNV");

            // Lấy thông tin nhân viên từ database
            NhanVien nv = null;
            if (maNV.HasValue)
            {
                nv = await _context.NhanVien.FindAsync(maNV.Value);
            }

            // Lấy TenNV và ChucVu từ Session (đã lưu khi đăng nhập)
            ViewBag.TenNV = HttpContext.Session.GetString("TenNV") ?? "Khách";
            ViewBag.ChucVu = HttpContext.Session.GetString("ChucVu") ?? "Không xác định";

            // Thống kê bàn - FIX: So sánh với enum
            var danhSachBan = await _context.BanBia.ToListAsync();
            ViewBag.BanTrong = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.Trong);
            ViewBag.BanDangChoi = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DangChoi);
            ViewBag.BanDaDat = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DaDat);

            // Doanh thu hôm nay - FIX: So sánh với enum
            var today = DateTime.Today;
            var doanhThuHomNay = await _context.HoaDon
                .Where(h => h.ThoiGianKetThuc.HasValue &&
                           h.ThoiGianKetThuc.Value.Date == today &&
                           h.TrangThai == TrangThaiHoaDon.DaThanhToan)
                .SumAsync(h => h.TongTien);
            ViewBag.DoanhThuHomNay = doanhThuHomNay.ToString("N0") + "đ";

            // Tổng khách hàng
            ViewBag.TongKhachHang = await _context.KhachHang.CountAsync();

            // Lấy danh sách bàn với thông tin liên quan - FIX: BanBi_a → BanBia
            var danhSachBanFull = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .Include(b => b.KhuVuc)
                .OrderBy(b => b.MaBan)
                .ToListAsync();

            return View(danhSachBanFull);
        }

        public async Task<IActionResult> ChiTietBan(int maBan)
        {
            // FIX: BanBi_a → BanBia
            var ban = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .Include(b => b.KhuVuc)
                .FirstOrDefaultAsync(b => b.MaBan == maBan);

            if (ban == null)
            {
                return NotFound();
            }

            // Lấy hóa đơn đang mở của bàn (nếu có) - FIX: So sánh với enum
            var hoaDon = await _context.HoaDon
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .FirstOrDefaultAsync(h => h.MaBan == maBan &&
                                         h.TrangThai == TrangThaiHoaDon.DangChoi);

            ViewBag.HoaDon = hoaDon;

            // Lấy chi tiết dịch vụ đã order (nếu có hóa đơn)
            if (hoaDon != null)
            {
                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .ToListAsync();
                ViewBag.ChiTietDichVu = chiTiet;
            }

            return View(ban);
        }

        [HttpPost]
        public async Task<IActionResult> BatDauChoi(int maBan, int? maKH)
        {
            try
            {
                // FIX: BanBi_a → BanBia
                var ban = await _context.BanBia.FindAsync(maBan);
                if (ban == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bàn" });
                }

                // FIX: So sánh với enum
                if (ban.TrangThai != TrangThaiBan.Trong)
                {
                    return Json(new { success = false, message = "Bàn đang được sử dụng" });
                }

                // Lấy MaNV từ Session
                int? maNV = HttpContext.Session.GetInt32("MaNV");

                if (!maNV.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // Cập nhật trạng thái bàn - FIX: Gán enum
                ban.TrangThai = TrangThaiBan.DangChoi;
                ban.GioBatDau = DateTime.Now;
                ban.MaKH = maKH;

                // Tạo hóa đơn mới - FIX: Gán enum
                var hoaDon = new HoaDon
                {
                    MaBan = maBan,
                    MaKH = maKH,
                    MaNV = maNV.Value,
                    ThoiGianBatDau = DateTime.Now,
                    TrangThai = TrangThaiHoaDon.DangChoi
                };

                _context.HoaDon.Add(hoaDon);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bắt đầu chơi thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> KetThucChoi(int maBan)
        {
            try
            {
                // FIX: BanBi_a → BanBia
                var ban = await _context.BanBia.FindAsync(maBan);
                if (ban == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bàn" });
                }

                // Tìm hóa đơn đang mở - FIX: So sánh với enum
                var hoaDon = await _context.HoaDon
                    .Include(h => h.BanBia)
                    .ThenInclude(b => b.LoaiBan)
                    .FirstOrDefaultAsync(h => h.MaBan == maBan &&
                                             h.TrangThai == TrangThaiHoaDon.DangChoi);

                if (hoaDon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Tính toán thời gian và tiền
                hoaDon.ThoiGianKetThuc = DateTime.Now;
                var duration = (hoaDon.ThoiGianKetThuc.Value - hoaDon.ThoiGianBatDau.Value).TotalMinutes;
                hoaDon.ThoiLuongPhut = (int)Math.Ceiling(duration);

                // Tính tiền bàn (làm tròn lên 15 phút)
                var soPhutLamTron = Math.Ceiling(duration / 15) * 15;
                var soGio = soPhutLamTron / 60;

                if (hoaDon.BanBia?.LoaiBan != null)
                {
                    hoaDon.TienBan = hoaDon.BanBia.LoaiBan.GiaGio * (decimal)soGio;
                }
                else
                {
                    hoaDon.TienBan = 0;
                }

                // Tính tiền dịch vụ
                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .ToListAsync();

                hoaDon.TienDichVu = chiTiet.Sum(ct => ct.ThanhTien ?? 0);
                hoaDon.TongTien = hoaDon.TienBan + hoaDon.TienDichVu - hoaDon.GiamGia;

                // Cập nhật trạng thái bàn - FIX: Gán enum
                ban.TrangThai = TrangThaiBan.Trong;
                ban.GioBatDau = null;
                ban.MaKH = null;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đã tính toán hóa đơn",
                    hoaDonId = hoaDon.MaHD,
                    tongTien = hoaDon.TongTien
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToan(int maHD, string phuongThuc)
        {
            try
            {
                var hoaDon = await _context.HoaDon.FindAsync(maHD);
                if (hoaDon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // FIX: Gán enum
                hoaDon.TrangThai = TrangThaiHoaDon.DaThanhToan;

                // FIX: Convert string to enum
                if (Enum.TryParse<PhuongThucThanhToan>(phuongThuc, out var pttt))
                {
                    hoaDon.PhuongThucThanhToan = pttt;
                }
                else
                {
                    hoaDon.PhuongThucThanhToan = PhuongThucThanhToan.TienMat; // Mặc định
                }

                // Cập nhật thông tin khách hàng (nếu có)
                if (hoaDon.MaKH.HasValue)
                {
                    var khachHang = await _context.KhachHang.FindAsync(hoaDon.MaKH.Value);
                    if (khachHang != null)
                    {
                        khachHang.TongChiTieu += hoaDon.TongTien;
                        khachHang.DiemTichLuy += (int)(hoaDon.TongTien / 1000m);
                        khachHang.LanDenCuoi = DateTime.Now;

                        // Cập nhật hạng thành viên - FIX: Gán enum
                        if (khachHang.TongChiTieu >= 10000000)
                            khachHang.HangTV = HangThanhVien.BachKim;
                        else if (khachHang.TongChiTieu >= 5000000)
                            khachHang.HangTV = HangThanhVien.Vang;
                        else if (khachHang.TongChiTieu >= 2000000)
                            khachHang.HangTV = HangThanhVien.Bac;
                        else
                            khachHang.HangTV = HangThanhVien.Dong;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}