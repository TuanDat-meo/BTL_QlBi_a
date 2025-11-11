#nullable enable
using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using BTL_QlBi_a.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace BTL_QlBi_a.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string gmailAddress = "snsnj6179@gmail.com";
        private readonly string gmailAppPassword = "gank qhxm mmsc bsvn";

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string? sdt, string? matkhau)
        {
            try
            {
                if (string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(matkhau))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                    return View();
                }

                string hashedPassword = HashPassword(matkhau);

                // Tìm nhân viên với mật khẩu đã hash
                var nhanVien = await _context.NhanVien
                    .Include(nv => nv.NhomQuyen)
                    .FirstOrDefaultAsync(nv => nv.SDT == sdt && nv.MatKhau == hashedPassword);

                // Nếu không tìm thấy, thử với mật khẩu không hash (backward compatibility)
                nhanVien ??= await _context.NhanVien
                    .Include(nv => nv.NhomQuyen)
                    .FirstOrDefaultAsync(nv => nv.SDT == sdt && nv.MatKhau == matkhau);

                if (nhanVien == null)
                {
                    ViewBag.Error = "Số điện thoại hoặc mật khẩu không đúng";
                    return View();
                }

                // So sánh với enum
                if (nhanVien.TrangThai != TrangThaiNhanVien.DangLam)
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị khóa";
                    return View();
                }

                // Lưu thông tin vào Session
                HttpContext.Session.SetInt32("MaNV", nhanVien.MaNV);
                HttpContext.Session.SetString("TenNV", nhanVien.TenNV);
                HttpContext.Session.SetInt32("MaNhom", nhanVien.MaNhom);

                // Lưu role name - QUAN TRỌNG: Dùng TenNhom từ database
                string roleName = nhanVien.NhomQuyen?.TenNhom ?? "Phục vụ";
                HttpContext.Session.SetString("TenNhom", roleName);
                HttpContext.Session.SetString("ChucVu", roleName);

                // Debug log
                Console.WriteLine($"Login: {nhanVien.TenNV}, Role: {roleName}, MaNhom: {nhanVien.MaNhom}");

                // Ghi log hoạt động
                var logHoatDong = new LichSuHoatDong
                {
                    MaNV = nhanVien.MaNV,
                    HanhDong = "Đăng nhập",
                    ChiTiet = $"Nhân viên {nhanVien.TenNV} ({roleName}) đăng nhập vào hệ thống",
                    ThoiGian = DateTime.Now
                };
                _context.LichSuHoatDong.Add(logHoatDong);
                await _context.SaveChangesAsync();

                return RedirectToAction("BanBia", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(string? tenNV, string? sdt, string? matkhau, string? xacnhanMatkhau)
        {
            if (string.IsNullOrEmpty(tenNV) || string.IsNullOrEmpty(sdt) ||
                string.IsNullOrEmpty(matkhau) || string.IsNullOrEmpty(xacnhanMatkhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            if (matkhau != xacnhanMatkhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View();
            }

            if (await _context.NhanVien.AnyAsync(nv => nv.SDT == sdt))
            {
                ViewBag.Error = "Số điện thoại đã được đăng ký";
                return View();
            }

            try
            {
                // Tìm nhóm quyền mặc định (ví dụ: "Nhân viên")
                var nhomMacDinh = await _context.NhomQuyen
                    .FirstOrDefaultAsync(n => n.TenNhom == "Nhân viên");

                if (nhomMacDinh == null)
                {
                    ViewBag.Error = "Không tìm thấy nhóm quyền mặc định";
                    return View();
                }

                // Tạo nhân viên mới
                var newNhanVien = new NhanVien
                {
                    TenNV = tenNV,
                    SDT = sdt,
                    MatKhau = HashPassword(matkhau),
                    MaNhom = nhomMacDinh.MaNhom,
                    TrangThai = TrangThaiNhanVien.DangLam,
                    CaMacDinh = CaLamViec.Sang,
                    LuongCoBan = 0,
                    PhuCap = 0
                };

                _context.NhanVien.Add(newNhanVien);
                await _context.SaveChangesAsync();

                // Ghi log hoạt động
                var logHoatDong = new LichSuHoatDong
                {
                    MaNV = newNhanVien.MaNV,
                    HanhDong = "Đăng ký",
                    ChiTiet = $"Nhân viên {tenNV} đăng ký tài khoản mới",
                    ThoiGian = DateTime.Now
                };
                _context.LichSuHoatDong.Add(logHoatDong);
                await _context.SaveChangesAsync();

                ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập.";
                return View("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đăng ký thất bại: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("Forgot");
        }

        //[HttpPost]
        //public async Task<IActionResult> ForgotPassword(string? email)
        //{
        //    // 1. Kiểm tra email đầu vào
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        ViewBag.Error = "Vui lòng nhập địa chỉ email";
        //        // Vẫn trả về View("Forgot") để xử lý lỗi đã đề cập ở câu hỏi trước
        //        return View("Forgot");
        //    }

        //    // 2. Tìm nhân viên theo Email
        //    // Thay đổi: .FirstOrDefaultAsync(nv => nv.SDT == sdt) thành .FirstOrDefaultAsync(nv => nv.Email == email)
        //    var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(nv => nv.mail == email);

        //    if (nhanVien == null)
        //    {
        //        // Thông báo chung chung để tránh lộ thông tin tài khoản
        //        ViewBag.Success = "Nếu địa chỉ email tồn tại trong hệ thống, mã xác nhận sẽ được gửi đến bạn.";
        //        return View("Forgot");
        //    }

        //    try
        //    {
        //        string otp = GenerateOTP();

        //        // Gửi OTP qua Email
        //        // Thay đổi: recipientEmail là email lấy từ input
        //        await SendOtpEmail(email, otp);

        //        // 3. Lưu OTP và Email vào Session
        //        // Thay đổi: "PasswordResetOTP_" + sdt thành "PasswordResetOTP_" + email
        //        HttpContext.Session.SetString("PasswordResetOTP_" + email, otp);
        //        // Thay đổi: "PasswordResetSDT" thành "PasswordResetEmail"
        //        HttpContext.Session.SetString("PasswordResetEmail", email);

        //        // 4. Chuyển hướng sang trang ResetPassword
        //        // Thay đổi: new { sdt } thành new { email }
        //        return RedirectToAction("ResetPassword", new { email });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Ghi log lỗi để kiểm tra dịch vụ Email
        //        Console.WriteLine($"Lỗi ForgotPassword: {ex.Message}");
        //        ViewBag.Error = "Lỗi trong quá trình gửi mã. Vui lòng thử lại sau.";
        //        return View("Forgot");
        //    }
        //}

        [HttpGet]
        public IActionResult ResetPassword(string? sdt)
        {
            if (HttpContext.Session.GetString("PasswordResetSDT") != sdt || string.IsNullOrEmpty(sdt))
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.SDT = sdt;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string? sdt, string? otp, string? matkhauMoi, string? xacnhanMatkhau)
        {
            if (string.IsNullOrEmpty(sdt))
            {
                ViewBag.Error = "Số điện thoại không hợp lệ.";
                return View("ForgotPassword");
            }

            var storedOtp = HttpContext.Session.GetString("PasswordResetOTP_" + sdt);

            if (storedOtp == null || HttpContext.Session.GetString("PasswordResetSDT") != sdt)
            {
                ViewBag.Error = "Phiên đặt lại mật khẩu đã hết hạn hoặc không hợp lệ. Vui lòng thử lại.";
                return View("ForgotPassword");
            }

            if (otp != storedOtp)
            {
                ViewBag.Error = "Mã xác nhận (OTP) không đúng.";
                ViewBag.SDT = sdt;
                return View();
            }

            if (matkhauMoi != xacnhanMatkhau)
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                ViewBag.SDT = sdt;
                return View();
            }

            // Tìm nhân viên theo SDT
            var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(nv => nv.SDT == sdt);

            if (nhanVien == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản. Vui lòng thử lại.";
                return View("ForgotPassword");
            }

            try
            {
                nhanVien.MatKhau = HashPassword(matkhauMoi!);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("PasswordResetOTP_" + sdt);
                HttpContext.Session.Remove("PasswordResetSDT");

                ViewBag.Success = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập.";
                return View("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi trong quá trình đặt lại mật khẩu: " + ex.Message;
                ViewBag.SDT = sdt;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                int? maNV = HttpContext.Session.GetInt32("MaNV");

                if (maNV.HasValue)
                {
                    var nhanVien = await _context.NhanVien.FindAsync(maNV.Value);
                    if (nhanVien != null)
                    {
                        var logHoatDong = new LichSuHoatDong
                        {
                            MaNV = nhanVien.MaNV,
                            HanhDong = "Đăng xuất",
                            ChiTiet = $"Nhân viên {nhanVien.TenNV} đăng xuất khỏi hệ thống",
                            ThoiGian = DateTime.Now
                        };
                        _context.LichSuHoatDong.Add(logHoatDong);
                        await _context.SaveChangesAsync();
                    }
                }

                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        private static string GenerateOTP()
        {
            Random random = new();
            return random.Next(100000, 999999).ToString();
        }

        private async Task SendOtpEmail(string recipientEmail, string otp)
        {
            try
            {
                using MailMessage mail = new();
                mail.From = new MailAddress(gmailAddress, "Hệ thống Quản Lý Quán Bi-a");
                mail.To.Add(recipientEmail);
                mail.Subject = "Mã xác nhận đặt lại mật khẩu (OTP)";
                mail.Body = $@"
                    <html>
                        <body>
                            <p>Xin chào,</p>
                            <p>Bạn đã yêu cầu đặt lại mật khẩu. Mã xác nhận (OTP) của bạn là:</p>
                            <h2 style='color:#1b6ec2; text-align:center;'>{otp}</h2>
                            <p>Mã này sẽ hết hạn trong thời gian ngắn. Vui lòng nhập mã để tiếp tục.</p>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                            <p>Trân trọng,</p>
                            <p>Đội ngũ Quản Lý Quán Bi-a Pro</p>
                        </body>
                    </html>
                ";
                mail.IsBodyHtml = true;

                using SmtpClient smtp = new("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(gmailAddress, gmailAppPassword);
                smtp.EnableSsl = true;
                smtp.Timeout = 20000;
                await smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email đến {recipientEmail}: {ex.Message}");
                throw new Exception("Lỗi dịch vụ gửi email. Vui lòng thử lại sau.", ex);
            }
        }
    }
}