using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QlBi_a.Controllers
{
    public class QLBanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public QLBanController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        #region Helper Methods

        private async Task LoadHeaderStats()
        {
            // Chỉ lấy bàn KHÔNG bị ẩn
            var danhSachBan = await _context.BanBia
                .Where(b => b.TrangThai != TrangThaiBan.NgungHoatDong) // THAY ĐỔI Ở ĐÂY
                .ToListAsync();

            // Sử dụng client-side evaluation để tránh lỗi LINQ
            ViewBag.BanTrong = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.Trong);
            ViewBag.BanDangChoi = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DangChoi);
            ViewBag.BanDaDat = danhSachBan.Count(b => b.TrangThai == TrangThaiBan.DaDat);

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var doanhThuHomNay = await _context.HoaDon
                .Where(h => h.ThoiGianKetThuc.HasValue &&
                           h.ThoiGianKetThuc.Value >= today &&
                           h.ThoiGianKetThuc.Value < tomorrow &&
                           h.TrangThai == TrangThaiHoaDon.DaThanhToan)
                .SumAsync(h => (decimal?)h.TongTien) ?? 0;

            ViewBag.DoanhThuHomNay = doanhThuHomNay.ToString("N0") + "đ";
            ViewBag.TongKhachHang = await _context.KhachHang.CountAsync();
        }

        [HttpGet]
        public async Task<IActionResult> DanhSachBanDaAn()
        {
            var tenNhom = HttpContext.Session.GetString("TenNhom");
            if (tenNhom != "Admin" && tenNhom != "Quản lý")
            {
                return Forbid();
            }

            var banDaAn = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhuVuc)
                .Where(b => b.TrangThai == TrangThaiBan.NgungHoatDong) // THAY ĐỔI Ở ĐÂY
                .OrderBy(b => b.MaBan)
                .ToListAsync();

            return View("~/Views/Home/DanhSachBanDaAn.cshtml", banDaAn);
        }
        [HttpPost]
        public async Task<IActionResult> KhoiPhucBan([FromBody] XoaBanRequest request)
        {
            try
            {
                var tenNhom = HttpContext.Session.GetString("TenNhom");
                if (tenNhom != "Admin" && tenNhom != "Quản lý")
                {
                    return Json(new { success = false, message = "Không có quyền thực hiện" });
                }

                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.TrangThai != TrangThaiBan.NgungHoatDong) // THAY ĐỔI Ở ĐÂY
                    return Json(new { success = false, message = "Bàn này không ở trạng thái ngưng hoạt động" });

                // Khôi phục bàn về trạng thái trống
                ban.TrangThai = TrangThaiBan.Trong;

                // Xóa prefix [NGƯNG] trong ghi chú
                if (!string.IsNullOrEmpty(ban.GhiChu))
                {
                    ban.GhiChu = ban.GhiChu.Replace("[NGƯNG HOẠT ĐỘNG]", "")
                                           .Replace("[NGƯNG]", "")
                                           .Trim();
                }

                await _context.SaveChangesAsync();

                // Ghi log
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Khôi phục bàn",
                        ChiTiet = $"Khôi phục bàn {ban.TenBan} (ID: {ban.MaBan}) từ trạng thái ngưng hoạt động"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Khôi phục bàn thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in KhoiPhucBan: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"ban_{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "asset", "img", "tables");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"tables/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                return null;
            }
        }

        private void DeleteImage(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName)) return;

                var filePath = Path.Combine(_environment.WebRootPath, "asset", "img", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }

        #endregion

        #region Views & Main Page

        public async Task<IActionResult> BanBia()
        {
            var tenNhom = HttpContext.Session.GetString("TenNhom") ?? "Nhân viên";
            ViewBag.TenNhom = tenNhom;
            ViewBag.ChucVu = tenNhom;

            await LoadHeaderStats();

            // Lọc bỏ bàn có trạng thái NgungHoatDong (đã ẩn)
            var danhSachBan = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .Include(b => b.KhuVuc)
                .Where(b => b.TrangThai != TrangThaiBan.NgungHoatDong) // THAY ĐỔI Ở ĐÂY
                .OrderBy(b => b.MaBan)
                .ToListAsync();

            return View("~/Views/Home/BanBia.cshtml", danhSachBan);
        }

        #endregion


        #region Chi tiết bàn & API

        // GET: Chi tiết bàn (Right Panel)
        [HttpGet]
        public async Task<IActionResult> ChiTietBan(int maBan)
        {
            var tenNhom = HttpContext.Session.GetString("TenNhom") ?? "Nhân viên";
            ViewBag.TenNhom = tenNhom;

            var ban = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .Include(b => b.KhuVuc)
                .FirstOrDefaultAsync(b => b.MaBan == maBan);

            if (ban == null)
                return NotFound(new { message = "Không tìm thấy bàn" });

            var hoaDon = await _context.HoaDon
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .FirstOrDefaultAsync(h => h.MaBan == maBan &&
                                         h.TrangThai == TrangThaiHoaDon.DangChoi);

            ViewBag.HoaDon = hoaDon;

            if (hoaDon != null)
            {
                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .ToListAsync();
                ViewBag.ChiTietDichVu = chiTiet;
            }

            return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_ChiTietBan.cshtml", ban);
        }

        // GET: Lấy danh sách bàn (API)
        [HttpGet]
        public async Task<IActionResult> LayDanhSachBan()
        {
            try
            {
                // Lấy tất cả bàn KHÔNG bao gồm bàn ngưng hoạt động
                var allBan = await _context.BanBia
                    .Include(b => b.LoaiBan)
                    .Include(b => b.KhuVuc)
                    .Include(b => b.KhachHang)
                    .Where(b => b.TrangThai != TrangThaiBan.NgungHoatDong) // THAY ĐỔI Ở ĐÂY
                    .ToListAsync();

                var danhSachBan = allBan
                    .Select(b => new
                    {
                        maBan = b.MaBan,
                        tenBan = b.TenBan,
                        khuVuc = b.KhuVuc?.TenKhuVuc ?? "",
                        loaiBan = b.LoaiBan?.TenLoai ?? "",
                        trangThai = b.TrangThai.ToString(),
                        viTriX = b.ViTriX ?? 0,
                        viTriY = b.ViTriY ?? 0,
                        gioBatDau = b.GioBatDau,
                        khachHang = b.KhachHang?.TenKH
                    })
                    .ToList();

                return Json(danhSachBan);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LayDanhSachBan: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region CRUD Bàn

        // GET: Form thêm bàn
        [HttpGet]
        public async Task<IActionResult> FormThemBan()
        {
            var loaiBan = await _context.LoaiBan.ToListAsync();
            var khuVuc = await _context.KhuVuc.ToListAsync();

            ViewBag.LoaiBan = loaiBan;
            ViewBag.KhuVuc = khuVuc;

            return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_AddBan.cshtml");
        }

        // GET: Form chỉnh sửa bàn
        [HttpGet]
        public async Task<IActionResult> FormChinhSuaBanBia(int maBan)
        {
            var ban = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhuVuc)
                .FirstOrDefaultAsync(b => b.MaBan == maBan);

            if (ban == null)
                return NotFound(new { message = "Không tìm thấy bàn" });

            var loaiBan = await _context.LoaiBan.ToListAsync();
            var khuVuc = await _context.KhuVuc.ToListAsync();

            ViewBag.LoaiBan = loaiBan;
            ViewBag.KhuVuc = khuVuc;

            return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_EditVtriBan.cshtml", ban);
        }

        // POST: Thêm bàn mới
        [HttpPost]
        public async Task<IActionResult> ThemBan([FromForm] ThemBanRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TenBan))
                    return Json(new { success = false, message = "Vui lòng nhập tên bàn" });

                if (request.MaLoai <= 0)
                    return Json(new { success = false, message = "Vui lòng chọn loại bàn" });

                if (request.MaKhuVuc <= 0)
                    return Json(new { success = false, message = "Vui lòng chọn khu vực" });

                var tenBanLower = request.TenBan.ToLower();
                var banTonTai = await _context.BanBia
                    .Where(b => b.TenBan.ToLower() == tenBanLower)
                    .AnyAsync();

                if (banTonTai)
                    return Json(new { success = false, message = "Tên bàn đã tồn tại" });

                string fileName = null;
                if (request.HinhAnh != null && request.HinhAnh.Length > 0)
                {
                    fileName = await SaveImageAsync(request.HinhAnh);
                }

                var banMoi = new BanBia
                {
                    TenBan = request.TenBan,
                    MaLoai = request.MaLoai,
                    MaKhuVuc = request.MaKhuVuc,
                    TrangThai = TrangThaiBan.Trong,
                    HinhAnh = fileName,
                    GhiChu = request.GhiChu
                };

                _context.BanBia.Add(banMoi);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm bàn thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ThemBan: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Cập nhật bàn
        [HttpPost]
        public async Task<IActionResult> CapNhatBan([FromForm] CapNhatBanRequest request)
        {
            try
            {
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (string.IsNullOrWhiteSpace(request.TenBan))
                    return Json(new { success = false, message = "Vui lòng nhập tên bàn" });

                var tenBanLower = request.TenBan.ToLower();
                var banTrung = await _context.BanBia
                    .Where(b => b.TenBan.ToLower() == tenBanLower && b.MaBan != request.MaBan)
                    .AnyAsync();

                if (banTrung)
                    return Json(new { success = false, message = "Tên bàn đã tồn tại" });

                if (request.HinhAnhMoi != null && request.HinhAnhMoi.Length > 0)
                {
                    if (!string.IsNullOrEmpty(ban.HinhAnh))
                    {
                        DeleteImage(ban.HinhAnh);
                    }

                    ban.HinhAnh = await SaveImageAsync(request.HinhAnhMoi);
                }

                ban.TenBan = request.TenBan;
                ban.MaLoai = request.MaLoai;
                ban.MaKhuVuc = request.MaKhuVuc;
                ban.GhiChu = request.GhiChu;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật bàn thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CapNhatBan: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Xóa bàn
        [HttpPost]
        public async Task<IActionResult> XoaBan([FromBody] XoaBanRequest request)
        {
            try
            {
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                // Kiểm tra trạng thái bàn
                if (ban.TrangThai == TrangThaiBan.DangChoi)
                    return Json(new { success = false, message = "Không thể ngưng hoạt động bàn đang được sử dụng" });

                if (ban.TrangThai == TrangThaiBan.DaDat)
                    return Json(new { success = false, message = "Không thể ngưng hoạt động bàn đã được đặt. Vui lòng hủy đặt bàn trước." });

                // Thay vì xóa, chuyển sang trạng thái NgungHoatDong
                ban.TrangThai = TrangThaiBan.NgungHoatDong; // THAY ĐỔI Ở ĐÂY
                ban.GhiChu = $"[NGƯNG HOẠT ĐỘNG] {ban.GhiChu ?? ""}".Trim();

                await _context.SaveChangesAsync();

                // Ghi log lịch sử
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Ngưng hoạt động bàn",
                        ChiTiet = $"Chuyển bàn {ban.TenBan} (ID: {ban.MaBan}) sang trạng thái ngưng hoạt động"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã chuyển bàn sang trạng thái ngưng hoạt động. Bàn sẽ không hiển thị nữa." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in XoaBan: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion
        #region Danh sách bàn đặt

        /// <summary>
        /// Hiển thị trang danh sách bàn đặt
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DanhSachBanDat()
        {
            try
            {
                var tenNhom = HttpContext.Session.GetString("TenNhom") ?? "Nhân viên";
                ViewBag.TenNhom = tenNhom;

                await LoadHeaderStats();

                // Lấy danh sách đặt bàn đang chờ và đã xác nhận
                var now = DateTime.Now;
                var danhSachDatBan = await _context.DatBan
                    .Include(d => d.BanBia)
                        .ThenInclude(b => b.LoaiBan)
                    .Include(d => d.BanBia)
                        .ThenInclude(b => b.KhuVuc)
                    .Include(d => d.KhachHang)
                    .Where(d => (d.TrangThai == TrangThaiDatBan.DangCho ||
                                d.TrangThai == TrangThaiDatBan.DaXacNhan) &&
                               d.ThoiGianDat >= now.AddHours(-2)) // Chỉ hiển thị đặt bàn trong 2h tới
                    .OrderBy(d => d.ThoiGianDat)
                    .ToListAsync();

                // Thống kê
                ViewBag.TongDatBan = danhSachDatBan.Count;
                ViewBag.DangCho = danhSachDatBan.Count(d => d.TrangThai == TrangThaiDatBan.DangCho);
                ViewBag.DaXacNhan = danhSachDatBan.Count(d => d.TrangThai == TrangThaiDatBan.DaXacNhan);

                return View("~/Views/Home/Partials/QLBan_Bi_a/_DanhSachBanDat.cshtml", danhSachDatBan);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DanhSachBanDat: {ex.Message}");
                return StatusCode(500, "Lỗi tải trang");
            }
        }

        /// <summary>
        /// API: Lấy danh sách đặt bàn (cho auto-refresh)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LayDanhSachDatBan()
        {
            try
            {
                var now = DateTime.Now;
                var danhSachDatBan = await _context.DatBan
                    .Include(d => d.BanBia)
                        .ThenInclude(b => b.LoaiBan)
                    .Include(d => d.BanBia)
                        .ThenInclude(b => b.KhuVuc)
                    .Include(d => d.KhachHang)
                    .Where(d => (d.TrangThai == TrangThaiDatBan.DangCho ||
                                d.TrangThai == TrangThaiDatBan.DaXacNhan) &&
                               d.ThoiGianDat >= now.AddHours(-2))
                    .OrderBy(d => d.ThoiGianDat)
                    .Select(d => new
                    {
                        maDat = d.MaDat,
                        maBan = d.MaBan,
                        tenBan = d.BanBia.TenBan,
                        khuVuc = d.BanBia.KhuVuc.TenKhuVuc,
                        loaiBan = d.BanBia.LoaiBan.TenLoai,
                        tenKhach = d.TenKhach,
                        sdt = d.SDT,
                        thoiGianDat = d.ThoiGianDat,
                        soGio = d.SoGio,
                        soNguoi = d.SoNguoi,
                        ghiChu = d.GhiChu,
                        trangThai = d.TrangThai.ToString(),
                        tenKH = d.KhachHang != null ? d.KhachHang.TenKH : null,
                        hangTV = d.KhachHang != null ? d.KhachHang.HangTV.ToString() : null
                    })
                    .ToListAsync();

                return Json(new { success = true, data = danhSachDatBan });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in LayDanhSachDatBan: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
        #region Quản lý trạng thái bàn

        // POST: Bắt đầu chơi
        [HttpPost]
        public async Task<IActionResult> BatDauChoi([FromBody] BatDauChoiRequest request)
        {
            try
            {
                var ban = await _context.BanBia
                    .Include(b => b.LoaiBan)
                    .FirstOrDefaultAsync(b => b.MaBan == request.MaBan);

                if (ban == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bàn" });
                }

                if (ban.TrangThai != TrangThaiBan.Trong && ban.TrangThai != TrangThaiBan.DaDat)
                {
                    return Json(new { success = false, message = "Bàn đang được sử dụng hoặc đã đặt trước" });
                }

                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (!maNV.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                int? maKH = null;
                string tenKhachHang = "Khách lẻ";

                // Nếu có số điện thoại, tìm hoặc tạo khách hàng
                if (!string.IsNullOrWhiteSpace(request.Sdt))
                {
                    var sdt = request.Sdt.Trim();

                    // Validate số điện thoại
                    if (sdt.Length < 10 || sdt.Length > 11 || !System.Text.RegularExpressions.Regex.IsMatch(sdt, @"^[0-9]+$"))
                    {
                        return Json(new { success = false, message = "Số điện thoại không hợp lệ (10-11 số)" });
                    }

                    // Tìm khách hàng hiện có
                    var khachHang = await _context.KhachHang
                        .FirstOrDefaultAsync(kh => kh.SDT == sdt);

                    if (khachHang != null)
                    {
                        // Khách hàng đã tồn tại
                        maKH = khachHang.MaKH;
                        tenKhachHang = khachHang.TenKH;

                        // Cập nhật lần đến cuối
                        khachHang.LanDenCuoi = DateTime.Now;

                        Console.WriteLine($"✅ Khách hàng tồn tại: {tenKhachHang} - SĐT: {sdt}");
                    }
                    else
                    {
                        // Tạo khách hàng mới với thông tin cơ bản
                        var khachHangMoi = new KhachHang
                        {
                            TenKH = $"Khách {sdt.Substring(sdt.Length - 4)}", // Tên tạm: Khách 1234
                            SDT = sdt,
                            HangTV = HangThanhVien.Dong,
                            DiemTichLuy = 0,
                            TongChiTieu = 0,
                            NgayDangKy = DateTime.Now,
                            LanDenCuoi = DateTime.Now
                        };

                        _context.KhachHang.Add(khachHangMoi);
                        await _context.SaveChangesAsync();

                        maKH = khachHangMoi.MaKH;
                        tenKhachHang = khachHangMoi.TenKH;

                        Console.WriteLine($"✅ Tạo khách hàng mới: {tenKhachHang} - SĐT: {sdt}");
                    }
                }
                else
                {
                    Console.WriteLine($"ℹ️ Khách vãng lai - không có SĐT");
                }

                // Cập nhật trạng thái bàn
                ban.TrangThai = TrangThaiBan.DangChoi;
                ban.GioBatDau = DateTime.Now;
                ban.MaKH = maKH;

                // Tạo hóa đơn mới
                var hoaDon = new HoaDon
                {
                    MaBan = ban.MaBan,
                    MaKH = maKH,
                    MaNV = maNV.Value,
                    ThoiGianBatDau = DateTime.Now,
                    TrangThai = TrangThaiHoaDon.DangChoi,
                    TienBan = 0,
                    TienDichVu = 0,
                    GiamGia = 0,
                    TongTien = 0
                };

                _context.HoaDon.Add(hoaDon);
                await _context.SaveChangesAsync();

                // Log hoạt động
                var lichSu = new LichSuHoatDong
                {
                    MaNV = maNV.Value,
                    ThoiGian = DateTime.Now,
                    HanhDong = "Bắt đầu chơi",
                    ChiTiet = $"Bàn {ban.TenBan}" +
                        (maKH.HasValue ? $" - Khách hàng: {tenKhachHang} (SĐT: {request.Sdt})" : " - Khách vãng lai")
                };
                _context.LichSuHoatDong.Add(lichSu);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Bắt đầu chơi thành công - Bàn: {ban.TenBan}, Khách: {tenKhachHang}");

                return Json(new
                {
                    success = true,
                    message = $"Bắt đầu chơi thành công!" +
                        (maKH.HasValue ? $"\nKhách hàng: {tenKhachHang}" : "\nKhách vãng lai"),
                    hoaDonId = hoaDon.MaHD,
                    khachHang = maKH.HasValue ? new { maKH, tenKH = tenKhachHang } : null
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in BatDauChoi: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // POST: Kết thúc chơi
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

                if (hoaDon.ThoiGianBatDau.HasValue)
                {
                    var duration = (hoaDon.ThoiGianKetThuc.Value - hoaDon.ThoiGianBatDau.Value).TotalMinutes;

                    // Làm tròn 15 phút
                    var soPhutLamTron = Math.Ceiling(duration / 15) * 15;
                    var soGio = soPhutLamTron / 60;

                    if (hoaDon.BanBia?.LoaiBan != null)
                        hoaDon.TienBan = hoaDon.BanBia.LoaiBan.GiaGio * (decimal)soGio;
                    else
                        hoaDon.TienBan = 0;
                }

                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .ToListAsync();

                hoaDon.TienDichVu = chiTiet.Sum(ct => ct.ThanhTien ?? 0);

                // Làm tròn tổng tiền lên nghìn
                var tongTienTruocLamTron = hoaDon.TienBan + hoaDon.TienDichVu - hoaDon.GiamGia;
                hoaDon.TongTien = Math.Ceiling(tongTienTruocLamTron / 1000) * 1000;

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
                Console.WriteLine($"Error in KetThucChoi: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion
        #region API kiểm tra khách hàng

        /// <summary>
        /// API: Kiểm tra khách hàng theo số điện thoại
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> KiemTraKhachHang(string sdt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sdt))
                {
                    return Json(new { success = false, message = "Số điện thoại không hợp lệ" });
                }

                // Tìm khách hàng theo số điện thoại
                var khachHang = await _context.KhachHang
                    .FirstOrDefaultAsync(kh => kh.SDT == sdt.Trim());

                if (khachHang != null)
                {
                    return Json(new
                    {
                        success = true,
                        khachHang = new
                        {
                            maKH = khachHang.MaKH,
                            tenKH = khachHang.TenKH,
                            sdt = khachHang.SDT,
                            hangTV = khachHang.HangTV.ToString(),
                            diemTichLuy = khachHang.DiemTichLuy
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Khách hàng chưa đăng ký" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in KiemTraKhachHang: {ex.Message}");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        #endregion
        #region Đặt bàn

        // GET: Panel đặt bàn
        [HttpGet]
        public async Task<IActionResult> PanelDatBan()
        {
            var allBan = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhuVuc)
                .ToListAsync();

            // Filter ở client-side
            var danhSachBan = allBan
                .Where(b => b.TrangThai == TrangThaiBan.Trong)
                .OrderBy(b => b.TenBan)
                .ToList();

            return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_PanelDatBan.cshtml", danhSachBan);
        }
        [HttpGet]
        public async Task<IActionResult> LayBanTrongTheoGio(string ngayDat, string gioBatDau, string gioKetThuc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ngayDat) || string.IsNullOrWhiteSpace(gioBatDau) || string.IsNullOrWhiteSpace(gioKetThuc))
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ" });
                }

                // Parse thời gian
                DateTime startTime = DateTime.Parse($"{ngayDat}T{gioBatDau}:00");
                DateTime endTime = DateTime.Parse($"{ngayDat}T{gioKetThuc}:00");

                // Xử lý trường hợp qua đêm
                var startHour = TimeSpan.Parse(gioBatDau).Hours;
                var endHour = TimeSpan.Parse(gioKetThuc).Hours;

                if (endHour < startHour)
                {
                    endTime = endTime.AddDays(1);
                    Console.WriteLine($"⚠️ Đặt bàn qua đêm: {startTime:yyyy-MM-dd HH:mm} -> {endTime:yyyy-MM-dd HH:mm}");
                }

                Console.WriteLine($"🔍 Checking availability: {startTime:yyyy-MM-dd HH:mm} -> {endTime:yyyy-MM-dd HH:mm}");

                // ✅ FIX: Lấy TẤT CẢ bàn (trừ bàn ngưng hoạt động)
                var allBan = await _context.BanBia
                    .Include(b => b.LoaiBan)
                    .Include(b => b.KhuVuc)
                    .Where(b => b.TrangThai != TrangThaiBan.NgungHoatDong) // Chỉ loại bỏ bàn ngưng hoạt động
                    .ToListAsync();

                // Lấy các đặt bàn trong khoảng thời gian cần kiểm tra
                var datePart = startTime.Date;
                var searchStartDate = datePart.AddDays(-1); // Lùi 1 ngày để bắt trường hợp qua đêm
                var searchEndDate = endTime.Date.AddDays(1); // Thêm 1 ngày để bắt trường hợp qua đêm

                var datBanCanKiemTra = await _context.DatBan
                    .Where(d => (d.TrangThai == TrangThaiDatBan.DangCho || d.TrangThai == TrangThaiDatBan.DaXacNhan) &&
                               d.ThoiGianDat.Date >= searchStartDate &&
                               d.ThoiGianDat.Date <= searchEndDate)
                    .ToListAsync();

                // ✅ FIX: Lấy các bàn ĐANG CHƠI (để loại bỏ khỏi danh sách)
                var banDangChoi = await _context.HoaDon
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DangChoi)
                    .Select(h => h.MaBan)
                    .ToListAsync();

                Console.WriteLine($"📊 Tổng số bàn: {allBan.Count}, Đặt bàn cần kiểm tra: {datBanCanKiemTra.Count}, Bàn đang chơi: {banDangChoi.Count}");

                var banTrong = new List<object>();

                foreach (var ban in allBan)
                {
                    // ✅ FIX: Bỏ qua bàn đang chơi (có hóa đơn đang mở)
                    if (banDangChoi.Contains(ban.MaBan))
                    {
                        Console.WriteLine($"⏭️ Bỏ qua bàn {ban.TenBan} - Đang chơi (có hóa đơn)");
                        continue;
                    }

                    bool isTrong = true;

                    // Kiểm tra các đặt bàn của bàn này
                    var datBanCuaBan = datBanCanKiemTra.Where(d => d.MaBan == ban.MaBan).ToList();

                    if (datBanCuaBan.Any())
                    {
                        Console.WriteLine($"🔍 Kiểm tra bàn {ban.TenBan} có {datBanCuaBan.Count} lượt đặt:");
                    }

                    foreach (var dat in datBanCuaBan)
                    {
                        DateTime datStart = dat.ThoiGianDat;
                        DateTime datEnd = dat.ThoiGianDat.AddHours(dat.SoGio);

                        // ✅ CÔNG THỨC KIỂM TRA TRÙNG CHÍNH XÁC:
                        // Hai khoảng thời gian [A1, A2] và [B1, B2] TRÙNG khi:
                        // A1 < B2 AND B1 < A2
                        bool isTrung = startTime < datEnd && datStart < endTime;

                        Console.WriteLine($"   📅 Đặt bàn: {datStart:dd/MM HH:mm} → {datEnd:dd/MM HH:mm} ({dat.SoGio}h)");
                        Console.WriteLine($"   🔍 Kiểm tra: {startTime:dd/MM HH:mm} → {endTime:dd/MM HH:mm}");
                        Console.WriteLine($"   ➡️ Logic: ({startTime:HH:mm} < {datEnd:HH:mm}) = {startTime < datEnd}");
                        Console.WriteLine($"             ({datStart:HH:mm} < {endTime:HH:mm}) = {datStart < endTime}");
                        Console.WriteLine($"   ✅ Kết quả: {(isTrung ? "❌ TRÙNG" : "✅ KHÔNG TRÙNG")}");

                        if (isTrung)
                        {
                            isTrong = false;
                            break;
                        }
                    }

                    if (isTrong)
                    {
                        Console.WriteLine($"✅ Bàn {ban.TenBan} TRỐNG");
                        banTrong.Add(new
                        {
                            maBan = ban.MaBan,
                            tenBan = ban.TenBan,
                            khuVuc = ban.KhuVuc?.TenKhuVuc ?? "",
                            loaiBan = ban.LoaiBan?.TenLoai ?? "",
                            giaGio = ban.LoaiBan?.GiaGio ?? 0,
                            trangThai = ban.TrangThai.ToString()
                        });
                    }
                    else
                    {
                        Console.WriteLine($"❌ Bàn {ban.TenBan} ĐÃ ĐƯỢC ĐẶT");
                    }
                }

                Console.WriteLine($"✅ Tìm thấy {banTrong.Count} bàn trống trong khung giờ {gioBatDau} - {gioKetThuc}");

                return Json(new
                {
                    success = true,
                    data = banTrong,
                    soLuong = banTrong.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in LayBanTrongTheoGio: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> TaoDatBan([FromBody] TaoDatBanRequest request)
        {
            try
            {
                var ban = await _context.BanBia
                    .Include(b => b.LoaiBan)
                    .FirstOrDefaultAsync(b => b.MaBan == request.MaBan);

                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                // ✅ FIX: Kiểm tra nếu bàn ĐANG CHƠI (có hóa đơn đang mở) thì không cho đặt
                var banDangChoi = await _context.HoaDon
                    .AnyAsync(h => h.MaBan == request.MaBan && h.TrangThai == TrangThaiHoaDon.DangChoi);

                if (banDangChoi)
                {
                    return Json(new { success = false, message = "Bàn đang được sử dụng, không thể đặt" });
                }

                DateTime startTime = DateTime.Parse(request.ThoiGianDat);
                DateTime endTime = DateTime.Parse(request.GioKetThuc);

                Console.WriteLine($"🔍 Đặt bàn: {startTime:yyyy-MM-dd HH:mm} -> {endTime:yyyy-MM-dd HH:mm}");

                double totalHours = (endTime - startTime).TotalHours;
                int soGio = (int)Math.Ceiling(totalHours);

                Console.WriteLine($"📋 Thời gian: {totalHours:F2}h (Làm tròn: {soGio}h)");

                // ✅ FIX: Kiểm tra trùng lịch CHÍNH XÁC
                var datePart = startTime.Date;
                var searchStartDate = datePart.AddDays(-1);
                var searchEndDate = endTime.Date.AddDays(1);

                var datBanTrung = await _context.DatBan
                    .Where(d => d.MaBan == request.MaBan &&
                               (d.TrangThai == TrangThaiDatBan.DangCho || d.TrangThai == TrangThaiDatBan.DaXacNhan) &&
                               d.ThoiGianDat.Date >= searchStartDate &&
                               d.ThoiGianDat.Date <= searchEndDate)
                    .ToListAsync();

                foreach (var dat in datBanTrung)
                {
                    DateTime datStart = dat.ThoiGianDat;
                    DateTime datEnd = dat.ThoiGianDat.AddHours(dat.SoGio);

                    // ✅ CÔNG THỨC KIỂM TRA TRÙNG
                    bool isTrung = startTime < datEnd && datStart < endTime;

                    if (isTrung)
                    {
                        Console.WriteLine($"❌ Trùng lịch: Đặt bàn hiện tại {datStart:HH:mm dd/MM} - {datEnd:HH:mm dd/MM}");
                        Console.WriteLine($"   Đặt bàn mới: {startTime:HH:mm dd/MM} - {endTime:HH:mm dd/MM}");

                        return Json(new
                        {
                            success = false,
                            message = $"Bàn đã được đặt trong khung giờ {datStart:HH:mm dd/MM} - {datEnd:HH:mm dd/MM}"
                        });
                    }
                }

                // Tìm hoặc tạo khách hàng
                var khachHang = await _context.KhachHang
                    .FirstOrDefaultAsync(k => k.SDT == request.Sdt);

                // ✅ FIX: KHÔNG thay đổi TrangThai của bàn
                // Bàn vẫn giữ nguyên trạng thái Trong hoặc DangChoi
                // Chỉ cập nhật MaKH để tracking
                if (khachHang != null)
                {
                    // Không gán ban.MaKH nữa, để tránh conflict khi có nhiều đặt bàn
                }

                // ✅ FIX: KHÔNG cập nhật GhiChu của bàn
                // Vì một bàn có thể có nhiều đặt bàn, không nên ghi chú vào bàn

                // Tạo đặt bàn
                var datBan = new DatBan
                {
                    MaBan = request.MaBan,
                    MaKH = khachHang?.MaKH,
                    TenKhach = request.TenKhach,
                    SDT = request.Sdt,
                    ThoiGianDat = startTime,
                    SoGio = soGio,
                    SoNguoi = request.SoNguoi,
                    GhiChu = $"Email: {request.Email}. {request.GhiChu}",
                    TrangThai = TrangThaiDatBan.DangCho, // ✅ FIX: Đặt bàn ở trạng thái Đang chờ
                    NgayTao = DateTime.Now
                };

                _context.DatBan.Add(datBan);
                await _context.SaveChangesAsync();

                // Ghi log
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    string ghiChuThoiGian;
                    if (endTime.Date > startTime.Date)
                    {
                        ghiChuThoiGian = $"từ {startTime:HH:mm dd/MM} đến {endTime:HH:mm dd/MM} ({soGio}h)";
                    }
                    else
                    {
                        ghiChuThoiGian = $"từ {startTime:HH:mm} đến {endTime:HH:mm} ({soGio}h)";
                    }

                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Đặt bàn",
                        ChiTiet = $"Đặt bàn {ban.TenBan} {ghiChuThoiGian} - KH: {request.TenKhach}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                Console.WriteLine($"✅ Đặt bàn thành công: {ban.TenBan} - {startTime:HH:mm} đến {endTime:HH:mm}");

                return Json(new
                {
                    success = true,
                    message = "Đặt bàn thành công!",
                    thongTin = new
                    {
                        tenBan = ban.TenBan,
                        ngayDat = startTime.ToString("dd/MM/yyyy"),
                        gioBatDau = startTime.ToString("HH:mm"),
                        gioKetThuc = endTime.ToString("HH:mm"),
                        ngayKetThuc = endTime.Date > startTime.Date ? endTime.ToString("dd/MM/yyyy") : null,
                        soGio = soGio,
                        tenKhach = request.TenKhach,
                        sdt = request.Sdt
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in TaoDatBan: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanDatBan([FromBody] XacNhanDatBanRequest request)
        {
            try
            {
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                // ✅ FIX: Kiểm tra bàn đang chơi
                var banDangChoi = await _context.HoaDon
                    .AnyAsync(h => h.MaBan == request.MaBan && h.TrangThai == TrangThaiHoaDon.DangChoi);

                if (banDangChoi)
                {
                    return Json(new { success = false, message = "Bàn đang được sử dụng" });
                }

                // ✅ FIX: Lấy TẤT CẢ đặt bàn phù hợp về client rồi xử lý
                var now = DateTime.Now;
                var allDatBan = await _context.DatBan
                    .Where(d => d.MaBan == request.MaBan &&
                               d.TrangThai == TrangThaiDatBan.DangCho &&
                               d.ThoiGianDat <= now.AddMinutes(30) &&
                               d.ThoiGianDat >= now.AddMinutes(-60))
                    .ToListAsync(); // ← Lấy về client trước

                if (!allDatBan.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy đặt bàn phù hợp hoặc đã quá giờ" });
                }

                // ✅ Xử lý trên client-side (không dùng Math.Abs trong LINQ)
                var datBan = allDatBan
                    .OrderBy(d => Math.Abs((d.ThoiGianDat - now).TotalMinutes))
                    .FirstOrDefault();

                if (datBan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đặt bàn phù hợp" });
                }

                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (!maNV.HasValue)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                // Cập nhật trạng thái bàn
                ban.TrangThai = TrangThaiBan.DangChoi;
                ban.GioBatDau = DateTime.Now;
                ban.MaKH = datBan.MaKH;

                // Tạo hóa đơn
                var hoaDon = new HoaDon
                {
                    MaBan = request.MaBan,
                    MaKH = datBan.MaKH,
                    MaNV = maNV.Value,
                    ThoiGianBatDau = DateTime.Now,
                    TrangThai = TrangThaiHoaDon.DangChoi
                };

                _context.HoaDon.Add(hoaDon);

                // Cập nhật trạng thái đặt bàn
                datBan.TrangThai = TrangThaiDatBan.DaXacNhan;

                await _context.SaveChangesAsync();

                // Ghi log
                var lichSu = new LichSuHoatDong
                {
                    MaNV = maNV.Value,
                    ThoiGian = DateTime.Now,
                    HanhDong = "Xác nhận đặt bàn",
                    ChiTiet = $"Bàn {ban.TenBan} - KH: {datBan.TenKhach} (Đặt lúc {datBan.ThoiGianDat:HH:mm dd/MM})"
                };
                _context.LichSuHoatDong.Add(lichSu);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xác nhận đặt bàn thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in XacNhanDatBan: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        // <summary>
        /// Hủy đặt bàn
        /// ✅ FIX: Chỉ hủy đặt bàn trong bảng DatBan, không thay đổi TrangThai của bàn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> HuyDatBan([FromBody] HuyDatBanRequest request)
        {
            try
            {
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                // ✅ FIX: Tìm đặt bàn cần hủy (có thể truyền thêm MaDat trong request)
                // Tạm thời lấy đặt bàn gần nhất chưa bị hủy
                var datBan = await _context.DatBan
                    .Where(d => d.MaBan == request.MaBan &&
                               (d.TrangThai == TrangThaiDatBan.DangCho || d.TrangThai == TrangThaiDatBan.DaXacNhan))
                    .OrderBy(d => d.ThoiGianDat)
                    .FirstOrDefaultAsync();

                if (datBan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đặt bàn cần hủy" });
                }

                // ✅ FIX: Chỉ cập nhật trạng thái đặt bàn
                datBan.TrangThai = TrangThaiDatBan.DaHuy;

                // ✅ FIX: KHÔNG thay đổi TrangThai của bàn
                // Vì bàn có thể còn đặt bàn khác

                await _context.SaveChangesAsync();

                // Ghi log
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Hủy đặt bàn",
                        ChiTiet = $"Bàn {ban.TenBan} - KH: {datBan.TenKhach} (Đặt lúc {datBan.ThoiGianDat:HH:mm dd/MM})"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Hủy đặt bàn thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in HuyDatBan: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Dịch vụ

        // GET: Lấy danh sách dịch vụ
        [HttpGet]
        public async Task<IActionResult> LayDanhSachDichVu()
        {
            try
            {
                if (HttpContext.Session.GetInt32("MaNV") == null)
                {
                    return StatusCode(401, "Vui lòng đăng nhập lại");
                }

                var tenNhom = HttpContext.Session.GetString("TenNhom") ?? "Nhân viên";
                ViewBag.TenNhom = tenNhom;

                var allDichVu = await _context.DichVus.ToListAsync();

                // Filter ở client-side
                var danhSachDV = allDichVu
                    .Where(dv => dv.TrangThai == TrangThaiDichVu.ConHang)
                    .OrderBy(dv => dv.Loai)
                    .ThenBy(dv => dv.TenDV)
                    .ToList();

                return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_MenuDichVu.cshtml", danhSachDV);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LayDanhSachDichVu: {ex.Message}");
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        // POST: Thêm dịch vụ
        [HttpPost]
        public async Task<IActionResult> ThemDichVu([FromBody] ThemDichVuRequest request)
        {
            try
            {
                var hoaDon = await _context.HoaDon
                    .FirstOrDefaultAsync(h => h.MaBan == request.MaBan &&
                                             h.TrangThai == TrangThaiHoaDon.DangChoi);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn đang chơi" });

                var dichVu = await _context.DichVus.FindAsync(request.MaDV);
                if (dichVu == null)
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ" });

                var chiTietCu = await _context.ChiTietHoaDon
                    .FirstOrDefaultAsync(ct => ct.MaHD == hoaDon.MaHD && ct.MaDV == request.MaDV);

                if (chiTietCu != null)
                {
                    chiTietCu.SoLuong += request.SoLuong;
                    chiTietCu.ThanhTien = chiTietCu.SoLuong * dichVu.Gia;
                }
                else
                {
                    var chiTiet = new ChiTietHoaDon
                    {
                        MaHD = hoaDon.MaHD,
                        MaDV = request.MaDV,
                        SoLuong = request.SoLuong,
                        ThanhTien = dichVu.Gia * request.SoLuong
                    };
                    _context.ChiTietHoaDon.Add(chiTiet);
                }

                await _context.SaveChangesAsync();

                var tongDichVu = await _context.ChiTietHoaDon
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .SumAsync(ct => ct.ThanhTien ?? 0);

                hoaDon.TienDichVu = tongDichVu;
                hoaDon.TongTien = hoaDon.TienBan + hoaDon.TienDichVu - hoaDon.GiamGia;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ThemDichVu: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Xóa dịch vụ
        [HttpPost]
        public async Task<IActionResult> XoaDichVu([FromBody] XoaDichVuRequest request)
        {
            try
            {
                var chiTiet = await _context.ChiTietHoaDon.FindAsync(request.Id);
                if (chiTiet == null)
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ" });

                _context.ChiTietHoaDon.Remove(chiTiet);
                await _context.SaveChangesAsync();

                // Cập nhật lại tổng tiền hóa đơn
                var hoaDon = await _context.HoaDon.FindAsync(chiTiet.MaHD);
                if (hoaDon != null)
                {
                    var tongDichVu = await _context.ChiTietHoaDon
                        .Where(ct => ct.MaHD == hoaDon.MaHD)
                        .SumAsync(ct => ct.ThanhTien ?? 0);

                    hoaDon.TienDichVu = tongDichVu;
                    hoaDon.TongTien = hoaDon.TienBan + hoaDon.TienDichVu - hoaDon.GiamGia;
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Xóa dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in XoaDichVu: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Cập nhật số lượng dịch vụ
        [HttpPost]
        public async Task<IActionResult> CapNhatSoLuongDichVu([FromBody] CapNhatSoLuongRequest request)
        {
            try
            {
                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .FirstOrDefaultAsync(ct => ct.ID == request.Id);

                if (chiTiet == null)
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ" });

                chiTiet.SoLuong = request.SoLuong;
                chiTiet.ThanhTien = chiTiet.SoLuong * (chiTiet.DichVu?.Gia ?? 0);

                await _context.SaveChangesAsync();

                // Cập nhật lại tổng tiền hóa đơn
                var hoaDon = await _context.HoaDon.FindAsync(chiTiet.MaHD);
                if (hoaDon != null)
                {
                    var tongDichVu = await _context.ChiTietHoaDon
                        .Where(ct => ct.MaHD == hoaDon.MaHD)
                        .SumAsync(ct => ct.ThanhTien ?? 0);

                    hoaDon.TienDichVu = tongDichVu;
                    hoaDon.TongTien = hoaDon.TienBan + hoaDon.TienDichVu - hoaDon.GiamGia;
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = "Cập nhật số lượng thành công",
                    thanhTien = chiTiet.ThanhTien
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CapNhatSoLuongDichVu: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Chỉnh sửa bàn

        // GET: Panel chỉnh sửa bàn (dịch vụ)
        [HttpGet]
        public async Task<IActionResult> PanelChinhSuaBan(int maBan)
        {
            var ban = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .FirstOrDefaultAsync(b => b.MaBan == maBan);

            if (ban == null)
                return NotFound();

            var hoaDon = await _context.HoaDon
                .FirstOrDefaultAsync(h => h.MaBan == maBan &&
                                         h.TrangThai == TrangThaiHoaDon.DangChoi);

            ViewBag.HoaDon = hoaDon;

            if (hoaDon != null)
            {
                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .ToListAsync();
                ViewBag.ChiTietDichVu = chiTiet;
            }

            return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_PanelChinhSuaBan.cshtml", ban);
        }

        // POST: Lưu chỉnh sửa bàn
        [HttpPost]
        public async Task<IActionResult> LuuChinhSuaBan([FromBody] LuuChinhSuaBanRequest request)
        {
            try
            {
                var hoaDon = await _context.HoaDon
                    .FirstOrDefaultAsync(h => h.MaBan == request.MaBan &&
                                             h.TrangThai == TrangThaiHoaDon.DangChoi);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

                hoaDon.ThoiGianBatDau = DateTime.Parse(request.GioBatDau);

                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban != null)
                {
                    ban.GioBatDau = DateTime.Parse(request.GioBatDau);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LuuChinhSuaBan: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Thanh toán

        // GET: Panel thanh toán
        [HttpGet]
        public async Task<IActionResult> PanelThanhToan(int maHD)
        {
            try
            {
                var hoaDon = await _context.HoaDon
                    .Include(h => h.BanBia)
                        .ThenInclude(b => b.LoaiBan)
                    .Include(h => h.BanBia)
                        .ThenInclude(b => b.KhuVuc)
                    .Include(h => h.KhachHang)
                    .Include(h => h.NhanVien)
                    .FirstOrDefaultAsync(h => h.MaHD == maHD);

                if (hoaDon == null)
                {
                    return NotFound(new { message = "Không tìm thấy hóa đơn" });
                }

                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == maHD)
                    .ToListAsync();

                ViewBag.ChiTietDichVu = chiTiet;
                ViewBag.TenNhom = HttpContext.Session.GetString("TenNhom") ?? "Nhân viên";

                return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_PanelThanhToan.cshtml", hoaDon);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in PanelThanhToan: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Lỗi khi tải panel thanh toán",
                    error = ex.Message
                });
            }
        }

        // POST: Xác nhận thanh toán
        [HttpPost]
        public async Task<IActionResult> XacNhanThanhToan([FromBody] XacNhanThanhToanRequest request)
        {
            try
            {
                // Kiểm tra request null
                if (request == null)
                {
                    Console.WriteLine("❌ Request is null!");
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                Console.WriteLine($"✅ Received payment: MaHD={request.MaHD}, Method={request.PhuongThucThanhToan}, Amount={request.TienKhachDua}");

                var hoaDon = await _context.HoaDon
                    .Include(h => h.BanBia)
                    .FirstOrDefaultAsync(h => h.MaHD == request.MaHD);

                if (hoaDon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Map string sang enum
                PhuongThucThanhToan phuongThuc;
                switch (request.PhuongThucThanhToan?.ToLower())
                {
                    case "tienmat":
                        phuongThuc = PhuongThucThanhToan.TienMat;
                        break;
                    case "chuyenkhoan":
                        phuongThuc = PhuongThucThanhToan.ChuyenKhoan;
                        break;
                    case "qrcode":
                        phuongThuc = PhuongThucThanhToan.QRTuDong;
                        break;
                    default:
                        phuongThuc = PhuongThucThanhToan.TienMat;
                        break;
                }

                // Kiểm tra tiền khách đưa nếu là tiền mặt
                if (phuongThuc == PhuongThucThanhToan.TienMat)
                {
                    if (request.TienKhachDua < hoaDon.TongTien)
                    {
                        return Json(new { success = false, message = "Số tiền khách đưa không đủ" });
                    }
                }

                hoaDon.TrangThai = TrangThaiHoaDon.DaThanhToan;
                hoaDon.PhuongThucThanhToan = phuongThuc;
                hoaDon.MaGiaoDichQR = request.MaGiaoDichQR;

                // Cập nhật trạng thái bàn
                if (hoaDon.BanBia != null)
                {
                    hoaDon.BanBia.TrangThai = TrangThaiBan.Trong;
                    hoaDon.BanBia.GioBatDau = null;
                    hoaDon.BanBia.MaKH = null;
                }

                // Ghi sổ quỹ
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var soQuy = new SoQuy
                    {
                        MaNV = maNV.Value,
                        NgayLap = DateTime.Now,
                        LoaiPhieu = LoaiPhieu.Thu,
                        SoTien = hoaDon.TongTien,
                        LyDo = $"Thanh toán hóa đơn #{hoaDon.MaHD} - Bàn {hoaDon.BanBia?.TenBan} - {phuongThuc}",
                        MaHDLienQuan = hoaDon.MaHD
                    };
                    _context.SoQuy.Add(soQuy);
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Payment completed for invoice {hoaDon.MaHD}");
                return Json(new { success = true, message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in XacNhanThanhToan: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Sơ đồ bàn

        // POST: Cập nhật vị trí bàn
        [HttpPost]
        public async Task<IActionResult> CapNhatViTriBan([FromBody] List<CapNhatViTriRequest> requests)
        {
            try
            {
                // Kiểm tra quyền
                var tenNhom = HttpContext.Session.GetString("TenNhom");
                if (tenNhom != "Admin" && tenNhom != "Quản lý")
                {
                    return Json(new { success = false, message = "Không có quyền thực hiện" });
                }

                foreach (var request in requests)
                {
                    var ban = await _context.BanBia.FindAsync(request.MaBan);
                    if (ban != null)
                    {
                        ban.ViTriX = request.ViTriX;
                        ban.ViTriY = request.ViTriY;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã cập nhật vị trí bàn" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CapNhatViTriBan: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion
    }

    #region Request Models

    public class ThemBanRequest
    {
        public string TenBan { get; set; } = "";
        public int MaLoai { get; set; }
        public int MaKhuVuc { get; set; }
        public IFormFile? HinhAnh { get; set; }
        public string? GhiChu { get; set; }
    }

    public class CapNhatBanRequest
    {
        public int MaBan { get; set; }
        public string TenBan { get; set; } = "";
        public int MaLoai { get; set; }
        public int MaKhuVuc { get; set; }
        public IFormFile? HinhAnhMoi { get; set; }
        public string? GhiChu { get; set; }
    }

    public class XoaBanRequest
    {
        public int MaBan { get; set; }
    }

    public class BatDauChoiRequest
    {
        public int MaBan { get; set; }
        public string? Sdt { get; set; }
    }

    public class KetThucChoiRequest
    {
        public int MaBan { get; set; }
    }

    public class XacNhanDatBanRequest
    {
        public int MaBan { get; set; }
    }

    public class ThemDichVuRequest
    {
        public int MaBan { get; set; }
        public int MaDV { get; set; }
        public int SoLuong { get; set; }
    }

    public class XoaDichVuRequest
    {
        public int Id { get; set; }
    }

    public class CapNhatSoLuongRequest
    {
        public int Id { get; set; }
        public int SoLuong { get; set; }
    }

    public class XacNhanThanhToanRequest
    {
        public int MaHD { get; set; }
        public string PhuongThucThanhToan { get; set; } = "TienMat";
        public decimal TienKhachDua { get; set; }
        public string? MaGiaoDichQR { get; set; }
    }

    public class TaoDatBanRequest
    {
        public int MaBan { get; set; }
        public string ThoiGianDat { get; set; } = "";
        public string GioKetThuc { get; set; } = "";
        public string TenKhach { get; set; } = "";
        public string Sdt { get; set; } = "";
        public string? Email { get; set; }
        public int SoNguoi { get; set; }
        public string? GhiChu { get; set; }
    }

    public class LuuChinhSuaBanRequest
    {
        public int MaBan { get; set; }
        public string GioBatDau { get; set; } = "";
    }

    public class HuyDatBanRequest
    {
        public int MaBan { get; set; }
    }

    public class CapNhatViTriRequest
    {
        public int MaBan { get; set; }
        public decimal ViTriX { get; set; }
        public decimal ViTriY { get; set; }
    }

    #endregion
}