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

            var danhSachBan = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .Include(b => b.KhuVuc)
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
                var danhSachBan = await _context.BanBia
                    .Include(b => b.LoaiBan)
                    .Include(b => b.KhuVuc)
                    .Include(b => b.KhachHang)
                    .Where(b => b.TrangThai != TrangThaiBan.BaoTri)
                    .Select(b => new
                    {
                        maBan = b.MaBan,
                        tenBan = b.TenBan,
                        khuVuc = b.KhuVuc.TenKhuVuc,
                        loaiBan = b.LoaiBan.TenLoai,
                        trangThai = b.TrangThai.ToString(),
                        viTriX = b.ViTriX ?? 0,
                        viTriY = b.ViTriY ?? 0,
                        gioBatDau = b.GioBatDau,
                        khachHang = b.KhachHang != null ? b.KhachHang.TenKH : null
                    })
                    .ToListAsync();

                return Json(danhSachBan);
            }
            catch (Exception ex)
            {
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

            return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_FormThemBan.cshtml");
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

            return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_FormChinhSuaBan.cshtml", ban);
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

                var banTonTai = await _context.BanBia
                    .AnyAsync(b => b.TenBan.ToLower() == request.TenBan.ToLower());

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

                var banTrung = await _context.BanBia
                    .AnyAsync(b => b.TenBan.ToLower() == request.TenBan.ToLower()
                                && b.MaBan != request.MaBan);

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

                if (ban.TrangThai != TrangThaiBan.Trong)
                    return Json(new { success = false, message = "Không thể xóa bàn đang sử dụng hoặc đã đặt" });

                var coHoaDon = await _context.HoaDon
                    .AnyAsync(h => h.MaBan == request.MaBan);

                if (coHoaDon)
                    return Json(new { success = false, message = "Không thể xóa bàn đã có lịch sử hóa đơn" });

                if (!string.IsNullOrEmpty(ban.HinhAnh))
                {
                    DeleteImage(ban.HinhAnh);
                }

                _context.BanBia.Remove(ban);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa bàn thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.TrangThai != TrangThaiBan.Trong && ban.TrangThai != TrangThaiBan.DaDat)
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
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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
                var duration = (hoaDon.ThoiGianKetThuc.Value - hoaDon.ThoiGianBatDau.Value).TotalMinutes;
                hoaDon.ThoiLuongPhut = (int)Math.Ceiling(duration);

                // Làm tròn 15 phút
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
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Đặt bàn
        // GET: Panel đặt bàn
        [HttpGet]
        public async Task<IActionResult> PanelDatBan()
        {
            var danhSachBan = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhuVuc)
                .Where(b => b.TrangThai == TrangThaiBan.Trong)
                .OrderBy(b => b.TenBan)
                .ToListAsync();

            return PartialView("~/Views/Home/Partials/QLBan_Bi_a/_PanelDatBan.cshtml", danhSachBan);
        }

        // POST: Tạo đặt bàn
        [HttpPost]
        public async Task<IActionResult> TaoDatBan([FromBody] TaoDatBanRequest request)
        {
            try
            {
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.TrangThai != TrangThaiBan.Trong)
                    return Json(new { success = false, message = "Bàn đã được đặt hoặc đang sử dụng" });

                var khachHang = await _context.KhachHang
                    .FirstOrDefaultAsync(k => k.SDT == request.Sdt);

                if (khachHang != null)
                {
                    ban.MaKH = khachHang.MaKH;
                }

                ban.TrangThai = TrangThaiBan.DaDat;
                ban.GhiChu = $"Đặt từ {DateTime.Parse(request.ThoiGianDat):HH:mm} đến {DateTime.Parse(request.GioKetThuc):HH:mm}";

                var datBan = new DatBan
                {
                    MaBan = request.MaBan,
                    MaKH = khachHang?.MaKH,
                    TenKhach = request.TenKhach,
                    SDT = request.Sdt,
                    ThoiGianDat = DateTime.Parse(request.ThoiGianDat),
                    SoNguoi = request.SoNguoi,
                    GhiChu = $"Email: {request.Email}. {request.GhiChu}",
                    TrangThai = TrangThaiDatBan.DangCho,
                    NgayTao = DateTime.Now
                };

                _context.DatBan.Add(datBan);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đặt bàn thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Xác nhận đặt bàn
        [HttpPost]
        public async Task<IActionResult> XacNhanDatBan([FromBody] XacNhanDatBanRequest request)
        {
            try
            {
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.TrangThai != TrangThaiBan.DaDat)
                    return Json(new { success = false, message = "Bàn không ở trạng thái đã đặt" });

                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (!maNV.HasValue)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                ban.TrangThai = TrangThaiBan.DangChoi;
                ban.GioBatDau = DateTime.Now;

                var hoaDon = new HoaDon
                {
                    MaBan = request.MaBan,
                    MaKH = ban.MaKH,
                    MaNV = maNV.Value,
                    ThoiGianBatDau = DateTime.Now,
                    TrangThai = TrangThaiHoaDon.DangChoi
                };

                _context.HoaDon.Add(hoaDon);

                var datBan = await _context.DatBan
                    .FirstOrDefaultAsync(d => d.MaBan == request.MaBan &&
                                            d.TrangThai == TrangThaiDatBan.DangCho);
                if (datBan != null)
                {
                    datBan.TrangThai = TrangThaiDatBan.DaXacNhan;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xác nhận đặt bàn thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Hủy đặt bàn
        [HttpPost]
        public async Task<IActionResult> HuyDatBan([FromBody] HuyDatBanRequest request)
        {
            try
            {
                var ban = await _context.BanBia.FindAsync(request.MaBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.TrangThai != TrangThaiBan.DaDat)
                    return Json(new { success = false, message = "Bàn không ở trạng thái đã đặt" });

                ban.TrangThai = TrangThaiBan.Trong;
                ban.MaKH = null;
                ban.GhiChu = null;

                var datBan = await _context.DatBan
                    .FirstOrDefaultAsync(d => d.MaBan == request.MaBan &&
                                            d.TrangThai == TrangThaiDatBan.DangCho);
                if (datBan != null)
                {
                    datBan.TrangThai = TrangThaiDatBan.DaHuy;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Hủy đặt bàn thành công" });
            }
            catch (Exception ex)
            {
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

                var danhSachDV = await _context.DichVu
                    .Where(dv => dv.TrangThai == TrangThaiDichVu.ConHang)
                    .OrderBy(dv => dv.Loai)
                    .ThenBy(dv => dv.TenDV)
                    .ToListAsync();

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

                var dichVu = await _context.DichVu.FindAsync(request.MaDV);
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
                var hoaDon = await _context.HoaDon
                    .Include(h => h.BanBia)
                    .FirstOrDefaultAsync(h => h.MaHD == request.MaHD);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

                // Kiểm tra tiền khách đưa nếu là tiền mặt
                if (request.PhuongThucThanhToan == PhuongThucThanhToan.TienMat)
                {
                    if (request.TienKhachDua < hoaDon.TongTien)
                        return Json(new { success = false, message = "Số tiền khách đưa không đủ" });
                }

                hoaDon.TrangThai = TrangThaiHoaDon.DaThanhToan;
                hoaDon.PhuongThucThanhToan = request.PhuongThucThanhToan;

                // Cập nhật trạng thái bàn về trống
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
                        LyDo = $"Thanh toán hóa đơn #{hoaDon.MaHD} - Bàn {hoaDon.BanBia?.TenBan}",
                        MaHDLienQuan = hoaDon.MaHD
                    };
                    _context.SoQuy.Add(soQuy);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
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
        public int? MaKH { get; set; }
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
        public PhuongThucThanhToan PhuongThucThanhToan { get; set; }
        public decimal TienKhachDua { get; set; }
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