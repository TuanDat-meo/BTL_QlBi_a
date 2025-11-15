using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using BTL_QlBi_a.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QlBi_a.Controllers
{
    [Route("Users")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Route("~/")]    // <--- QUAN TRỌNG: Dấu ~ nghĩa là gốc website (Trang chủ)
        [Route("")]      // Xử lý đường dẫn /Users
        [Route("Index")] 
        public async Task<IActionResult> Index()
        {
            var dsBan = await _context.BanBia
                .Include(b => b.LoaiBan)
                .Include(b => b.KhuVuc)
                .OrderBy(b => b.TenBan)
                .ToListAsync();

            return View(dsBan);
        }

        [HttpGet("TraCuuThanhVien")]
        public async Task<IActionResult> TraCuuNhanVien(string sdt)
        {
            if (string.IsNullOrEmpty(sdt))
            {
                return Json(new { success = false, message = "Vui lòng nhập SĐT" });
            }
            var kh = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.SDT == sdt && k.HoatDong == true);

            if (kh == null) return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });

            return Json(new
            {
                success = true,
                data = new
                {
                    ten = kh.TenKH,
                    hang = kh.HangTV.ToString(),
                    diem = kh.DiemTichLuy,
                    avatar = kh.Avatar ?? "/asset/img/user.svg",
                    chiTieu = kh.TongChiTieu.ToString("N0") + " đ"
                }
            });
        }

        [HttpPost("DatBan")]
        public async Task<IActionResult> DatBan(int maBan, DateTime thoiGian, string ghiChu, int soGio = 1, int soNguoi = 2)
        {
            try
            {
                // 1. Kiểm tra đăng nhập
                int? maKH = HttpContext.Session.GetInt32("MaKH");
                if (maKH == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đặt bàn." });
                }

                // 2. Lấy thông tin khách hàng
                var khachHang = await _context.KhachHang.FindAsync(maKH);
                if (khachHang == null) return Json(new { success = false, message = "Lỗi thông tin khách hàng." });

                // 3. Validate thời gian (Đặt trước ít nhất 30 phút)
                if (thoiGian < DateTime.Now.AddMinutes(30))
                {
                    return Json(new { success = false, message = "Vui lòng đặt trước ít nhất 30 phút." });
                }

                // 4. KIỂM TRA TRÙNG LỊCH
                // Giả sử mỗi lượt đặt giữ chỗ trong 1 tiếng
                var gioKetThuc = thoiGian.AddHours(soGio);

                // Kiểm tra xem có đơn nào KHÁC 'DaHuy' đang chiếm chỗ không
                var lichTrung = await _context.DatBan
                            .Where(d => d.MaBan == maBan && d.TrangThai != TrangThaiDatBan.DaHuy)
                            .Where(d =>
                                // Logic trùng lịch: (Start A < End B) && (End A > Start B)
                                // A là đơn khách đang đặt, B là đơn đã có trong DB (d)

                                // Giờ khách đặt (A) < Giờ kết thúc của đơn B
                                thoiGian < d.ThoiGianDat.AddHours(d.SoGio) &&

                                // Giờ kết thúc của khách (A) > Giờ bắt đầu đơn B
                                gioKetThuc > d.ThoiGianDat
                            )
                            .FirstOrDefaultAsync();

                if (lichTrung != null)
                {
                    // Tính giờ kết thúc của người kia để báo cho khách biết
                    var gioKetThucNguoiKia = lichTrung.ThoiGianDat.AddHours(lichTrung.SoGio);
                    return Json(new { success = false, message = $"Đã có người đặt khung giờ này (đến {gioKetThucNguoiKia:HH:mm}). Vui lòng chọn giờ khác." });
                }

                // 5. LƯU VÀO DB
                var donDat = new DatBan
                {
                    MaBan = maBan,
                    MaKH = maKH,
                    TenKhach = khachHang.TenKH,
                    SDT = khachHang.SDT,
                    ThoiGianDat = thoiGian,
                    SoNguoi = 1,
                    SoGio=soGio,
                    GhiChu = ghiChu,
                    TrangThai = TrangThaiDatBan.DangCho, // Mặc định là Đang chờ
                    NgayTao = DateTime.Now
                };

                _context.DatBan.Add(donDat);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        #region Profile

        //============
        // Xem ho so
        //===========
        [Route("Profile")]
        public async Task<IActionResult> HoSo()
        {
            int? maKH = HttpContext.Session.GetInt32("MaKH");
            if (maKH == null) return RedirectToAction("Login", "Account");

            var kh = await _context.KhachHang.FindAsync(maKH);

            // lich su dat ban
            var historyBooking = await _context.DatBan
                .Where(d => d.MaKH == maKH)
                .Include(d => d.BanBia)
                .OrderByDescending(d => d.ThoiGianDat)
                .ToListAsync();

            // lich su hoa don
            var historyBill = await _context.HoaDon
                .Where(h => h.MaKH == maKH && h.TrangThai == TrangThaiHoaDon.DaThanhToan)
                .Include(h => h.BanBia)
                .OrderByDescending(h => h.ThoiGianKetThuc)
                .ToListAsync();

            var viewModel = new UserProfileViewModel
            {
                KhachHang = kh,
                LichSuDatBan = historyBooking,
                LichSuHoaDon = historyBill,
            };

            return View(viewModel);
        }

        //============
        // Thong tin ca nhaan
        //===========
        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(KhachHang model, IFormFile? fileAvatar)
        {
            int? maKH = HttpContext.Session.GetInt32("MaKH");
            var kh = await _context.KhachHang.FindAsync(maKH);
            if (kh == null) return RedirectToAction("Login", "Account");

            kh.TenKH = model.TenKH;
            kh.Email = model.Email;
            kh.NgaySinh = model.NgaySinh;

            // String Avatar
            if (fileAvatar != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAvatar.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset/img/avatar_khach_hang", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await fileAvatar.CopyToAsync(stream);
                }
                kh.Avatar = "/asset/img/avatar_khach_hang/" + fileName;
            }

            _context.KhachHang.Update(kh);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("TenKH", kh.TenKH);

            return RedirectToAction("HoSo");
        }

        //============
        // Huy dat ban
        //===========
        [HttpPost("HuyLich")]
        public async Task<IActionResult> HuyLich(int maDat)
        {
            int? maKH = HttpContext.Session.GetInt32("MaKH");
            var lich = await _context.DatBan.FirstOrDefaultAsync(d => d.MaDat == maDat && d.MaKH == maKH);

            if (lich == null) return Json(new { success = false, message = "Không tìm thấy lịch." });

            if (lich.TrangThai != TrangThaiDatBan.DangCho)
            {
                return Json(new { success = false, message = "Không thể hủy lịch đã xác nhận hoặc đã hoàn thành." });
            }

            // Cập nhật trạng thái sang Đã Hủy
            lich.TrangThai = TrangThaiDatBan.DaHuy;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy lịch đặt thành công." });
        }

        //============
        // Xem chi tiet hoa don
        //===========
        [HttpGet("ChiTietHoaDon")] 
        public async Task<IActionResult> GetChiTietHoaDon(int maHD)
        {
            try
            {
                int? maKH = HttpContext.Session.GetInt32("MaKH");
                if (maKH == null) return Json(new { success = false, message = "Vui lòng đăng nhập." });

                // 1. Tìm hóa đơn (Phải khớp MaKH để bảo mật)
                var hoaDon = await _context.HoaDon
                    .Include(h => h.BanBia)
                    .Include(h => h.BanBia.LoaiBan)
                    .FirstOrDefaultAsync(h => h.MaHD == maHD && h.MaKH == maKH);

                if (hoaDon == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn." });

                // 2. Lấy chi tiết dịch vụ
                var chiTiet = await _context.ChiTietHoaDon
                    .Where(ct => ct.MaHD == maHD)
                    .Include(ct => ct.DichVu)
                    .Select(ct => new {
                        tenDV = ct.DichVu.TenDV,
                        soLuong = ct.SoLuong,
                        donGia = ct.DichVu.Gia,
                        thanhTien = ct.ThanhTien
                    })
                    .ToListAsync();

                // 3. Trả về JSON
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        maHD = hoaDon.MaHD,
                        ngay = hoaDon.ThoiGianKetThuc?.ToString("dd/MM/yyyy HH:mm"),
                        ban = hoaDon.BanBia.TenBan,
                        tienGio = hoaDon.TienBan,
                        tienDichVu = hoaDon.TienDichVu,
                        giamGia = hoaDon.GiamGia,
                        tongTien = hoaDon.TongTien,
                        dichVus = chiTiet // Danh sách món
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        #endregion
    }
}
