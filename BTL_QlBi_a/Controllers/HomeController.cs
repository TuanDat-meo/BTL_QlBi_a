using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Cần thêm để làm việc với User/Claims

namespace BTL_QlBi_a.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // GIẢ ĐỊNH: Lấy MaNV từ Claims/Session. Cần có cơ chế đăng nhập thực tế.
            // Nếu dùng Claims-based authentication:
            int? maNV = null;
            var maNVClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giả sử MaNV được lưu trong NameIdentifier
            if (int.TryParse(maNVClaim, out int nvId))
            {
                maNV = nvId;
            }

            // Lấy thông tin nhân viên từ database thay vì khởi tạo sẵn
            NhanVien nv = null;
            if (maNV.HasValue)
            {
                nv = await _context.NhanVien.FindAsync(maNV.Value);
            }

            // SỬA: Lấy TenNV và ChucVu từ database (hoặc gán giá trị mặc định nếu chưa đăng nhập)
            ViewBag.TenNV = nv?.TenNV ?? "Khách";
            ViewBag.ChucVu = nv?.ChucVu ?? "Không xác định";

            // Thống kê bàn
            var danhSachBan = await _context.BanBi_a.ToListAsync();
            // Đã là chuỗi string, không cần sửa đổi thêm
            ViewBag.BanTrong = danhSachBan.Count(b => b.TrangThai == "Trong");
            ViewBag.BanDangChoi = danhSachBan.Count(b => b.TrangThai == "DangChoi");
            ViewBag.BanDaDat = danhSachBan.Count(b => b.TrangThai == "DaDat");

            // Doanh thu hôm nay
            var today = DateTime.Today;
            var doanhThuHomNay = await _context.HoaDon
                .Where(h => h.ThoiGianKetThuc.HasValue &&
                           h.ThoiGianKetThuc.Value.Date == today &&
                           h.TrangThai == "DaThanhToan")
                .SumAsync(h => h.TongTien);
            ViewBag.DoanhThuHomNay = doanhThuHomNay.ToString("N0") + "đ";

            // Tổng khách hàng
            ViewBag.TongKhachHang = await _context.KhachHang.CountAsync();

            // Lấy danh sách bàn với thông tin liên quan
            var danhSachBanFull = await _context.BanBi_a
                .Include(b => b.LoaiBan)       // Tải thông tin Loại Bàn
                .Include(b => b.KhachHang)     // Tải thông tin Khách Hàng (nếu bàn đang chơi)
                .OrderBy(b => b.MaBan)
                .ToListAsync();

            return View(danhSachBanFull);
        }

        public async Task<IActionResult> ChiTietBan(int maBan)
        {
            var ban = await _context.BanBi_a
                .Include(b => b.LoaiBan)
                .Include(b => b.KhachHang)
                .FirstOrDefaultAsync(b => b.MaBan == maBan);

            if (ban == null)
            {
                return NotFound();
            }

            // Lấy hóa đơn đang mở của bàn (nếu có)
            var hoaDon = await _context.HoaDon
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .FirstOrDefaultAsync(h => h.MaBan == maBan &&
                                         h.TrangThai == "DangChoi");

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
                var ban = await _context.BanBi_a.FindAsync(maBan);
                if (ban == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bàn" });
                }

                if (ban.TrangThai != "Trong")
                {
                    return Json(new { success = false, message = "Bàn đang được sử dụng" });
                }

                // Lấy MaNV từ session/claims thực tế
                int? maNV = null;
                var maNVClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(maNVClaim, out int nvId))
                {
                    maNV = nvId;
                }

                if (!maNV.HasValue)
                {
                    // Trả về lỗi nếu không xác định được nhân viên thực hiện thao tác
                    return Json(new { success = false, message = "Không xác định được nhân viên" });
                }


                // Cập nhật trạng thái bàn
                ban.TrangThai = "DangChoi";
                ban.GioBatDau = DateTime.Now;
                ban.MaKH = maKH;

                // Tạo hóa đơn mới
                var hoaDon = new HoaDon
                {
                    MaBan = maBan,
                    MaKH = maKH,
                    MaNV = maNV, // SỬA: Lấy từ Claims/Session thay vì khởi tạo cứng là 1
                    ThoiGianBatDau = DateTime.Now,
                    TrangThai = "DangChoi"
                };

                _context.HoaDon.Add(hoaDon);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bắt đầu chơi thành công" });
            }
            catch (Exception ex)
            {
                // Có thể log ex.Message cho mục đích debug
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> KetThucChoi(int maBan)
        {
            try
            {
                var ban = await _context.BanBi_a.FindAsync(maBan);
                if (ban == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bàn" });
                }

                // Tìm hóa đơn đang mở
                var hoaDon = await _context.HoaDon
                    .Include(h => h.BanBia)
                    .ThenInclude(b => b.LoaiBan)
                    .FirstOrDefaultAsync(h => h.MaBan == maBan &&
                                             h.TrangThai == "DangChoi");

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
                // Đảm bảo hoaDon.BanBia.LoaiBan không null trước khi truy cập GiaGio
                if (hoaDon.BanBia?.LoaiBan != null)
                {
                    hoaDon.TienBan = hoaDon.BanBia.LoaiBan.GiaGio * (decimal)soGio;
                }
                else
                {
                    // Có thể thêm logic xử lý lỗi hoặc gán giá mặc định nếu không load được LoaiBan
                    // Trong trường hợp này, ta sẽ để TienBan = 0 nếu không có thông tin loại bàn.
                    hoaDon.TienBan = 0;
                }

                // Tính tiền dịch vụ
                var chiTiet = await _context.ChiTietHoaDon
                    .Include(ct => ct.DichVu)
                    .Where(ct => ct.MaHD == hoaDon.MaHD)
                    .ToListAsync();

                hoaDon.TienDichVu = chiTiet.Sum(ct => ct.ThanhTien ?? 0);
                hoaDon.TongTien = hoaDon.TienBan + hoaDon.TienDichVu - hoaDon.GiamGia;

                // Cập nhật trạng thái bàn
                ban.TrangThai = "Trong";
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
                // Có thể log ex.Message cho mục đích debug
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

                hoaDon.TrangThai = "DaThanhToan";
                // SỬA: phuongThuc đã là string, chỉ cần gán
                hoaDon.PhuongThucThanhToan = phuongThuc;

                // Cập nhật thông tin khách hàng (nếu có)
                if (hoaDon.MaKH.HasValue)
                {
                    var khachHang = await _context.KhachHang.FindAsync(hoaDon.MaKH.Value);
                    if (khachHang != null)
                    {
                        khachHang.TongChiTieu += hoaDon.TongTien;
                        // SỬA: Đảm bảo phép chia không gây lỗi nếu TongTien là decimal
                        khachHang.DiemTichLuy += (int)(hoaDon.TongTien / 1000m); // Sử dụng 1000m để đảm bảo phép toán decimal

                        khachHang.LanDenCuoi = DateTime.Now;

                        // Cập nhật hạng thành viên
                        // Các chuỗi hạng đã được sử dụng đúng
                        if (khachHang.TongChiTieu >= 10000000)
                            khachHang.HangTV = "BachKim";
                        else if (khachHang.TongChiTieu >= 5000000)
                            khachHang.HangTV = "Vang";
                        else if (khachHang.TongChiTieu >= 2000000)
                            khachHang.HangTV = "Bac";
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
                // Có thể log ex.Message cho mục đích debug
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}