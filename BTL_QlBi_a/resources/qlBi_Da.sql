IF EXISTS (SELECT name FROM sys.databases WHERE name = N'QL_QuanBi_a')
BEGIN
    use master
    ALTER DATABASE QL_QuanBi_a SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QL_QuanBi_a;
END
GO

CREATE DATABASE QL_QuanBi_a;
GO

USE QL_QuanBi_a;
GO

-- Bảng khách hàng
CREATE TABLE khach_hang (
    ma_kh INT IDENTITY(1,1) PRIMARY KEY,
    ten_kh NVARCHAR(100) NOT NULL,
    sdt VARCHAR(15) UNIQUE CHECK (sdt LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]%' AND LEN(sdt) BETWEEN 9 AND 11),
    mat_khau VARCHAR(255) NULL, 
    email NVARCHAR(100),
    ngay_sinh DATE,
    hang_tv NVARCHAR(20) DEFAULT N'Đồng' CHECK (hang_tv IN (N'Đồng', N'Bạc', N'Vàng', N'Bạch kim')),
    diem_tich_luy INT DEFAULT 0,
    tong_chi_tieu DECIMAL(12,0) DEFAULT 0,
    ngay_dang_ky DATETIME DEFAULT GETDATE(),
    lan_den_cuoi DATETIME,
    hoat_dong BIT DEFAULT 0,
    avatar NVARCHAR(255)
);
GO

-- Bảng loại bàn
CREATE TABLE loai_ban (
    ma_loai INT IDENTITY(1,1) PRIMARY KEY,
    ten_loai NVARCHAR(50) NOT NULL UNIQUE,
    mo_ta NVARCHAR(255),
    gia_gio DECIMAL(10,0) NOT NULL,
    trang_thai NVARCHAR(20) DEFAULT N'Đang áp dụng' CHECK (trang_thai IN (N'Đang áp dụng', N'Ngừng áp dụng'))
);
GO

-- Bảng khu vực
CREATE TABLE khu_vuc (
    ma_khu_vuc INT IDENTITY(1,1) PRIMARY KEY,
    ten_khu_vuc NVARCHAR(50) NOT NULL UNIQUE,
    mo_ta NVARCHAR(255)
);
GO

-- Bảng bàn bi-a
CREATE TABLE ban_bia (
    ma_ban INT IDENTITY(1,1) PRIMARY KEY,
    ten_ban NVARCHAR(50) NOT NULL UNIQUE,
    ma_loai INT NOT NULL,
    ma_khu_vuc INT NOT NULL, 
    trang_thai NVARCHAR(20) DEFAULT N'Trống' CHECK (trang_thai IN (N'Trống', N'Đang chơi', N'Đã đặt', N'Bảo trì')),
    gio_bat_dau DATETIME NULL,
    ma_kh INT NULL,
    vi_tri_x INT DEFAULT 0,
    vi_tri_y INT DEFAULT 0,
    ghi_chu NVARCHAR(255),
    ngay_tao DATETIME DEFAULT GETDATE(),
    hinh_anh NVARCHAR(255) NULL,
    FOREIGN KEY (ma_loai) REFERENCES loai_ban(ma_loai),
    FOREIGN KEY (ma_kh) REFERENCES khach_hang(ma_kh),
    FOREIGN KEY (ma_khu_vuc) REFERENCES khu_vuc(ma_khu_vuc)
);
GO

-- Bảng đặt bàn
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

-- Bảng nhà cung cấp
CREATE TABLE nha_cung_cap (
    ma_ncc INT IDENTITY(1,1) PRIMARY KEY,
    ten_ncc NVARCHAR(100) NOT NULL,
    sdt VARCHAR(15),
    dia_chi NVARCHAR(255),
    email NVARCHAR(100)
);
GO

-- Bảng mặt hàng
CREATE TABLE mat_hang (
    ma_hang INT IDENTITY(1,1) PRIMARY KEY,
    ten_hang NVARCHAR(100) NOT NULL,
    loai NVARCHAR(20) DEFAULT N'Khác' CHECK (loai IN (N'Đồ uống', N'Đồ ăn', N'Dụng cụ bi-a', N'Khác')),
    don_vi NVARCHAR(50) DEFAULT N'cái',
    gia DECIMAL(10,0) NOT NULL,
    so_luong_ton INT DEFAULT 0,
    nguong_canh_bao INT DEFAULT 10,
    ma_ncc_default INT NULL, 
    ngay_nhap_gan_nhat DATE,
    trang_thai NVARCHAR(20) DEFAULT N'Còn hàng' CHECK (trang_thai IN (N'Còn hàng', N'Hết hàng', N'Ngừng kinh doanh')),
    mo_ta NVARCHAR(255),
    hinh_anh NVARCHAR(255),
    ngay_tao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_ncc_default) REFERENCES nha_cung_cap(ma_ncc)
);
GO

-- Bảng phiếu nhập
CREATE TABLE phieu_nhap (
    ma_pn INT IDENTITY(1,1) PRIMARY KEY,
    ma_nv INT NOT NULL, 
    ma_ncc INT NOT NULL,
    ngay_nhap DATETIME DEFAULT GETDATE(),
    tong_tien DECIMAL(12,0) DEFAULT 0,
    ghi_chu NVARCHAR(255),
    FOREIGN KEY (ma_ncc) REFERENCES nha_cung_cap(ma_ncc)
);
GO

-- Bảng chi tiết phiếu nhập
CREATE TABLE chi_tiet_phieu_nhap (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma_pn INT NOT NULL,
    ma_hang INT NOT NULL,
    so_luong_nhap INT NOT NULL,
    don_gia_nhap DECIMAL(10,0) NOT NULL,
    thanh_tien AS (so_luong_nhap * don_gia_nhap) PERSISTED,
    FOREIGN KEY (ma_pn) REFERENCES phieu_nhap(ma_pn),
    FOREIGN KEY (ma_hang) REFERENCES mat_hang(ma_hang)
);
GO

-- Bảng dịch vụ
CREATE TABLE dich_vu (
    ma_dv INT IDENTITY(1,1) PRIMARY KEY,
    ten_dv NVARCHAR(100) NOT NULL,
    loai NVARCHAR(20) DEFAULT N'Khác' CHECK (loai IN (N'Đồ uống', N'Đồ ăn', N'Khác')),
    gia DECIMAL(10,0) NOT NULL, 
    don_vi NVARCHAR(50) DEFAULT N'phần',
    ma_hang INT NULL, 
    trang_thai NVARCHAR(20) DEFAULT N'Còn hàng' CHECK (trang_thai IN (N'Còn hàng', N'Hết hàng', N'Ngừng bán')),
    mo_ta NVARCHAR(255),
    hinh_anh NVARCHAR(255),
    ngay_tao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ma_hang) REFERENCES mat_hang(ma_hang)
);
GO

-- Bảng nhóm quyền
CREATE TABLE nhom_quyen (
    ma_nhom INT IDENTITY(1,1) PRIMARY KEY,
    ten_nhom NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- Bảng nhân viên
CREATE TABLE nhan_vien (
    ma_nv INT IDENTITY(1,1) PRIMARY KEY,
    ten_nv NVARCHAR(100) NOT NULL,
    ma_van_tay VARCHAR(50) UNIQUE,
    faceid_hash VARCHAR(255) UNIQUE,
    faceid_anh NVARCHAR(255),
    ma_nhom INT NOT NULL,
    sdt VARCHAR(15),
    luong_co_ban DECIMAL(12,0) DEFAULT 0,
    phu_cap DECIMAL(12,0) DEFAULT 0,
    ca_mac_dinh NVARCHAR(10) DEFAULT N'Sáng' CHECK (ca_mac_dinh IN (N'Sáng', N'Chiều', N'Tối')),
    trang_thai NVARCHAR(20) DEFAULT N'Đang làm' CHECK (trang_thai IN (N'Đang làm', N'Nghỉ')),
    mat_khau VARCHAR(255) NOT NULL,
    FOREIGN KEY (ma_nhom) REFERENCES nhom_quyen(ma_nhom)
);
GO

-- Thêm foreign key cho phieu_nhap
ALTER TABLE phieu_nhap
ADD FOREIGN KEY (ma_nv) REFERENCES nhan_vien(ma_nv);
GO

-- Bảng chức năng
CREATE TABLE chuc_nang (
    ma_cn VARCHAR(50) PRIMARY KEY, 
    ten_cn NVARCHAR(100) NOT NULL,
    mo_ta NVARCHAR(255)
);
GO

-- Bảng phân quyền
CREATE TABLE phan_quyen (
    ma_nhom INT NOT NULL,
    ma_cn VARCHAR(50) NOT NULL,
    PRIMARY KEY (ma_nhom, ma_cn),
    FOREIGN KEY (ma_nhom) REFERENCES nhom_quyen(ma_nhom),
    FOREIGN KEY (ma_cn) REFERENCES chuc_nang(ma_cn)
);
GO

-- Bảng chấm công
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

-- Bảng bảng lương
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

-- Bảng hóa đơn
CREATE TABLE hoa_don (
    ma_hd INT IDENTITY(1,1) PRIMARY KEY,
    ma_ban INT,
    ma_kh INT NULL,
    ma_nv INT,
    thoi_gian_bat_dau DATETIME,
    thoi_gian_ket_thuc DATETIME NULL,
    thoi_luong_phut AS (
        CASE 
            WHEN thoi_gian_ket_thuc IS NOT NULL 
            THEN DATEDIFF(MINUTE, thoi_gian_bat_dau, thoi_gian_ket_thuc)
            ELSE 0
        END
    ) PERSISTED,
    tien_ban DECIMAL(12,0) DEFAULT 0,
    tien_dich_vu DECIMAL(12,0) DEFAULT 0,
    giam_gia DECIMAL(12,0) DEFAULT 0,
    ghi_chu_giam_gia NVARCHAR(255) NULL, 
    tong_tien DECIMAL(12,0) DEFAULT 0,
    phuong_thuc_thanh_toan NVARCHAR(20) DEFAULT N'Tiền mặt' 
        CHECK (phuong_thuc_thanh_toan IN (
            N'Tiền mặt', N'Chuyển khoản', N'Thẻ', N'Ví điện tử', N'QR Tự động'
        )),
    trang_thai NVARCHAR(20) DEFAULT N'Đang chơi' 
        CHECK (trang_thai IN (N'Đang chơi', N'Đã thanh toán', N'Đã hủy')),
    ma_giao_dich_qr VARCHAR(100) NULL,
    qr_code_url VARCHAR(500) NULL,
    ghi_chu NVARCHAR(MAX),
    FOREIGN KEY (ma_ban) REFERENCES ban_bia(ma_ban),
    FOREIGN KEY (ma_kh) REFERENCES khach_hang(ma_kh),
    FOREIGN KEY (ma_nv) REFERENCES nhan_vien(ma_nv)
);
GO

-- Bảng chi tiết hóa đơn
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

-- Bảng sổ quỹ
CREATE TABLE so_quy (
    ma_phieu INT IDENTITY(1,1) PRIMARY KEY,
    ma_nv INT NOT NULL,
    ngay_lap DATETIME DEFAULT GETDATE(),
    loai_phieu NVARCHAR(10) NOT NULL CHECK (loai_phieu IN (N'Thu', N'Chi')),
    so_tien DECIMAL(12,0) NOT NULL,
    ly_do NVARCHAR(500) NOT NULL,
    ma_hd_lien_quan INT NULL, 
    FOREIGN KEY (ma_nv) REFERENCES nhan_vien(ma_nv),
    FOREIGN KEY (ma_hd_lien_quan) REFERENCES hoa_don(ma_hd)
);
GO

-- Bảng giá giờ chơi
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

-- Bảng lịch sử hoạt động
CREATE TABLE lich_su_hoat_dong (
    id INT IDENTITY(1,1) PRIMARY KEY,
    thoi_gian DATETIME DEFAULT GETDATE(),
    ma_nv INT,
    hanh_dong NVARCHAR(255),
    chi_tiet NVARCHAR(MAX),
    FOREIGN KEY (ma_nv) REFERENCES nhan_vien(ma_nv)
);
GO


-- Trigger: Cộng số lượng tồn khi nhập kho
CREATE TRIGGER trg_CongSoLuongTon_NhapKho
ON chi_tiet_phieu_nhap
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mh
    SET 
        mh.so_luong_ton = mh.so_luong_ton + i.so_luong_nhap,
        mh.gia = i.don_gia_nhap, 
        mh.ngay_nhap_gan_nhat = GETDATE()
    FROM mat_hang mh
    INNER JOIN inserted i ON mh.ma_hang = i.ma_hang;
END;
GO

-- Trigger: Trừ số lượng tồn khi bán hàng
CREATE TRIGGER trg_TruSoLuongTon_BanHang
ON chi_tiet_hoa_don
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mh
    SET mh.so_luong_ton = mh.so_luong_ton - i.so_luong
    FROM mat_hang mh
    INNER JOIN dich_vu dv ON mh.ma_hang = dv.ma_hang
    INNER JOIN inserted i ON dv.ma_dv = i.ma_dv
    WHERE dv.ma_hang IS NOT NULL; 
END;
GO

-- Trigger: Cộng lại số lượng tồn khi hủy món
CREATE TRIGGER trg_CongLaiSoLuongTon_HuyMon
ON chi_tiet_hoa_don
AFTER DELETE 
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mh
    SET mh.so_luong_ton = mh.so_luong_ton + d.so_luong
    FROM mat_hang mh
    INNER JOIN dich_vu dv ON mh.ma_hang = dv.ma_hang
    INNER JOIN deleted d ON dv.ma_dv = d.ma_dv
    WHERE dv.ma_hang IS NOT NULL;
END;
GO

PRINT N'=== TẠO DATABASE VÀ CÁC BẢNG THÀNH CÔNG ==='
GO