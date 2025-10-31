IF EXISTS (SELECT name FROM sys.databases WHERE name = N'QL_Bi_a')
BEGIN
    use master
    ALTER DATABASE QL_Bi_a SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QL_Bi_a;
END
GO

-- Tạo database mới
CREATE DATABASE QL_Bi_a;
GO

USE QL_Bi_a;
GO

CREATE TABLE khach_hang (
    ma_kh INT IDENTITY(1,1) PRIMARY KEY,
    ten_kh NVARCHAR(100) NOT NULL,
    sdt VARCHAR(15) UNIQUE CHECK (sdt LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]%' AND LEN(sdt) BETWEEN 9 AND 11),
    email NVARCHAR(100),
    ngay_sinh DATE,
    hang_tv NVARCHAR(20) DEFAULT N'Đồng' CHECK (hang_tv IN (N'Đồng', N'Bạc', N'Vàng', N'Bạch kim')),
    diem_tich_luy INT DEFAULT 0,
    tong_chi_tieu DECIMAL(12,0) DEFAULT 0,
    ngay_dang_ky DATETIME DEFAULT GETDATE(),
    lan_den_cuoi DATETIME
);
GO

CREATE TABLE loai_ban (
    ma_loai INT IDENTITY(1,1) PRIMARY KEY,
    ten_loai NVARCHAR(50) NOT NULL UNIQUE,
    mo_ta NVARCHAR(255),
    gia_gio DECIMAL(10,0) NOT NULL,
    trang_thai NVARCHAR(20) DEFAULT N'Đang áp dụng' CHECK (trang_thai IN (N'Đang áp dụng', N'Ngừng áp dụng'))
);
GO

CREATE TABLE ban_bia (
    ma_ban INT IDENTITY(1,1) PRIMARY KEY,
    ten_ban NVARCHAR(50) NOT NULL UNIQUE,
    ma_loai INT NOT NULL,
    khu_vuc NVARCHAR(20) DEFAULT N'Tầng 1' CHECK (khu_vuc IN (N'Tầng 1', N'Tầng 2', N'VIP')),
    trang_thai NVARCHAR(20) DEFAULT N'Trống' CHECK (trang_thai IN (N'Trống', N'Đang chơi', N'Đã đặt', N'Bảo trì')),
    gio_bat_dau DATETIME NULL,
    ma_kh INT NULL,
    ghi_chu NVARCHAR(255),
    ngay_tao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_loai) REFERENCES loai_ban(ma_loai),
    FOREIGN KEY (ma_kh) REFERENCES khach_hang(ma_kh)
);
GO

CREATE TABLE dat_ban (
    ma_dat INT IDENTITY(1,1) PRIMARY KEY,
    ma_ban INT,
    ma_kh INT,
    ten_khach NVARCHAR(100) NOT NULL,
    sdt VARCHAR(15) NOT NULL,
    thoi_gian_dat DATETIME NOT NULL,
    so_nguoi INT,
    ghi_chu NVARCHAR(MAX),
    trang_thai NVARCHAR(20) DEFAULT N'Đang chờ' CHECK (trang_thai IN (N'Đang chờ', N'Đã xác nhận', N'Đã đến', N'Đã hủy')),
    ngay_tao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_ban) REFERENCES ban_bia(ma_ban),
    FOREIGN KEY (ma_kh) REFERENCES khach_hang(ma_kh)
);
GO

-- Bảng mặt hàng: CHỈ CÓ GIÁ NHẬP
CREATE TABLE mat_hang (
    ma_hang INT IDENTITY(1,1) PRIMARY KEY,
    ten_hang NVARCHAR(100) NOT NULL,
    loai NVARCHAR(20) DEFAULT N'Khác' CHECK (loai IN (N'Đồ uống', N'Đồ ăn', N'Dụng cụ bi-a', N'Khác')),
    don_vi NVARCHAR(50) DEFAULT N'cái',
    gia DECIMAL(10,0) NOT NULL, -- CHỈ CÓ GIÁ NHẬP
    so_luong_ton INT DEFAULT 0,
    nha_cung_cap NVARCHAR(100),
    ngay_nhap_gan_nhat DATE,
    trang_thai NVARCHAR(20) DEFAULT N'Còn hàng' CHECK (trang_thai IN (N'Còn hàng', N'Hết hàng', N'Ngừng kinh doanh')),
    mo_ta NVARCHAR(255),
    hinh_anh NVARCHAR(255),
    ngay_tao DATETIME DEFAULT GETDATE()
);
GO

-- Bảng dịch vụ: CÓ GIÁ BÁN
CREATE TABLE dich_vu (
    ma_dv INT IDENTITY(1,1) PRIMARY KEY,
    ten_dv NVARCHAR(100) NOT NULL,
    loai NVARCHAR(20) DEFAULT N'Khác' CHECK (loai IN (N'Đồ uống', N'Đồ ăn', N'Khác')),
    gia DECIMAL(10,0) NOT NULL, -- GIÁ BÁN (sẽ được trigger tự động tính từ giá nhập)
    don_vi NVARCHAR(50) DEFAULT N'phần',
    so_luong_ton INT DEFAULT 0,
    ma_hang INT NULL,
    ti_le_loi_nhuan DECIMAL(5,2) DEFAULT 30.00, -- Tỷ lệ % lợi nhuận (mặc định 30%)
    trang_thai NVARCHAR(20) DEFAULT N'Còn hàng' CHECK (trang_thai IN (N'Còn hàng', N'Hết hàng', N'Ngừng bán')),
    mo_ta NVARCHAR(255),
    hinh_anh NVARCHAR(255),
    ngay_tao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_hang) REFERENCES mat_hang(ma_hang)
);
GO

CREATE TABLE nhan_vien (
    ma_nv INT IDENTITY(1,1) PRIMARY KEY,
    ten_nv NVARCHAR(100) NOT NULL,
    ma_van_tay VARCHAR(50) UNIQUE,
    faceid_hash VARCHAR(255) UNIQUE,
    faceid_anh NVARCHAR(255),
    chuc_vu NVARCHAR(20) DEFAULT N'Phục vụ' CHECK (chuc_vu IN (N'Quản lý', N'Thu ngân', N'Phục vụ')),
    sdt VARCHAR(15),
    luong_co_ban DECIMAL(12,0) DEFAULT 0,
    phu_cap DECIMAL(12,0) DEFAULT 0,
    ca_mac_dinh NVARCHAR(10) DEFAULT N'Sáng' CHECK (ca_mac_dinh IN (N'Sáng', N'Chiều', N'Tối')),
    trang_thai NVARCHAR(20) DEFAULT N'Đang làm' CHECK (trang_thai IN (N'Đang làm', N'Nghỉ')),
    mat_khau VARCHAR(255) NOT NULL
);
GO

CREATE TABLE cham_cong (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_nv INT NOT NULL,
    ngay DATE NOT NULL,
    gio_vao DATETIME,
    gio_ra DATETIME,
    hinh_anh_vao NVARCHAR(255),
    hinh_anh_ra NVARCHAR(255),
    xac_thuc_bang NVARCHAR(20) DEFAULT N'Thủ công' CHECK (xac_thuc_bang IN (N'Thủ công', N'Vân tay', N'FaceID')),
    so_gio_lam AS (
        CASE 
            WHEN gio_ra IS NOT NULL 
            THEN CAST(DATEDIFF(MINUTE, gio_vao, gio_ra) AS DECIMAL(5,2)) / 60.0
            ELSE 0 
        END
    ) PERSISTED,
    trang_thai NVARCHAR(20) DEFAULT N'Đúng giờ' CHECK (trang_thai IN (N'Đúng giờ', N'Đi trễ', N'Về sớm', N'Vắng', N'Ca thêm')),
    ghi_chu NVARCHAR(255),
    FOREIGN KEY (ma_nv) REFERENCES nhan_vien(ma_nv)
);
GO

CREATE TABLE bang_luong (
    ma_luong INT IDENTITY(1,1) PRIMARY KEY,
    ma_nv INT NOT NULL,
    thang INT NOT NULL,
    nam INT NOT NULL,
    tong_gio DECIMAL(8,2) DEFAULT 0,
    luong_co_ban DECIMAL(12,0) DEFAULT 0,
    phu_cap DECIMAL(12,0) DEFAULT 0,
    thuong DECIMAL(12,0) DEFAULT 0,
    phat DECIMAL(12,0) DEFAULT 0,
    tong_luong DECIMAL(12,0) DEFAULT 0,
    ngay_tinh DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_nv) REFERENCES nhan_vien(ma_nv)
);
GO

CREATE TABLE hoa_don (
    ma_hd INT IDENTITY(1,1) PRIMARY KEY,
    ma_ban INT,
    ma_kh INT NULL,
    ma_nv INT,
    thoi_gian_bat_dau DATETIME,
    thoi_gian_ket_thuc DATETIME NULL,
    thoi_luong_phut INT,
    tien_ban DECIMAL(12,0) DEFAULT 0,
    tien_dich_vu DECIMAL(12,0) DEFAULT 0,
    giam_gia DECIMAL(12,0) DEFAULT 0,
    tong_tien DECIMAL(12,0) DEFAULT 0,
    phuong_thuc_thanh_toan NVARCHAR(20) DEFAULT N'Tiền mặt' CHECK (phuong_thuc_thanh_toan IN (N'Tiền mặt', N'Chuyển khoản', N'Thẻ', N'Ví điện tử')),
    trang_thai NVARCHAR(20) DEFAULT N'Đang chơi' CHECK (trang_thai IN (N'Đang chơi', N'Đã thanh toán', N'Đã hủy')),
    ghi_chu NVARCHAR(MAX),
    FOREIGN KEY (ma_ban) REFERENCES ban_bia(ma_ban),
    FOREIGN KEY (ma_kh) REFERENCES khach_hang(ma_kh),
    FOREIGN KEY (ma_nv) REFERENCES nhan_vien(ma_nv)
);
GO

CREATE TABLE chi_tiet_hoa_don (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_hd INT,
    ma_dv INT,
    so_luong INT DEFAULT 1,
    thanh_tien DECIMAL(12,0),
    FOREIGN KEY (ma_hd) REFERENCES hoa_don(ma_hd),
    FOREIGN KEY (ma_dv) REFERENCES dich_vu(ma_dv)
);
GO

CREATE TABLE gia_gio_choi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    khung_gio NVARCHAR(50) NOT NULL,
    gio_bat_dau TIME NOT NULL,
    gio_ket_thuc TIME NOT NULL,
    ma_loai INT,
    gia DECIMAL(10,0) NOT NULL,
    ap_dung_tu_ngay DATE,
    trang_thai NVARCHAR(20) DEFAULT N'Đang áp dụng' CHECK (trang_thai IN (N'Đang áp dụng', N'Hết hiệu lực')),
    FOREIGN KEY (ma_loai) REFERENCES loai_ban(ma_loai)
);
GO

CREATE TABLE lich_su_hoat_dong (
    id INT IDENTITY(1,1) PRIMARY KEY,
    thoi_gian DATETIME DEFAULT GETDATE(),
    ma_nv INT,
    hanh_dong NVARCHAR(255),
    chi_tiet NVARCHAR(MAX),
    FOREIGN KEY (ma_nv) REFERENCES nhan_vien(ma_nv)
);
GO

-- =============================================
-- TRIGGER: Tự động tính giá bán khi INSERT dịch vụ
-- =============================================
CREATE TRIGGER trg_DichVu_TinhGiaBan_Insert
ON dich_vu
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dv
    SET dv.gia = CEILING(mh.gia * (1 + ISNULL(i.ti_le_loi_nhuan, 30.00) / 100.0))
    FROM dich_vu dv
    INNER JOIN inserted i ON dv.ma_dv = i.ma_dv
    INNER JOIN mat_hang mh ON i.ma_hang = mh.ma_hang
    WHERE i.ma_hang IS NOT NULL;
END;
GO

-- =============================================
-- TRIGGER: Tự động tính lại giá bán khi UPDATE dịch vụ
-- =============================================
CREATE TRIGGER trg_DichVu_TinhGiaBan_Update
ON dich_vu
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Chỉ tính lại giá khi ma_hang hoặc ti_le_loi_nhuan thay đổi
    IF UPDATE(ma_hang) OR UPDATE(ti_le_loi_nhuan)
    BEGIN
        UPDATE dv
        SET dv.gia = CEILING(mh.gia * (1 + ISNULL(i.ti_le_loi_nhuan, 30.00) / 100.0))
        FROM dich_vu dv
        INNER JOIN inserted i ON dv.ma_dv = i.ma_dv
        INNER JOIN mat_hang mh ON i.ma_hang = mh.ma_hang
        WHERE i.ma_hang IS NOT NULL;
    END
END;
GO

-- =============================================
-- TRIGGER: Tự động cập nhật giá bán dịch vụ khi giá nhập mặt hàng thay đổi
-- =============================================
CREATE TRIGGER trg_MatHang_CapNhatGiaBan
ON mat_hang
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Chỉ cập nhật khi giá thay đổi
    IF UPDATE(gia)
    BEGIN
        UPDATE dv
        SET dv.gia = CEILING(i.gia * (1 + ISNULL(dv.ti_le_loi_nhuan, 30.00) / 100.0))
        FROM dich_vu dv
        INNER JOIN inserted i ON dv.ma_hang = i.ma_hang;
    END
END;
GO

-- =============================================
-- DATA MẪU
-- =============================================

-- Thêm mặt hàng (CHỈ GIÁ NHẬP)
INSERT INTO mat_hang (ten_hang, loai, don_vi, gia, so_luong_ton, nha_cung_cap)
VALUES 
    (N'Bia Tiger', N'Đồ uống', N'chai', 15000, 100, N'Công ty TNHH Heineken VN'),
    (N'Bia Heineken', N'Đồ uống', N'chai', 20000, 80, N'Công ty TNHH Heineken VN'),
    (N'Nước ngọt Coca', N'Đồ uống', N'chai', 8000, 150, N'Công ty Coca-Cola VN'),
    (N'Khô bò', N'Đồ ăn', N'đĩa', 30000, 50, N'Nhà cung cấp ABC'),
    (N'Đậu phộng', N'Đồ ăn', N'đĩa', 10000, 200, N'Nhà cung cấp XYZ');
GO

-- Thêm dịch vụ (GIÁ BÁN sẽ tự động tính từ trigger)
-- Giá ban đầu có thể để bất kỳ, trigger sẽ tự động cập nhật
INSERT INTO dich_vu (ten_dv, loai, don_vi, ma_hang, ti_le_loi_nhuan, gia)
VALUES 
    (N'Bia Tiger', N'Đồ uống', N'chai', 1, 40.00, 0), -- Giá sẽ = 15000 * 1.4 = 21000
    (N'Bia Heineken', N'Đồ uống', N'chai', 2, 35.00, 0), -- Giá sẽ = 20000 * 1.35 = 27000
    (N'Coca Cola', N'Đồ uống', N'chai', 3, 50.00, 0), -- Giá sẽ = 8000 * 1.5 = 12000
    (N'Khô bò rang muối', N'Đồ ăn', N'đĩa', 4, 60.00, 0), -- Giá sẽ = 30000 * 1.6 = 48000
    (N'Đậu phộng tỏi', N'Đồ ăn', N'đĩa', 5, 80.00, 0); -- Giá sẽ = 10000 * 1.8 = 18000
GO

-- Kiểm tra giá bán đã được tính tự động
SELECT 
    dv.ma_dv,
    dv.ten_dv,
    mh.gia AS gia_nhap,
    dv.ti_le_loi_nhuan,
    dv.gia AS gia_ban
FROM dich_vu dv
INNER JOIN mat_hang mh ON dv.ma_hang = mh.ma_hang;
GO