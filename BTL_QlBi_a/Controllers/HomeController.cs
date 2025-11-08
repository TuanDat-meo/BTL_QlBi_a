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

        public async Task<IActionResult> Index()
        {
            await LoadHeaderStats();

            var danhSachBan = await _context.BanBia
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
            Console.WriteLine($"=== ChiTietBan called for MaBan: {maBan} ===");

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
                Console.WriteLine($"BatDauChoi - MaBan: {request.MaBan}, MaKH: {request.MaKH}");

                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.TrangThai != TrangThaiBan.Trong)
                    return Json(new { success = false, message = "Bàn đang được sử dụng" });

                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (!maNV.HasValue)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                ban.TrangThai = TrangThaiBan.DangChoi;
                ban.GioBatDau = DateTime.Now;
                ban.MaKH = request.MaKH;

                var hoaDon = new HoaDon
                {
                    MaBan = request.MaBan,
                    MaKH = request.MaKH,
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
                Console.WriteLine($"Lỗi BatDauChoi: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> KetThucChoi([FromBody] KetThucChoiRequest request)
        {
            try
            {
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                var hoaDon = await _context.HoaDon
                    .Include(h => h.BanBia)
                    .ThenInclude(b => b.LoaiBan)
                    .FirstOrDefaultAsync(h => h.MaBan == request.MaBan &&
                                             h.TrangThai == TrangThaiHoaDon.DangChoi);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

                hoaDon.ThoiGianKetThuc = DateTime.Now;
                var duration = (hoaDon.ThoiGianKetThuc.Value - hoaDon.ThoiGianBatDau.Value).TotalMinutes;
                hoaDon.ThoiLuongPhut = (int)Math.Ceiling(duration);

                var soPhutLamTron = Math.Ceiling(duration / 15) * 15;
                var soGio = soPhutLamTron / 60;

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
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public async Task<IActionResult> DichVu()
        {
            await LoadHeaderStats();
            var danhSachDV = await _context.DichVu.ToListAsync();
            return View(danhSachDV);
        }

        public async Task<IActionResult> KhachHang()
        {
            await LoadHeaderStats();
            var danhSachKH = await _context.KhachHang
                .OrderByDescending(k => k.TongChiTieu)
                .ToListAsync();

            return View(danhSachKH);
        }
        public async Task<IActionResult> ChiTietKhachHang(int maKH)
        {
            Console.WriteLine($"=== ChiTietKhachHang called for MaKH: {maKH} ===");

            var khachHang = await _context.KhachHang
                .Include(kh => kh.HoaDons) // Include danh sách hóa đơn của khách
                    .ThenInclude(hd => hd.BanBia) // Kèm thông tin bàn của hóa đơn đó
                .FirstOrDefaultAsync(kh => kh.MaKH == maKH);

            if (khachHang == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng" });
            }

            if (khachHang.HoaDons != null)
            {
                khachHang.HoaDons = khachHang.HoaDons
                    .OrderByDescending(hd => hd.ThoiGianKetThuc ?? hd.ThoiGianBatDau)
                    .ToList();
            }
            return PartialView("_ChiTietKhachHang", khachHang);
        }
        public async Task<IActionResult> TaiFormKhachHang(int maKH = 0)
        {
            if (maKH == 0)
            {
                // THÊM MỚI: Trả về 1 model KhachHang trống
                var newCustomer = new KhachHang
                {
                    NgayDangKy = DateTime.Now // Có thể set giá trị mặc định
                };
                return PartialView("_FormKhachHang", newCustomer);
            }

            // SỬA: Tìm khách hàng và trả về
            var khachHang = await _context.KhachHang.FindAsync(maKH);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng");
            }

            return PartialView("_FormKhachHang", khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật
        public async Task<IActionResult> LuuKhachHang([FromForm] KhachHang model)
        {
            // Bỏ qua validate Hạng TV vì nó không có trên form "Thêm"
            ModelState.Remove("HangTV");

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.MaKH == 0) // THÊM MỚI
                    {
                        // Set giá trị mặc định cho khách mới
                        model.HangTV = HangThanhVien.Dong; // Mặc định là Đồng
                        model.NgayDangKy = DateTime.Now;
                        model.DiemTichLuy = 0;
                        model.TongChiTieu = 0;

                        _context.KhachHang.Add(model);
                    }
                    else // SỬA
                    {
                        // Lấy bản gốc từ DB
                        var existingCustomer = await _context.KhachHang.FindAsync(model.MaKH);
                        if (existingCustomer == null)
                        {
                            return Json(new { success = false, message = "Lỗi: Không tìm thấy khách hàng!" });
                        }

                        // Cập nhật các trường được phép sửa
                        existingCustomer.TenKH = model.TenKH;
                        existingCustomer.SDT = model.SDT;
                        existingCustomer.Email = model.Email;
                        // Không cập nhật Hạng, Điểm, Tổng chi tiêu ở đây

                        _context.KhachHang.Update(existingCustomer);
                    }

                    await _context.SaveChangesAsync();

                    // Trả về success
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            // Nếu model không hợp lệ
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
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