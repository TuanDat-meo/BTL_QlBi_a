/**
 * Module quản lý danh sách bàn đặt
 */
const DanhSachBanDatManager = {
    currentStatus: 'all',
    autoRefreshInterval: null,

    /**
     * Khởi tạo
     */
    init: function () {
        console.log('🎯 DanhSachBanDatManager initialized');

        // Bắt đầu auto-refresh mỗi 30 giây
        this.startAutoRefresh();
    },

    /**
     * Bắt đầu auto-refresh
     */
    startAutoRefresh: function () {
        this.autoRefreshInterval = setInterval(() => {
            console.log('🔄 Auto refreshing...');
            this.refresh(true); // true = silent refresh
        }, 30000); // 30 giây

        console.log('✅ Auto-refresh started (30s interval)');
    },

    /**
     * Dừng auto-refresh
     */
    stopAutoRefresh: function () {
        if (this.autoRefreshInterval) {
            clearInterval(this.autoRefreshInterval);
            this.autoRefreshInterval = null;
            console.log('⏹️ Auto-refresh stopped');
        }
    },

    /**
     * Làm mới danh sách
     */
    refresh: async function (silent = false) {
        try {
            if (!silent && window.Loading) Loading.show();

            const response = await fetch('/QLBan/LayDanhSachDatBan');
            const result = await response.json();

            if (!silent && window.Loading) Loading.hide();

            if (result.success) {
                this.updateGrid(result.data);

                if (!silent && window.Toast) {
                    Toast.success('Đã cập nhật danh sách');
                }

                console.log(`✅ Refreshed: ${result.data.length} reservations`);
            } else {
                console.error('❌ Failed to refresh:', result.message);
                if (!silent && window.Toast) {
                    Toast.error(result.message || 'Không thể tải danh sách');
                }
            }
        } catch (error) {
            if (!silent && window.Loading) Loading.hide();
            console.error('❌ Error refreshing:', error);

            if (!silent && window.Toast) {
                Toast.error('Có lỗi xảy ra khi làm mới');
            }
        }
    },

    /**
     * Cập nhật grid từ data
     */
    updateGrid: function (data) {
        const grid = document.getElementById('reservationsGrid');
        if (!grid) return;

        if (data.length === 0) {
            grid.innerHTML = `
                <div class="empty-state" style="grid-column: 1 / -1;">
                    <div class="empty-icon">📅</div>
                    <h3>Không có đặt bàn nào</h3>
                    <p class="empty-text">Chưa có đặt bàn trong thời gian tới</p>
                </div>
            `;
            return;
        }

        let html = '';
        data.forEach(dat => {
            const trangThaiClass = dat.trangThai === 'DangCho' ? 'waiting' : 'confirmed';
            const trangThaiText = dat.trangThai === 'DangCho' ? 'Đang chờ' : 'Đã xác nhận';

            const thoiGianDat = new Date(dat.thoiGianDat);
            const gioKetThuc = new Date(thoiGianDat.getTime() + dat.soGio * 60 * 60 * 1000);
            const timeUntil = (thoiGianDat - new Date()) / 1000 / 60; // minutes
            const isUrgent = timeUntil <= 15 && timeUntil >= 0;

            let countdownHtml = '';
            if (timeUntil > 0) {
                const hours = Math.floor(timeUntil / 60);
                const minutes = Math.floor(timeUntil % 60);
                const timeText = timeUntil < 60
                    ? `${minutes} phút`
                    : `${hours} giờ ${minutes} phút`;

                countdownHtml = `
                    <div class="countdown">
                        <span class="countdown-icon">⏱️</span>
                        <span class="countdown-text">Còn ${timeText}</span>
                    </div>
                `;
            }

            const hangBadge = dat.hangTV ? this.getHangBadge(dat.hangTV) : '';

            html += `
                <div class="reservation-card ${isUrgent ? 'urgent' : ''}" 
                     data-status="${dat.trangThai}"
                     data-ma-dat="${dat.maDat}"
                     data-search="${dat.tenKhach} ${dat.sdt} ${dat.tenBan}">
                    
                    <div class="card-header">
                        <div class="reservation-info">
                            <span class="reservation-id">#${dat.maDat}</span>
                            <span class="status-badge ${trangThaiClass}">${trangThaiText}</span>
                            ${isUrgent ? '<span class="urgent-badge">🔥 Sắp đến giờ</span>' : ''}
                        </div>
                        <div class="table-name">🪑 ${dat.tenBan}</div>
                    </div>

                    <div class="card-body">
                        <div class="info-row">
                            <span class="label">Khu vực:</span>
                            <span class="value">${dat.khuVuc}</span>
                        </div>
                        <div class="info-row">
                            <span class="label">Loại bàn:</span>
                            <span class="value">${dat.loaiBan}</span>
                        </div>
                        <div class="info-row">
                            <span class="label">👤 Khách hàng:</span>
                            <span class="value">
                                <strong>${dat.tenKhach}</strong>
                                ${hangBadge}
                            </span>
                        </div>
                        <div class="info-row">
                            <span class="label">📞 SĐT:</span>
                            <span class="value">${dat.sdt}</span>
                        </div>
                        <div class="info-row highlight">
                            <span class="label">🕐 Thời gian:</span>
                            <span class="value">
                                ${this.formatDateTime(thoiGianDat)} 
                                → ${this.formatTime(gioKetThuc)}
                                (${dat.soGio} giờ)
                            </span>
                        </div>
                        <div class="info-row">
                            <span class="label">👥 Số người:</span>
                            <span class="value">${dat.soNguoi || 1} người</span>
                        </div>
                        ${dat.ghiChu ? `
                            <div class="info-row note">
                                <span class="label">📝 Ghi chú:</span>
                                <span class="value">${dat.ghiChu}</span>
                            </div>
                        ` : ''}
                        
                        ${countdownHtml}
                    </div>

                    <div class="card-actions">
                        ${dat.trangThai === 'DangCho' ? `
                            <button class="btn btn-success btn-sm" 
                                    onclick="DanhSachBanDatManager.xacNhanDat(${dat.maDat}, ${dat.maBan})"
                                    title="Xác nhận khách đã đến">
                                ✅ Xác nhận
                            </button>
                        ` : ''}
                        <button class="btn btn-danger btn-sm" 
                                onclick="DanhSachBanDatManager.huyDat(${dat.maDat})"
                                title="Hủy đặt bàn">
                            ❌ Hủy
                        </button>
                    </div>
                </div>
            `;
        });

        grid.innerHTML = html;
        this.applyFilters();
    },

    /**
     * Lọc theo trạng thái
     */
    filterByStatus: function (status, event) {
        console.log('🔍 Filter by status:', status);
        this.currentStatus = status;

        const buttons = document.querySelectorAll('.filter-btn');
        buttons.forEach(btn => btn.classList.remove('active'));

        if (event && event.target) {
            event.target.classList.add('active');
        }

        this.applyFilters();
    },

    /**
     * Tìm kiếm
     */
    search: function () {
        const searchInput = document.getElementById('searchReservations');
        if (!searchInput) return;

        const searchTerm = searchInput.value.toLowerCase().trim();
        console.log('🔍 Searching for:', searchTerm);

        this.applyFilters(searchTerm);
    },

    /**
     * Áp dụng filters
     */
    applyFilters: function (searchTerm = '') {
        const cards = document.querySelectorAll('.reservation-card');
        let visibleCount = 0;

        cards.forEach(card => {
            const status = card.getAttribute('data-status') || '';
            const searchData = card.getAttribute('data-search')?.toLowerCase() || '';

            const statusMatch = this.currentStatus === 'all' || status === this.currentStatus;
            const searchMatch = searchTerm === '' || searchData.includes(searchTerm);

            if (statusMatch && searchMatch) {
                card.style.display = 'block';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        console.log(`✅ ${visibleCount} cards visible`);
    },

    /**
     * Xác nhận đặt bàn
     */
    xacNhanDat: async function (maDat, maBan) {
        if (!confirm('Xác nhận khách hàng đã đến?')) {
            return;
        }

        try {
            if (window.Loading) Loading.show();

            const response = await fetch('/QLBan/XacNhanDatBan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ maBan: maBan })
            });

            const result = await response.json();

            if (window.Loading) Loading.hide();

            if (result.success) {
                if (window.Toast) Toast.success(result.message || 'Xác nhận thành công');

                setTimeout(() => {
                    window.location.href = '/QLBan/BanBia';
                }, 1000);
            } else {
                if (window.Toast) Toast.error(result.message || 'Không thể xác nhận');
            }
        } catch (error) {
            if (window.Loading) Loading.hide();
            console.error('❌ Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra');
        }
    },

    /**
     * Hủy đặt bàn
     */
    huyDat: async function (maDat) {
        if (!confirm('Bạn có chắc muốn hủy đặt bàn này?')) {
            return;
        }

        try {
            if (window.Loading) Loading.show();

            // Lấy maBan từ card
            const card = document.querySelector(`[data-ma-dat="${maDat}"]`);
            if (!card) {
                throw new Error('Không tìm thấy thông tin đặt bàn');
            }

            const response = await fetch('/QLBan/HuyDatBan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ maBan: parseInt(card.dataset.maBan) })
            });

            const result = await response.json();

            if (window.Loading) Loading.hide();

            if (result.success) {
                if (window.Toast) Toast.success(result.message || 'Hủy đặt bàn thành công');

                this.refresh();
            } else {
                if (window.Toast) Toast.error(result.message || 'Không thể hủy đặt bàn');
            }
        } catch (error) {
            if (window.Loading) Loading.hide();
            console.error('❌ Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra');
        }
    },

    /**
     * Format datetime
     */
    formatDateTime: function (date) {
        const d = new Date(date);
        const hours = d.getHours().toString().padStart(2, '0');
        const minutes = d.getMinutes().toString().padStart(2, '0');
        const day = d.getDate().toString().padStart(2, '0');
        const month = (d.getMonth() + 1).toString().padStart(2, '0');
        const year = d.getFullYear();

        return `${hours}:${minutes} ${day}/${month}/${year}`;
    },

    /**
     * Format time
     */
    formatTime: function (date) {
        const d = new Date(date);
        const hours = d.getHours().toString().padStart(2, '0');
        const minutes = d.getMinutes().toString().padStart(2, '0');

        return `${hours}:${minutes}`;
    },

    /**
     * Get hạng thành viên badge
     */
    getHangBadge: function (hang) {
        const badges = {
            'Dong': '<span class="membership-badge">🥉 Đồng</span>',
            'Bac': '<span class="membership-badge">🥈 Bạc</span>',
            'Vang': '<span class="membership-badge">🥇 Vàng</span>',
            'KimCuong': '<span class="membership-badge">💎 Kim Cương</span>'
        };
        return badges[hang] || '';
    }
};

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function () {
    console.log('✅ DanhSachBanDat page loaded');
    DanhSachBanDatManager.init();
});

// Dừng auto-refresh khi rời trang
window.addEventListener('beforeunload', function () {
    DanhSachBanDatManager.stopAutoRefresh();
});

// Export to window
window.DanhSachBanDatManager = DanhSachBanDatManager;