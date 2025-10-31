---------------------------------------------------
-- Dữ liệu mẫu (Seed Data) cho cơ sở dữ liệu Bida
---------------------------------------------------

-- 1. INSERT loai_ban (Loại Bàn)
INSERT INTO loai_ban (ten_loai, mo_ta, gia_gio, trang_thai)
VALUES
(N'Bàn Lỗ 9 bi', N'Bàn bi-a lỗ cỡ tiêu chuẩn 9 feet', 50000, N'Đang áp dụng'), -- ma_loai = 1
(N'Bàn Phăng Carom', N'Bàn bi-a phăng carom 3 băng', 60000, N'Đang áp dụng'), -- ma_loai = 2
(N'Bàn Snooker', N'Bàn snooker cỡ lớn (Ngừng áp dụng)', 80000, N'Ngừng áp dụng'), -- ma_loai = 3
(N'Bàn VIP Lỗ', N'Bàn bi-a lỗ phòng VIP (Giá cao hơn)', 100000, N'Đang áp dụng'); -- ma_loai = 4
GO

-- 2. INSERT khach_hang (Khách Hàng)
INSERT INTO khach_hang (ten_kh, sdt, email, ngay_sinh, hang_tv, diem_tich_luy, tong_chi_tieu, lan_den_cuoi)
VALUES
(N'Nguyễn Văn An', '0901234567', 'nguyenvanan@gmail.com', '1990-05-15', N'Bạch kim', 500, 5000000, '2025-10-20'), -- ma_kh = 1
(N'Trần Thị Bình', '0912345678', 'tranthibinh@gmail.com', '1995-08-20', N'Vàng', 300, 3000000, '2025-10-18'), -- ma_kh = 2
(N'Lê Hoàng Cường', '0923456789', 'lehoangcuong@gmail.com', '1988-12-10', N'Bạc', 150, 1500000, '2025-10-15'), -- ma_kh = 3
(N'Phạm Thị Dung', '0934567890', 'phamthidung@gmail.com', '1992-03-25', N'Đồng', 50, 500000, '2025-10-10'), -- ma_kh = 4
(N'Hoàng Văn Em', '0945678901', 'hoangvanem@gmail.com', '1998-07-18', N'Đồng', 30, 300000, '2025-10-05'); -- ma_kh = 5
GO

-- 3. INSERT nhan_vien (Nhân Viên)
INSERT INTO nhan_vien (ten_nv, email, sdt, chuc_vu, luong_co_ban, phu_cap, ca_mac_dinh, trang_thai, mat_khau)
VALUES
(N'Quản lý Minh', 'minh.ql@bida.com', '0971234567', N'Quản lý', 15000000, 3000000, N'Sáng', N'Đang làm', 'hashed_password_1'), -- ma_nv = 1
(N'Thu ngân Lan', 'lan.tn@bida.com', '0982345678', N'Thu ngân', 8000000, 1000000, N'Sáng', N'Đang làm', 'hashed_password_2'), -- ma_nv = 2
(N'Phục vụ Hùng', 'hung.pv@bida.com', '0993456789', N'Phục vụ', 7000000, 500000, N'Tối', N'Đang làm', 'hashed_password_3'), -- ma_nv = 3
(N'Phục vụ Linh', 'linh.pv@bida.com', '0964567890', N'Phục vụ', 7000000, 500000, N'Chiều', N'Nghỉ', 'hashed_password_4'); -- ma_nv = 4
GO

-- 4. INSERT mat_hang (Mặt Hàng - Nguyên liệu/Hàng hóa)
INSERT INTO mat_hang (ten_hang, loai, don_vi, gia, so_luong_ton, nha_cung_cap, ngay_nhap_gan_nhat, trang_thai)
VALUES
(N'Bia Tiger', N'Đồ uống', N'chai', 15000, 100, N'Heineken VN', '2025-10-20', N'Còn hàng'), -- ma_hang = 1
(N'Khô bò', N'Đồ ăn', N'đĩa', 30000, 50, N'Nhà cung cấp ABC', '2025-10-22', N'Còn hàng'), -- ma_hang = 2
(N'Đậu phộng', N'Đồ ăn', N'đĩa', 10000, 200, N'Nhà cung cấp XYZ', '2025-10-22', N'Ngừng kinh doanh'), -- ma_hang = 3
(N'Nước ép cam hộp', N'Đồ uống', N'hộp', 18000, 40, N'Công ty Nước ép', '2025-10-24', N'Còn hàng'); -- ma_hang = 4
GO

-- 5. INSERT dich_vu (Dịch Vụ - Sản phẩm bán ra)
INSERT INTO dich_vu (ten_dv, loai, gia, don_vi, so_luong_ton, ma_hang, ti_le_loi_nhuan, trang_thai, mo_ta)
VALUES
(N'Bia Tiger lạnh', N'Đồ uống', 25000, N'chai', 100, 1, 66.67, N'Còn hàng', N'Bia Tiger bán lẻ tại quán'), -- ma_dv = 1 (Liên kết với ma_hang 1)
(N'Khô bò rang muối', N'Đồ ăn', 45000, N'đĩa', 50, 2, 50.00, N'Còn hàng', N'Món ăn nhẹ phục vụ bàn'), -- ma_dv = 2 (Liên kết với ma_hang 2)
(N'Thuê cơ VIP', N'Dụng cụ bi-a', 50000, N'lần', 10, NULL, 100.00, N'Còn hàng', N'Thuê cơ riêng cao cấp'), -- ma_dv = 3 (Không liên kết với mặt hàng nào)
(N'Nước ép cam tươi', N'Đồ uống', 35000, N'cốc', 40, NULL, 30.00, N'Còn hàng', N'Nước ép chế biến tại quán'); -- ma_dv = 4
GO

-- 6. INSERT ban_bia (Bàn Bi-A)
INSERT INTO ban_bia (ten_ban, ma_loai, khu_vuc, trang_thai, gio_bat_dau, ma_kh, ghi_chu)
VALUES
(N'Bàn 01', 1, N'Tầng 1', N'Đang chơi', '2025-10-28 09:30:00', 1, N'Khách Bạch kim - Nguyễn Văn An'), -- ma_ban = 1 (ma_loai 1 - Lỗ 9 bi)
(N'Bàn 02', 1, N'Tầng 1', N'Trống', NULL, NULL, NULL), -- ma_ban = 2
(N'Bàn 03', 2, N'Tầng 1', N'Đang chơi', '2025-10-28 10:00:00', 2, N'Khách Vàng - Trần Thị Bình'), -- ma_ban = 3 (ma_loai 2 - Phăng Carom)
(N'Bàn VIP 01', 4, N'VIP', N'Đã đặt', NULL, 3, N'Phòng riêng cho khách Lê Hoàng Cường'), -- ma_ban = 4 (ma_loai 4 - VIP Lỗ)
(N'Bàn 05', 2, N'Tầng 2', N'Bảo trì', NULL, NULL, N'Thay vải'); -- ma_ban = 5
GO
-- Thêm các bàn mới vào bảng ban_bia (tiếp theo ma_ban = 5)
INSERT INTO ban_bia (ten_ban, ma_loai, khu_vuc, trang_thai, gio_bat_dau, ma_kh, ghi_chu)
VALUES
-- Tầng 1 (Bàn Lỗ 9 bi)
(N'Bàn 04', 1, N'Tầng 1', N'Trống', NULL, NULL, NULL),       -- ma_ban = 6
(N'Bàn 06', 1, N'Tầng 1', N'Đã đặt', NULL, 4, N'Khách Phạm Thị Dung đặt lúc 16:30'), -- ma_ban = 7
(N'Bàn 07', 1, N'Tầng 1', N'Đang chơi', '2025-10-28 11:30:00', NULL, N'Khách lẻ'), -- ma_ban = 8

-- Tầng 2 (Bàn Phăng Carom)
(N'Bàn 08', 2, N'Tầng 2', N'Trống', NULL, NULL, NULL),       -- ma_ban = 9
(N'Bàn 09', 2, N'Tầng 2', N'Đang chơi', '2025-10-28 12:45:00', 5, N'Khách Đồng - Hoàng Văn Em'), -- ma_ban = 10
(N'Bàn 10', 2, N'Tầng 2', N'Trống', NULL, NULL, NULL),       -- ma_ban = 11

-- VIP (Bàn VIP Lỗ)
(N'Bàn VIP 02', 4, N'VIP', N'Đang chơi', '2025-10-28 09:00:00', 1, N'Khách Bạch kim - Yêu cầu phục vụ riêng'), -- ma_ban = 12
(N'Bàn VIP 03', 4, N'VIP', N'Trống', NULL, NULL, NULL);       -- ma_ban = 13
GO
-- 7. INSERT gia_gio_choi (Giá Giờ Chơi)
INSERT INTO gia_gio_choi (khung_gio, gio_bat_dau, gio_ket_thuc, ma_loai, gia, ap_dung_tu_ngay, trang_thai)
VALUES
(N'Sáng (9h-12h) Lỗ 9 bi', '09:00:00', '12:00:00', 1, 50000, '2025-01-01', N'Đang áp dụng'),
(N'Chiều (12h-17h) Lỗ 9 bi', '12:00:00', '17:00:00', 1, 60000, '2025-01-01', N'Đang áp dụng'),
(N'Cao điểm (17h-22h) Phăng Carom', '17:00:00', '22:00:00', 2, 80000, '2025-01-01', N'Đang áp dụng'),
(N'VIP (Cả ngày)', '09:00:00', '22:00:00', 4, 100000, '2025-01-01', N'Đang áp dụng');
GO

-- 8. INSERT dat_ban (Đặt Bàn)
INSERT INTO dat_ban (ma_ban, ma_kh, ten_khach, sdt, thoi_gian_dat, so_nguoi, ghi_chu, trang_thai)
VALUES
(4, 3, N'Lê Hoàng Cường', '0923456789', '2025-10-28 15:00:00', 4, N'Phòng VIP 01, yêu cầu nước suối', N'Đã xác nhận'),
(NULL, 5, N'Hoàng Văn Em', '0945678901', '2025-10-29 18:00:00', 2, N'Yêu cầu bàn lỗ, khu vực tầng 2', N'Đang chờ'),
(1, NULL, N'Khách vãng lai A', '0900000001', '2025-10-27 20:00:00', 3, N'Đặt nhầm ngày', N'Đã hủy');
GO

-- 9. INSERT hoa_don (Hóa Đơn)
-- Giả định có 1 hóa đơn đã thanh toán (HĐ 1) và 1 hóa đơn đang mở (HĐ 2)
INSERT INTO hoa_don (ma_ban, ma_kh, ma_nv, thoi_gian_bat_dau, thoi_gian_ket_thuc, thoi_luong_phut, tien_ban, tien_dich_vu, giam_gia, tong_tien, phuong_thuc_thanh_toan, trang_thai, ghi_chu)
VALUES
(1, 1, 2, '2025-10-28 09:30:00', '2025-10-28 11:00:00', 90, 75000, 50000, 10000, 115000, N'Chuyển khoản', N'Đã thanh toán', N'Khách thanh toán nhanh'), -- ma_hd = 1
(3, 2, 2, '2025-10-28 10:00:00', NULL, NULL, 0, 0, 0, 0, NULL, N'Đang chơi', NULL); -- ma_hd = 2
GO

-- 10. INSERT chi_tiet_hoa_don (Chi Tiết Hóa Đơn)
INSERT INTO chi_tiet_hoa_don (ma_hd, ma_dv, so_luong, thanh_tien)
VALUES
-- Hóa đơn 1 (Đã thanh toán)
(1, 1, 2, 50000), -- 2 Bia Tiger lạnh (2 * 25,000)
(1, 3, 1, 50000), -- 1 Thuê cơ VIP (1 * 50,000)
-- Hóa đơn 2 (Đang chơi)
(2, 4, 1, 35000); -- 1 Nước ép cam tươi (1 * 35,000)
GO

-- 11. INSERT cham_cong (Chấm Công)
INSERT INTO cham_cong (ma_nv, ngay, gio_vao, gio_ra, xac_thuc_bang, trang_thai, ghi_chu)
VALUES
(2, '2025-10-28', '2025-10-28 08:58:00', '2025-10-28 17:05:00', N'Vân tay', N'Đúng giờ', NULL), -- Thu ngân Lan
(3, '2025-10-28', '2025-10-28 17:35:00', NULL, N'Thủ công', N'Đúng giờ', NULL), -- Phục vụ Hùng (Ca tối đang làm)
(4, '2025-10-28', '2025-10-28 08:05:00', '2025-10-28 17:00:00', N'FaceID', N'Đi trễ', N'Đã trừ 5 phút'); -- Phục vụ Linh (Đã nghỉ việc nhưng có dữ liệu cũ)
GO

-- 12. INSERT bang_luong (Bảng Lương)
INSERT INTO bang_luong (ma_nv, thang, nam, tong_gio, luong_co_ban, phu_cap, thuong, phat, tong_luong)
VALUES
(2, 9, 2025, 176.5, 8000000, 1000000, 500000, 0, 9500000), -- Thu ngân Lan (Tháng 9)
(3, 9, 2025, 175.0, 7000000, 500000, 300000, 50000, 7750000); -- Phục vụ Hùng (Tháng 9)
GO

-- 13. INSERT lich_su_hoat_dong (Lịch Sử Hoạt Động)
INSERT INTO lich_su_hoat_dong (ma_nv, hanh_dong, chi_tiet)
VALUES
(1, N'Cập nhật giá', N'Quản lý Minh cập nhật giá giờ chơi loại 1'),
(2, N'Thanh toán', N'Thu ngân Lan hoàn tất hoá đơn #1 cho Bàn 01. Tổng tiền: 115,000 VND'),
(3, N'Thao tác bàn', N'Phục vụ Hùng mở bàn 03 cho khách Trần Thị Bình');
GO