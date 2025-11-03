#nullable enable
using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using BTL_QlBi_a.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
<<<<<<< HEAD
            var danhSachBan = await _context.BanBia.ToListAsync();
            ViewBag.BanTrong = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.Trong);
            ViewBag.BanDangChoi = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DangChoi);
            ViewBag.BanDaDat = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DaDat);

=======
            // Lấy MaNV từ Session
            int? maNV = HttpContext.Session.GetInt32("MaNV");

            // Lấy thông tin nhân viên từ database
            NhanVien? nv = null;
            if (maNV.HasValue)
            {
                nv = await _context.NhanVien.FindAsync(maNV.Value);
            }

            // Lấy TenNV và ChucVu từ Session (đã lưu khi đăng nhập)
            ViewBag.TenNV = HttpContext.Session.GetString("TenNV") ?? "Khách";
            ViewBag.ChucVu = HttpContext.Session.GetString("ChucVu") ?? "Không xác định";

            // Thống kê bàn - So sánh với enum
            var danhSachBan = await _context.BanBia.ToListAsync();
            ViewBag.BanTrong = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.Trong);
            ViewBag.BanDangChoi = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DangChoi);
            ViewBag.BanDaDat = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DaDat);

            // Doanh thu hôm nay - So sánh với enum
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
            var today = DateTime.Today;
            var doanhThuHomNay = await _context.HoaDon
                .Where(h => h.ThoiGianKetThuc.HasValue &&
                           h.ThoiGianKetThuc.Value.Date == today &&
                           h.TrangThai == TrangThaiHoaDon.DaThanhToan)
                .SumAsync(h => h.TongTien);
            ViewBag.DoanhThuHomNay = doanhThuHomNay.ToString("N0") + "đ";

            ViewBag.TongKhachHang = await _context.KhachHang.CountAsync();
        }

<<<<<<< HEAD
        public async Task<IActionResult> Index()
        {
            await LoadHeaderStats();

            var danhSachBan = await _context.BanBia
=======
            // Lấy danh sách bàn với thông tin liên quan
            var danhSachBanFull = await _context.BanBia
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .Include(b => b.KhuVuc)
                .OrderBy(b => b.MaBan)
                .ToListAsync();

            return View("Index", danhSachBan);
        }

        // FIX: Thêm log và đảm bảo return PartialView đúng
        public async Task<IActionResult> ChiTietBan(int maBan)
        {
<<<<<<< HEAD
            Console.WriteLine($"=== ChiTietBan called for MaBan: {maBan} ===");

=======
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
            var ban = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .Include(b => b.KhuVuc)
                .FirstOrDefaultAsync(b => b.MaBan == maBan);

            if (ban == null)
            {
                Console.WriteLine($"Bàn {maBan} không tìm thấy!");
                return NotFound(new { message = "Không tìm thấy bàn" });
            }

            Console.WriteLine($"Bàn tìm thấy: {ban.TenBan}, Trạng thái: {ban.TrangThai}");

            var hoaDon = await _context.HoaDon
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .FirstOrDefaultAsync(h => h.MaBan == maBan &&
                                         h.TrangThai == TrangThaiHoaDon.DangChoi);

            ViewBag.HoaDon = hoaDon;
            Console.WriteLine($"Hóa đơn: {(hoaDon != null ? $"Có - MaHD: {hoaDon.MaHD}" : "Không có")}");

            if (hoaDon != null)
            {
                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .ToListAsync();
                ViewBag.ChiTietDichVu = chiTiet;
                Console.WriteLine($"Chi tiết dịch vụ: {chiTiet.Count} items");
            }

            // QUAN TRỌNG: Return PartialView
            return PartialView("ChiTietBan", ban);
        }

        [HttpPost]
        public async Task<IActionResult> BatDauChoi([FromBody] BatDauChoiRequest request)
        {
            try
            {
<<<<<<< HEAD
                Console.WriteLine($"BatDauChoi - MaBan: {request.MaBan}, MaKH: {request.MaKH}");

                var ban = await _context.BanBia.FindAsync(request.MaBan);
=======
                var ban = await _context.BanBia.FindAsync(maBan);
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.TrangThai != TrangThaiBan.Trong)
<<<<<<< HEAD
                    return Json(new { success = false, message = "Bàn đang được sử dụng" });
=======
                {
                    return Json(new { success = false, message = "Bàn đang được sử dụng" });
                }

                // Lấy MaNV từ Session
                int? maNV = HttpContext.Session.GetInt32("MaNV");
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641

                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (!maNV.HasValue)
<<<<<<< HEAD
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

=======
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // Cập nhật trạng thái bàn
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
                ban.TrangThai = TrangThaiBan.DangChoi;
                ban.GioBatDau = DateTime.Now;
                ban.MaKH = request.MaKH;

                var hoaDon = new HoaDon
                {
<<<<<<< HEAD
                    MaBan = request.MaBan,
                    MaKH = request.MaKH,
=======
                    MaBan = maBan,
                    MaKH = maKH,
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
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
<<<<<<< HEAD
                Console.WriteLine($"Lỗi BatDauChoi: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
=======
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
            }
        }

        [HttpPost]
        public async Task<IActionResult> KetThucChoi([FromBody] KetThucChoiRequest request)
        {
            try
            {
<<<<<<< HEAD
                var ban = await _context.BanBia.FindAsync(request.MaBan);
=======
                var ban = await _context.BanBia.FindAsync(maBan);
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                var hoaDon = await _context.HoaDon
                    .Include(h => h.BanBia)
                    .ThenInclude(b => b.LoaiBan)
<<<<<<< HEAD
                    .FirstOrDefaultAsync(h => h.MaBan == request.MaBan &&
=======
                    .FirstOrDefaultAsync(h => h.MaBan == maBan &&
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
                                             h.TrangThai == TrangThaiHoaDon.DangChoi);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

                hoaDon.ThoiGianKetThuc = DateTime.Now;
                var duration = (hoaDon.ThoiGianKetThuc.Value - hoaDon.ThoiGianBatDau.Value).TotalMinutes;
                hoaDon.ThoiLuongPhut = (int)Math.Ceiling(duration);

                var soPhutLamTron = Math.Ceiling(duration / 15) * 15;
                var soGio = soPhutLamTron / 60;
<<<<<<< HEAD
=======

                if (hoaDon.BanBia?.LoaiBan != null)
                {
                    hoaDon.TienBan = hoaDon.BanBia.LoaiBan.GiaGio * (decimal)soGio;
                }
                else
                {
                    hoaDon.TienBan = 0;
                }
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641

                if (hoaDon.BanBia?.LoaiBan != null)
                    hoaDon.TienBan = hoaDon.BanBia.LoaiBan.GiaGio * (decimal)soGio;
                else
                    hoaDon.TienBan = 0;

                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .ToListAsync();

                hoaDon.TienDichVu = chiTiet.Sum(ct => ct.ThanhTien ?? 0);
                hoaDon.TongTien = hoaDon.TienBan + hoaDon.TienDichVu - hoaDon.GiamGia;

<<<<<<< HEAD
=======
                // Cập nhật trạng thái bàn
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
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
<<<<<<< HEAD
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public async Task<IActionResult> DichVu()
=======
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToan(int maHD, string? phuongThuc)
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
        {
            await LoadHeaderStats();
            var danhSachDV = await _context.DichVu.ToListAsync();
            return View(danhSachDV);
        }

<<<<<<< HEAD
        public async Task<IActionResult> HoaDon()
        {
            await LoadHeaderStats();
            var danhSachHD = await _context.HoaDon
                .Include(h => h.BanBia)
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .OrderByDescending(h => h.ThoiGianBatDau)
                .ToListAsync();

            return View(danhSachHD);
        }

        public async Task<IActionResult> KhachHang()
        {
            await LoadHeaderStats();
            var danhSachKH = await _context.KhachHang
                .OrderByDescending(k => k.TongChiTieu)
                .ToListAsync();

            return View(danhSachKH);
        }
=======
                hoaDon.TrangThai = TrangThaiHoaDon.DaThanhToan;

                // Convert string to enum
                if (!string.IsNullOrEmpty(phuongThuc) &&
                    Enum.TryParse<PhuongThucThanhToan>(phuongThuc, out var pttt))
                {
                    hoaDon.PhuongThucThanhToan = pttt;
                }
                else
                {
                    hoaDon.PhuongThucThanhToan = PhuongThucThanhToan.TienMat;
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

                        // Cập nhật hạng thành viên
                        khachHang.HangTV = khachHang.TongChiTieu switch
                        {
                            >= 10000000 => HangThanhVien.BachKim,
                            >= 5000000 => HangThanhVien.Vang,
                            >= 2000000 => HangThanhVien.Bac,
                            _ => HangThanhVien.Dong
                        };
                    }
                }
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641

        public async Task<IActionResult> ThongKe()
        {
            await LoadHeaderStats();

<<<<<<< HEAD
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
=======
                return Json(new { success = true, message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
>>>>>>> e7f15b64503b8426516cf40bbd21e1680e35d641
        }
    }

    // Request models
    public class BatDauChoiRequest
    {
        public int MaBan { get; set; }
        public int? MaKH { get; set; }
    }

    public class KetThucChoiRequest
    {
        public int MaBan { get; set; }
    }
}