using BTL_QlBi_a.Controllers;
using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BTL_QlBi_a.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public NhanVienController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        #region Helper Methods

        private bool KiemTraQuyen(string[] requiredRoles)
        {
            var tenNhom = HttpContext.Session.GetString("TenNhom");
            return requiredRoles.Contains(tenNhom);
        }

        private async Task LoadHeaderStats()
        {
            ViewBag.TongNhanVien = await _context.NhanVien.CountAsync();
            ViewBag.NhanVienDangLam = await _context.NhanVien
                .CountAsync(nv => nv.TrangThai == TrangThaiNhanVien.DangLam);
            ViewBag.NhanVienNghi = await _context.NhanVien
                .CountAsync(nv => nv.TrangThai == TrangThaiNhanVien.Nghi);

            var today = DateTime.Today;
            ViewBag.ChamCongHomNay = await _context.ChamCong
                .CountAsync(cc => cc.Ngay.Date == today);
        }

        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"face_{Guid.NewGuid()}{extension}";

                // QUAN TRỌNG: Đường dẫn phải khớp với cấu trúc thư mục
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "asset", "img", "employees");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    Console.WriteLine($"✅ Created directory: {uploadsFolder}");
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Console.WriteLine($"✅ Image saved: {filePath}");

                // Trả về đường dẫn TƯƠNG ĐỐI từ wwwroot
                return $"employees/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving image: {ex.Message}");
                return null;
            }
        }

        private async Task<string> GenerateFaceHashFromFile(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(imageBytes);

            var builder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        #endregion

        #region Views & Main Page

        [HttpGet]
        public async Task<IActionResult> QuanLyNhanVien()
        {
            var tenNhom = HttpContext.Session.GetString("TenNhom") ?? "Nhân viên";

            if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
            {
                return RedirectToAction("BanBia", "Home");
            }

            ViewBag.TenNhom = tenNhom;
            await LoadHeaderStats();

            var danhSachNhanVien = await _context.NhanVien
                .Include(nv => nv.NhomQuyen)
                .OrderBy(nv => nv.MaNV)
                .ToListAsync();

            return View("~/Views/Home/NhanVien.cshtml", danhSachNhanVien);
        }

        #endregion

        #region Chi tiết nhân viên

        [HttpGet]
        public async Task<IActionResult> ChiTietNhanVien(int maNV)
        {
            if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
            {
                return Forbid();
            }

            var nhanVien = await _context.NhanVien
                .Include(nv => nv.NhomQuyen)
                .FirstOrDefaultAsync(nv => nv.MaNV == maNV);

            if (nhanVien == null)
                return NotFound(new { message = "Không tìm thấy nhân viên" });

            var thangHienTai = DateTime.Now.Month;
            var namHienTai = DateTime.Now.Year;

            var chamCongThang = await _context.ChamCong
                .Where(cc => cc.MaNV == maNV &&
                            cc.Ngay.Month == thangHienTai &&
                            cc.Ngay.Year == namHienTai)
                .ToListAsync();

            ViewBag.SoNgayLam = chamCongThang.Count;
            ViewBag.TongGioLam = chamCongThang.Sum(cc => cc.SoGioLam ?? 0);
            ViewBag.SoLanDiTre = chamCongThang.Count(cc => cc.TrangThai == TrangThaiChamCong.DiTre);

            var bangLuong = await _context.BangLuong
                .Where(bl => bl.MaNV == maNV)
                .OrderByDescending(bl => bl.Nam)
                .ThenByDescending(bl => bl.Thang)
                .FirstOrDefaultAsync();

            ViewBag.BangLuong = bangLuong;

            return PartialView("~/Views/Home/Partials/NhanVien/_ChiTietNhanVien.cshtml", nhanVien);
        }

        #endregion

        #region CRUD Nhân viên

        [HttpGet]
        public async Task<IActionResult> FormThemNhanVien()
        {
            try
            {
                // Return partial view cho modal chấm công
                return PartialView("~/Views/Home/Partials/NhanVien/_AddNhanVien.cshtml");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAttendanceModal: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThemNhanVien([FromForm] ThemNhanVienRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền thực hiện" });
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.TenNV))
                    return Json(new { success = false, message = "Vui lòng nhập tên nhân viên" });

                if (string.IsNullOrWhiteSpace(request.SDT))
                    return Json(new { success = false, message = "Vui lòng nhập số điện thoại" });

                if (string.IsNullOrWhiteSpace(request.MatKhau))
                    return Json(new { success = false, message = "Vui lòng nhập mật khẩu" });

                // Check duplicate phone number
                var sdtTonTai = await _context.NhanVien.AnyAsync(nv => nv.SDT == request.SDT);
                if (sdtTonTai)
                    return Json(new { success = false, message = "Số điện thoại đã được sử dụng" });

                string faceIdPath = null;
                string faceIdHash = null;

                // Process Face ID if uploaded
                if (request.FaceIDAnh != null && request.FaceIDAnh.Length > 0)
                {
                    Console.WriteLine($"📸 Processing Face ID image: {request.FaceIDAnh.FileName}");

                    faceIdPath = await SaveImageAsync(request.FaceIDAnh);
                    if (!string.IsNullOrEmpty(faceIdPath))
                    {
                        faceIdHash = await GenerateFaceHashFromFile(request.FaceIDAnh);
                        Console.WriteLine($"✅ Face ID saved: {faceIdPath}");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Failed to save Face ID image");
                    }
                }

                // Create new employee
                // NOTE: CaMacDinh defaults to 0 (or you can set a default value)
                var nhanVienMoi = new NhanVien
                {
                    TenNV = request.TenNV,
                    SDT = request.SDT,
                    Email = request.Email,
                    MaNhom = request.MaNhom,
                    LuongCoBan = request.LuongCoBan,
                    PhuCap = request.PhuCap,
                    CaMacDinh = CaLamViec.Sang, // Default shift - can be changed later
                    TrangThai = TrangThaiNhanVien.DangLam,
                    MatKhau = HashPassword(request.MatKhau),
                    FaceIDAnh = faceIdPath,
                    FaceIDHash = faceIdHash
                };

                _context.NhanVien.Add(nhanVienMoi);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Employee added: #{nhanVienMoi.MaNV} - {nhanVienMoi.TenNV}");

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Thêm nhân viên",
                        ChiTiet = $"Thêm nhân viên {request.TenNV} (ID: {nhanVienMoi.MaNV})" +
                                 (faceIdPath != null ? " - Có Face ID" : "")
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = "Thêm nhân viên thành công" + (faceIdPath != null ? " (Đã lưu Face ID)" : ""),
                    employeeId = nhanVienMoi.MaNV
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ThemNhanVien: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CalculateSalary([FromBody] CalculateSalaryRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền tính lương" });
                }

                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                var existingSalary = await _context.BangLuong
                    .FirstOrDefaultAsync(bl => bl.MaNV == request.MaNV &&
                                              bl.Thang == request.Month &&
                                              bl.Nam == request.Year);

                if (existingSalary != null)
                {
                    return Json(new { success = false, message = "Đã tính lương tháng này rồi" });
                }

                var chamCong = await _context.ChamCong
                    .Where(cc => cc.MaNV == request.MaNV &&
                                cc.Ngay.Month == request.Month &&
                                cc.Ngay.Year == request.Year)
                    .ToListAsync();

                var tongGio = chamCong.Sum(cc => cc.SoGioLam ?? 0);
                var soLanDiTre = chamCong.Count(cc => cc.TrangThai == TrangThaiChamCong.DiTre);

                var luongCoBan = nhanVien.LuongCoBan;
                var phuCap = nhanVien.PhuCap;
                var thuong = 0m;
                var phat = soLanDiTre * 50000m;

                var tongLuong = luongCoBan + phuCap + thuong - phat;

                var bangLuong = new BangLuong
                {
                    MaNV = request.MaNV,
                    Thang = request.Month,
                    Nam = request.Year,
                    LuongCoBan = luongCoBan,
                    PhuCap = phuCap,
                    Thuong = thuong,
                    Phat = phat,
                    TongLuong = tongLuong,
                    TongGio = (int)tongGio,
                    NgayTinh = DateTime.Now
                };

                _context.BangLuong.Add(bangLuong);
                await _context.SaveChangesAsync();

                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Tính lương",
                        ChiTiet = $"Tính lương tháng {request.Month}/{request.Year} cho {nhanVien.TenNV}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Tính lương thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateSalary: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Kích hoạt/Vô hiệu hóa

        [HttpPost]
        public async Task<IActionResult> Deactivate(int maNV)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền thực hiện" });
                }

                var nhanVien = await _context.NhanVien.FindAsync(maNV);
                if (nhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                nhanVien.TrangThai = TrangThaiNhanVien.Nghi;
                await _context.SaveChangesAsync();

                int? maNVHienTai = HttpContext.Session.GetInt32("MaNV");
                if (maNVHienTai.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNVHienTai.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Cho nhân viên nghỉ việc",
                        ChiTiet = $"Chuyển {nhanVien.TenNV} sang trạng thái Nghỉ việc"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã chuyển nhân viên sang trạng thái Nghỉ việc" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Deactivate: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int maNV)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền thực hiện" });
                }

                var nhanVien = await _context.NhanVien.FindAsync(maNV);
                if (nhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                nhanVien.TrangThai = TrangThaiNhanVien.DangLam;
                await _context.SaveChangesAsync();

                int? maNVHienTai = HttpContext.Session.GetInt32("MaNV");
                if (maNVHienTai.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNVHienTai.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Kích hoạt lại nhân viên",
                        ChiTiet = $"Kích hoạt lại {nhanVien.TenNV}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã kích hoạt lại nhân viên" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Activate: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Lịch sử hoạt động

        [HttpGet]
        public async Task<IActionResult> LichSuHoatDong(int maNV)
        {
            if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
            {
                return Forbid();
            }

            var lichSu = await _context.LichSuHoatDong
                .Where(ls => ls.MaNV == maNV)
                .OrderByDescending(ls => ls.ThoiGian)
                .Take(50)
                .ToListAsync();

            return PartialView("~/Views/Home/Partials/NhanVien/_LichSuHoatDong.cshtml", lichSu);
        }

        #endregion

        #region Face ID Management

        [HttpPost]
        public async Task<IActionResult> UpdateFaceId([FromBody] UpdateFaceIdRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền cập nhật Face ID" });
                }

                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                // Store the hash for face recognition
                nhanVien.FaceIDHash = request.Hash;

                if (!string.IsNullOrEmpty(request.ImageUrl))
                {
                    nhanVien.FaceIDAnh = request.ImageUrl;
                }

                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Cập nhật Face ID",
                        ChiTiet = $"Cập nhật Face ID cho {nhanVien.TenNV} (ID: {nhanVien.MaNV})"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Cập nhật Face ID thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateFaceId: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFaceId([FromBody] DeleteFaceIdRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền xóa Face ID" });
                }

                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                // Delete face data
                nhanVien.FaceIDHash = null;
                nhanVien.FaceIDAnh = null;

                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Xóa Face ID",
                        ChiTiet = $"Xóa Face ID của {nhanVien.TenNV} (ID: {nhanVien.MaNV})"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã xóa Face ID" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteFaceId: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Upload Image

        [HttpPost]
        public async Task<IActionResult> UploadFaceImage([FromForm] IFormFile image, [FromForm] int maNV)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền tải ảnh" });
                }

                if (image == null || image.Length == 0)
                {
                    return Json(new { success = false, message = "Không có ảnh được chọn" });
                }

                // Validate image
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { success = false, message = "Chỉ chấp nhận ảnh JPG, JPEG, PNG" });
                }

                if (image.Length > 5 * 1024 * 1024) // 5MB
                {
                    return Json(new { success = false, message = "Kích thước ảnh không được vượt quá 5MB" });
                }

                // Save image
                var imagePath = await SaveImageAsync(image);
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { success = false, message = "Không thể lưu ảnh" });
                }

                // Generate hash
                var hash = await GenerateFaceHashFromFile(image);

                // Update database
                var nhanVien = await _context.NhanVien.FindAsync(maNV);
                if (nhanVien != null)
                {
                    nhanVien.FaceIDAnh = imagePath;
                    nhanVien.FaceIDHash = hash;
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = "Tải ảnh thành công",
                    url = imagePath,
                    hash = hash
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadFaceImage: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Reports & Statistics

        [HttpGet]
        public async Task<IActionResult> GetEmployeeStatistics()
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền xem thống kê" });
                }

                var today = DateTime.Today;
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var stats = new
                {
                    tongNhanVien = await _context.NhanVien.CountAsync(),
                    dangLam = await _context.NhanVien.CountAsync(nv => nv.TrangThai == TrangThaiNhanVien.DangLam),
                    nghi = await _context.NhanVien.CountAsync(nv => nv.TrangThai == TrangThaiNhanVien.Nghi),
                    coFaceId = await _context.NhanVien.CountAsync(nv => !string.IsNullOrEmpty(nv.FaceIDHash)),
                    chamCongHomNay = await _context.ChamCong.CountAsync(cc => cc.Ngay.Date == today),
                    diTreThangNay = await _context.ChamCong
                        .CountAsync(cc => cc.Ngay.Month == currentMonth &&
                                         cc.Ngay.Year == currentYear &&
                                         cc.TrangThai == TrangThaiChamCong.DiTre),
                    tongGioLamThangNay = await _context.ChamCong
                        .Where(cc => cc.Ngay.Month == currentMonth && cc.Ngay.Year == currentYear)
                        .SumAsync(cc => cc.SoGioLam ?? 0),

                    // Phân bổ theo nhóm quyền
                    theoNhomQuyen = await _context.NhanVien
                        .Include(nv => nv.NhomQuyen)
                        .Where(nv => nv.TrangThai == TrangThaiNhanVien.DangLam)
                        .GroupBy(nv => nv.NhomQuyen.TenNhom)
                        .Select(g => new { tenNhom = g.Key, soLuong = g.Count() })
                        .ToListAsync(),

                    // Phân bổ theo ca làm việc
                    theoCa = await _context.NhanVien
                        .Where(nv => nv.TrangThai == TrangThaiNhanVien.DangLam)
                        .GroupBy(nv => nv.CaMacDinh)
                        .Select(g => new { ca = g.Key.ToString(), soLuong = g.Count() })
                        .ToListAsync()
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetEmployeeStatistics: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceReport(int? month, int? year)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền xem báo cáo" });
                }

                var thang = month ?? DateTime.Now.Month;
                var nam = year ?? DateTime.Now.Year;

                var report = await _context.ChamCong
                    .Include(cc => cc.NhanVien)
                    .Where(cc => cc.Ngay.Month == thang && cc.Ngay.Year == nam)
                    .GroupBy(cc => new { cc.MaNV, cc.NhanVien.TenNV })
                    .Select(g => new
                    {
                        maNV = g.Key.MaNV,
                        tenNV = g.Key.TenNV,
                        soNgayLam = g.Count(),
                        tongGioLam = g.Sum(cc => cc.SoGioLam ?? 0),
                        soLanDiTre = g.Count(cc => cc.TrangThai == TrangThaiChamCong.DiTre),
                        soLanDungGio = g.Count(cc => cc.TrangThai == TrangThaiChamCong.DungGio),
                        soLanVang = g.Count(cc => cc.TrangThai == TrangThaiChamCong.Vang)
                    })
                    .OrderByDescending(x => x.tongGioLam)
                    .ToListAsync();

                return Json(new { success = true, data = report, thang = thang, nam = nam });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAttendanceReport: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportAttendanceReport(int? month, int? year)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Forbid();
                }

                var thang = month ?? DateTime.Now.Month;
                var nam = year ?? DateTime.Now.Year;

                var records = await _context.ChamCong
                    .Include(cc => cc.NhanVien)
                    .ThenInclude(nv => nv.NhomQuyen)
                    .Where(cc => cc.Ngay.Month == thang && cc.Ngay.Year == nam)
                    .OrderBy(cc => cc.MaNV)
                    .ThenBy(cc => cc.Ngay)
                    .ToListAsync();

                // Create CSV content
                var csv = new StringBuilder();
                csv.AppendLine("\uFEFF"); // UTF-8 BOM
                csv.AppendLine($"BÁO CÁO CHẤM CÔNG THÁNG {thang}/{nam}");
                csv.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
                csv.AppendLine();
                csv.AppendLine("Mã NV,Tên NV,Nhóm quyền,Ngày,Giờ vào,Giờ ra,Số giờ làm,Trạng thái,Xác thực,Ghi chú");

                foreach (var record in records)
                {
                    csv.AppendLine($"{record.MaNV}," +
                        $"\"{record.NhanVien.TenNV}\"," +
                        $"\"{record.NhanVien.NhomQuyen?.TenNhom}\"," +
                        $"{record.Ngay:dd/MM/yyyy}," +
                        $"{(record.GioVao.HasValue ? record.GioVao.Value.ToString("HH:mm") : "")}," +
                        $"{(record.GioRa.HasValue ? record.GioRa.Value.ToString("HH:mm") : "")}," +
                        $"{record.SoGioLam?.ToString("F1")}," +
                        $"\"{record.TrangThai}\"," +
                        $"\"{record.XacThucBang}\"," +
                        $"\"{record.GhiChu?.Replace("\"", "\"\"")}\"");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"BaoCaoChamCong_{thang}_{nam}.csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportAttendanceReport: {ex.Message}");
                return BadRequest(new { message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Search & Filter

        [HttpGet]
        public async Task<IActionResult> SearchEmployees(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return Json(new { success = false, message = "Vui lòng nhập từ khóa tìm kiếm" });
                }

                keyword = keyword.ToLower().Trim();

                var employees = await _context.NhanVien
                    .Include(nv => nv.NhomQuyen)
                    .Where(nv =>
                        nv.TenNV.ToLower().Contains(keyword) ||
                        nv.SDT.Contains(keyword) ||
                        nv.Email.ToLower().Contains(keyword) ||
                        nv.MaNV.ToString().Contains(keyword))
                    .Select(nv => new
                    {
                        maNV = nv.MaNV,
                        tenNV = nv.TenNV,
                        sdt = nv.SDT,
                        email = nv.Email,
                        tenNhom = nv.NhomQuyen.TenNhom,
                        caMacDinh = nv.CaMacDinh.ToString(),
                        trangThai = nv.TrangThai.ToString(),
                        luongCoBan = nv.LuongCoBan,
                        phuCap = nv.PhuCap,
                        faceIDAnh = nv.FaceIDAnh
                    })
                    .Take(20)
                    .ToListAsync();

                return Json(new { success = true, data = employees });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchEmployees: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterEmployees(string? trangThai, string? nhomQuyen, string? ca)
        {
            try
            {
                var query = _context.NhanVien
                    .Include(nv => nv.NhomQuyen)
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(trangThai) && trangThai != "all")
                {
                    if (Enum.TryParse<TrangThaiNhanVien>(trangThai, out var status))
                    {
                        query = query.Where(nv => nv.TrangThai == status);
                    }
                }

                // Filter by role
                if (!string.IsNullOrEmpty(nhomQuyen) && nhomQuyen != "all")
                {
                    query = query.Where(nv => nv.NhomQuyen.TenNhom == nhomQuyen);
                }

                // Filter by shift
                if (!string.IsNullOrEmpty(ca) && ca != "all")
                {
                    if (Enum.TryParse<CaLamViec>(ca, out var shift))
                    {
                        query = query.Where(nv => nv.CaMacDinh == shift);
                    }
                }

                var employees = await query
                    .Select(nv => new
                    {
                        maNV = nv.MaNV,
                        tenNV = nv.TenNV,
                        sdt = nv.SDT,
                        tenNhom = nv.NhomQuyen.TenNhom,
                        caMacDinh = nv.CaMacDinh.ToString(),
                        trangThai = nv.TrangThai.ToString(),
                        luongCoBan = nv.LuongCoBan,
                        phuCap = nv.PhuCap
                    })
                    .ToListAsync();

                return Json(new { success = true, data = employees });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FilterEmployees: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Bulk Operations

        [HttpPost]
        public async Task<IActionResult> BulkDeactivate([FromBody] BulkActionRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền thực hiện" });
                }

                if (request.MaNVList == null || !request.MaNVList.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn nhân viên" });
                }

                var employees = await _context.NhanVien
                    .Where(nv => request.MaNVList.Contains(nv.MaNV))
                    .ToListAsync();

                foreach (var emp in employees)
                {
                    emp.TrangThai = TrangThaiNhanVien.Nghi;
                }

                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Vô hiệu hóa hàng loạt",
                        ChiTiet = $"Vô hiệu hóa {employees.Count} nhân viên"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = $"Đã vô hiệu hóa {employees.Count} nhân viên" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BulkDeactivate: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkActivate([FromBody] BulkActionRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền thực hiện" });
                }

                if (request.MaNVList == null || !request.MaNVList.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn nhân viên" });
                }

                var employees = await _context.NhanVien
                    .Where(nv => request.MaNVList.Contains(nv.MaNV))
                    .ToListAsync();

                foreach (var emp in employees)
                {
                    emp.TrangThai = TrangThaiNhanVien.DangLam;
                }

                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Kích hoạt hàng loạt",
                        ChiTiet = $"Kích hoạt {employees.Count} nhân viên"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = $"Đã kích hoạt {employees.Count} nhân viên" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BulkActivate: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateShift([FromBody] BulkUpdateShiftRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền thực hiện" });
                }

                if (request.MaNVList == null || !request.MaNVList.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn nhân viên" });
                }

                var employees = await _context.NhanVien
                    .Where(nv => request.MaNVList.Contains(nv.MaNV))
                    .ToListAsync();

                foreach (var emp in employees)
                {
                    emp.CaMacDinh = request.CaMacDinh;
                }

                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Cập nhật ca làm việc hàng loạt",
                        ChiTiet = $"Cập nhật ca {request.CaMacDinh} cho {employees.Count} nhân viên"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = $"Đã cập nhật ca làm việc cho {employees.Count} nhân viên" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BulkUpdateShift: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkDelete([FromBody] BulkActionRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin" }))
                {
                    return Json(new { success = false, message = "Chỉ Admin mới có quyền xóa hàng loạt" });
                }

                if (request.MaNVList == null || !request.MaNVList.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn nhân viên" });
                }

                int? currentMaNV = HttpContext.Session.GetInt32("MaNV");
                if (currentMaNV.HasValue && request.MaNVList.Contains(currentMaNV.Value))
                {
                    return Json(new { success = false, message = "Không thể xóa tài khoản của chính bạn" });
                }

                var employees = await _context.NhanVien
                    .Where(nv => request.MaNVList.Contains(nv.MaNV))
                    .ToListAsync();

                int deletedCount = 0;
                int deactivatedCount = 0;

                foreach (var emp in employees)
                {
                    var hasRelatedData = await _context.HoaDon.AnyAsync(hd => hd.MaNV == emp.MaNV) ||
                                        await _context.ChamCong.AnyAsync(cc => cc.MaNV == emp.MaNV);

                    if (hasRelatedData)
                    {
                        emp.TrangThai = TrangThaiNhanVien.Nghi;
                        deactivatedCount++;
                    }
                    else
                    {
                        _context.NhanVien.Remove(emp);
                        deletedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                // Log activity
                if (currentMaNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = currentMaNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Xóa hàng loạt",
                        ChiTiet = $"Xóa {deletedCount} nhân viên, chuyển {deactivatedCount} sang trạng thái Nghỉ"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"Đã xóa {deletedCount} nhân viên, chuyển {deactivatedCount} sang trạng thái Nghỉ",
                    deleted = deletedCount,
                    deactivated = deactivatedCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BulkDelete: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Advanced Attendance

        [HttpPost]
        public async Task<IActionResult> ManualCheckInOut([FromBody] ManualCheckInOutRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền chấm công thủ công" });
                }

                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                var targetDate = request.Ngay.Date;

                var existingRecord = await _context.ChamCong
                    .FirstOrDefaultAsync(cc => cc.MaNV == request.MaNV && cc.Ngay.Date == targetDate);

                if (existingRecord != null)
                {
                    // Update existing record
                    if (request.GioVao.HasValue)
                        existingRecord.GioVao = request.GioVao.Value;

                    if (request.GioRa.HasValue)
                        existingRecord.GioRa = request.GioRa.Value;

                    existingRecord.GhiChu = request.GhiChu;
                    existingRecord.XacThucBang = PhuongThucXacThuc.ThuCong;

                    // Recalculate status
                    if (existingRecord.GioVao.HasValue)
                    {
                        var gioVao = existingRecord.GioVao.Value.TimeOfDay;
                        var gioChuan = new TimeSpan(7, 0, 0);
                        var gioTreMax = new TimeSpan(7, 15, 0);

                        if (gioVao <= gioChuan)
                            existingRecord.TrangThai = TrangThaiChamCong.DungGio;
                        else if (gioVao <= gioTreMax)
                            existingRecord.TrangThai = TrangThaiChamCong.DiTre;
                        else
                            existingRecord.TrangThai = TrangThaiChamCong.Vang;
                    }
                }
                else
                {
                    // Create new record
                    var newRecord = new ChamCong
                    {
                        MaNV = request.MaNV,
                        Ngay = targetDate,
                        GioVao = request.GioVao,
                        GioRa = request.GioRa,
                        GhiChu = request.GhiChu,
                        XacThucBang = PhuongThucXacThuc.ThuCong,
                        TrangThai = TrangThaiChamCong.DungGio
                    };

                    _context.ChamCong.Add(newRecord);
                }

                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Chấm công thủ công",
                        ChiTiet = $"Chấm công thủ công cho {nhanVien.TenNV} ngày {targetDate:dd/MM/yyyy}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Chấm công thủ công thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ManualCheckInOut: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttendanceRecord([FromBody] DeleteAttendanceRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền xóa bản ghi chấm công" });
                }

                var record = await _context.ChamCong.FindAsync(request.ID);
                if (record == null)
                    return Json(new { success = false, message = "Không tìm thấy bản ghi" });

                _context.ChamCong.Remove(record);
                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Xóa bản ghi chấm công",
                        ChiTiet = $"Xóa bản ghi chấm công ID: {request.ID}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã xóa bản ghi chấm công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteAttendanceRecord: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceDetail(int id)
        {
            try
            {
                var record = await _context.ChamCong
                    .Include(cc => cc.NhanVien)
                    .ThenInclude(nv => nv.NhomQuyen)
                    .FirstOrDefaultAsync(cc => cc.ID == id);

                if (record == null)
                    return Json(new { success = false, message = "Không tìm thấy bản ghi" });

                var result = new
                {
                    id = record.ID,
                    maNV = record.MaNV,
                    tenNV = record.NhanVien.TenNV,
                    tenNhom = record.NhanVien.NhomQuyen?.TenNhom,
                    ngay = record.Ngay,
                    gioVao = record.GioVao,
                    gioRa = record.GioRa,
                    soGioLam = record.SoGioLam,
                    trangThai = record.TrangThai.ToString(),
                    xacThucBang = record.XacThucBang.ToString(),
                    ghiChu = record.GhiChu,
                    hinhAnhVao = record.HinhAnhVao,
                    hinhAnhRa = record.HinhAnhRa
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAttendanceDetail: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Salary Management Extended

        [HttpPost]
        public async Task<IActionResult> UpdateSalaryComponents([FromBody] UpdateSalaryRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền cập nhật lương" });
                }

                var bangLuong = await _context.BangLuong.FindAsync(request.MaLuong);
                if (bangLuong == null)
                    return Json(new { success = false, message = "Không tìm thấy bảng lương" });

                bangLuong.LuongCoBan = request.LuongCoBan;
                bangLuong.PhuCap = request.PhuCap;
                bangLuong.Thuong = request.Thuong;
                bangLuong.Phat = request.Phat;
                bangLuong.TongLuong = request.LuongCoBan + request.PhuCap + request.Thuong - request.Phat;

                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Cập nhật bảng lương",
                        ChiTiet = $"Cập nhật bảng lương ID: {request.MaLuong}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Cập nhật bảng lương thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateSalaryComponents: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSalary([FromBody] DeleteSalaryRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin" }))
                {
                    return Json(new { success = false, message = "Chỉ Admin mới có quyền xóa bảng lương" });
                }

                var bangLuong = await _context.BangLuong.FindAsync(request.MaLuong);
                if (bangLuong == null)
                    return Json(new { success = false, message = "Không tìm thấy bảng lương" });

                _context.BangLuong.Remove(bangLuong);
                await _context.SaveChangesAsync();

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Xóa bảng lương",
                        ChiTiet = $"Xóa bảng lương ID: {request.MaLuong}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã xóa bảng lương" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteSalary: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSalaryComparison(int maNV, int year)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền xem so sánh lương" });
                }

                var salaries = await _context.BangLuong
                    .Where(bl => bl.MaNV == maNV && bl.Nam == year)
                    .OrderBy(bl => bl.Thang)
                    .Select(bl => new
                    {
                        thang = bl.Thang,
                        tongLuong = bl.TongLuong,
                        tongGio = bl.TongGio,
                        thuong = bl.Thuong,
                        phat = bl.Phat
                    })
                    .ToListAsync();

                var tongLuongNam = salaries.Sum(s => s.tongLuong);
                var trungBinhThang = salaries.Any() ? salaries.Average(s => s.tongLuong) : 0;
                var thangCaoNhat = salaries.OrderByDescending(s => s.tongLuong).FirstOrDefault();
                var thangThapNhat = salaries.OrderBy(s => s.tongLuong).FirstOrDefault();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        chiTiet = salaries,
                        tongLuongNam = tongLuongNam,
                        trungBinhThang = trungBinhThang,
                        thangCaoNhat = thangCaoNhat,
                        thangThapNhat = thangThapNhat
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSalaryComparison: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Dashboard & Analytics

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền xem dashboard" });
                }

                var today = DateTime.Today;
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var lastMonth = DateTime.Now.AddMonths(-1);

                // Basic stats
                var tongNhanVien = await _context.NhanVien.CountAsync();
                var dangLam = await _context.NhanVien.CountAsync(nv => nv.TrangThai == TrangThaiNhanVien.DangLam);

                // Attendance today
                var chamCongHomNay = await _context.ChamCong
                    .Where(cc => cc.Ngay.Date == today)
                    .GroupBy(cc => 1)
                    .Select(g => new
                    {
                        total = g.Count(),
                        dungGio = g.Count(cc => cc.TrangThai == TrangThaiChamCong.DungGio),
                        diTre = g.Count(cc => cc.TrangThai == TrangThaiChamCong.DiTre),
                        chuaCheckOut = g.Count(cc => !cc.GioRa.HasValue)
                    })
                    .FirstOrDefaultAsync();

                // Attendance this month
                var chamCongThangNay = await _context.ChamCong
                    .Where(cc => cc.Ngay.Month == currentMonth && cc.Ngay.Year == currentYear)
                    .GroupBy(cc => 1)
                    .Select(g => new
                    {
                        tongNgayLam = g.Count(),
                        tongGioLam = g.Sum(cc => cc.SoGioLam ?? 0),
                        diTre = g.Count(cc => cc.TrangThai == TrangThaiChamCong.DiTre)
                    })
                    .FirstOrDefaultAsync();

                // Compare with last month
                var chamCongThangTruoc = await _context.ChamCong
                    .Where(cc => cc.Ngay.Month == lastMonth.Month && cc.Ngay.Year == lastMonth.Year)
                    .SumAsync(cc => cc.SoGioLam ?? 0);

                // Top performers
                var topPerformers = await _context.ChamCong
                    .Include(cc => cc.NhanVien)
                    .Where(cc => cc.Ngay.Month == currentMonth && cc.Ngay.Year == currentYear)
                    .GroupBy(cc => new { cc.MaNV, cc.NhanVien.TenNV })
                    .Select(g => new
                    {
                        maNV = g.Key.MaNV,
                        tenNV = g.Key.TenNV,
                        tongGioLam = g.Sum(cc => cc.SoGioLam ?? 0),
                        soNgayLam = g.Count(),
                        dungGio = g.Count(cc => cc.TrangThai == TrangThaiChamCong.DungGio)
                    })
                    .OrderByDescending(x => x.tongGioLam)
                    .Take(5)
                    .ToListAsync();

                // Attendance trend (last 7 days)
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => today.AddDays(-i))
                    .ToList();

                var attendanceTrend = new List<object>();
                foreach (var date in last7Days)
                {
                    var count = await _context.ChamCong
                        .Where(cc => cc.Ngay.Date == date)
                        .GroupBy(cc => 1)
                        .Select(g => new
                        {
                            date = date,
                            total = g.Count(),
                            dungGio = g.Count(cc => cc.TrangThai == TrangThaiChamCong.DungGio),
                            diTre = g.Count(cc => cc.TrangThai == TrangThaiChamCong.DiTre)
                        })
                        .FirstOrDefaultAsync();

                    attendanceTrend.Add(count ?? new { date = date, total = 0, dungGio = 0, diTre = 0 });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        tongNhanVien = tongNhanVien,
                        dangLam = dangLam,
                        chamCongHomNay = chamCongHomNay ?? new { total = 0, dungGio = 0, diTre = 0, chuaCheckOut = 0 },
                        chamCongThangNay = chamCongThangNay ?? new { tongNgayLam = 0, tongGioLam = 0m, diTre = 0 },
                        chamCongThangTruoc = chamCongThangTruoc,
                        topPerformers = topPerformers,
                        attendanceTrend = attendanceTrend.OrderBy(x => ((dynamic)x).date).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDashboardData: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLateStatistics(int? month, int? year)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền xem thống kê" });
                }

                var thang = month ?? DateTime.Now.Month;
                var nam = year ?? DateTime.Now.Year;

                var lateStats = await _context.ChamCong
                    .Include(cc => cc.NhanVien)
                    .Where(cc => cc.Ngay.Month == thang &&
                                cc.Ngay.Year == nam &&
                                cc.TrangThai == TrangThaiChamCong.DiTre)
                    .GroupBy(cc => new { cc.MaNV, cc.NhanVien.TenNV })
                    .Select(g => new
                    {
                        maNV = g.Key.MaNV,
                        tenNV = g.Key.TenNV,
                        soLanDiTre = g.Count(),
                        ngayDiTre = g.Select(cc => cc.Ngay).ToList()
                    })
                    .OrderByDescending(x => x.soLanDiTre)
                    .ToListAsync();

                return Json(new { success = true, data = lateStats, thang = thang, nam = nam });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLateStatistics: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion
        #region Attendance Modal

        [HttpGet]
        public IActionResult GetAttendanceModal()
        {
            try
            {
                // Return partial view cho modal chấm công
                return PartialView("~/Views/Home/Partials/NhanVien/_AttendanceModal.cshtml");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAttendanceModal: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTodayAttendance(int maNV)
        {
            try
            {
                var today = DateTime.Today;

                var attendance = await _context.ChamCong
                    .FirstOrDefaultAsync(cc => cc.MaNV == maNV && cc.Ngay.Date == today);

                if (attendance == null)
                {
                    return Json(new { success = false, message = "Chưa có chấm công hôm nay" });
                }

                return Json(new
                {
                    success = true,
                    gioVao = attendance.GioVao,
                    gioRa = attendance.GioRa,
                    soGioLam = attendance.SoGioLam,
                    trangThai = attendance.TrangThai.ToString()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTodayAttendance: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RecognizeFace([FromForm] IFormFile faceImage)
        {
            try
            {
                if (faceImage == null || faceImage.Length == 0)
                {
                    return Json(new { success = false, message = "Không có ảnh" });
                }

                // Generate hash from uploaded image
                var uploadedHash = await GenerateFaceHashFromFile(faceImage);

                // Find employee with matching face hash
                var employee = await _context.NhanVien
                    .Include(nv => nv.NhomQuyen)
                    .FirstOrDefaultAsync(nv => nv.FaceIDHash == uploadedHash &&
                                              nv.TrangThai == TrangThaiNhanVien.DangLam);

                if (employee == null)
                {
                    return Json(new { success = false, message = "Không nhận diện được khuôn mặt" });
                }

                // Get today's attendance
                var today = DateTime.Today;
                var todayAttendance = await _context.ChamCong
                    .FirstOrDefaultAsync(cc => cc.MaNV == employee.MaNV && cc.Ngay.Date == today);

                return Json(new
                {
                    success = true,
                    employee = new
                    {
                        maNV = employee.MaNV,
                        tenNV = employee.TenNV,
                        tenNhom = employee.NhomQuyen?.TenNhom,
                        caMacDinh = employee.CaMacDinh.ToString(),
                        faceIDAnh = employee.FaceIDAnh
                    },
                    todayAttendance = todayAttendance == null ? null : new
                    {
                        gioVao = todayAttendance.GioVao,
                        gioRa = todayAttendance.GioRa,
                        soGioLam = todayAttendance.SoGioLam,
                        trangThai = todayAttendance.TrangThai.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RecognizeFace: {ex.Message}");
                return Json(new { success = false, message = "Lỗi nhận diện: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckAttendance([FromBody] CheckAttendanceRequest request)
        {
            try
            {
                var employee = await _context.NhanVien.FindAsync(request.MaNV);
                if (employee == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                var today = DateTime.Today;
                var now = DateTime.Now;

                var existingRecord = await _context.ChamCong
                    .FirstOrDefaultAsync(cc => cc.MaNV == request.MaNV && cc.Ngay.Date == today);

                if (request.IsCheckIn)
                {
                    // Check-in
                    if (existingRecord != null && existingRecord.GioVao.HasValue)
                    {
                        return Json(new { success = false, message = "Đã check-in rồi" });
                    }

                    var gioVao = now.TimeOfDay;
                    var gioChuan = new TimeSpan(7, 0, 0);
                    var gioTreMax = new TimeSpan(7, 15, 0);

                    TrangThaiChamCong trangThai;
                    if (gioVao <= gioChuan)
                        trangThai = TrangThaiChamCong.DungGio;
                    else if (gioVao <= gioTreMax)
                        trangThai = TrangThaiChamCong.DiTre;
                    else
                        trangThai = TrangThaiChamCong.Vang;

                    if (existingRecord == null)
                    {
                        existingRecord = new ChamCong
                        {
                            MaNV = request.MaNV,
                            Ngay = today,
                            GioVao = now,
                            TrangThai = trangThai,
                            XacThucBang = request.XacThucBang ?? PhuongThucXacThuc.ThuCong,
                            GhiChu = request.GhiChu
                        };
                        _context.ChamCong.Add(existingRecord);
                    }
                    else
                    {
                        existingRecord.GioVao = now;
                        existingRecord.TrangThai = trangThai;
                        existingRecord.XacThucBang = request.XacThucBang ?? PhuongThucXacThuc.ThuCong;
                        existingRecord.GhiChu = request.GhiChu;
                    }
                }
                else
                {
                    // Check-out
                    if (existingRecord == null || !existingRecord.GioVao.HasValue)
                    {
                        return Json(new { success = false, message = "Chưa check-in" });
                    }

                    if (existingRecord.GioRa.HasValue)
                    {
                        return Json(new { success = false, message = "Đã check-out rồi" });
                    }

                    existingRecord.GioRa = now;

                    // Tính số giờ làm
                    var soGio = (now - existingRecord.GioVao.Value).TotalHours;
                    existingRecord.SoGioLam = (decimal)soGio;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Chấm công thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckAttendance: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceHistory(int maNV, int month, int year)
        {
            try
            {
                var records = await _context.ChamCong
                    .Where(cc => cc.MaNV == maNV &&
                                cc.Ngay.Month == month &&
                                cc.Ngay.Year == year)
                    .OrderByDescending(cc => cc.Ngay)
                    .Select(cc => new
                    {
                        ngay = cc.Ngay,
                        gioVao = cc.GioVao,
                        gioRa = cc.GioRa,
                        soGioLam = cc.SoGioLam,
                        trangThai = cc.TrangThai.ToString(),
                        xacThucBang = cc.XacThucBang.ToString(),
                        ghiChu = cc.GhiChu
                    })
                    .ToListAsync();

                return Json(records);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAttendanceHistory: {ex.Message}");
                return Json(new List<object>());
            }
        }

        #endregion
        #region Schedule Management APIs

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _context.NhanVien
                    .Include(nv => nv.NhomQuyen)
                    .Where(nv => nv.TrangThai == TrangThaiNhanVien.DangLam)
                    .Select(nv => new
                    {
                        maNV = nv.MaNV,
                        tenNV = nv.TenNV,
                        tenNhom = nv.NhomQuyen.TenNhom,
                        caMacDinh = nv.CaMacDinh.ToString(),
                        sdt = nv.SDT
                    })
                    .ToListAsync();

                return Json(employees);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllEmployees: {ex.Message}");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWeekSchedule(string startDate, string endDate)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { });
                }

                var start = DateTime.Parse(startDate);
                var end = DateTime.Parse(endDate);

                // Lấy lịch làm việc từ database (giả sử có bảng LichLamViec)
                // Nếu chưa có, trả về dữ liệu mẫu hoặc empty

                var scheduleData = new Dictionary<string, Dictionary<string, List<object>>>();

                // Query từ database hoặc tạo structure
                for (var date = start; date <= end; date = date.AddDays(1))
                {
                    var dateKey = date.ToString("yyyy-MM-dd");
                    scheduleData[dateKey] = new Dictionary<string, List<object>>
                    {
                        ["morning"] = new List<object>(),
                        ["afternoon"] = new List<object>(),
                        ["evening"] = new List<object>(),
                        ["fullday"] = new List<object>()
                    };
                }

                return Json(scheduleData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetWeekSchedule: {ex.Message}");
                return Json(new { });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignEmployeesToShift([FromBody] AssignShiftRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền phân công" });
                }

                var date = DateTime.Parse(request.Date);

                // Kiểm tra không phân công cho ngày trong quá khứ
                if (date.Date < DateTime.Today)
                {
                    return Json(new { success = false, message = "Không thể phân công cho ngày trong quá khứ" });
                }

                // Lưu vào database (giả sử có bảng LichLamViec)
                // Ở đây tạo logic lưu lịch

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Phân công lịch làm việc",
                        ChiTiet = $"Phân công {request.EmployeeIds.Count} nhân viên cho ca {request.Shift} ngày {date:dd/MM/yyyy}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Phân công thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignEmployeesToShift: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkAssignSchedule([FromBody] BulkAssignRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền phân công" });
                }

                var startDate = DateTime.Parse(request.StartDate);
                var endDate = DateTime.Parse(request.EndDate);

                // Kiểm tra
                if (startDate.Date < DateTime.Today)
                {
                    return Json(new { success = false, message = "Ngày bắt đầu không được trong quá khứ" });
                }

                if (startDate > endDate)
                {
                    return Json(new { success = false, message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc" });
                }

                // Đếm số ngày
                int daysCount = 0;

                // Lưu cho từng ngày
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Logic lưu lịch cho mỗi ngày
                    daysCount++;
                }

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Phân công hàng loạt",
                        ChiTiet = $"Phân công {request.EmployeeIds.Count} nhân viên cho {daysCount} ngày"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"Đã phân công cho {daysCount} ngày",
                    daysCount = daysCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BulkAssignSchedule: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEmployeeFromShift([FromBody] RemoveFromShiftRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền" });
                }

                // Logic xóa nhân viên khỏi ca làm việc

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Xóa khỏi lịch làm việc",
                        ChiTiet = $"Xóa nhân viên #{request.EmployeeId} khỏi ca {request.Shift} ngày {request.Date}"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã xóa" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveEmployeeFromShift: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateScheduleNote([FromBody] UpdateNoteRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền" });
                }

                // Logic cập nhật ghi chú

                return Json(new { success = true, message = "Đã cập nhật ghi chú" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateScheduleNote: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportSchedule(string startDate, string endDate)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Forbid();
                }

                var start = DateTime.Parse(startDate);
                var end = DateTime.Parse(endDate);

                // Tạo file Excel (cần cài package ClosedXML hoặc EPPlus)
                // Ở đây tạm trả về CSV

                var csv = new StringBuilder();
                csv.AppendLine("\uFEFF"); // UTF-8 BOM
                csv.AppendLine($"LỊCH LÀM VIỆC TỪ {start:dd/MM/yyyy} ĐẾN {end:dd/MM/yyyy}");
                csv.AppendLine();
                csv.AppendLine("Ngày,Ca,Nhân viên,Ghi chú");

                // Query data và thêm vào CSV
                // ...

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"LichLamViec_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportSchedule: {ex.Message}");
                return BadRequest(new { message = "Lỗi: " + ex.Message });
            }
        }

        #endregion
        // Thêm vào NhanVienController.cs

        #region Face Recognition for Attendance

        [HttpPost]
        public async Task<IActionResult> RecognizeFaceForAttendance([FromForm] IFormFile faceImage)
        {
            try
            {
                if (faceImage == null || faceImage.Length == 0)
                {
                    return Json(new { success = false, message = "Không có ảnh" });
                }

                // Generate hash từ ảnh upload
                var uploadedHash = await GenerateFaceHashFromFile(faceImage);

                // Tìm nhân viên có face hash khớp
                var employee = await _context.NhanVien
                    .Include(nv => nv.NhomQuyen)
                    .FirstOrDefaultAsync(nv =>
                        !string.IsNullOrEmpty(nv.FaceIDHash) &&
                        nv.FaceIDHash == uploadedHash &&
                        nv.TrangThai == TrangThaiNhanVien.DangLam);

                if (employee == null)
                {
                    // Thử matching gần đúng (giảm độ chính xác)
                    var allEmployees = await _context.NhanVien
                        .Where(nv => !string.IsNullOrEmpty(nv.FaceIDHash) &&
                                    nv.TrangThai == TrangThaiNhanVien.DangLam)
                        .ToListAsync();

                    // Simple similarity check (trong thực tế nên dùng ML model)
                    foreach (var emp in allEmployees)
                    {
                        if (IsFaceHashSimilar(emp.FaceIDHash, uploadedHash))
                        {
                            employee = emp;
                            break;
                        }
                    }

                    if (employee == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Không nhận diện được khuôn mặt. Vui lòng thử lại hoặc sử dụng chấm công thủ công."
                        });
                    }
                }

                // Lấy chấm công hôm nay
                var today = DateTime.Today;
                var todayAttendance = await _context.ChamCong
                    .FirstOrDefaultAsync(cc => cc.MaNV == employee.MaNV && cc.Ngay.Date == today);

                return Json(new
                {
                    success = true,
                    employee = new
                    {
                        maNV = employee.MaNV,
                        tenNV = employee.TenNV,
                        tenNhom = employee.NhomQuyen?.TenNhom,
                        caMacDinh = employee.CaMacDinh.ToString(),
                        faceIDAnh = employee.FaceIDAnh
                    },
                    todayAttendance = todayAttendance == null ? null : new
                    {
                        gioVao = todayAttendance.GioVao,
                        gioRa = todayAttendance.GioRa,
                        soGioLam = todayAttendance.SoGioLam,
                        trangThai = todayAttendance.TrangThai.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RecognizeFaceForAttendance: {ex.Message}");
                return Json(new { success = false, message = "Lỗi nhận diện: " + ex.Message });
            }
        }

        // Helper method để so sánh hash (đơn giản hóa)
        private bool IsFaceHashSimilar(string hash1, string hash2)
        {
            if (string.IsNullOrEmpty(hash1) || string.IsNullOrEmpty(hash2))
                return false;

            // Đơn giản: so sánh % ký tự giống nhau
            // Trong thực tế nên dùng ML model hoặc Face Recognition API
            int minLength = Math.Min(hash1.Length, hash2.Length);
            int matchCount = 0;

            for (int i = 0; i < minLength; i++)
            {
                if (hash1[i] == hash2[i])
                    matchCount++;
            }

            double similarity = (double)matchCount / minLength;
            return similarity >= 0.85; // 85% giống nhau
        }

        #endregion

        #region Search Employee by Phone Number

        [HttpGet]
        public async Task<IActionResult> SearchEmployeeByPhone(string sdt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sdt))
                {
                    return Json(new { success = false, message = "Vui lòng nhập số điện thoại" });
                }

                // Remove spaces and special characters
                sdt = sdt.Trim().Replace(" ", "").Replace("-", "").Replace(".", "");

                // Validate phone number format
                if (!System.Text.RegularExpressions.Regex.IsMatch(sdt, @"^[0-9]{10,11}$"))
                {
                    return Json(new { success = false, message = "Số điện thoại không hợp lệ (phải là 10-11 chữ số)" });
                }

                // Search employee
                var employee = await _context.NhanVien
                    .Include(nv => nv.NhomQuyen)
                    .FirstOrDefaultAsync(nv => nv.SDT == sdt);

                if (employee == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy nhân viên với số điện thoại này"
                    });
                }

                return Json(new
                {
                    success = true,
                    employee = new
                    {
                        maNV = employee.MaNV,
                        tenNV = employee.TenNV,
                        sdt = employee.SDT,
                        email = employee.Email,
                        tenNhom = employee.NhomQuyen?.TenNhom,
                        caMacDinh = employee.CaMacDinh.ToString(),
                        luongCoBan = employee.LuongCoBan,
                        phuCap = employee.PhuCap,
                        trangThai = employee.TrangThai.ToString(),
                        faceIDAnh = employee.FaceIDAnh
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchEmployeeByPhone: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion

        #region Manual Attendance with Validation

        [HttpPost]
        public async Task<IActionResult> ManualCheckAttendance([FromBody] ManualCheckAttendanceRequest request)
        {
            try
            {
                var employee = await _context.NhanVien.FindAsync(request.MaNV);
                if (employee == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                if (employee.TrangThai != TrangThaiNhanVien.DangLam)
                    return Json(new { success = false, message = "Nhân viên đã nghỉ việc" });

                var today = DateTime.Today;
                var now = DateTime.Now;

                var existingRecord = await _context.ChamCong
                    .FirstOrDefaultAsync(cc => cc.MaNV == request.MaNV && cc.Ngay.Date == today);

                if (request.IsCheckIn)
                {
                    // Check-in
                    if (existingRecord != null && existingRecord.GioVao.HasValue)
                    {
                        return Json(new { success = false, message = "Đã check-in hôm nay rồi" });
                    }

                    var gioVao = now.TimeOfDay;
                    var gioChuan = new TimeSpan(7, 0, 0); // 7:00 AM
                    var gioTreMax = new TimeSpan(7, 15, 0); // 7:15 AM

                    TrangThaiChamCong trangThai;
                    if (gioVao <= gioChuan)
                        trangThai = TrangThaiChamCong.DungGio;
                    else if (gioVao <= gioTreMax)
                        trangThai = TrangThaiChamCong.DiTre;
                    else
                        trangThai = TrangThaiChamCong.Vang;

                    if (existingRecord == null)
                    {
                        existingRecord = new ChamCong
                        {
                            MaNV = request.MaNV,
                            Ngay = today,
                            GioVao = now,
                            TrangThai = trangThai,
                            XacThucBang = request.XacThucBang ?? PhuongThucXacThuc.ThuCong,
                            GhiChu = request.GhiChu
                        };
                        _context.ChamCong.Add(existingRecord);
                    }
                    else
                    {
                        existingRecord.GioVao = now;
                        existingRecord.TrangThai = trangThai;
                        existingRecord.XacThucBang = request.XacThucBang ?? PhuongThucXacThuc.ThuCong;
                        if (!string.IsNullOrEmpty(request.GhiChu))
                            existingRecord.GhiChu = request.GhiChu;
                    }
                }
                else
                {
                    // Check-out
                    if (existingRecord == null || !existingRecord.GioVao.HasValue)
                    {
                        return Json(new { success = false, message = "Chưa check-in hôm nay" });
                    }

                    if (existingRecord.GioRa.HasValue)
                    {
                        return Json(new { success = false, message = "Đã check-out rồi" });
                    }

                    existingRecord.GioRa = now;

                    // Tính số giờ làm
                    var soGio = (now - existingRecord.GioVao.Value).TotalHours;
                    existingRecord.SoGioLam = (decimal)soGio;

                    if (!string.IsNullOrEmpty(request.GhiChu))
                    {
                        existingRecord.GhiChu = string.IsNullOrEmpty(existingRecord.GhiChu)
                            ? request.GhiChu
                            : existingRecord.GhiChu + " | " + request.GhiChu;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Chấm công thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ManualCheckAttendance: {ex.Message}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetManualAttendanceModal()
        {
            try
            {
                return PartialView("~/Views/Home/Partials/NhanVien/_ManualAttendanceModal.cshtml");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetManualAttendanceModal: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        #endregion
        [HttpGet]
        public async Task<IActionResult> FormChinhSuaNhanVien(int maNV)
        {
            if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
            {
                return Forbid();
            }

            var nhanVien = await _context.NhanVien
                .Include(nv => nv.NhomQuyen)
                .FirstOrDefaultAsync(nv => nv.MaNV == maNV);

            if (nhanVien == null)
                return NotFound(new { message = "Không tìm thấy nhân viên" });

            var nhomQuyen = await _context.NhomQuyen.ToListAsync();
            ViewBag.NhomQuyen = nhomQuyen;

            return PartialView("~/Views/Home/Partials/NhanVien/_EditNhanVien.cshtml", nhanVien);
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatNhanVien([FromForm] CapNhatNhanVienRequest request)
        {
            try
            {
                if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
                {
                    return Json(new { success = false, message = "Không có quyền cập nhật" });
                }

                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                Console.WriteLine($"🔄 Updating employee #{request.MaNV}");

                // Update basic info
                nhanVien.TenNV = request.TenNV;
                nhanVien.SDT = request.SDT;
                nhanVien.Email = request.Email;
                nhanVien.MaNhom = request.MaNhom;
                nhanVien.LuongCoBan = request.LuongCoBan;
                nhanVien.PhuCap = request.PhuCap;
                nhanVien.CaMacDinh = request.CaMacDinh;
                nhanVien.TrangThai = request.TrangThai;

                // Update password if provided
                if (!string.IsNullOrEmpty(request.MatKhauMoi))
                {
                    nhanVien.MatKhau = HashPassword(request.MatKhauMoi);
                    Console.WriteLine("🔑 Password updated");
                }

                // Handle Face ID deletion
                if (request.DeleteFaceID)
                {
                    Console.WriteLine("🗑️ Deleting Face ID");

                    // Delete old image file if exists
                    if (!string.IsNullOrEmpty(nhanVien.FaceIDAnh))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, "asset", "img", nhanVien.FaceIDAnh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                                Console.WriteLine($"✅ Deleted old image: {oldImagePath}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Could not delete old image: {ex.Message}");
                            }
                        }
                    }

                    // Clear Face ID data
                    nhanVien.FaceIDAnh = null;
                    nhanVien.FaceIDHash = null;
                }

                // Handle Face ID update (new image uploaded or captured)
                if (request.FaceIDAnh != null && request.FaceIDAnh.Length > 0)
                {
                    Console.WriteLine($"📸 Processing new Face ID image: {request.FaceIDAnh.FileName}");

                    // Delete old image if exists (and not already deleted)
                    if (!request.DeleteFaceID && !string.IsNullOrEmpty(nhanVien.FaceIDAnh))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, "asset", "img", nhanVien.FaceIDAnh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                                Console.WriteLine($"✅ Deleted old image before update");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Could not delete old image: {ex.Message}");
                            }
                        }
                    }

                    // Save new image
                    var faceIdPath = await SaveImageAsync(request.FaceIDAnh);
                    var faceIdHash = await GenerateFaceHashFromFile(request.FaceIDAnh);

                    if (!string.IsNullOrEmpty(faceIdPath))
                    {
                        nhanVien.FaceIDAnh = faceIdPath;
                        nhanVien.FaceIDHash = faceIdHash;
                        Console.WriteLine($"✅ New Face ID saved: {faceIdPath}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Failed to save new Face ID image");
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Employee updated successfully");

                // Log activity
                int? maNV = HttpContext.Session.GetInt32("MaNV");
                if (maNV.HasValue)
                {
                    var lichSu = new LichSuHoatDong
                    {
                        MaNV = maNV.Value,
                        ThoiGian = DateTime.Now,
                        HanhDong = "Cập nhật nhân viên",
                        ChiTiet = $"Cập nhật thông tin {nhanVien.TenNV} (ID: {nhanVien.MaNV})"
                    };
                    _context.LichSuHoatDong.Add(lichSu);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Cập nhật nhân viên thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CapNhatNhanVien: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetEditForm(int maNV)
        {
            if (!KiemTraQuyen(new[] { "Admin", "Quản lý" }))
            {
                return Forbid();
            }

            var nhanVien = await _context.NhanVien
                .Include(nv => nv.NhomQuyen)
                .FirstOrDefaultAsync(nv => nv.MaNV == maNV);

            if (nhanVien == null)
                return NotFound(new { message = "Không tìm thấy nhân viên" });

            var nhomQuyen = await _context.NhomQuyen.ToListAsync();
            ViewBag.NhomQuyen = nhomQuyen;

            return PartialView("~/Views/Home/Partials/NhanVien/_EditNhanVien.cshtml", nhanVien);
        }
    }

    #region Request Models
    public class ManualCheckAttendanceRequest
    {
        public int MaNV { get; set; }
        public bool IsCheckIn { get; set; }
        public PhuongThucXacThuc? XacThucBang { get; set; }
        public string? GhiChu { get; set; }
    }
    public class AssignShiftRequest
    {
        public string Date { get; set; }
        public string Shift { get; set; }
        public List<int> EmployeeIds { get; set; }
        public string? Note { get; set; }
    }

    public class BulkAssignRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Shift { get; set; }
        public List<int> EmployeeIds { get; set; }
    }

    public class RemoveFromShiftRequest
    {
        public int EmployeeId { get; set; }
        public string Date { get; set; }
        public string Shift { get; set; }
    }

    public class UpdateNoteRequest
    {
        public int EmployeeId { get; set; }
        public string Date { get; set; }
        public string Shift { get; set; }
        public string Note { get; set; }
    }
    public class ThemNhanVienRequest
    {
        public string TenNV { get; set; } = "";
        public string SDT { get; set; } = "";
        public string? Email { get; set; }
        public int MaNhom { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal PhuCap { get; set; }
        public string MatKhau { get; set; } = "";
        public IFormFile? FaceIDAnh { get; set; }
    }
    public class CapNhatNhanVienRequest
    {
        public int MaNV { get; set; }
        public string TenNV { get; set; } = "";
        public string SDT { get; set; } = "";
        public string? Email { get; set; }
        public int MaNhom { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal PhuCap { get; set; }
        public CaLamViec CaMacDinh { get; set; }
        public TrangThaiNhanVien TrangThai { get; set; }
        public string? MatKhauMoi { get; set; }
        public IFormFile? FaceIDAnh { get; set; }
        public bool DeleteFaceID { get; set; } = false; // New field
    }

    public class XoaNhanVienRequest
    {
        public int MaNV { get; set; }
    }

    public class CheckAttendanceRequest
    {
        public int MaNV { get; set; }
        public bool IsCheckIn { get; set; }
        public PhuongThucXacThuc? XacThucBang { get; set; }
        public string? GhiChu { get; set; }
    }

    public class CalculateSalaryRequest
    {
        public int MaNV { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class UpdateFaceIdRequest
    {
        public int MaNV { get; set; }
        public string Hash { get; set; } = "";
        public string? ImageUrl { get; set; }
    }

    public class DeleteFaceIdRequest
    {
        public int MaNV { get; set; }
    }

    public class BulkActionRequest
    {
        public List<int> MaNVList { get; set; } = new();
    }

    public class BulkUpdateShiftRequest
    {
        public List<int> MaNVList { get; set; } = new();
        public CaLamViec CaMacDinh { get; set; }
    }

    public class ManualCheckInOutRequest
    {
        public int MaNV { get; set; }
        public DateTime Ngay { get; set; }
        public DateTime? GioVao { get; set; }
        public DateTime? GioRa { get; set; }
        public string? GhiChu { get; set; }
    }

    public class DeleteAttendanceRequest
    {
        public int ID { get; set; }
    }

    public class UpdateSalaryRequest
    {
        public int MaLuong { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal PhuCap { get; set; }
        public decimal Thuong { get; set; }
        public decimal Phat { get; set; }
    }

    public class DeleteSalaryRequest
    {
        public int MaLuong { get; set; }
    }

    #endregion
}