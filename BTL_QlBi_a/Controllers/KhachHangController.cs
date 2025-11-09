using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QlBi_a.Controllers
{
    public class KhachHangController : Controller
    {

        private readonly ApplicationDbContext _context;

        public KhachHangController(ApplicationDbContext context)
        {
            _context = context;
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

        public async Task<IActionResult> KhachHang()
        {
            await LoadHeaderStats();
            var danhSachKH = await _context.KhachHang
                .OrderByDescending(k => k.TongChiTieu)
                .ToListAsync();

            return View("~/Views/Home/KhachHang.cshtml",danhSachKH);
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
                // THÊM MỚI: Trả về 1 model KhachHang trống
                var newCustomer = new KhachHang
                {
                    NgayDangKy = DateTime.Now // Có thể set giá trị mặc định
                };
                return PartialView("~/Views/Home/Partials/KhachHang/_FormKhachHang.cshtml", newCustomer);
            }

            // SỬA: Tìm khách hàng và trả về
            var khachHang = await _context.KhachHang.FindAsync(maKH);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng");
            }

            return PartialView("~/Views/Home/Partials/KhachHang/_FormKhachHang.cshtml", khachHang);
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

    }
}
