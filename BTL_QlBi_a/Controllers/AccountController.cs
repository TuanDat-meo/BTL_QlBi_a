using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
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
        public async Task<IActionResult> Login(string sdt, string matkhau)
        {
            try
            {
                if (string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(matkhau))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                    return View();
                }

                string hashedPassword = HashPassword(matkhau);

                var nhanVien = await _context.NhanVien
                    .FirstOrDefaultAsync(nv => nv.SDT == sdt && nv.MatKhau == hashedPassword);

                if (nhanVien == null)
                {
                    nhanVien = await _context.NhanVien
                        .FirstOrDefaultAsync(nv => nv.SDT == sdt && nv.MatKhau == matkhau);
                }

                if (nhanVien == null)
                {
                    ViewBag.Error = "Số điện thoại hoặc mật khẩu không đúng";
                    return View();
                }

                if (nhanVien.TrangThai != "Đang làm")
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị khóa";
                    return View();
                }

                HttpContext.Session.SetInt32("MaNV", nhanVien.MaNV);
                HttpContext.Session.SetString("TenNV", nhanVien.TenNV);
                HttpContext.Session.SetString("ChucVu", nhanVien.ChucVu);

                var logHoatDong = new LichSuHoatDong
                {
                    MaNV = nhanVien.MaNV,
                    HanhDong = "Đăng nhập",
                    ChiTiet = $"Nhân viên {nhanVien.TenNV} đăng nhập vào hệ thống",
                    ThoiGian = DateTime.Now
                };
                _context.LichSuHoatDong.Add(logHoatDong);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View();
            }
        }

        // =================================================================================
        // THÊM: Action Đăng Ký (Signup)
        // =================================================================================

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(string tenNV, string sdt, string email, string matkhau, string xacnhanMatkhau)
        {
            if (string.IsNullOrEmpty(tenNV) || string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matkhau) || string.IsNullOrEmpty(xacnhanMatkhau))
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

            // Kiểm tra email duy nhất (tùy chọn)
            if (await _context.NhanVien.AnyAsync(nv => nv.Email == email))
            {
                ViewBag.Error = "Email đã được đăng ký";
                return View();
            }

            try
            {
                // Tạo nhân viên mới
                var newNhanVien = new NhanVien
                {
                    TenNV = tenNV,
                    SDT = sdt,
                    Email = email,
                    MatKhau = HashPassword(matkhau), // Hash mật khẩu trước khi lưu
                    ChucVu = "Nhân viên", // Gán chức vụ mặc định
                    TrangThai = "Đang làm", // Mặc định là đang làm
                    //NgayVaoLam = DateTime.Now
                };

                _context.NhanVien.Add(newNhanVien);

                // Ghi log hoạt động
                var logHoatDong = new LichSuHoatDong
                {
                    MaNV = newNhanVien.MaNV, // MaNV sẽ được set sau SaveChanges() nếu MaNV là Identity
                    HanhDong = "Đăng ký",
                    ChiTiet = $"Nhân viên {tenNV} đăng ký tài khoản mới",
                    ThoiGian = DateTime.Now
                };
                _context.LichSuHoatDong.Add(logHoatDong);

                await _context.SaveChangesAsync();

                ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập.";
                // Sau khi đăng ký thành công, chuyển hướng về trang Login
                return View("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đăng ký thất bại: " + ex.Message;
                return View();
            }
        }


        // =================================================================================
        // THÊM: Action Quên Mật Khẩu (Forgot Password - Gửi OTP qua Email)
        // =================================================================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("Forgot");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập địa chỉ email";
                return View();
            }

            var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(nv => nv.Email == email);

            if (nhanVien == null)
            {
                // Tránh lộ thông tin email đã đăng ký hay chưa
                ViewBag.Success = "Nếu email tồn tại trong hệ thống, mã xác nhận sẽ được gửi đến email của bạn.";
                return View();
            }

            try
            {
                // 1. Tạo mã OTP ngẫu nhiên.
                string otp = GenerateOTP();

                // 2. Gửi OTP qua Email
                await SendOtpEmail(email, otp); // THAY THẾ: Gọi phương thức gửi email đã tạo

                // 3. Lưu OTP vào Session
                HttpContext.Session.SetString("PasswordResetOTP_" + email, otp);
                HttpContext.Session.SetString("PasswordResetEmail", email);

                // Chuyển hướng đến trang nhập OTP
                return RedirectToAction("ResetPassword", new { email = email });
            }
            catch (Exception ex)
            {
                // Sử dụng thông báo lỗi chung nếu lỗi là do dịch vụ gửi email
                ViewBag.Error = "Lỗi trong quá trình gửi mã. Vui lòng kiểm tra lại địa chỉ email hoặc thử lại sau.";
                return View();
            }
        }
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            // Kiểm tra xem có email nào đang trong quá trình reset không
            if (HttpContext.Session.GetString("PasswordResetEmail") != email || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = email;
            return View(); // Cần tạo View ResetPassword.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string otp, string matkhauMoi, string xacnhanMatkhau)
        {
            var storedOtp = HttpContext.Session.GetString("PasswordResetOTP_" + email);

            if (storedOtp == null || HttpContext.Session.GetString("PasswordResetEmail") != email)
            {
                ViewBag.Error = "Phiên đặt lại mật khẩu đã hết hạn hoặc không hợp lệ. Vui lòng thử lại.";
                return View("ForgotPassword");
            }

            if (otp != storedOtp)
            {
                ViewBag.Error = "Mã xác nhận (OTP) không đúng.";
                ViewBag.Email = email;
                return View();
            }

            if (matkhauMoi != xacnhanMatkhau)
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                ViewBag.Email = email;
                return View();
            }

            var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(nv => nv.Email == email);

            if (nhanVien == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản. Vui lòng thử lại.";
                return View("ForgotPassword");
            }

            try
            {
                // Cập nhật mật khẩu mới (đã hash)
                nhanVien.MatKhau = HashPassword(matkhauMoi);
                await _context.SaveChangesAsync();

                // Xóa OTP khỏi Session
                HttpContext.Session.Remove("PasswordResetOTP_" + email);
                HttpContext.Session.Remove("PasswordResetEmail");

                ViewBag.Success = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập.";
                return View("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi trong quá trình đặt lại mật khẩu: " + ex.Message;
                ViewBag.Email = email;
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
                        // Ghi log hoạt động
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

                // Xóa Session
                HttpContext.Session.Clear();

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method để hash password
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Helper method để tạo OTP ngẫu nhiên (6 chữ số)
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        private async Task SendOtpEmail(string recipientEmail, string otp)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
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

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(gmailAddress, gmailAppPassword);
                        smtp.EnableSsl = true;
                        // Thời gian chờ (tùy chọn)
                        smtp.Timeout = 20000;
                        await smtp.SendMailAsync(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi email nhưng không ném lỗi ra ngoài để tránh lộ thông tin nhạy cảm
                // Có thể thay bằng một dịch vụ logging thích hợp hơn
                Console.WriteLine($"Lỗi gửi email đến {recipientEmail}: {ex.Message}");
                throw new Exception("Lỗi dịch vụ gửi email. Vui lòng thử lại sau.", ex);
            }
        }

    }
}