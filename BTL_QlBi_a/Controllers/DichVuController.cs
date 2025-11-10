using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using BTL_QlBi_a.Models.ViewModels;
using Microsoft.AspNetCore.Antiforgery;
namespace BTL_QlBi_a.Controllers
{
    public class DichVuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IAntiforgery _antiforgery;
        public DichVuController(ApplicationDbContext context, IWebHostEnvironment environment,IAntiforgery antiforgery)
        {
            _context = context;
            _environment = environment;
            _antiforgery = antiforgery;
        }

        // GET: DichVu/Index
        public async Task<IActionResult> Index()
        {
            var dichVus = await _context.DichVus
                .Include(d => d.MatHang)
                .OrderBy(d => d.Loai)
                .ThenBy(d => d.TenDV)
                .ToListAsync();

            // Load danh sách mặt hàng cho dropdown trong modal
            ViewBag.MatHangs = await _context.MatHang
                .Where(m => m.SoLuongTon > 0)
                .ToListAsync();

            return View("~/Views/Home/DichVu.cshtml", dichVus);
        }

        // POST: DichVu/ThemDichVu
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ThemDichVu([FromForm] DichVuViewModel model)
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(HttpContext);
                Console.WriteLine("=== ThemDichVu called ===");
                Console.WriteLine($"TenDV: {model.TenDV}");
                Console.WriteLine($"Loai: {model.Loai}");
                Console.WriteLine($"Gia: {model.Gia}");

                if (string.IsNullOrWhiteSpace(model.TenDV))
                {
                    return Json(new { success = false, message = "Tên dịch vụ không được để trống!" });
                }

                // Xử lý upload ảnh
                string imagePath = null;
                if (model.HinhAnhFile != null && model.HinhAnhFile.Length > 0)
                {
                    Console.WriteLine($"Uploading image: {model.HinhAnhFile.FileName}");
                    imagePath = await SaveImageAsync(model.HinhAnhFile);
                    if (imagePath == null)
                    {
                        return Json(new { success = false, message = "Không thể lưu hình ảnh!" });
                    }
                }

                var dichVu = new DichVu
                {
                    TenDV = model.TenDV,
                    Loai = model.Loai,
                    Gia = model.Gia,
                    DonVi = model.DonVi ?? "phần",
                    MaHang = model.MaHang > 0 ? model.MaHang : null,
                    TrangThai = model.TrangThai,
                    MoTa = model.MoTa,
                    HinhAnh = imagePath,
                    NgayTao = DateTime.Now
                };

                _context.DichVus.Add(dichVu);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Added service: {dichVu.MaDV}");
                return Json(new { success = true, message = "Thêm dịch vụ thành công!" });
            }
            catch (AntiforgeryValidationException ex)
            {
                Console.WriteLine($"❌ Error in ThemDichVu (Antiforgery): {ex.Message}");
                return Json(new { success = false, message = "Lỗi bảo mật, không thể xác thực request!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ThemDichVu: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: DichVu/SuaDichVu
        [HttpPost]

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SuaDichVu([FromForm] DichVuViewModel model)
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(HttpContext);
                Console.WriteLine("=== SuaDichVu called ===");
                Console.WriteLine($"MaDV: {model.MaDV}");    

                if (!model.MaDV.HasValue || model.MaDV <= 0)
                {
                    return Json(new { success = false, message = "Mã dịch vụ không hợp lệ!" });
                }

                var dichVu = await _context.DichVus.FindAsync(model.MaDV.Value);
                if (dichVu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ!" });
                }

                // Xử lý upload ảnh mới
                if (model.HinhAnhFile != null && model.HinhAnhFile.Length > 0)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(dichVu.HinhAnh))
                    {
                        DeleteImage(dichVu.HinhAnh);
                    }

                    dichVu.HinhAnh = await SaveImageAsync(model.HinhAnhFile);
                }

                // Cập nhật thông tin
                dichVu.TenDV = model.TenDV;
                dichVu.Loai = model.Loai;
                dichVu.Gia = model.Gia;
                dichVu.DonVi = model.DonVi ?? "phần";
                dichVu.MaHang = model.MaHang > 0 ? model.MaHang : null;
                dichVu.TrangThai = model.TrangThai;
                dichVu.MoTa = model.MoTa;

                _context.DichVus.Update(dichVu);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Updated service: {dichVu.MaDV}");
                return Json(new { success = true, message = "Cập nhật dịch vụ thành công!" });
            }
            catch (AntiforgeryValidationException ex)
            {
                Console.WriteLine($"❌ Error in SuaDichVu (Antiforgery): {ex.Message}");
                return Json(new { success = false, message = "Lỗi bảo mật, không thể xác thực request!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SuaDichVu: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: DichVu/XoaDichVu
        [HttpPost]
        public async Task<IActionResult> XoaDichVu([FromBody] XoaDichVuMenuRequest request)
        {
            try
            {
                if (request == null || request.MaDV <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                var dichVu = await _context.DichVus.FindAsync(request.MaDV);
                if (dichVu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ!" });
                }

                // Kiểm tra xem dịch vụ có đang được sử dụng không
                var daDuocSuDung = await _context.ChiTietHoaDon
                    .AnyAsync(ct => ct.MaDV == request.MaDV);

                if (daDuocSuDung)
                {
                    return Json(new { success = false, message = "Không thể xóa! Dịch vụ đã được sử dụng trong hóa đơn." });
                }

                // Xóa ảnh nếu có
                if (!string.IsNullOrEmpty(dichVu.HinhAnh))
                {
                    DeleteImage(dichVu.HinhAnh);
                }

                _context.DichVus.Remove(dichVu);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa dịch vụ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: DichVu/LayThongTin
        [HttpGet]
        public async Task<IActionResult> LayThongTin(int maDV)
        {
            try
            {
                var dichVu = await _context.DichVus
                    .Include(d => d.MatHang)
                    .FirstOrDefaultAsync(d => d.MaDV == maDV);

                if (dichVu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ!" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        dichVu.MaDV,
                        dichVu.TenDV,
                        Loai = ((int)dichVu.Loai).ToString(),
                        dichVu.Gia,
                        dichVu.DonVi,
                        dichVu.MaHang,
                        TrangThai = ((int)dichVu.TrangThai).ToString(),
                        dichVu.MoTa,
                        dichVu.HinhAnh
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Private methods
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"service_{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "asset", "img");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                return null;
            }
        }

        private void DeleteImage(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath))
                {
                    // --- SỬA ĐỔI ---
                    // imagePath giờ chỉ là tên file, trỏ trực tiếp
                    var fullPath = Path.Combine(_environment.WebRootPath, "asset", "img", imagePath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }
    }
}