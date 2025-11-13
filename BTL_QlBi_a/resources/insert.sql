USE QL_QuanBi_a;
GO

-- =============================================
-- 0. LỆNH DỌN DẸP DỮ LIỆU CŨ (An toàn khi chạy lại)
-- =============================================

-- Tắt Identity Insert nếu có đang bật (để đảm bảo)
SET IDENTITY_INSERT hoa_don OFF;
SET IDENTITY_INSERT chi_tiet_hoa_don OFF;
SET IDENTITY_INSERT so_quy OFF;

-- Xóa các bảng phụ thuộc vào ID
DELETE FROM lich_su_hoat_dong;
DELETE FROM bang_luong;
DELETE FROM cham_cong;
DELETE FROM chi_tiet_phieu_nhap;
DELETE FROM phieu_nhap;
DELETE FROM chi_tiet_hoa_don; 
DELETE FROM so_quy;
DELETE FROM hoa_don; 
DELETE FROM dich_vu;
DELETE FROM mat_hang;
DELETE FROM dat_ban;
DELETE FROM ban_bia;

-- Xóa các bảng chính
DELETE FROM khach_hang;
DELETE FROM nhan_vien;
DELETE FROM phan_quyen;
DELETE FROM chuc_nang;
DELETE FROM nhom_quyen;
DELETE FROM gia_gio_choi;
DELETE FROM loai_ban;
DELETE FROM khu_vuc;
DELETE FROM nha_cung_cap;
GO

-- Đặt lại giá trị IDENTITY cho các bảng
DBCC CHECKIDENT ('khu_vuc', RESEED, 0);
DBCC CHECKIDENT ('nha_cung_cap', RESEED, 0);
DBCC CHECKIDENT ('nhom_quyen', RESEED, 0);
DBCC CHECKIDENT ('loai_ban', RESEED, 0);
DBCC CHECKIDENT ('khach_hang', RESEED, 0);
DBCC CHECKIDENT ('nhan_vien', RESEED, 0);
DBCC CHECKIDENT ('mat_hang', RESEED, 0);
DBCC CHECKIDENT ('dich_vu', RESEED, 0);
DBCC CHECKIDENT ('ban_bia', RESEED, 0);
DBCC CHECKIDENT ('gia_gio_choi', RESEED, 0);
DBCC CHECKIDENT ('dat_ban', RESEED, 0);
DBCC CHECKIDENT ('phieu_nhap', RESEED, 0);
DBCC CHECKIDENT ('cham_cong', RESEED, 0);
DBCC CHECKIDENT ('bang_luong', RESEED, 0);
DBCC CHECKIDENT ('lich_su_hoat_dong', RESEED, 0);
GO


-- =============================================
-- 1. KHU VỰC 
-- =============================================
INSERT INTO khu_vuc (ten_khu_vuc, mo_ta) VALUES 
(N'Tầng 1', N'Khu vực tầng 1 - Phổ thông'), -- ma_khu_vuc = 1
(N'Tầng 2', N'Khu vực tầng 2 - Rộng rãi'), -- ma_khu_vuc = 2
(N'VIP', N'Khu vực VIP - Phòng riêng biệt'); -- ma_khu_vuc = 3
GO


-- =============================================
-- 2. NHÀ CUNG CẤP
-- =============================================
INSERT INTO nha_cung_cap (ten_ncc, sdt, dia_chi, email) VALUES
(N'Heineken Việt Nam', '02838222755', N'Quận 1, TP. HCM', 'contact@heinekenvn.com'),
(N'Sabeco - Bia Sài Gòn', '02838521433', N'Quận 1, TP. HCM', 'info@sabeco.com.vn'),
(N'Công ty Thực phẩm ABC', '0909123456', N'Bình Dương', 'abc@food.vn'),
(N'Nhà cung cấp đồ ăn XYZ', '0909789012', N'Đồng Nai', 'xyz@supplier.vn'),
(N'Công ty Nước giải khát Fresh', '02838945900', N'Quận 3, TP. HCM', 'fresh@drinks.vn'),
(N'Nhà phân phối dụng cụ Billiard Pro', '0912345678', N'Quận 10, TP. HCM', 'pro@billiard.vn');
GO


-- =============================================
-- 3. NHÓM QUYỀN
-- =============================================
INSERT INTO nhom_quyen (ten_nhom) VALUES 
(N'Admin'),
(N'Quản lý'),
(N'Thu ngân'),
(N'Phục vụ');
GO


-- =============================================
-- 4. CHỨC NĂNG
-- =============================================
INSERT INTO chuc_nang (ma_cn, ten_cn, mo_ta) VALUES 
('VIEW_REPORT', N'Xem báo cáo doanh thu', N'Xem các báo cáo thống kê'),
('EDIT_PRICE', N'Chỉnh sửa giá dịch vụ', N'Thay đổi giá bàn, dịch vụ'),
('MANAGE_STAFF', N'Quản lý nhân viên', N'Thêm, sửa, xóa nhân viên'),
('MANAGE_PERMISSION', N'Quản lý phân quyền', N'Phân quyền cho nhóm'),
('MANAGE_INVENTORY', N'Quản lý kho', N'Nhập hàng, kiểm kho'),
('MANAGE_SETTINGS', N'Truy cập cài đặt', N'Cài đặt hệ thống'),
('PERFORM_PAYMENT', N'Thực hiện thanh toán', N'Thu tiền khách hàng'),
('APPLY_DISCOUNT', N'Áp dụng giảm giá', N'Giảm giá hóa đơn'),
('MANAGE_TABLE', N'Quản lý bàn', N'Mở/đóng bàn, chuyển bàn'),
('VIEW_CUSTOMER', N'Xem thông tin khách hàng', N'Tra cứu khách hàng');
GO

-- =============================================
-- 5. PHÂN QUYỀN
-- =============================================

-- Admin (ma_nhom = 1) - Có tất cả quyền
INSERT INTO phan_quyen (ma_nhom, ma_cn) 
SELECT 1, ma_cn FROM chuc_nang;
GO

-- Quản lý (ma_nhom = 2)
INSERT INTO phan_quyen (ma_nhom, ma_cn) VALUES
(2, 'VIEW_REPORT'), (2, 'EDIT_PRICE'), (2, 'MANAGE_STAFF'), 
(2, 'MANAGE_INVENTORY'), (2, 'PERFORM_PAYMENT'), (2, 'APPLY_DISCOUNT'),
(2, 'MANAGE_TABLE'), (2, 'VIEW_CUSTOMER');
GO

-- Thu ngân (ma_nhom = 3)
INSERT INTO phan_quyen (ma_nhom, ma_cn) VALUES
(3, 'PERFORM_PAYMENT'), (3, 'APPLY_DISCOUNT'), (3, 'MANAGE_TABLE'), (3, 'VIEW_CUSTOMER');
GO

-- Phục vụ (ma_nhom = 4)
INSERT INTO phan_quyen (ma_nhom, ma_cn) VALUES
(4, 'MANAGE_TABLE'), (4, 'VIEW_CUSTOMER');
GO


-- =============================================
-- 6. LOẠI BÀN
-- =============================================
INSERT INTO loai_ban (ten_loai, mo_ta, gia_gio, trang_thai) VALUES
(N'Bàn Lỗ 9 bi', N'Bàn bi-a lỗ cỡ tiêu chuẩn 9 feet', 50000, N'Đang áp dụng'), -- ma_loai = 1
(N'Bàn Phăng Carom', N'Bàn bi-a phăng carom 3 băng', 60000, N'Đang áp dụng'), -- ma_loai = 2
(N'Bàn Snooker', N'Bàn snooker cỡ lớn 12 feet', 80000, N'Ngừng áp dụng'), -- ma_loai = 3
(N'Bàn VIP Lỗ', N'Bàn bi-a lỗ phòng VIP', 100000, N'Đang áp dụng'), -- ma_loai = 4
(N'Bàn VIP Phăng', N'Bàn phăng phòng VIP', 120000, N'Đang áp dụng'); -- ma_loai = 5
GO
-- =============================================
-- 7. KHÁCH HÀNG
-- =============================================
INSERT INTO khach_hang (ten_kh, sdt, mat_khau, email, ngay_sinh, hang_tv, diem_tich_luy, tong_chi_tieu, ngay_dang_ky, lan_den_cuoi) VALUES
(N'Nguyễn Văn An', '0901234567', 'hashed_pass_001', 'nguyenvanan@gmail.com', '1990-05-15', N'Bạch kim', 500, 5000000, '2024-01-15', '2025-10-28'),
(N'Trần Thị Bình', '0912345678', 'hashed_pass_002', 'tranthibinh@gmail.com', '1995-08-20', N'Vàng', 300, 3000000, '2024-03-20', '2025-10-27'),
(N'Lê Hoàng Cường', '0923456789', 'hashed_pass_003', 'lehoangcuong@gmail.com', '1988-12-10', N'Bạc', 150, 1500000, '2024-06-10', '2025-10-26'),
(N'Phạm Thị Dung', '0934567890', 'hashed_pass_004', 'phamthidung@gmail.com', '1992-03-25', N'Đồng', 50, 500000, '2024-08-05', '2025-10-25'),
(N'Hoàng Văn Em', '0945678901', 'hashed_pass_005', 'hoangvanem@gmail.com', '1998-07-18', N'Đồng', 30, 300000, '2024-09-12', '2025-10-24'),
(N'Vũ Minh Khang', '0956789012', 'hashed_pass_006', 'vuminhkhang@gmail.com', '1991-11-30', N'Bạc', 180, 1800000, '2024-04-15', '2025-10-28'),
(N'Đặng Thu Hà', '0967890123', 'hashed_pass_007', 'dangthuha@gmail.com', '1994-06-08', N'Vàng', 250, 2500000, '2024-02-20', '2025-10-27'),
(N'Trịnh Quốc Dũng', '0978901234', 'hashed_pass_008', 'trinhquocdung@gmail.com', '1989-09-22', N'Bạch kim', 600, 6000000, '2023-12-10', '2025-10-28'),
(N'Ngô Thị Mai', '0989012345', 'hashed_pass_009', 'ngothimai@gmail.com', '1996-04-15', N'Đồng', 80, 800000, '2024-07-18', '2025-10-23'),
(N'Bùi Văn Tú', '0990123456', 'hashed_pass_010', 'buivantu@gmail.com', '1993-02-28', N'Bạc', 120, 1200000, '2024-05-25', '2025-10-26');
GO

-- =============================================
-- 8. NHÂN VIÊN
-- =============================================
INSERT INTO nhan_vien (ten_nv, sdt, ma_nhom, luong_co_ban, phu_cap, ca_mac_dinh, trang_thai, mat_khau, ma_van_tay, faceid_hash, email, faceid_anh) VALUES 
(N'Admin Hệ Thống', '0971111111', 1, 20000000, 5000000, N'Sáng', N'Đang làm', 'admin_pass', 'VT_ADMIN', 'FACE_ADMIN', 'admin@billiards.com', '/faces/admin.jpg'),
(N'Quản lý Minh', '0971234567', 2, 15000000, 3000000, N'Sáng', N'Đang làm', 'ql_minh_pass', 'VT001', 'FACE001', 'ql.minh@billiards.com', '/faces/qlminh.jpg'),
(N'Thu ngân Lan', '0982345678', 3, 8000000, 1000000, N'Sáng', N'Đang làm', 'tn_lan_pass', 'VT002', 'FACE002', 'tn.lan@billiards.com', '/faces/tnlan.jpg'),
(N'Thu ngân Hương', '0983456789', 3, 8000000, 1000000, N'Chiều', N'Đang làm', 'tn_huong_pass', 'VT003', 'FACE003', 'tn.huong@billiards.com', '/faces/tnhuong.jpg'),
(N'Phục vụ Hùng', '0993456789', 4, 7000000, 500000, N'Tối', N'Đang làm', 'pv_hung_pass', 'VT004', 'FACE004', 'pv.hung@billiards.com', '/faces/pvhung.jpg'),
(N'Phục vụ Linh', '0964567890', 4, 7000000, 500000, N'Chiều', N'Đang làm', 'pv_linh_pass', 'VT005', 'FACE005', 'pv.linh@billiards.com', '/faces/pvlinh.jpg'),
(N'Phục vụ Nam', '0965678901', 4, 7000000, 500000, N'Sáng', N'Đang làm', 'pv_nam_pass', 'VT006', 'FACE006', 'pv.nam@billiards.com', '/faces/pvnam.jpg'),
(N'Phục vụ Nga', '0976789012', 4, 7000000, 500000, N'Tối', N'Nghỉ', 'pv_nga_pass', 'VT007', 'FACE007', 'pv.nga@billiards.com', '/faces/pvnga.jpg');
GO


-- =============================================
-- 9. MẶT HÀNG
-- =============================================
INSERT INTO mat_hang (ten_hang, loai, don_vi, gia, so_luong_ton, ma_ncc_default, ngay_nhap_gan_nhat, trang_thai, nguong_canh_bao, mo_ta, hinh_anh) VALUES
-- Đồ uống
(N'Bia Tiger', N'Đồ uống', N'chai', 15000, 200, 1, '2025-10-25', N'Còn hàng', 30, N'Bia Tiger 330ml', 'img/tiger.jpg'),
(N'Bia Heineken', N'Đồ uống', N'chai', 18000, 150, 1, '2025-10-25', N'Còn hàng', 30, N'Bia Heineken 330ml', 'img/heineken.jpg'),
(N'Bia Sài Gòn', N'Đồ uống', N'chai', 12000, 250, 2, '2025-10-26', N'Còn hàng', 40, N'Bia Sài Gòn 330ml', 'img/saigon.jpg'),
(N'Coca Cola', N'Đồ uống', N'lon', 10000, 180, 5, '2025-10-27', N'Còn hàng', 30, N'Coca Cola 330ml', 'img/coca.jpg'),
(N'Pepsi', N'Đồ uống', N'lon', 10000, 160, 5, '2025-10-27', N'Còn hàng', 30, N'Pepsi 330ml', 'img/pepsi.jpg'),
(N'Nước suối Lavie', N'Đồ uống', N'chai', 5000, 300, 5, '2025-10-28', N'Còn hàng', 50, N'Nước suối 500ml', 'img/nuocsuoi.jpg'),
(N'Nước cam ép', N'Đồ uống', N'hộp', 18000, 80, 5, '2025-10-26', N'Còn hàng', 15, N'Nước cam ép hộp 1L', 'img/nuoccamhop.jpg'),
(N'Sting dâu', N'Đồ uống', N'chai', 12000, 120, 5, '2025-10-27', N'Còn hàng', 20, N'Sting 330ml', 'img/sting.jpg'),
(N'Trà xanh C2', N'Đồ uống', N'chai', 8000, 140, 5, '2025-10-27', N'Còn hàng', 25, N'Trà xanh C2 455ml', 'img/traxanh.jpg'),
-- Đồ ăn
(N'Khô bò', N'Đồ ăn', N'đĩa', 30000, 100, 3, '2025-10-24', N'Còn hàng', 15, N'Khô bò tươi', 'img/khobo.jpg'),
(N'Đậu phộng rang', N'Đồ ăn', N'đĩa', 10000, 200, 3, '2025-10-25', N'Còn hàng', 30, N'Đậu phộng rang muối', 'img/dauphong.jpg'),
(N'Mực nướng', N'Đồ ăn', N'đĩa', 35000, 70, 4, '2025-10-26', N'Còn hàng', 12, N'Mực nướng sa tế', 'img/mucnuong.jpg'),
(N'Nem chua rán', N'Đồ ăn', N'đĩa', 25000, 60, 4, '2025-10-26', N'Còn hàng', 10, N'Nem chua Thanh Hóa rán', 'img/nemchua.jpg'),
(N'Chân gà sả tắc', N'Đồ ăn', N'đĩa', 40000, 50, 4, '2025-10-25', N'Còn hàng', 10, N'Chân gà sả tắc cay', 'img/changa.jpg'),
(N'Khoai tây chiên', N'Đồ ăn', N'đĩa', 20000, 90, 3, '2025-10-27', N'Còn hàng', 15, N'Khoai tây chiên giòn', 'img/khoaitay.jpg'),
(N'Tôm chiên giòn', N'Đồ ăn', N'đĩa', 50000, 40, 4, '2025-10-24', N'Còn hàng', 8, N'Tôm chiên bột', 'img/tomchien.jpg'),
-- Dụng cụ bi-a
(N'Cơ bi-a cao cấp', N'Dụng cụ bi-a', N'cái', 500000, 20, 6, '2025-10-20', N'Còn hàng', 5, N'Cơ bi-a nhập khẩu', 'img/cobiacap.jpg'),
(N'Phấn bi-a', N'Dụng cụ bi-a', N'hộp', 50000, 80, 6, '2025-10-22', N'Còn hàng', 10, N'Phấn bi-a xanh', 'img/phanbia.jpg'),
(N'Bộ bi lỗ', N'Dụng cụ bi-a', N'bộ', 800000, 15, 6, '2025-10-15', N'Còn hàng', 3, N'Bộ bi lỗ chuẩn', 'img/bilochuan.jpg'),
(N'Bộ bi phăng', N'Dụng cụ bi-a', N'bộ', 1200000, 10, 6, '2025-10-15', N'Còn hàng', 3, N'Bộ bi carom', 'img/biphang.jpg');
GO


-- =============================================
-- 10. DỊCH VỤ
-- =============================================
INSERT INTO dich_vu (ten_dv, loai, gia, don_vi, ma_hang, trang_thai, mo_ta, hinh_anh) VALUES
-- Đồ uống (liên kết với mặt hàng)
(N'Bia Tiger lạnh', N'Đồ uống', 25000, N'chai', 1, N'Còn hàng', N'Bia Tiger bán lẻ', N'biatiger.jpg'),
(N'Bia Heineken lạnh', N'Đồ uống', 30000, N'chai', 2, N'Còn hàng', N'Bia Heineken bán lẻ', N'/biaheineken.jpg'),
(N'Bia Sài Gòn lạnh', N'Đồ uống', 20000, N'chai', 3, N'Còn hàng', N'Bia Sài Gòn bán lẻ', N'biasaigon.jpg'),
(N'Coca Cola', N'Đồ uống', 15000, N'lon', 4, N'Còn hàng', N'Nước ngọt', N'cocacola.jpg'),
(N'Pepsi', N'Đồ uống', 15000, N'lon', 5, N'Còn hàng', N'Nước ngọt', N'pepsi.jpg'),
(N'Nước suối', N'Đồ uống', 8000, N'chai', 6, N'Còn hàng', N'Nước suối lạnh', N'nuocsuoilavie.jpg'),
(N'Nước cam ép tươi', N'Đồ uống', 35000, N'cốc', NULL, N'Còn hàng', N'Nước cam ép tại quán', N'nuoccamep.jpg'),
(N'Sting', N'Đồ uống', 18000, N'chai', 8, N'Còn hàng', N'Nước tăng lực', N'stingdau.jpg'),
(N'Trà xanh', N'Đồ uống', 12000, N'chai', 9, N'Còn hàng', N'Trà xanh C2', N'traxanhc2.jpg'),
-- Đồ ăn (liên kết với mặt hàng)
(N'Khô bò rang muối', N'Đồ ăn', 45000, N'đĩa', 10, N'Còn hàng', N'Khô bò tươi', N'khobo.jpg'),
(N'Đậu phộng', N'Đồ ăn', 15000, N'đĩa', 11, N'Còn hàng', N'Đậu phộng rang muối', N'dauphongrang.jpg'),
(N'Mực nướng sa tế', N'Đồ ăn', 55000, N'đĩa', 12, N'Còn hàng', N'Mực nướng thơm ngon', N'mucnuong.jpg'),
(N'Nem chua rán', N'Đồ ăn', 40000, N'đĩa', 13, N'Còn hàng', N'Nem chua rán giòn', N'nemchuaran.jpg'),
(N'Chân gà sả tắc', N'Đồ ăn', 60000, N'đĩa', 14, N'Còn hàng', N'Chân gà cay nồng', N'changasatac.jpg'),
(N'Khoai tây chiên', N'Đồ ăn', 30000, N'đĩa', 15, N'Còn hàng', N'Khoai tây chiên', N'khoaitaychien.jpg'),
(N'Tôm chiên giòn', N'Đồ ăn', 70000, N'đĩa', 16, N'Còn hàng', N'Tôm tươi chiên bột', N'tomchiengion.jpg'),
-- Dịch vụ khác (không liên kết mặt hàng)
(N'Thuê cơ VIP', N'Khác', 50000, N'lần', NULL, N'Còn hàng', N'Thuê cơ cao cấp', N'cobiaaoccap.jpg'),
(N'Thuê cơ thường', N'Khác', 20000, N'lần', NULL, N'Còn hàng', N'Thuê cơ phổ thông', N'cobia.jpg'),
(N'Thuê phấn', N'Khác', 10000, N'lần', NULL, N'Còn hàng', N'Thuê phấn bi-a', N'img/phanbia.jpg'),
(N'Phục vụ riêng VIP', N'Khác', 100000, N'giờ', NULL, N'Còn hàng', N'Phục vụ riêng phòng VIP', NULL);
GO
-- ====


-- =============================================
-- 11. BÀN BI-A (Cập nhật tọa độ)
-- Vị trí dùng % (DECIMAL(5,2)) cho phù hợp với logic /images/
-- =============================================
INSERT INTO ban_bia (ten_ban, ma_loai, ma_khu_vuc, trang_thai, gio_bat_dau, ma_kh, ghi_chu, vi_tri_x, vi_tri_y, hinh_anh) VALUES
-- Tầng 1 (ma_khu_vuc = 1): 12 bàn (2 hàng x 6 cột)
-- Hàng 1 (Y: 10-40%)
(N'Bàn 01', 1, 1, N'Đang chơi', '2025-10-28 09:30:00', 1, N'Khách VIP Nguyễn Văn An', 5.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 02', 1, 1, N'Trống', NULL, NULL, NULL, 20.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 03', 2, 1, N'Đang chơi', '2025-10-28 10:00:00', 2, N'Khách Trần Thị Bình', 35.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 04', 1, 1, N'Trống', NULL, NULL, NULL, 50.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 05', 1, 1, N'Đã đặt', NULL, 4, N'Đặt lúc 16:30', 65.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 06', 2, 1, N'Đang chơi', '2025-10-28 11:30:00', NULL, N'Khách lẻ', 80.00, 10.00, N'ban_maxim.jpg'),
-- Hàng 2 (Y: 50-80%)
(N'Bàn 07', 1, 1, N'Trống', NULL, NULL, NULL, 5.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 08', 1, 1, N'Trống', NULL, NULL, NULL, 20.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 09', 2, 1, N'Đang chơi', '2025-10-28 13:00:00', 6, N'Khách Vũ Minh Khang', 35.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 10', 1, 1, N'Trống', NULL, NULL, NULL, 50.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 11', 2, 1, N'Trống', NULL, NULL, NULL, 65.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 12', 1, 1, N'Bảo trì', NULL, NULL, N'Thay vải bàn', 80.00, 60.00, N'ban_maxim.jpg'),
-- Tầng 2 (ma_khu_vuc = 2): 10 bàn (2 hàng x 5 cột)
-- Hàng 1 (Y: 10-40%)
(N'Bàn 13', 1, 2, N'Trống', NULL, NULL, NULL, 5.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 14', 2, 2, N'Đang chơi', '2025-10-28 12:45:00', 5, N'Khách Hoàng Văn Em', 25.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 15', 2, 2, N'Trống', NULL, NULL, NULL, 45.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 16', 1, 2, N'Trống', NULL, NULL, NULL, 65.00, 10.00, N'ban_maxim.jpg'),
(N'Bàn 17', 2, 2, N'Đã đặt', NULL, 7, N'Đặt cho tối nay', 85.00, 10.00, N'ban_maxim.jpg'),
-- Hàng 2 (Y: 50-80%)
(N'Bàn 18', 1, 2, N'Trống', NULL, NULL, NULL, 5.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 19', 1, 2, N'Trống', NULL, NULL, NULL, 25.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 20', 2, 2, N'Đang chơi', '2025-10-28 14:00:00', 9, N'Khách Ngô Thị Mai', 45.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 21', 1, 2, N'Trống', NULL, NULL, NULL, 65.00, 60.00, N'ban_maxim.jpg'),
(N'Bàn 22', 2, 2, N'Bảo trì', NULL, NULL, N'Sửa chân bàn', 85.00, 60.00, N'ban_maxim.jpg'),
-- VIP (ma_khu_vuc = 3): 5 bàn (1 hàng x 5 cột)
(N'Bàn VIP 01', 4, 3, N'Đang chơi', '2025-10-28 09:00:00', 1, N'Khách VIP - Phục vụ riêng', 5.00, 35.00, N'ban_vip.jpg'),
(N'Bàn VIP 02', 4, 3, N'Đã đặt', NULL, 3, N'Đặt phòng 18:00', 25.00, 35.00, N'ban_vip.jpg'),
(N'Bàn VIP 03', 5, 3, N'Trống', NULL, NULL, NULL, 45.00, 35.00, N'ban_vip.jpg'),
(N'Bàn VIP 04', 4, 3, N'Đang chơi', '2025-10-28 15:00:00', 8, N'Khách Trịnh Quốc Dũng', 65.00, 35.00, N'ban_vip.jpg'),
(N'Bàn VIP 05', 5, 3, N'Trống', NULL, NULL, NULL, 85.00, 35.00, N'ban_vip.jpg');
GO


-- =============================================
-- 12. GIÁ GIỜ CHƠI
-- =============================================
INSERT INTO gia_gio_choi (khung_gio, gio_bat_dau, gio_ket_thuc, ma_loai, gia, ap_dung_tu_ngay, trang_thai) VALUES
-- Bàn Lỗ 9 bi (ma_loai = 1)
(N'Sáng (9h-12h)', '09:00:00', '12:00:00', 1, 50000, '2025-01-01', N'Đang áp dụng'),
(N'Chiều (12h-17h)', '12:00:00', '17:00:00', 1, 60000, '2025-01-01', N'Đang áp dụng'),
(N'Tối (17h-22h)', '17:00:00', '22:00:00', 1, 70000, '2025-01-01', N'Đang áp dụng'),
(N'Khuya (22h-2h)', '22:00:00', '02:00:00', 1, 80000, '2025-01-01', N'Đang áp dụng'),
-- Bàn Phăng Carom (ma_loai = 2)
(N'Sáng (9h-12h)', '09:00:00', '12:00:00', 2, 60000, '2025-01-01', N'Đang áp dụng'),
(N'Chiều (12h-17h)', '12:00:00', '17:00:00', 2, 70000, '2025-01-01', N'Đang áp dụng'),
(N'Tối (17h-22h)', '17:00:00', '22:00:00', 2, 90000, '2025-01-01', N'Đang áp dụng'),
(N'Khuya (22h-2h)', '22:00:00', '02:00:00', 2, 100000, '2025-01-01', N'Đang áp dụng'),
-- Bàn VIP Lỗ (ma_loai = 4)
(N'Cả ngày', '09:00:00', '23:00:00', 4, 120000, '2025-01-01', N'Đang áp dụng'),
-- Bàn VIP Phăng (ma_loai = 5)
(N'Cả ngày', '09:00:00', '23:00:00', 5, 150000, '2025-01-01', N'Đang áp dụng');
GO


-- =============================================
-- 13. ĐẶT BÀN
-- =============================================
INSERT INTO dat_ban (ma_ban, ma_kh, ten_khach, sdt, thoi_gian_dat, so_nguoi, ghi_chu, trang_thai, ngay_tao) VALUES
(23, 3, N'Lê Hoàng Cường', '0923456789', '2025-10-28 18:00:00', 4, N'Phòng VIP 02, yêu cầu nước suối', N'Đã xác nhận', '2025-10-27 10:00:00'),
(17, 7, N'Đặng Thu Hà', '0967890123', '2025-10-28 19:30:00', 3, N'Bàn 17 tầng 2', N'Đã xác nhận', '2025-10-27 14:30:00'),
(NULL, 5, N'Hoàng Văn Em', '0945678901', '2025-10-29 18:00:00', 2, N'Yêu cầu bàn lỗ tầng 2', N'Đang chờ', '2025-10-28 09:00:00'),
(NULL, NULL, N'Khách vãng lai A', '0900000001', '2025-10-27 20:00:00', 3, N'Đặt nhầm ngày', N'Đã hủy', '2025-10-26 15:00:00'),
(NULL, 6, N'Vũ Minh Khang', '0956789012', '2025-10-29 14:00:00', 4, N'Bàn phăng, khu tầng 1', N'Đang chờ', '2025-10-28 11:00:00'),
(NULL, 8, N'Trịnh Quốc Dũng', '0978901234', '2025-10-30 10:00:00', 6, N'VIP, có tiệc nhỏ', N'Đã xác nhận', '2025-10-28 16:00:00'),
(NULL, 1, N'Nguyễn Văn An', '0901234567', '2025-10-30 20:00:00', 6, N'Đặt bàn VIP cho tiệc sinh nhật', N'Đã xác nhận', '2025-10-28 17:00:00'),
(NULL, 11, N'Cao Thị Phương', '0911111111', '2025-10-29 15:00:00', 3, N'Khách mới, bàn lỗ thường', N'Đang chờ', '2025-10-28 18:00:00'),
(NULL, 12, N'Lý Văn Đạt', '0922222222', '2025-10-31 18:30:00', 4, N'Yêu cầu bàn phăng', N'Đang chờ', '2025-10-28 19:00:00');
GO


-- =============================================
-- 14. HÓA ĐƠN
-- =============================================
SET IDENTITY_INSERT hoa_don ON;
-- Hóa đơn đã thanh toán (27/10)
INSERT INTO hoa_don (ma_hd, ma_ban, ma_kh, ma_nv, thoi_gian_bat_dau, thoi_gian_ket_thuc, tien_ban, tien_dich_vu, giam_gia, tong_tien, phuong_thuc_thanh_toan, trang_thai, ghi_chu_giam_gia, ghi_chu) VALUES
(1, 1, 1, 3, '2025-10-27 09:30:00', '2025-10-27 11:30:00', 100000, 140000, 20000, 220000, N'Chuyển khoản', N'Đã thanh toán', N'Giảm 20k cho khách VIP', N'Khách thanh toán nhanh'),
(2, 3, 2, 3, '2025-10-27 10:00:00', '2025-10-27 13:00:00', 180000, 85000, 0, 265000, N'Tiền mặt', N'Đã thanh toán', NULL, NULL),
(3, 14, 5, 4, '2025-10-27 14:00:00', '2025-10-27 16:30:00', 150000, 120000, 10000, 260000, N'Ví điện tử', N'Đã thanh toán', N'Giảm 10k - khuyến mãi', NULL),
(4, 25, 8, 3, '2025-10-27 16:00:00', '2025-10-27 19:00:00', 360000, 280000, 50000, 590000, N'Chuyển khoản', N'Đã thanh toán', N'Giảm 50k - khách VIP', N'Phục vụ tốt'),
(5, 9, 6, 5, '2025-10-27 18:00:00', '2025-10-27 20:30:00', 150000, 95000, 0, 245000, N'Tiền mặt', N'Đã thanh toán', NULL, NULL);

-- Hóa đơn đang chơi (28/10)
INSERT INTO hoa_don (ma_hd, ma_ban, ma_kh, ma_nv, thoi_gian_bat_dau, thoi_gian_ket_thuc, tien_ban, tien_dich_vu, giam_gia, tong_tien, phuong_thuc_thanh_toan, trang_thai, ghi_chu_giam_gia, ghi_chu) VALUES
(6, 1, 1, 3, '2025-10-28 09:30:00', NULL, 0, 50000, 0, 50000, N'Tiền mặt', N'Đang chơi', NULL, N'Đã gọi 2 Tiger'),
(7, 3, 2, 3, '2025-10-28 10:00:00', NULL, 0, 95000, 0, 95000, N'Tiền mặt', N'Đang chơi', NULL, N'Gọi thêm khô bò và Heineken'),
(8, 6, NULL, 5, '2025-10-28 11:30:00', NULL, 0, 30000, 0, 30000, N'Tiền mặt', N'Đang chơi', NULL, N'Khách lẻ'),
(9, 9, 6, 4, '2025-10-28 13:00:00', NULL, 0, 60000, 0, 60000, N'Tiền mặt', N'Đang chơi', NULL, NULL),
(10, 14, 5, 5, '2025-10-28 12:45:00', NULL, 0, 85000, 0, 85000, N'Tiền mặt', N'Đang chơi', NULL, NULL),
(11, 22, 1, 3, '2025-10-28 09:00:00', NULL, 0, 220000, 0, 220000, N'Chuyển khoản', N'Đang chơi', NULL, N'VIP - Phục vụ riêng'),
(12, 25, 8, 3, '2025-10-28 15:00:00', NULL, 0, 150000, 0, 150000, N'Tiền mặt', N'Đang chơi', NULL, NULL),
(14, 20, 9, 5, '2025-10-28 14:00:00', NULL, 0, 90000, 0, 90000, N'Tiền mặt', N'Đang chơi', NULL, NULL);

-- Hóa đơn đã thanh toán cũ (24-26/10)
INSERT INTO hoa_don (ma_hd, ma_ban, ma_kh, ma_nv, thoi_gian_bat_dau, thoi_gian_ket_thuc, tien_ban, tien_dich_vu, giam_gia, tong_tien, phuong_thuc_thanh_toan, trang_thai, ghi_chu) VALUES
(15, 2, NULL, 3, '2025-10-24 10:00:00', '2025-10-24 12:30:00', 125000, 60000, 0, 185000, N'Tiền mặt', N'Đã thanh toán', N'Khách lẻ'),
(16, 7, 6, 3, '2025-10-24 14:00:00', '2025-10-24 16:00:00', 120000, 85000, 10000, 195000, N'Chuyển khoản', N'Đã thanh toán', NULL),
(17, 15, NULL, 4, '2025-10-24 18:00:00', '2025-10-24 20:30:00', 150000, 145000, 0, 295000, N'Tiền mặt', N'Đã thanh toán', N'Khách lẻ'),
(18, 4, 7, 3, '2025-10-25 09:00:00', '2025-10-25 11:00:00', 100000, 70000, 5000, 165000, N'Ví điện tử', N'Đã thanh toán', NULL),
(19, 9, NULL, 4, '2025-10-25 13:00:00', '2025-10-25 15:30:00', 150000, 110000, 0, 260000, N'Tiền mặt', N'Đã thanh toán', N'Khách lẻ'),
(20, 16, 2, 3, '2025-10-25 16:00:00', '2025-10-25 18:00:00', 120000, 120000, 15000, 225000, N'Chuyển khoản', N'Đã thanh toán', N'Giảm giá khách quen'),
(21, 23, 8, 3, '2025-10-25 19:00:00', '2025-10-25 22:00:00', 360000, 260000, 50000, 570000, N'Thẻ', N'Đã thanh toán', N'VIP - Giảm giá đặc biệt'),
(22, 1, 1, 3, '2025-10-26 08:30:00', '2025-10-26 10:30:00', 100000, 110000, 10000, 200000, N'Chuyển khoản', N'Đã thanh toán', NULL),
(23, 5, NULL, 4, '2025-10-26 11:00:00', '2025-10-26 13:00:00', 100000, 88000, 0, 188000, N'Tiền mặt', N'Đã thanh toán', N'Khách lẻ'),
(24, 13, 9, 5, '2025-10-26 14:30:00', '2025-10-26 17:00:00', 150000, 145000, 0, 295000, N'Ví điện tử', N'Đã thanh toán', NULL),
(25, 19, 10, 4, '2025-10-26 18:00:00', '2025-10-26 20:00:00', 120000, 120000, 5000, 235000, N'Tiền mặt', N'Đã thanh toán', NULL);
GO
SET IDENTITY_INSERT hoa_don OFF;


-- =============================================
-- 15. CHI TIẾT HÓA ĐƠN
-- =============================================
SET IDENTITY_INSERT chi_tiet_hoa_don ON;
INSERT INTO chi_tiet_hoa_don (id, ma_hd, ma_dv, so_luong, thanh_tien) VALUES
-- Chi tiết hóa đơn #1 -> #12
(1, 1, 1, 3, 75000), (2, 1, 10, 1, 45000), (3, 1, 18, 1, 20000),
(4, 2, 3, 2, 40000), (5, 2, 11, 1, 15000), (6, 2, 6, 2, 16000), (7, 2, 13, 1, 40000),
(8, 3, 2, 2, 60000), (9, 3, 12, 1, 55000), (10, 3, 4, 1, 15000),
(11, 4, 1, 5, 125000), (12, 4, 10, 2, 90000), (13, 4, 14, 1, 60000), (14, 4, 20, 1, 100000),
(15, 5, 3, 3, 60000), (16, 5, 11, 2, 30000), (17, 5, 8, 1, 18000),
(18, 6, 1, 2, 50000),
(19, 7, 2, 2, 60000), (20, 7, 10, 1, 45000),
(21, 8, 4, 2, 30000),
(22, 9, 1, 1, 25000), (23, 9, 11, 2, 30000), (24, 9, 8, 1, 18000),
(25, 10, 3, 3, 60000), (26, 10, 12, 1, 55000),
(27, 11, 2, 4, 120000), (28, 11, 16, 1, 70000), (29, 11, 14, 1, 60000), (30, 11, 17, 2, 100000),
(31, 12, 1, 3, 75000), (32, 12, 13, 2, 80000), (33, 12, 15, 1, 30000),
(36, 14, 3, 2, 40000), (37, 14, 4, 1, 15000), (38, 14, 7, 1, 35000),

-- HD #15 -> #25
(39, 15, 3, 2, 40000), (40, 15, 11, 1, 15000), (41, 15, 4, 1, 15000),
(42, 16, 1, 2, 50000), (43, 16, 10, 1, 45000),
(44, 17, 2, 3, 90000), (45, 17, 12, 1, 55000),
(46, 18, 3, 2, 40000), (47, 18, 11, 2, 30000),
(48, 19, 1, 2, 50000), (49, 19, 10, 1, 45000), (50, 19, 4, 1, 15000),
(51, 20, 2, 2, 60000), (52, 20, 14, 1, 60000),
(53, 21, 1, 4, 100000), (54, 21, 10, 2, 90000), (55, 21, 16, 1, 70000), (56, 21, 20, 1, 100000),
(57, 22, 1, 2, 50000), (58, 22, 10, 1, 45000), (59, 22, 4, 1, 15000),
(60, 23, 3, 2, 40000), (61, 23, 11, 2, 30000), (62, 23, 8, 1, 18000),
(63, 24, 2, 3, 90000), (64, 24, 12, 1, 55000),
(65, 25, 3, 2, 40000), (66, 25, 13, 2, 80000);
GO
SET IDENTITY_INSERT chi_tiet_hoa_don OFF;


-- =============================================
-- 16. PHIẾU NHẬP
-- =============================================
INSERT INTO phieu_nhap (ma_nv, ma_ncc, ngay_nhap, tong_tien, ghi_chu) VALUES 
(2, 1, '2025-10-20', 3000000, N'Nhập bia Heineken và Tiger'),
(2, 2, '2025-10-21', 2400000, N'Nhập bia Sài Gòn'),
(2, 3, '2025-10-22', 1500000, N'Nhập đồ ăn vặt'),
(2, 4, '2025-10-23', 1800000, N'Nhập thực phẩm tươi'),
(2, 5, '2025-10-24', 2000000, N'Nhập nước giải khát'),
(2, 6, '2025-10-25', 5000000, N'Nhập dụng cụ bi-a'),
(2, 1, '2025-10-26', 1500000, N'Nhập thêm Tiger và Heineken'),
(2, 3, '2025-10-27', 900000, N'Nhập đồ ăn bổ sung');
GO


-- =============================================
-- 17. CHI TIẾT PHIẾU NHẬP
-- =============================================
INSERT INTO chi_tiet_phieu_nhap (ma_pn, ma_hang, so_luong_nhap, don_gia_nhap) VALUES 
-- Phiếu nhập #1 -> #8
(1, 1, 100, 15000), (1, 2, 100, 18000),
(2, 3, 200, 12000),
(3, 10, 50, 30000), (3, 11, 100, 10000),
(4, 12, 40, 35000), (4, 13, 30, 25000), (4, 14, 25, 40000), (4, 16, 20, 50000),
(5, 4, 100, 10000), (5, 5, 80, 10000), (5, 6, 150, 5000), (5, 7, 40, 18000), (5, 8, 60, 12000), (5, 9, 70, 8000),
(6, 17, 10, 500000), (6, 18, 40, 50000), (6, 19, 5, 800000), (6, 20, 5, 1200000),
(7, 1, 50, 15000), (7, 2, 50, 18000),
(8, 10, 30, 30000), (8, 15, 45, 20000);
GO


-- =============================================
-- 18. CHẤM CÔNG
-- =============================================
INSERT INTO cham_cong (ma_nv, ngay, gio_vao, gio_ra, xac_thuc_bang, trang_thai, ghi_chu) VALUES
-- Ngày 2025-10-25
(2, '2025-10-25', '2025-10-25 08:00:00', '2025-10-25 17:00:00', N'Vân tay', N'Đúng giờ', NULL),
(3, '2025-10-25', '2025-10-25 08:55:00', '2025-10-25 17:05:00', N'FaceID', N'Đúng giờ', NULL),
(4, '2025-10-25', '2025-10-25 12:58:00', '2025-10-25 21:02:00', N'Vân tay', N'Đúng giờ', NULL),
(5, '2025-10-25', '2025-10-25 17:30:00', '2025-10-26 01:00:00', N'FaceID', N'Đúng giờ', NULL),
(6, '2025-10-25', '2025-10-25 13:00:00', '2025-10-25 21:00:00', N'Vân tay', N'Đúng giờ', NULL),
(7, '2025-10-25', '2025-10-25 08:00:00', '2025-10-25 17:00:00', N'FaceID', N'Đúng giờ', NULL),
-- Ngày 2025-10-26
(2, '2025-10-26', '2025-10-26 08:05:00', '2025-10-26 17:00:00', N'Vân tay', N'Đi trễ', N'Trễ 5 phút'),
(3, '2025-10-26', '2025-10-26 08:58:00', '2025-10-26 17:10:00', N'FaceID', N'Đúng giờ', NULL),
(4, '2025-10-26', '2025-10-26 13:00:00', '2025-10-26 20:50:00', N'Vân tay', N'Về sớm', N'Xin về sớm 10 phút'),
(5, '2025-10-26', '2025-10-26 17:35:00', '2025-10-27 01:05:00', N'FaceID', N'Đúng giờ', NULL),
(6, '2025-10-26', '2025-10-26 13:00:00', '2025-10-26 21:00:00', N'Vân tay', N'Đúng giờ', NULL),
(7, '2025-10-26', '2025-10-26 08:00:00', '2025-10-26 17:00:00', N'FaceID', N'Đúng giờ', NULL),
-- Ngày 2025-10-27
(2, '2025-10-27', '2025-10-27 08:00:00', '2025-10-27 17:00:00', N'Vân tay', N'Đúng giờ', NULL),
(3, '2025-10-27', '2025-10-27 08:55:00', '2025-10-27 17:00:00', N'FaceID', N'Đúng giờ', NULL),
(4, '2025-10-27', '2025-10-27 13:05:00', '2025-10-27 21:00:00', N'Vân tay', N'Đi trễ', N'Trễ 5 phút'),
(5, '2025-10-27', '2025-10-27 17:30:00', '2025-10-28 01:00:00', N'FaceID', N'Đúng giờ', NULL),
(6, '2025-10-27', '2025-10-27 13:00:00', '2025-10-27 21:00:00', N'Vân tay', N'Đúng giờ', NULL),
(7, '2025-10-27', '2025-10-27 08:00:00', '2025-10-27 17:00:00', N'FaceID', N'Đúng giờ', NULL),
-- Ngày 2025-10-28
(2, '2025-10-28', '2025-10-28 08:00:00', NULL, N'Vân tay', N'Đúng giờ', N'Đang làm việc'),
(3, '2025-10-28', '2025-10-28 08:58:00', NULL, N'FaceID', N'Đúng giờ', N'Đang làm việc'),
(4, '2025-10-28', '2025-10-28 13:00:00', NULL, N'Vân tay', N'Đúng giờ', N'Đang làm việc'),
(5, '2025-10-28', '2025-10-28 17:35:00', NULL, N'FaceID', N'Đúng giờ', N'Đang làm việc'),
(6, '2025-10-28', '2025-10-28 13:00:00', NULL, N'Vân tay', N'Đúng giờ', N'Đang làm việc'),
(7, '2025-10-28', '2025-10-28 08:00:00', NULL, N'FaceID', N'Đúng giờ', N'Đang làm việc');
GO


-- =============================================
-- 19. BẢNG LƯƠNG
-- =============================================
INSERT INTO bang_luong (ma_nv, thang, nam, tong_gio, luong_co_ban, phu_cap, thuong, phat, tong_luong, ngay_tinh) VALUES
-- Tháng 9/2025
(2, 9, 2025, 176.5, 15000000, 3000000, 1000000, 0, 19000000, '2025-10-05'),
(3, 9, 2025, 175.0, 8000000, 1000000, 500000, 0, 9500000, '2025-10-05'),
(4, 9, 2025, 174.0, 8000000, 1000000, 300000, 0, 9300000, '2025-10-05'),
(5, 9, 2025, 176.0, 7000000, 500000, 200000, 100000, 7600000, '2025-10-05'),
(6, 9, 2025, 176.0, 7000000, 500000, 200000, 0, 7700000, '2025-10-05'),
(7, 9, 2025, 176.0, 7000000, 500000, 200000, 0, 7700000, '2025-10-05'),
-- Tháng 8/2025
(2, 8, 2025, 174.0, 15000000, 3000000, 800000, 0, 18800000, '2025-09-05'),
(3, 8, 2025, 176.0, 8000000, 1000000, 400000, 0, 9400000, '2025-09-05'),
(4, 8, 2025, 173.5, 8000000, 1000000, 300000, 50000, 9250000, '2025-09-05'),
(5, 8, 2025, 175.0, 7000000, 500000, 150000, 0, 7650000, '2025-09-05'),
(6, 8, 2025, 176.0, 7000000, 500000, 200000, 0, 7700000, '2025-09-05'),
(7, 8, 2025, 174.0, 7000000, 500000, 150000, 0, 7650000, '2025-09-05');
GO


-- =============================================
-- 20. SỔ QUỸ
-- =============================================
SET IDENTITY_INSERT so_quy ON;
INSERT INTO so_quy (ma_phieu, ma_nv, ngay_lap, loai_phieu, so_tien, ly_do, ma_hd_lien_quan) VALUES
-- Thu từ hóa đơn đã thanh toán (27/10)
(1, 3, '2025-10-27 11:30:00', N'Thu', 220000, N'Thu tiền hóa đơn #1 - Bàn 01', 1),
(2, 3, '2025-10-27 13:00:00', N'Thu', 265000, N'Thu tiền hóa đơn #2 - Bàn 03', 2),
(3, 4, '2025-10-27 16:30:00', N'Thu', 260000, N'Thu tiền hóa đơn #3 - Bàn 14', 3),
(4, 3, '2025-10-27 19:00:00', N'Thu', 590000, N'Thu tiền hóa đơn #4 - VIP 01', 4),
(5, 5, '2025-10-27 20:30:00', N'Thu', 245000, N'Thu tiền hóa đơn #5 - Bàn 09', 5),
-- Chi tiền nhập hàng
(6, 2, '2025-10-20 10:00:00', N'Chi', 3000000, N'Chi tiền nhập hàng PN #1', NULL),
(7, 2, '2025-10-21 10:00:00', N'Chi', 2400000, N'Chi tiền nhập hàng PN #2', NULL),
(8, 2, '2025-10-22 10:00:00', N'Chi', 1500000, N'Chi tiền nhập hàng PN #3', NULL),
(9, 2, '2025-10-23 10:00:00', N'Chi', 1800000, N'Chi tiền nhập hàng PN #4', NULL),
(10, 2, '2025-10-24 10:00:00', N'Chi', 2000000, N'Chi tiền nhập hàng PN #5', NULL),
(11, 2, '2025-10-25 10:00:00', N'Chi', 5000000, N'Chi tiền nhập hàng PN #6 - Dụng cụ', NULL),
(12, 2, '2025-10-26 10:00:00', N'Chi', 1500000, N'Chi tiền nhập hàng PN #7', NULL),
(13, 2, '2025-10-27 10:00:00', N'Chi', 900000, N'Chi tiền nhập hàng PN #8', NULL),
-- Chi khác
(14, 2, '2025-10-25 14:00:00', N'Chi', 500000, N'Chi phí sửa chữa bàn', NULL),
(15, 2, '2025-10-26 15:00:00', N'Chi', 300000, N'Chi phí điện nước', NULL),
(16, 2, '2025-10-27 09:00:00', N'Chi', 200000, N'Chi phí vệ sinh', NULL),
(17, 2, '2025-10-27 16:00:00', N'Chi', 150000, N'Mua đồ dùng văn phòng', NULL),
-- Thu từ các hóa đơn cũ (24-26/10)
(18, 3, '2025-10-24 12:30:00', N'Thu', 185000, N'Thu tiền HD #15', 15),
(19, 3, '2025-10-24 16:00:00', N'Thu', 195000, N'Thu tiền HD #16', 16),
(20, 4, '2025-10-24 20:30:00', N'Thu', 295000, N'Thu tiền HD #17', 17),
(21, 3, '2025-10-25 11:00:00', N'Thu', 165000, N'Thu tiền HD #18', 18),
(22, 4, '2025-10-25 15:30:00', N'Thu', 260000, N'Thu tiền HD #19', 19),
(23, 3, '2025-10-25 18:00:00', N'Thu', 225000, N'Thu tiền HD #20', 20),
(24, 3, '2025-10-25 22:00:00', N'Thu', 570000, N'Thu tiền HD #21 (VIP)', 21),
(25, 3, '2025-10-26 10:30:00', N'Thu', 200000, N'Thu tiền HD #22', 22),
(26, 4, '2025-10-26 13:00:00', N'Thu', 188000, N'Thu tiền HD #23', 23),
(27, 5, '2025-10-26 17:00:00', N'Thu', 295000, N'Thu tiền HD #24', 24),
(28, 4, '2025-10-26 20:00:00', N'Thu', 235000, N'Thu tiền HD #25', 25);
GO
SET IDENTITY_INSERT so_quy OFF;


-- =============================================
-- 21. LỊCH SỬ HOẠT ĐỘNG
-- =============================================
INSERT INTO lich_su_hoat_dong (thoi_gian, ma_nv, hanh_dong, chi_tiet) VALUES
-- Hoạt động ngày 2025-10-27
('2025-10-27 08:05:00', 2, N'Đăng nhập', N'Quản lý Minh đăng nhập vào hệ thống'),
('2025-10-27 08:30:00', 2, N'Cập nhật giá', N'Cập nhật giá giờ chơi khung tối'),
('2025-10-27 09:00:00', 3, N'Đăng nhập', N'Thu ngân Lan đăng nhập'),
('2025-10-27 09:30:00', 3, N'Mở bàn', N'Mở bàn 01 cho khách Nguyễn Văn An'),
('2025-10-27 09:35:00', 3, N'Gọi món', N'Thêm dịch vụ cho hóa đơn #1: 3 Tiger, 1 Khô bò'),
('2025-10-27 10:00:00', 3, N'Mở bàn', N'Mở bàn 03 cho khách Trần Thị Bình'),
('2025-10-27 10:15:00', 3, N'Gọi món', N'Thêm dịch vụ cho hóa đơn #2'),
('2025-10-27 11:30:00', 3, N'Thanh toán', N'Hoàn tất thanh toán hóa đơn #1. Tổng: 220,000 VND'),
('2025-10-27 13:00:00', 3, N'Thanh toán', N'Hoàn tất thanh toán hóa đơn #2. Tổng: 265,000 VND'),
('2025-10-27 14:00:00', 4, N'Đăng nhập', N'Thu ngân Hương đăng nhập'),
('2025-10-27 14:00:00', 4, N'Mở bàn', N'Mở bàn 14 cho khách Hoàng Văn Em'),
('2025-10-27 16:30:00', 4, N'Thanh toán', N'Hoàn tất thanh toán hóa đơn #3. Tổng: 260,000 VND'),
('2025-10-27 16:00:00', 3, N'Mở bàn', N'Mở bàn VIP 01 cho khách Trịnh Quốc Dũng'),
('2025-10-27 16:05:00', 3, N'Gọi món', N'Thêm dịch vụ cho hóa đơn #4 (VIP)'),
('2025-10-27 18:00:00', 5, N'Đăng nhập', N'Phục vụ Hùng đăng nhập'),
('2025-10-27 18:00:00', 5, N'Mở bàn', N'Mở bàn 09 cho khách Vũ Minh Khang'),
('2025-10-27 19:00:00', 3, N'Thanh toán', N'Hoàn tất thanh toán hóa đơn #4. Tổng: 590,000 VND'),
('2025-10-27 20:30:00', 5, N'Thanh toán', N'Hoàn tất thanh toán hóa đơn #5. Tổng: 245,000 VND'),
-- Hoạt động ngày 2025-10-28
('2025-10-28 08:00:00', 2, N'Đăng nhập', N'Quản lý Minh đăng nhập'),
('2025-10-28 08:30:00', 2, N'Xem báo cáo', N'Xem báo cáo doanh thu ngày 27/10'),
('2025-10-28 09:00:00', 3, N'Đăng nhập', N'Thu ngân Lan đăng nhập'),
('2025-10-28 09:00:00', 3, N'Mở bàn', N'Mở bàn VIP 01 cho khách Nguyễn Văn An'),
('2025-10-28 09:30:00', 3, N'Mở bàn', N'Mở bàn 01 cho khách Nguyễn Văn An'),
('2025-10-28 09:35:00', 3, N'Gọi món', N'Thêm 2 Tiger cho hóa đơn #6'),
('2025-10-28 10:00:00', 3, N'Mở bàn', N'Mở bàn 03 cho khách Trần Thị Bình'),
('2025-10-28 10:05:00', 3, N'Gọi món', N'Thêm dịch vụ cho hóa đơn #7'),
('2025-10-28 11:30:00', 5, N'Đăng nhập', N'Phục vụ Hùng đăng nhập'),
('2025-10-28 11:30:00', 5, N'Mở bàn', N'Mở bàn 06 cho khách lẻ'),
('2025-10-28 12:45:00', 5, N'Mở bàn', N'Mở bàn 14 cho khách Hoàng Văn Em'),
('2025-10-28 13:00:00', 4, N'Đăng nhập', N'Thu ngân Hương đăng nhập'),
('2025-10-28 13:00:00', 4, N'Mở bàn', N'Mở bàn 09 cho khách Vũ Minh Khang'),
('2025-10-28 14:00:00', 5, N'Mở bàn', N'Mở bàn 20 cho khách Ngô Thị Mai'),
('2025-10-28 15:00:00', 3, N'Mở bàn', N'Mở bàn VIP 04 cho khách Trịnh Quốc Dũng'),
('2025-10-28 15:05:00', 3, N'Gọi món', N'Thêm dịch vụ cho hóa đơn #12 (VIP)'),
('2025-10-28 16:30:00', 2, N'Cập nhật thông tin', N'Cập nhật thông tin khách hàng'),
('2025-10-28 17:00:00', 2, N'Xem báo cáo', N'Xem thống kê doanh thu theo giờ');
GO