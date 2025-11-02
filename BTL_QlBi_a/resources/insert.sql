
INSERT INTO khu_vuc (ten_khu_vuc) VALUES (N'Tầng 1'), (N'Tầng 2'), (N'VIP');
GO

-- 3. INSERT nha_cung_cap
INSERT INTO nha_cung_cap (ten_ncc, sdt, dia_chi)
VALUES
(N'Heineken VN', '02838222755', N'TP. HCM'),
(N'Nhà cung cấp ABC', '0909123456', N'Bình Dương'),
(N'Nhà cung cấp XYZ', '0909789012', N'Đồng Nai'),
(N'Công ty Nước ép', '02838945900', N'TP. HCM');
GO

-- 4. INSERT nhom_quyen
INSERT INTO nhom_quyen (ten_nhom) 
VALUES (N'Admin'), (N'Quản lý'), (N'Thu ngân'), (N'Phục vụ');
GO

-- 5. INSERT chuc_nang
INSERT INTO chuc_nang (ma_cn, ten_cn) VALUES 
('VIEW_REPORT', N'Xem báo cáo doanh thu'),
('EDIT_PRICE', N'Chỉnh sửa giá dịch vụ'),
('MANAGE_STAFF', N'Quản lý nhân viên'),
('MANAGE_PERMISSION', N'Quản lý phân quyền'),
('MANAGE_INVENTORY', N'Quản lý kho (nhập hàng)'),
('MANAGE_SETTINGS', N'Truy cập cài đặt'),
('PERFORM_PAYMENT', N'Thực hiện thanh toán'),
('APPLY_DISCOUNT', N'Áp dụng giảm giá');
GO

-- 6. INSERT phan_quyen
-- Admin (ma_nhom = 1)
INSERT INTO phan_quyen (ma_nhom, ma_cn) 
SELECT 1, ma_cn FROM chuc_nang;
GO

-- Quản lý (ma_nhom = 2)
INSERT INTO phan_quyen (ma_nhom, ma_cn) VALUES
(2, 'VIEW_REPORT'), (2, 'EDIT_PRICE'), (2, 'MANAGE_STAFF'), 
(2, 'MANAGE_INVENTORY'), (2, 'PERFORM_PAYMENT'), (2, 'APPLY_DISCOUNT');
GO

-- Thu ngân (ma_nhom = 3)
INSERT INTO phan_quyen (ma_nhom, ma_cn) VALUES
(3, 'PERFORM_PAYMENT'), (3, 'APPLY_DISCOUNT');
GO

-- 7. INSERT loai_ban
INSERT INTO loai_ban (ten_loai, mo_ta, gia_gio, trang_thai)
VALUES
(N'Bàn Lỗ 9 bi', N'Bàn bi-a lỗ cỡ tiêu chuẩn 9 feet', 50000, N'Đang áp dụng'),
(N'Bàn Phăng Carom', N'Bàn bi-a phăng carom 3 băng', 60000, N'Đang áp dụng'),
(N'Bàn Snooker', N'Bàn snooker cỡ lớn (Ngừng áp dụng)', 80000, N'Ngừng áp dụng'),
(N'Bàn VIP Lỗ', N'Bàn bi-a lỗ phòng VIP (Giá cao hơn)', 100000, N'Đang áp dụng');
GO

-- 8. INSERT khach_hang
INSERT INTO khach_hang (ten_kh, sdt, mat_khau, email, ngay_sinh, hang_tv, diem_tich_luy, tong_chi_tieu, lan_den_cuoi)
VALUES
(N'Nguyễn Văn An', '0901234567', 'hashed_pass_kh1', 'nguyenvanan@gmail.com', '1990-05-15', N'Bạch kim', 500, 5000000, '2025-10-20'),
(N'Trần Thị Bình', '0912345678', 'hashed_pass_kh2', 'tranthibinh@gmail.com', '1995-08-20', N'Vàng', 300, 3000000, '2025-10-18'),
(N'Lê Hoàng Cường', '0923456789', 'hashed_pass_kh3', 'lehoangcuong@gmail.com', '1988-12-10', N'Bạc', 150, 1500000, '2025-10-15'),
(N'Phạm Thị Dung', '0934567890', 'hashed_pass_kh4', 'phamthidung@gmail.com', '1992-03-25', N'Đồng', 50, 500000, '2025-10-10'),
(N'Hoàng Văn Em', '0945678901', 'hashed_pass_kh5', 'hoangvanem@gmail.com', '1998-07-18', N'Đồng', 30, 300000, '2025-10-05');
GO

-- 9. INSERT nhan_vien (SỬA: Thêm giá trị UNIQUE cho cả ma_van_tay VÀ faceid_hash)
INSERT INTO nhan_vien (ten_nv, sdt, ma_nhom, luong_co_ban, phu_cap, ca_mac_dinh, trang_thai, mat_khau, ma_van_tay, faceid_hash)
VALUES 
(N'Quản lý Minh', '0971234567', 2, 15000000, 3000000, N'Sáng', N'Đang làm', 'hashed_password_1', 'VT001', 'FACE001');

INSERT INTO nhan_vien (ten_nv, sdt, ma_nhom, luong_co_ban, phu_cap, ca_mac_dinh, trang_thai, mat_khau, ma_van_tay, faceid_hash)
VALUES 
(N'Thu ngân Lan', '0982345678', 3, 8000000, 1000000, N'Sáng', N'Đang làm', 'hashed_password_2', 'VT002', 'FACE002');

INSERT INTO nhan_vien (ten_nv, sdt, ma_nhom, luong_co_ban, phu_cap, ca_mac_dinh, trang_thai, mat_khau, ma_van_tay, faceid_hash)
VALUES 
(N'Phục vụ Hùng', '0993456789', 4, 7000000, 500000, N'Tối', N'Đang làm', 'hashed_password_3', 'VT003', 'FACE003');

INSERT INTO nhan_vien (ten_nv, sdt, ma_nhom, luong_co_ban, phu_cap, ca_mac_dinh, trang_thai, mat_khau, ma_van_tay, faceid_hash)
VALUES 
(N'Phục vụ Linh', '0964567890', 4, 7000000, 500000, N'Chiều', N'Nghỉ', 'hashed_password_4', 'VT004', 'FACE004');
GO

PRINT N'=== ĐÃ THÊM 4 NHÂN VIÊN ==='
GO

-- 10. INSERT mat_hang
INSERT INTO mat_hang (ten_hang, loai, don_vi, gia, so_luong_ton, ma_ncc_default, ngay_nhap_gan_nhat, trang_thai, nguong_canh_bao)
VALUES
(N'Bia Tiger', N'Đồ uống', N'chai', 15000, 100, 1, '2025-10-20', N'Còn hàng', 20),
(N'Khô bò', N'Đồ ăn', N'đĩa', 30000, 50, 2, '2025-10-22', N'Còn hàng', 10),
(N'Đậu phộng', N'Đồ ăn', N'đĩa', 10000, 200, 3, '2025-10-22', N'Ngừng kinh doanh', 20),
(N'Nước ép cam hộp', N'Đồ uống', N'hộp', 18000, 40, 4, '2025-10-24', N'Còn hàng', 10);
GO

-- 11. INSERT dich_vu (Loại 'Dụng cụ bi-a' → 'Khác')
INSERT INTO dich_vu (ten_dv, loai, gia, don_vi, ma_hang, trang_thai, mo_ta)
VALUES
(N'Bia Tiger lạnh', N'Đồ uống', 25000, N'chai', 1, N'Còn hàng', N'Bia Tiger bán lẻ tại quán'),
(N'Khô bò rang muối', N'Đồ ăn', 45000, N'đĩa', 2, N'Còn hàng', N'Món ăn nhẹ phục vụ bàn'),
(N'Thuê cơ VIP', N'Khác', 50000, N'lần', NULL, N'Còn hàng', N'Thuê cơ riêng cao cấp'),
(N'Nước ép cam tươi', N'Đồ uống', 35000, N'cốc', NULL, N'Còn hàng', N'Nước ép chế biến tại quán');
GO

-- 12. INSERT ban_bia
-- 12. INSERT ban_bia (Đã thêm cột hinh_anh và sử dụng giá trị mặc định)
INSERT INTO ban_bia (ten_ban, ma_loai, ma_khu_vuc, trang_thai, gio_bat_dau, ma_kh, ghi_chu, vi_tri_x, vi_tri_y, hinh_anh)
VALUES
(N'Bàn 01', 1, 1, N'Đang chơi', '2025-10-28 09:30:00', 1, N'Khách Bạch kim - Nguyễn Văn An', 10, 10, N'/images/ban_maxim.jpg'),
(N'Bàn 02', 1, 1, N'Trống', NULL, NULL, NULL, 10, 70, N'/images/ban_maxim.jpg'),
(N'Bàn 03', 2, 1, N'Đang chơi', '2025-10-28 10:00:00', 2, N'Khách Vàng - Trần Thị Bình', 10, 130, N'/images/ban_maxim.jpg'),
(N'Bàn VIP 01', 4, 3, N'Đã đặt', NULL, 3, N'Phòng riêng cho khách Lê Hoàng Cường', 300, 10, N'/images/ban_maxim.jpg'),
(N'Bàn 05', 2, 2, N'Bảo trì', NULL, NULL, N'Thay vải', 150, 10, N'/images/ban_maxim.jpg'),
(N'Bàn 04', 1, 1, N'Trống', NULL, NULL, NULL, 10, 190, N'/images/ban_maxim.jpg'),
(N'Bàn 06', 1, 1, N'Đã đặt', NULL, 4, N'Khách Phạm Thị Dung đặt lúc 16:30', 10, 250, N'/images/ban_maxim.jpg'),
(N'Bàn 07', 1, 1, N'Đang chơi', '2025-10-28 11:30:00', NULL, N'Khách lẻ', 10, 310, N'/images/ban_maxim.jpg'),
(N'Bàn 08', 2, 2, N'Trống', NULL, NULL, NULL, 150, 70, N'/images/ban_maxim.jpg'),
(N'Bàn 09', 2, 2, N'Đang chơi', '2025-10-28 12:45:00', 5, N'Khách Đồng - Hoàng Văn Em', 150, 130, N'/images/ban_maxim.jpg'),
(N'Bàn 10', 2, 2, N'Trống', NULL, NULL, NULL, 150, 190, N'/images/ban_maxim.jpg'),
(N'Bàn VIP 02', 4, 3, N'Đang chơi', '2025-10-28 09:00:00', 1, N'Khách Bạch kim - Yêu cầu phục vụ riêng', 300, 70, N'/images/ban_maxim.jpg'),
(N'Bàn VIP 03', 4, 3, N'Trống', NULL, NULL, NULL, 300, 130, N'/images/ban_maxim.jpg');
GO

-- 13. INSERT gia_gio_choi
INSERT INTO gia_gio_choi (khung_gio, gio_bat_dau, gio_ket_thuc, ma_loai, gia, ap_dung_tu_ngay, trang_thai)
VALUES
(N'Sáng (9h-12h) Lỗ 9 bi', '09:00:00', '12:00:00', 1, 50000, '2025-01-01', N'Đang áp dụng'),
(N'Chiều (12h-17h) Lỗ 9 bi', '12:00:00', '17:00:00', 1, 60000, '2025-01-01', N'Đang áp dụng'),
(N'Cao điểm (17h-22h) Phăng Carom', '17:00:00', '22:00:00', 2, 80000, '2025-01-01', N'Đang áp dụng'),
(N'VIP (Cả ngày)', '09:00:00', '22:00:00', 4, 100000, '2025-01-01', N'Đang áp dụng');
GO

-- 14. INSERT dat_ban
INSERT INTO dat_ban (ma_ban, ma_kh, ten_khach, sdt, thoi_gian_dat, so_nguoi, ghi_chu, trang_thai)
VALUES
(4, 3, N'Lê Hoàng Cường', '0923456789', '2025-10-28 15:00:00', 4, N'Phòng VIP 01, yêu cầu nước suối', N'Đã xác nhận'),
(NULL, 5, N'Hoàng Văn Em', '0945678901', '2025-10-29 18:00:00', 2, N'Yêu cầu bàn lỗ, khu vực tầng 2', N'Đang chờ'),
(1, NULL, N'Khách vãng lai A', '0900000001', '2025-10-27 20:00:00', 3, N'Đặt nhầm ngày', N'Đã hủy');
GO

-- 15. INSERT hoa_don
INSERT INTO hoa_don (ma_ban, ma_kh, ma_nv, thoi_gian_bat_dau, thoi_gian_ket_thuc, tien_ban, tien_dich_vu, giam_gia, tong_tien, phuong_thuc_thanh_toan, trang_thai, ghi_chu)
VALUES
(1, 1, 2, '2025-10-28 09:30:00', '2025-10-28 11:00:00', 75000, 50000, 10000, 115000, N'Chuyển khoản', N'Đã thanh toán', N'Khách thanh toán nhanh'),
(3, 2, 2, '2025-10-28 10:00:00', NULL, 0, 0, 0, 0, N'Tiền mặt', N'Đang chơi', NULL);
GO

-- 16. INSERT chi_tiet_hoa_don
INSERT INTO chi_tiet_hoa_don (ma_hd, ma_dv, so_luong, thanh_tien)
VALUES
(1, 1, 2, 50000),
(1, 3, 1, 50000),
(2, 4, 1, 35000);
GO

-- 17. INSERT cham_cong
INSERT INTO cham_cong (ma_nv, ngay, gio_vao, gio_ra, xac_thuc_bang, trang_thai, ghi_chu)
VALUES
(2, '2025-10-28', '2025-10-28 08:58:00', '2025-10-28 17:05:00', N'Vân tay', N'Đúng giờ', NULL),
(3, '2025-10-28', '2025-10-28 17:35:00', NULL, N'Thủ công', N'Đúng giờ', NULL),
(4, '2025-10-28', '2025-10-28 08:05:00', '2025-10-28 17:00:00', N'FaceID', N'Đi trễ', N'Đã trừ 5 phút');
GO

-- 18. INSERT bang_luong
INSERT INTO bang_luong (ma_nv, thang, nam, tong_gio, luong_co_ban, phu_cap, thuong, phat, tong_luong)
VALUES
(2, 9, 2025, 176.5, 8000000, 1000000, 500000, 0, 9500000),
(3, 9, 2025, 175.0, 7000000, 500000, 300000, 50000, 7750000);
GO

-- 19. INSERT lich_su_hoat_dong
INSERT INTO lich_su_hoat_dong (ma_nv, hanh_dong, chi_tiet)
VALUES
(1, N'Cập nhật giá', N'Quản lý Minh cập nhật giá giờ chơi loại 1'),
(2, N'Thanh toán', N'Thu ngân Lan hoàn tất hoá đơn #1 cho Bàn 01. Tổng tiền: 115,000 VND'),
(3, N'Thao tác bàn', N'Phục vụ Hùng mở bàn 03 cho khách Trần Thị Bình');
GO

-- 20. INSERT phieu_nhap
INSERT INTO phieu_nhap (ma_nv, ma_ncc, ngay_nhap, tong_tien, ghi_chu)
VALUES 
(1, 1, '2025-10-29', 750000, N'Nhập thêm 50 Tiger');
GO

-- 21. INSERT chi_tiet_phieu_nhap
INSERT INTO chi_tiet_phieu_nhap (ma_pn, ma_hang, so_luong_nhap, don_gia_nhap)
VALUES 
(1, 1, 50, 15000);
GO

-- 22. INSERT so_quy
INSERT INTO so_quy (ma_nv, ngay_lap, loai_phieu, so_tien, ly_do, ma_hd_lien_quan)
VALUES
(2, '2025-10-28 11:01:00', N'Thu', 115000, N'Thu tiền thanh toán hóa đơn #1 (Bàn 01)', 1),
(1, '2025-10-29 10:00:00', N'Chi', 750000, N'Chi tiền mặt nhập hàng (PN #1)', NULL);
GO

