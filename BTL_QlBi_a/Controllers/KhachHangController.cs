using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace BTL_QlBi_a.Controllers
{
    public class KhachHangController : Controller
    {

        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public KhachHangController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Hàm Phụ
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


        #endregion

        public async Task<IActionResult> KhachHang(bool showDeleted = false)
        {
            await LoadHeaderStats();

            ViewBag.IsTrash = showDeleted;

            var query = _context.KhachHang.AsQueryable();

            if (showDeleted)
            {
                query = query.Where(k => k.HoatDong == false);
            }
            else
            {
                query = query.Where(k => k.HoatDong == true);
            }

            var danhSachKH = await query
                .OrderByDescending(k => k.TongChiTieu)
                .ToListAsync();

            return View("~/Views/Home/KhachHang.cshtml", danhSachKH);
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
            return PartialView("~/Views/Home/Partials/KhachHang/_ChiTietKhachHang.cshtml", khachHang);
        }
        public async Task<IActionResult> FormKhachHang(int maKH = 0)
        {
            if (maKH == 0)
            {
                var newCustomer = new KhachHang
                {
                    NgayDangKy = DateTime.Now
                };
                return PartialView("~/Views/Home/Partials/KhachHang/_FormKhachHang.cshtml", newCustomer);
            }

            var khachHang = await _context.KhachHang.FindAsync(maKH);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng");
            }

            return PartialView("~/Views/Home/Partials/KhachHang/_FormKhachHang.cshtml", khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật
        public async Task<IActionResult> LuuKhachHang(KhachHang model, IFormFile? fileAvatar, string? AvatarCu)
        {
            // Bỏ qua validate Hạng TV vì nó không có trên form "Thêm"
            ModelState.Remove("HangTV");
            ModelState.Remove("fileAvatar");

            if (ModelState.IsValid)
            {
                string avatarPath = AvatarCu;

                if (fileAvatar != null && fileAvatar.Length > 0)
                {
                    // Tạo tên file độc nhất để tránh trùng
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAvatar.FileName);

                    // Đường dẫn lưu file: wwwroot/asset/img/avatar_khach_hang
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "asset", "img", "avatar_khach_hang");

                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileAvatar.CopyToAsync(fileStream);
                    }

                    avatarPath = "/asset/img/avatar_khach_hang/" + fileName;
                }

                if (model.MaKH == 0) // THÊM MỚI
                {
                    // Set giá trị mặc định cho khách mới
                    model.HangTV = HangThanhVien.Dong; // Mặc định là Đồng
                    model.NgayDangKy = DateTime.Now;
                    model.DiemTichLuy = 0;
                    model.TongChiTieu = 0;

                    model.Avatar = avatarPath;
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
                    existingCustomer.Avatar = avatarPath;
                    // Không cập nhật Hạng, Điểm, Tổng chi tiêu ở đây

                    _context.KhachHang.Update(existingCustomer);
                }

                await _context.SaveChangesAsync();

                // Trả về success
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

        }

        // Nếu model không hợp lệ

        [HttpPost]
public async Task<IActionResult> XoaKhachHang(int maKH)
{
    try
    {
        var kh = await _context.KhachHang.FindAsync(maKH);
        if (kh == null)
        {
            return Json(new { success = false, message = "Khong tim` thay khach hang`" });
        }
        kh.HoatDong = false;

        _context.KhachHang.Update(kh);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi: " + ex.Message });
    }
}

[HttpPost]
public async Task<IActionResult> KhoiPhucKhachHang(int maKH)
{
    try
    {
        var kh = await _context.KhachHang.FindAsync(maKH);

        if (kh == null)
            return Json(new { success = false, message = "Khong tim` thay" });

        kh.HoatDong = true;

        _context.KhachHang.Update(kh);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}


public async Task<IActionResult> ExportKhachHangToExcel(string rank = "all", string search = null)
{
    var query = _context.KhachHang.AsQueryable();

    if (rank != null)
    {
        if (Enum.TryParse<HangThanhVien>(rank, out var rankEnum))
        {
            query = query.Where(k => k.HangTV == rankEnum);
        }
    }

    if (!string.IsNullOrEmpty(search))
    {
        var searchLower = search.ToLower();
        query = query.Where(k => k.TenKH.ToLower().Contains(searchLower) ||
                                k.SDT.Contains(searchLower));
    }
    var listKhachHang = await query.OrderByDescending(k => k.TongChiTieu).ToArrayAsync();

    // Khai bao' ban? quyen`
    ExcelPackage.License.SetNonCommercialPersonal("Duy Binh");

    using (var package = new ExcelPackage())
    {
        var worksheet = package.Workbook.Worksheets.Add("KhachHang");

        // Header
        worksheet.Cells[1, 1].Value = "Mã KH";
        worksheet.Cells[1, 2].Value = "Họ Tên";
        worksheet.Cells[1, 3].Value = "SĐT";
        worksheet.Cells[1, 4].Value = "Email";
        worksheet.Cells[1, 5].Value = "Hạng TV";
        worksheet.Cells[1, 6].Value = "Điểm tích lũy";
        worksheet.Cells[1, 7].Value = "Tổng chi tiêu";

        // Style Header
        using (var range = worksheet.Cells["A1:G1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(40, 167, 69)); // Màu xanh lá
            range.Style.Font.Color.SetColor(Color.White);
        }

        //Data
        for (int i = 0; i < listKhachHang.Count(); i++)
        {
            var kh = listKhachHang[i];
            int row = i + 2;

            worksheet.Cells[row, 1].Value = kh.MaKH;
            worksheet.Cells[row, 2].Value = kh.TenKH;
            worksheet.Cells[row, 3].Value = kh.SDT;
            worksheet.Cells[row, 4].Value = kh.Email;
            worksheet.Cells[row, 5].Value = kh.HangTV.ToString();
            worksheet.Cells[row, 6].Value = kh.DiemTichLuy;
            worksheet.Cells[row, 7].Value = kh.TongChiTieu;

            // Format tien` 
            worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0";
        }

        worksheet.Cells.AutoFitColumns();

        var fileBytes = package.GetAsByteArray();
        string fileName = $"DanhSachKhachHang_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}

    }
}
