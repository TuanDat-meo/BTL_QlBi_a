using BTL_QlBi_a.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QlBi_a.Controllers.Users
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var dsBan = await _context.BanBia
                .Include(b => b.LoaiBan)
                .OrderBy(b => b.TenBan)
                .ToListAsync();

            return View(dsBan);
        }

        [HttpGet]
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
    }
}
