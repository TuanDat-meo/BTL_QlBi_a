using BTL_QlBi_a.Models.EF;
using BTL_QlBi_a.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace BTL_QlBi_a.Controllers
{
    public class HoaDonController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
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
        public async Task<IActionResult> HoaDon(
            DateTime? fromDate, DateTime? toDate, string status = "All")
        {
            await LoadHeaderStats();

            var query = _context.HoaDon
                .Include(h => h.BanBia)
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .AsQueryable();


            TrangThaiHoaDon? statusEnum = null;
            if (status != "All" && !string.IsNullOrEmpty(status))
            {
                if (status == "Đang chơi")
                    statusEnum = TrangThaiHoaDon.DangChoi;
                else if (status == "Đã thanh toán")
                    statusEnum = TrangThaiHoaDon.DaThanhToan;
                else if (status == "Đã hủy")
                    statusEnum = TrangThaiHoaDon.DaHuy;
            }

            if (statusEnum.HasValue)
            {
                query = query.Where(h => h.TrangThai == statusEnum.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.ThoiGianBatDau >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(h => h.ThoiGianBatDau < toDate.Value.AddDays(1));
            }

            var danhSachHD = await query
                .OrderByDescending(h => h.ThoiGianBatDau)
                .ToListAsync();

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Status = status;

            return View("~/Views/Home/HoaDon.cshtml", danhSachHD);
        }
        public async Task<IActionResult> ChiTietHoaDon(int maHD)
        {
            // Lấy TenNhom từ Session (giống như bạn làm ở _ChiTietBan)
            var tenNhom = HttpContext.Session.GetString("TenNhom") ?? "Nhân viên";
            ViewBag.TenNhom = tenNhom;

            var hoaDon = await _context.HoaDon
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.BanBia) // <-- Include Bàn
                    .ThenInclude(b => b.LoaiBan) // <-- Include LoaiBan để lấy Giá Giờ
                .FirstOrDefaultAsync(h => h.MaHD == maHD);

            if (hoaDon == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn" });
            }

            var chiTiet = await _context.ChiTietHoaDon
                .Include(ct => ct.DichVu) // <-- Include DichVu để lấy Tên, Giá
                .Where(ct => ct.MaHD == hoaDon.MaHD)
                .ToListAsync();

            ViewBag.ChiTietDichVu = chiTiet;

            return PartialView("~/Views/Home/Partials/HoaDon/_ChiTietHoaDon.cshtml", hoaDon);
        }
        public async Task<IActionResult> InHoaDon(int maHD)
        {
            Console.WriteLine($"=== InHoaDon called for MaHD: {maHD} ===");

            var hoaDon = await _context.HoaDon
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.BanBia)
                    .ThenInclude(b => b.LoaiBan)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.DichVu)
                .FirstOrDefaultAsync(h => h.MaHD == maHD);

            if (hoaDon == null)
            {
                return NotFound("Không tìm thấy hóa đơn.");
            }

            return PartialView("~/Views/Home/Partials/HoaDon/_InHoaDon.cshtml", hoaDon);
        }
    
        public async Task<IActionResult> ExportToExcel(
            string status = "all",
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string search = null)
        {
            var query = _context.HoaDon
                .Include(h => h.BanBia)
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .AsQueryable();

            if (status!= "all")
            {
                if(Enum.TryParse<TrangThaiHoaDon>(status, out var trangThai))
                {
                    query = query.Where(h => h.TrangThai == trangThai);
                }

            }

            if (dateFrom.HasValue)
            {
                query = query.Where(h => h.ThoiGianBatDau >= dateFrom.Value.Date);
            }
            if (dateTo.HasValue)
            {
                // Lấy đến hết ngày 23:59:59
                var denNgay = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(h => h.ThoiGianBatDau <= denNgay);
            }
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(h =>
                    (h.KhachHang != null && h.KhachHang.TenKH.ToLower().Contains(searchLower)) ||
                    h.MaHD.ToString().Contains(searchLower)
                );
            }

            var hoaDons = await query.OrderByDescending(h=> h.ThoiGianBatDau).ToListAsync();

            // Tạo File Excel bằng EPPLUS
            ExcelPackage.License.SetNonCommercialPersonal("Duy Binh");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DanhSachHoaDon");

                // Set Header
                worksheet.Cells[1, 1].Value = "Mã HĐ";
                worksheet.Cells[1, 2].Value = "Bàn";
                worksheet.Cells[1, 3].Value = "Khách Hàng";
                worksheet.Cells[1, 4].Value = "Nhân viên";
                worksheet.Cells[1, 5].Value = "Thời gian bắt đầu";
                worksheet.Cells[1, 6].Value = "Tổng tiền";
                worksheet.Cells[1, 7].Value = "Trạng thái";

                // Định dạng Header
                using (var range = worksheet.Cells["A1:G1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                    range.Style.Font.Color.SetColor(Color.White);

                }

                // Đổ dữ liệu
                for (int i = 0; i< hoaDons.Count; i++)
                {
                    var hd = hoaDons[i];
                    int row = i + 2; // bat dau tu dong` thu 2

                    worksheet.Cells[row, 1].Value = $"HD{hd.MaHD:D4}";
                    worksheet.Cells[row, 2].Value = hd.BanBia?.TenBan;
                    worksheet.Cells[row, 3].Value = hd.KhachHang?.TenKH ?? "Khách lẻ";
                    worksheet.Cells[row, 4].Value = hd.NhanVien?.TenNV;
                    worksheet.Cells[row, 5].Value = hd.ThoiGianBatDau?.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0 đ";
                    worksheet.Cells[row, 7].Value = hd.TrangThai == TrangThaiHoaDon.DaThanhToan ? "Đã thanh toán" : "Đang chơi";

                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Trả về file
                var fileBytes = package.GetAsByteArray();
                string fileName = $"BaoCao_HoaDon_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                // Return file
                return File(
                    fileContents: fileBytes,
                    contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Là file xlsx                    
                    fileDownloadName: fileName

                    );
            }
        }
    }
}
