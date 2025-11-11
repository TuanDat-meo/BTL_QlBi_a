using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace BTL_QlBi_a.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhanVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Helper Methods

        /// <summary>
        /// Kiểm tra quyền quản lý (Admin hoặc Quản lý)
        /// </summary>
        private bool CanManage()
        {
            var role = HttpContext.Session.GetString("ChucVu");
            return role == "Admin" || role == "Quản lý";
        }

        /// <summary>
        /// Kiểm tra quyền Admin
        /// </summary>
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("ChucVu");
            return role == "Admin";
        }

        /// <summary>
        /// Hash mật khẩu bằng SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Ghi log hoạt động
        /// </summary>
        private async Task LogActivity(string action, string? details = null)
        {
            var maNV = HttpContext.Session.GetInt32("MaNV");
            if (maNV.HasValue)
            {
                _context.LichSuHoatDong.Add(new LichSuHoatDong
                {
                    MaNV = maNV.Value,
                    HanhDong = action,
                    ChiTiet = details,
                    ThoiGian = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Employee CRUD

        /// <summary>
        /// GET: NhanVien/GetDetail/5
        /// Lấy chi tiết nhân viên
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var nhanVien = await _context.NhanVien
                    .Include(n => n.NhomQuyen)
                    .FirstOrDefaultAsync(n => n.MaNV == id);

                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                // Calculate salary data
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var salary = await _context.BangLuong
                    .Where(bl => bl.MaNV == id && bl.Thang == currentMonth && bl.Nam == currentYear)
                    .FirstOrDefaultAsync();

                ViewBag.TotalSalary = salary?.TongLuong ?? 0;
                ViewBag.Bonus = salary?.Thuong ?? 0;
                ViewBag.Penalty = salary?.Phat ?? 0;
                ViewBag.TotalHours = salary?.TongGio ?? 0;

                // Calculate work stats
                var startOfMonth = new DateTime(currentYear, currentMonth, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var attendanceRecords = await _context.ChamCong
                    .Where(c => c.MaNV == id && c.Ngay >= startOfMonth && c.Ngay <= endOfMonth)
                    .ToListAsync();

                ViewBag.WorkDays = attendanceRecords.Count(a => a.GioVao != null);
                ViewBag.LateDays = attendanceRecords.Count(a => a.TrangThai == TrangThaiChamCong.DiTre);
                ViewBag.AbsentDays = attendanceRecords.Count(a => a.TrangThai == TrangThaiChamCong.Vang);

                ViewBag.CurrentRole = HttpContext.Session.GetString("ChucVu");

                return PartialView("~/Views/Home/Partials/NhanVien/_NhanVienDetail.cshtml", nhanVien);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: NhanVien/GetEditForm/5
        /// Lấy form chỉnh sửa nhân viên
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEditForm(int id)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var nhanVien = await _context.NhanVien
                    .Include(n => n.NhomQuyen)
                    .FirstOrDefaultAsync(n => n.MaNV == id);

                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                ViewBag.DanhSachNhom = await _context.NhomQuyen.ToListAsync();

                return PartialView("~/Views/Home/Partials/NhanVien/_EditEmployeeForm.cshtml", nhanVien);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: NhanVien/Create
        /// Thêm nhân viên mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NhanVienCreateRequest model)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(model.TenNV))
                    return BadRequest(new { success = false, message = "Tên nhân viên không được để trống" });

                if (string.IsNullOrWhiteSpace(model.MatKhau) || model.MatKhau.Length < 6)
                    return BadRequest(new { success = false, message = "Mật khẩu phải có ít nhất 6 ký tự" });

                // Check if phone number exists
                if (!string.IsNullOrWhiteSpace(model.SDT))
                {
                    var existingPhone = await _context.NhanVien
                        .AnyAsync(n => n.SDT == model.SDT && n.TrangThai == TrangThaiNhanVien.DangLam);

                    if (existingPhone)
                        return BadRequest(new { success = false, message = "Số điện thoại đã được sử dụng" });
                }

                // Create new employee
                var nhanVien = new NhanVien
                {
                    TenNV = model.TenNV.Trim(),
                    SDT = model.SDT?.Trim(),
                    MaNhom = model.MaNhom,
                    CaMacDinh = model.CaMacDinh,
                    LuongCoBan = model.LuongCoBan,
                    PhuCap = model.PhuCap,
                    MatKhau = HashPassword(model.MatKhau),
                    TrangThai = TrangThaiNhanVien.DangLam
                };

                _context.NhanVien.Add(nhanVien);
                await _context.SaveChangesAsync();

                await LogActivity($"Thêm nhân viên: {nhanVien.TenNV}", $"Mã NV: {nhanVien.MaNV}");

                return Ok(new { success = true, message = "Thêm nhân viên thành công", maNV = nhanVien.MaNV });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: NhanVien/Update
        /// Cập nhật thông tin nhân viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] NhanVienUpdateRequest model)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var nhanVien = await _context.NhanVien.FindAsync(model.MaNV);
                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                // Check if phone number exists (exclude current employee)
                if (!string.IsNullOrWhiteSpace(model.SDT))
                {
                    var existingPhone = await _context.NhanVien
                        .AnyAsync(n => n.SDT == model.SDT && n.MaNV != model.MaNV && n.TrangThai == TrangThaiNhanVien.DangLam);

                    if (existingPhone)
                        return BadRequest(new { success = false, message = "Số điện thoại đã được sử dụng" });
                }

                // Update fields
                nhanVien.TenNV = model.TenNV.Trim();
                nhanVien.SDT = model.SDT?.Trim();
                nhanVien.MaNhom = model.MaNhom;
                nhanVien.CaMacDinh = model.CaMacDinh;
                nhanVien.LuongCoBan = model.LuongCoBan;
                nhanVien.PhuCap = model.PhuCap;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(model.MatKhau) && model.MatKhau.Length >= 6)
                {
                    nhanVien.MatKhau = HashPassword(model.MatKhau);
                }

                await _context.SaveChangesAsync();

                await LogActivity($"Cập nhật nhân viên: {nhanVien.TenNV}", $"Mã NV: {nhanVien.MaNV}");

                return Ok(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: NhanVien/Deactivate/5
        /// Cho nhân viên nghỉ việc
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var nhanVien = await _context.NhanVien.FindAsync(id);
                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                nhanVien.TrangThai = TrangThaiNhanVien.Nghi;
                await _context.SaveChangesAsync();

                await LogActivity($"Cho nghỉ việc: {nhanVien.TenNV}", $"Mã NV: {nhanVien.MaNV}");

                return Ok(new { success = true, message = "Đã chuyển sang trạng thái nghỉ việc" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: NhanVien/Activate/5
        /// Kích hoạt lại nhân viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var nhanVien = await _context.NhanVien.FindAsync(id);
                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                nhanVien.TrangThai = TrangThaiNhanVien.DangLam;
                await _context.SaveChangesAsync();

                await LogActivity($"Cho làm lại: {nhanVien.TenNV}", $"Mã NV: {nhanVien.MaNV}");

                return Ok(new { success = true, message = "Đã chuyển sang trạng thái đang làm" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        #endregion

        #region Attendance Management

        /// <summary>
        /// POST: NhanVien/CheckAttendance
        /// Chấm công
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckAttendance([FromBody] ChamCongRequest model)
        {
            try
            {
                var today = DateTime.Now.Date;
                var existing = await _context.ChamCong
                    .FirstOrDefaultAsync(c => c.MaNV == model.MaNV && c.Ngay.Date == today);

                if (existing != null)
                {
                    // Update checkout time
                    if (model.GioRa != null)
                    {
                        existing.GioRa = model.GioRa;
                        existing.GhiChu = model.GhiChu;
                        existing.XacThucBang = model.XacThucBang;

                        // Determine if leaving early
                        var standardEndTime = new TimeSpan(15, 0, 0); // 3:00 PM
                        if (model.GioRa.Value.TimeOfDay < standardEndTime)
                        {
                            existing.TrangThai = TrangThaiChamCong.VeSom;
                        }
                    }
                }
                else
                {
                    // Create new attendance record
                    var chamCong = new ChamCong
                    {
                        MaNV = model.MaNV,
                        Ngay = today,
                        GioVao = model.GioVao ?? DateTime.Now,
                        GioRa = model.GioRa,
                        GhiChu = model.GhiChu,
                        XacThucBang = model.XacThucBang,
                        TrangThai = TrangThaiChamCong.DungGio
                    };

                    var standardTime = new TimeSpan(8, 0, 0); 
                    if (chamCong.GioVao.HasValue && chamCong.GioVao.Value.TimeOfDay > standardTime)
                    {
                        chamCong.TrangThai = TrangThaiChamCong.DiTre;
                    }

                    _context.ChamCong.Add(chamCong);
                }

                await _context.SaveChangesAsync();

                var action = existing != null ? "Chấm công ra" : "Chấm công vào";
                await LogActivity($"{action}: NV{model.MaNV:D4}", $"Phương thức: {model.XacThucBang}");

                return Ok(new { success = true, message = "Chấm công thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: NhanVien/GetAttendanceHistory/5
        /// Lấy lịch sử chấm công
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAttendanceHistory(int id, int? month = null, int? year = null)
        {
            try
            {
                month ??= DateTime.Now.Month;
                year ??= DateTime.Now.Year;

                var startDate = new DateTime(year.Value, month.Value, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var records = await _context.ChamCong
                    .Where(c => c.MaNV == id && c.Ngay >= startDate && c.Ngay <= endDate)
                    .OrderByDescending(c => c.Ngay)
                    .Select(c => new
                    {
                        c.Ngay,
                        c.GioVao,
                        c.GioRa,
                        c.SoGioLam,
                        TrangThai = c.TrangThai.ToString(),
                        c.GhiChu,
                        c.XacThucBang
                    })
                    .ToListAsync();

                return Json(records);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: NhanVien/ExportAttendanceReport
        /// Xuất báo cáo chấm công
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportAttendanceReport(string month)
        {
            try
            {
                var parts = month.Split('/');
                if (parts.Length != 2)
                    return BadRequest("Định dạng tháng không hợp lệ");

                int monthNum = int.Parse(parts[0]);
                int year = int.Parse(parts[1]);

                var startDate = new DateTime(year, monthNum, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var records = await _context.ChamCong
                    .Include(c => c.NhanVien)
                    .Where(c => c.Ngay >= startDate && c.Ngay <= endDate)
                    .OrderBy(c => c.Ngay)
                    .ThenBy(c => c.MaNV)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Mã NV,Tên NV,Ngày,Giờ vào,Giờ ra,Số giờ,Trạng thái,Phương thức,Ghi chú");

                foreach (var record in records)
                {
                    var gioVao = record.GioVao?.ToString("HH:mm") ?? "--:--";
                    var gioRa = record.GioRa?.ToString("HH:mm") ?? "--:--";
                    var soGio = record.SoGioLam?.ToString("0.00") ?? "0";

                    csv.AppendLine($"NV{record.MaNV:D4}," +
                        $"\"{record.NhanVien.TenNV}\"," +
                        $"{record.Ngay:dd/MM/yyyy}," +
                        $"{gioVao}," +
                        $"{gioRa}," +
                        $"{soGio}," +
                        $"{record.TrangThai}," +
                        $"{record.XacThucBang}," +
                        $"\"{record.GhiChu}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

                await LogActivity("Xuất báo cáo chấm công", $"Tháng {month}");

                return File(bytes, "text/csv", $"BaoCaoChamCong_{month.Replace("/", "_")}.csv");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Schedule Management

        /// <summary>
        /// GET: NhanVien/GetSchedule/5
        /// Lấy lịch làm việc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSchedule(int id, int year, int month)
        {
            try
            {
                // TODO: Implement schedule table in database
                // For now, return default schedule based on CaMacDinh
                var nhanVien = await _context.NhanVien.FindAsync(id);
                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                var schedule = new Dictionary<string, string>();

                // Generate schedule for the month
                var startDate = new DateTime(year, month, 1);
                var daysInMonth = DateTime.DaysInMonth(year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    var dateStr = date.ToString("yyyy-MM-dd");

                    // Skip Sundays
                    if (date.DayOfWeek != DayOfWeek.Sunday)
                    {
                        schedule[dateStr] = nhanVien.CaMacDinh.ToString();
                    }
                }

                return Json(schedule);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: NhanVien/SaveSchedule
        /// Lưu lịch làm việc
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveSchedule([FromBody] ScheduleRequest request)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                // TODO: Save to schedule table
                // For now, just log the action

                await LogActivity($"Cập nhật lịch làm việc: NV{request.MaNV:D4}",
                    $"Số ngày: {request.Schedule.Count}");

                return Ok(new { success = true, message = "Lưu lịch làm việc thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Biometric Authentication

        /// <summary>
        /// POST: NhanVien/UpdateFaceId
        /// Cập nhật Face ID
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateFaceId([FromBody] FaceIdRequest request)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                // Check if hash already exists for another employee
                if (!string.IsNullOrWhiteSpace(request.Hash))
                {
                    var existingHash = await _context.NhanVien
                        .AnyAsync(n => n.FaceIDHash == request.Hash && n.MaNV != request.MaNV);

                    if (existingHash)
                        return BadRequest(new { success = false, message = "Face ID này đã được đăng ký cho nhân viên khác" });
                }

                nhanVien.FaceIDHash = request.Hash;
                nhanVien.FaceIDAnh = request.ImageUrl;

                await _context.SaveChangesAsync();

                await LogActivity($"Cập nhật Face ID: {nhanVien.TenNV}", $"Mã NV: {nhanVien.MaNV}");

                return Ok(new { success = true, message = "Cập nhật Face ID thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: NhanVien/UpdateFingerprint
        /// Cập nhật vân tay
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateFingerprint([FromBody] FingerprintRequest request)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                // Check if fingerprint hash already exists
                if (!string.IsNullOrWhiteSpace(request.FingerprintHash))
                {
                    var existingHash = await _context.NhanVien
                        .AnyAsync(n => n.MaVanTay == request.FingerprintHash && n.MaNV != request.MaNV);

                    if (existingHash)
                        return BadRequest(new { success = false, message = "Vân tay này đã được đăng ký cho nhân viên khác" });
                }

                nhanVien.MaVanTay = request.FingerprintHash;

                await _context.SaveChangesAsync();

                await LogActivity($"Cập nhật vân tay: {nhanVien.TenNV}", $"Mã NV: {nhanVien.MaNV}");

                return Ok(new { success = true, message = "Cập nhật vân tay thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: NhanVien/VerifyFingerprint
        /// Xác thực vân tay
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VerifyFingerprint([FromBody] VerifyFingerprintRequest request)
        {
            try
            {
                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null || string.IsNullOrEmpty(nhanVien.MaVanTay))
                    return Ok(new { success = false, message = "Chưa đăng ký vân tay" });

                // In production, compare with actual fingerprint device
                // For demo, simulate verification
                bool isMatch = !string.IsNullOrEmpty(nhanVien.MaVanTay);

                if (isMatch)
                {
                    await LogActivity($"Xác thực vân tay thành công: {nhanVien.TenNV}", $"Mã NV: {nhanVien.MaNV}");
                }

                return Ok(new
                {
                    success = isMatch,
                    message = isMatch ? "Xác thực thành công" : "Vân tay không khớp"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Salary Management

        /// <summary>
        /// GET: NhanVien/GetSalaryHistory/5
        /// Lấy lịch sử lương
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSalaryHistory(int id, int? year = null)
        {
            try
            {
                year ??= DateTime.Now.Year;

                var salaries = await _context.BangLuong
                    .Where(bl => bl.MaNV == id && bl.Nam == year)
                    .OrderByDescending(bl => bl.Thang)
                    .Select(bl => new
                    {
                        bl.MaLuong,
                        bl.Thang,
                        bl.Nam,
                        bl.LuongCoBan,
                        bl.PhuCap,
                        bl.Thuong,
                        bl.Phat,
                        bl.TongGio,
                        bl.TongLuong,
                        bl.NgayTinh
                    })
                    .ToListAsync();

                return Json(salaries);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: NhanVien/CalculateSalary
        /// Tính lương tự động
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CalculateSalary([FromBody] CalculateSalaryRequest request)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var nhanVien = await _context.NhanVien.FindAsync(request.MaNV);
                if (nhanVien == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

                // Get attendance records for the month
                var startDate = new DateTime(request.Year, request.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var attendances = await _context.ChamCong
                    .Where(c => c.MaNV == request.MaNV && c.Ngay >= startDate && c.Ngay <= endDate)
                    .ToListAsync();

                // Calculate total hours
                decimal totalHours = attendances.Sum(a => a.SoGioLam ?? 0);

                // Calculate penalties
                int lateDays = attendances.Count(a => a.TrangThai == TrangThaiChamCong.DiTre);
                int absentDays = attendances.Count(a => a.TrangThai == TrangThaiChamCong.Vang);
                decimal penalty = (lateDays * 50000) + (absentDays * 200000); // 50k per late, 200k per absent

                // Calculate bonus (example: 100k per 40 hours worked)
                decimal bonus = Math.Floor(totalHours / 40) * 100000;

                // Calculate total salary
                decimal totalSalary = nhanVien.LuongCoBan + nhanVien.PhuCap + bonus - penalty;

                // Check if salary record exists
                var salary = await _context.BangLuong
                    .FirstOrDefaultAsync(bl => bl.MaNV == request.MaNV &&
                                              bl.Thang == request.Month &&
                                              bl.Nam == request.Year);

                if (salary == null)
                {
                    salary = new BangLuong
                    {
                        MaNV = request.MaNV,
                        Thang = request.Month,
                        Nam = request.Year,
                        LuongCoBan = nhanVien.LuongCoBan,
                        PhuCap = nhanVien.PhuCap,
                        Thuong = bonus,
                        Phat = penalty,
                        TongGio = totalHours,
                        TongLuong = totalSalary,
                        NgayTinh = DateTime.Now
                    };
                    _context.BangLuong.Add(salary);
                }
                else
                {
                    salary.LuongCoBan = nhanVien.LuongCoBan;
                    salary.PhuCap = nhanVien.PhuCap;
                    salary.Thuong = bonus;
                    salary.Phat = penalty;
                    salary.TongGio = totalHours;
                    salary.TongLuong = totalSalary;
                    salary.NgayTinh = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                await LogActivity($"Tính lương tháng {request.Month}/{request.Year}: {nhanVien.TenNV}",
                    $"Tổng lương: {totalSalary:N0}đ");

                return Ok(new
                {
                    success = true,
                    message = "Tính lương thành công",
                    data = new
                    {
                        salary.MaLuong,
                        salary.LuongCoBan,
                        salary.PhuCap,
                        salary.Thuong,
                        salary.Phat,
                        salary.TongGio,
                        salary.TongLuong,
                        WorkDays = attendances.Count,
                        LateDays = lateDays,
                        AbsentDays = absentDays
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: NhanVien/GetSalaryReport
        /// Xuất báo cáo lương
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSalaryReport(int month, int year)
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var salaries = await _context.BangLuong
                    .Include(bl => bl.NhanVien)
                    .ThenInclude(nv => nv.NhomQuyen)
                    .Where(bl => bl.Thang == month && bl.Nam == year)
                    .OrderBy(bl => bl.MaNV)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Mã NV,Tên NV,Chức vụ,Lương cơ bản,Phụ cấp,Thưởng,Phạt,Tổng giờ,Tổng lương");

                foreach (var salary in salaries)
                {
                    csv.AppendLine($"NV{salary.MaNV:D4}," +
                        $"\"{salary.NhanVien.TenNV}\"," +
                        $"\"{salary.NhanVien.NhomQuyen.TenNhom}\"," +
                        $"{salary.LuongCoBan}," +
                        $"{salary.PhuCap}," +
                        $"{salary.Thuong}," +
                        $"{salary.Phat}," +
                        $"{salary.TongGio}," +
                        $"{salary.TongLuong}");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

                await LogActivity("Xuất báo cáo lương", $"Tháng {month}/{year}");

                return File(bytes, "text/csv", $"BaoCaoLuong_{month}_{year}.csv");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Permission Management

        /// <summary>
        /// GET: NhanVien/GetPermissions
        /// Lấy danh sách phân quyền
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            if (!CanManage())
                return Forbid();

            try
            {
                var roles = await _context.NhomQuyen
                    .Include(n => n.PhanQuyens)
                    .ThenInclude(p => p.ChucNang)
                    .Select(n => new
                    {
                        n.MaNhom,
                        n.TenNhom,
                        PhanQuyens = n.PhanQuyens.Select(p => new
                        {
                            p.MaCN,
                            p.ChucNang.TenCN
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: NhanVien/UpdatePermissions
        /// Cập nhật phân quyền
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdatePermissions([FromBody] UpdatePermissionsRequest request)
        {
            if (!IsAdmin())
                return Forbid();

            try
            {
                // Don't allow changing Admin permissions
                var role = await _context.NhomQuyen.FindAsync(request.MaNhom);
                if (role == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhóm quyền" });

                if (role.TenNhom == "Admin")
                    return BadRequest(new { success = false, message = "Không thể thay đổi quyền của Admin" });

                // Remove existing permissions
                var existing = await _context.PhanQuyen
                    .Where(p => p.MaNhom == request.MaNhom)
                    .ToListAsync();
                _context.PhanQuyen.RemoveRange(existing);

                // Add new permissions
                foreach (var maCN in request.Permissions)
                {
                    // Check if permission exists
                    var chucNang = await _context.ChucNang.FindAsync(maCN);
                    if (chucNang != null)
                    {
                        _context.PhanQuyen.Add(new PhanQuyen
                        {
                            MaNhom = request.MaNhom,
                            MaCN = maCN
                        });
                    }
                }

                await _context.SaveChangesAsync();

                await LogActivity($"Cập nhật phân quyền: {role.TenNhom}",
                    $"Số quyền: {request.Permissions.Count}");

                return Ok(new { success = true, message = "Cập nhật phân quyền thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: NhanVien/GetAllFunctions
        /// Lấy danh sách tất cả chức năng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllFunctions()
        {
            if (!IsAdmin())
                return Forbid();

            try
            {
                var functions = await _context.ChucNang
                    .Select(cn => new
                    {
                        cn.MaCN,
                        cn.TenCN,
                        cn.MoTa
                    })
                    .ToListAsync();

                return Json(functions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// GET: NhanVien/GetStatistics
        /// Lấy thống kê nhân viên
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = new
                {
                    TotalEmployees = await _context.NhanVien.CountAsync(n => n.TrangThai == TrangThaiNhanVien.DangLam),
                    InactiveEmployees = await _context.NhanVien.CountAsync(n => n.TrangThai == TrangThaiNhanVien.Nghi),
                    TodayAttendance = await _context.ChamCong
                        .CountAsync(c => c.Ngay.Date == DateTime.Today && c.GioVao != null),
                    LateToday = await _context.ChamCong
                        .CountAsync(c => c.Ngay.Date == DateTime.Today && c.TrangThai == TrangThaiChamCong.DiTre),
                    ByRole = await _context.NhanVien
                        .Where(n => n.TrangThai == TrangThaiNhanVien.DangLam)
                        .GroupBy(n => n.NhomQuyen.TenNhom)
                        .Select(g => new { Role = g.Key, Count = g.Count() })
                        .ToListAsync()
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: NhanVien/GetAttendanceSummary
        /// Lấy tổng hợp chấm công
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAttendanceSummary(int id, int month, int year)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var attendances = await _context.ChamCong
                    .Where(c => c.MaNV == id && c.Ngay >= startDate && c.Ngay <= endDate)
                    .ToListAsync();

                var summary = new
                {
                    TotalDays = attendances.Count,
                    WorkedDays = attendances.Count(a => a.GioVao != null),
                    LateDays = attendances.Count(a => a.TrangThai == TrangThaiChamCong.DiTre),
                    AbsentDays = attendances.Count(a => a.TrangThai == TrangThaiChamCong.Vang),
                    EarlyLeaveDays = attendances.Count(a => a.TrangThai == TrangThaiChamCong.VeSom),
                    TotalHours = attendances.Sum(a => a.SoGioLam ?? 0),
                    AverageHours = attendances.Any() ? attendances.Average(a => a.SoGioLam ?? 0) : 0
                };

                return Json(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Search and Filter

        /// <summary>
        /// POST: NhanVien/Search
        /// Tìm kiếm nhân viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            try
            {
                var query = _context.NhanVien
                    .Include(n => n.NhomQuyen)
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (Enum.TryParse<TrangThaiNhanVien>(request.Status, out var status))
                    {
                        query = query.Where(n => n.TrangThai == status);
                    }
                }

                // Filter by role
                if (request.MaNhom.HasValue)
                {
                    query = query.Where(n => n.MaNhom == request.MaNhom.Value);
                }

                // Search by name or phone
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    var keyword = request.Keyword.ToLower();
                    query = query.Where(n =>
                        n.TenNV.ToLower().Contains(keyword) ||
                        (n.SDT != null && n.SDT.Contains(keyword)));
                }

                var results = await query
                    .OrderBy(n => n.TrangThai)
                    .ThenBy(n => n.TenNV)
                    .Select(n => new
                    {
                        n.MaNV,
                        n.TenNV,
                        n.SDT,
                        Role = n.NhomQuyen.TenNhom,
                        n.CaMacDinh,
                        n.LuongCoBan,
                        n.PhuCap,
                        n.TrangThai,
                        n.FaceIDAnh,
                        HasFingerprint = !string.IsNullOrEmpty(n.MaVanTay),
                        HasFaceID = !string.IsNullOrEmpty(n.FaceIDHash)
                    })
                    .ToListAsync();

                return Json(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }

    #region Request Models

    public class NhanVienCreateRequest
    {
        public string TenNV { get; set; }
        public string? SDT { get; set; }
        public int MaNhom { get; set; }
        public CaLamViec CaMacDinh { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal PhuCap { get; set; }
        public string MatKhau { get; set; }
    }

    public class NhanVienUpdateRequest
    {
        public int MaNV { get; set; }
        public string TenNV { get; set; }
        public string? SDT { get; set; }
        public int MaNhom { get; set; }
        public CaLamViec CaMacDinh { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal PhuCap { get; set; }
        public string? MatKhau { get; set; }
    }

    public class ChamCongRequest
    {
        public int MaNV { get; set; }
        public DateTime? GioVao { get; set; }
        public DateTime? GioRa { get; set; }
        public string? GhiChu { get; set; }
        public PhuongThucXacThuc XacThucBang { get; set; } = PhuongThucXacThuc.ThuCong;
    }

    public class ScheduleRequest
    {
        public int MaNV { get; set; }
        public Dictionary<string, string> Schedule { get; set; }
    }

    public class FaceIdRequest
    {
        public int MaNV { get; set; }
        public string Hash { get; set; }
        public string ImageUrl { get; set; }
    }

    public class FingerprintRequest
    {
        public int MaNV { get; set; }
        public string FingerprintHash { get; set; }
    }

    public class VerifyFingerprintRequest
    {
        public int MaNV { get; set; }
        public string ScannedHash { get; set; }
    }

    public class CalculateSalaryRequest
    {
        public int MaNV { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class UpdatePermissionsRequest
    {
        public int MaNhom { get; set; }
        public List<string> Permissions { get; set; }
    }

    public class SearchRequest
    {
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public int? MaNhom { get; set; }
    }

    #endregion
}