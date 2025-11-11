// ThongKeManager - Quản lý thống kê
const ThongKeManager = {
    charts: {},
    currentTab: 'tongquan',

    init() {
        // --- ĐÃ SỬA ---
        const now = new Date();

        // Đặt 'từ ngày' là đầu ngày hôm nay (00:00)
        const startOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0);
        document.getElementById('tuNgay').value = this.formatDateForInput(startOfDay);

        // Đặt 'đến ngày' là cuối ngày hôm nay (23:59)
        const endOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59);
        document.getElementById('denNgay').value = this.formatDateForInput(endOfDay);
        // --- KẾT THÚC SỬA ---

        document.getElementById('namThongKe').value = new Date().getFullYear();

        // Load initial data
        this.loadTongQuan();
        this.loadDoanhThu7Ngay();
        this.loadDoanhThuThang();
        this.loadKhungGio();
        this.loadPhuongThuc();
    },

    switchTab(tab) {
        // Update tab buttons
        document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
        event.target.classList.add('active');

        // Update tab content
        document.querySelectorAll('.tab-content').forEach(content => {
            content.style.display = 'none';
        });

        this.currentTab = tab;

        // Show selected tab
        if (tab === 'tongquan') {
            document.getElementById('tabTongQuan').style.display = 'block';
        } else if (tab === 'dichvu') {
            document.getElementById('tabDichVu').style.display = 'block';
            this.loadTopDichVu();
            this.loadLoaiDichVu();
            this.loadLoaiBan();
        } else if (tab === 'khachhang') {
            document.getElementById('tabKhachHang').style.display = 'block';
            this.loadTopKhachHang();
        } else if (tab === 'khac') {
            document.getElementById('tabKhac').style.display = 'block';
            this.loadSoSanh('ngay');
        }
    },

    applyFilter() {
        const tuNgay = document.getElementById('tuNgay').value;
        const denNgay = document.getElementById('denNgay').value;

        if (!tuNgay || !denNgay) {
            alert('Vui lòng chọn khoảng thời gian!');
            return;
        }

        if (new Date(tuNgay) > new Date(denNgay)) {
            alert('Ngày bắt đầu phải nhỏ hơn ngày kết thúc!');
            return;
        }

        this.loadTongQuan();
        this.loadKhungGio();
        this.loadPhuongThuc();

        if (this.currentTab === 'dichvu') {
            this.loadTopDichVu();
            this.loadLoaiDichVu();
            this.loadLoaiBan();
        } else if (this.currentTab === 'khachhang') {
            this.loadTopKhachHang();
        }
    },

    resetFilter() {
        // --- ĐÃ SỬA ---
        const now = new Date();

        // Đặt 'từ ngày' là đầu ngày hôm nay (00:00)
        const startOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0);
        document.getElementById('tuNgay').value = this.formatDateForInput(startOfDay);

        // Đặt 'đến ngày' là cuối ngày hôm nay (23:59)
        const endOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59);
        document.getElementById('denNgay').value = this.formatDateForInput(endOfDay);
        // --- KẾT THÚC SỬA ---

        this.applyFilter();
    },

    formatCurrency(value) {
        return new Intl.NumberFormat('vi-VN').format(value) + ' đ';
    },

    /**
     * HÀM MỚI: Định dạng ngày giờ cho input datetime-local
     * @param {Date} date - Đối tượng Date
     * @returns {string} - Chuỗi định dạng 'YYYY-MM-DDTHH:mm'
     */
    formatDateForInput(date) {
        const pad = (num) => String(num).padStart(2, '0');
        const yyyy = date.getFullYear();
        const MM = pad(date.getMonth() + 1);
        const dd = pad(date.getDate());
        const HH = pad(date.getHours());
        const mm = pad(date.getMinutes());
        return `${yyyy}-${MM}-${dd}T${HH}:${mm}`;
    },

    formatNumber(value) {
        return new Intl.NumberFormat('vi-VN').format(value);
    },

    // Load tổng quan
    async loadTongQuan() {
        try {
            const tuNgay = document.getElementById('tuNgay').value;
            const denNgay = document.getElementById('denNgay').value;

            const response = await fetch(`/ThongKe/GetTongQuan?tuNgay=${tuNgay}&denNgay=${denNgay}`);
            const result = await response.json();

            if (result.success) {
                const data = result.data;
                document.getElementById('tongDoanhThu').textContent = this.formatCurrency(data.tongDoanhThu);
                document.getElementById('soHoaDon').textContent = this.formatNumber(data.soHoaDon);
                document.getElementById('soKhachHang').textContent = this.formatNumber(data.soKhachHang);
                document.getElementById('trungBinh').textContent = this.formatCurrency(data.doanhThuTrungBinh);
            }
        } catch (error) {
            console.error('Error loading tong quan:', error);
        }
    },

    // Load doanh thu 7 ngày
    async loadDoanhThu7Ngay() {
        try {
            const response = await fetch('/ThongKe/GetDoanhThuTheoNgay');
            const result = await response.json();

            if (result.success) {
                const ctx = document.getElementById('chartDoanhThu7Ngay');

                if (this.charts['doanhThu7Ngay']) {
                    this.charts['doanhThu7Ngay'].destroy();
                }

                this.charts['doanhThu7Ngay'] = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: result.data.map(d => d.ngay),
                        datasets: [{
                            label: 'Doanh thu (VNĐ)',
                            data: result.data.map(d => d.doanhThu),
                            backgroundColor: 'rgba(102, 126, 234, 0.8)',
                            borderColor: 'rgba(102, 126, 234, 1)',
                            borderWidth: 2,
                            borderRadius: 8
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: false
                            },
                            tooltip: {
                                callbacks: {
                                    label: (context) => {
                                        return 'Doanh thu: ' + this.formatCurrency(context.parsed.y);
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: {
                                    callback: (value) => {
                                        return (value / 1000000).toFixed(1) + 'M';
                                    }
                                }
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading doanh thu 7 ngay:', error);
        }
    },

    // Load doanh thu theo tháng
    async loadDoanhThuThang() {
        try {
            const nam = document.getElementById('namThongKe').value;
            const response = await fetch(`/ThongKe/GetDoanhThuTheoThang?nam=${nam}`);
            const result = await response.json();

            if (result.success) {
                const ctx = document.getElementById('chartDoanhThuThang');

                if (this.charts['doanhThuThang']) {
                    this.charts['doanhThuThang'].destroy();
                }

                this.charts['doanhThuThang'] = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: result.data.map(d => d.thang),
                        datasets: [{
                            label: 'Doanh thu (VNĐ)',
                            data: result.data.map(d => d.doanhThu),
                            backgroundColor: 'rgba(102, 126, 234, 0.1)',
                            borderColor: 'rgba(102, 126, 234, 1)',
                            borderWidth: 3,
                            fill: true,
                            tension: 0.4,
                            pointRadius: 5,
                            pointHoverRadius: 7,
                            pointBackgroundColor: 'rgba(102, 126, 234, 1)'
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: false
                            },
                            tooltip: {
                                callbacks: {
                                    label: (context) => {
                                        return 'Doanh thu: ' + this.formatCurrency(context.parsed.y);
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: {
                                    callback: (value) => {
                                        return (value / 1000000).toFixed(1) + 'M';
                                    }
                                }
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading doanh thu thang:', error);
        }
    },

    // Load doanh thu theo khung giờ
    async loadKhungGio() {
        try {
            const tuNgay = document.getElementById('tuNgay').value;
            const denNgay = document.getElementById('denNgay').value;

            const response = await fetch(`/ThongKe/GetDoanhThuTheoGio?tuNgay=${tuNgay}&denNgay=${denNgay}`);
            const result = await response.json();

            if (result.success) {
                const ctx = document.getElementById('chartKhungGio');

                if (this.charts['khungGio']) {
                    this.charts['khungGio'].destroy();
                }

                const colors = [
                    'rgba(102, 126, 234, 0.8)',
                    'rgba(118, 75, 162, 0.8)',
                    'rgba(255, 193, 7, 0.8)',
                    'rgba(220, 53, 69, 0.8)'
                ];

                this.charts['khungGio'] = new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        labels: result.data.map(d => d.khungGio),
                        datasets: [{
                            data: result.data.map(d => d.doanhThu),
                            backgroundColor: colors,
                            borderWidth: 0
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'bottom'
                            },
                            tooltip: {
                                callbacks: {
                                    label: (context) => {
                                        const label = context.label || '';
                                        const value = this.formatCurrency(context.parsed);
                                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                        const percent = ((context.parsed / total) * 100).toFixed(1);
                                        return `${label}: ${value} (${percent}%)`;
                                    }
                                }
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading khung gio:', error);
        }
    },

    // Load phương thức thanh toán
    async loadPhuongThuc() {
        try {
            const tuNgay = document.getElementById('tuNgay').value;
            const denNgay = document.getElementById('denNgay').value;

            const response = await fetch(`/ThongKe/GetPhuongThucThanhToan?tuNgay=${tuNgay}&denNgay=${denNgay}`);
            const result = await response.json();

            if (result.success) {
                const ctx = document.getElementById('chartPhuongThuc');

                if (this.charts['phuongThuc']) {
                    this.charts['phuongThuc'].destroy();
                }

                const colors = [
                    'rgba(40, 167, 69, 0.8)',
                    'rgba(23, 162, 184, 0.8)',
                    'rgba(255, 193, 7, 0.8)',
                    'rgba(220, 53, 69, 0.8)',
                    'rgba(102, 126, 234, 0.8)'
                ];

                this.charts['phuongThuc'] = new Chart(ctx, {
                    type: 'pie',
                    data: {
                        labels: result.data.map(d => d.phuongThuc),
                        datasets: [{
                            data: result.data.map(d => d.tongTien),
                            backgroundColor: colors,
                            borderWidth: 0
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'bottom'
                            },
                            tooltip: {
                                callbacks: {
                                    label: (context) => {
                                        const label = context.label || '';
                                        const value = this.formatCurrency(context.parsed);
                                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                        const percent = ((context.parsed / total) * 100).toFixed(1);
                                        return `${label}: ${value} (${percent}%)`;
                                    }
                                }
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading phuong thuc:', error);
        }
    },

    // Continued in next artifact...

    // Load top dịch vụ
    async loadTopDichVu() {
        try {
            const tuNgay = document.getElementById('tuNgay').value;
            const denNgay = document.getElementById('denNgay').value;

            const response = await fetch(`/ThongKe/GetTopDichVu?tuNgay=${tuNgay}&denNgay=${denNgay}&top=10`);
            const result = await response.json();

            if (result.success) {
                const ctx = document.getElementById('chartTopDichVu');

                if (this.charts['topDichVu']) {
                    this.charts['topDichVu'].destroy();
                }

                this.charts['topDichVu'] = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: result.data.map(d => d.tenDV),
                        datasets: [{
                            label: 'Số lượng bán',
                            data: result.data.map(d => d.soLuong),
                            backgroundColor: 'rgba(102, 126, 234, 0.8)',
                            borderColor: 'rgba(102, 126, 234, 1)',
                            borderWidth: 2,
                            borderRadius: 8
                        }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: false
                            },
                            tooltip: {
                                callbacks: {
                                    label: (context) => {
                                        return 'Đã bán: ' + this.formatNumber(context.parsed.x) + ' ' + (context.parsed.x > 1 ? 'phần' : 'phần');
                                    }
                                }
                            }
                        },
                        scales: {
                            x: {
                                beginAtZero: true
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading top dich vu:', error);
        }
    },

    // Load doanh thu theo loại dịch vụ
    async loadLoaiDichVu() {
        try {
            const tuNgay = document.getElementById('tuNgay').value;
            const denNgay = document.getElementById('denNgay').value;

            const response = await fetch(`/ThongKe/GetDoanhThuTheoLoaiDichVu?tuNgay=${tuNgay}&denNgay=${denNgay}`);
            const result = await response.json();

            if (result.success) {
                const ctx = document.getElementById('chartLoaiDichVu');

                if (this.charts['loaiDichVu']) {
                    this.charts['loaiDichVu'].destroy();
                }

                const colors = [
                    'rgba(255, 193, 7, 0.8)',
                    'rgba(40, 167, 69, 0.8)',
                    'rgba(23, 162, 184, 0.8)'
                ];

                this.charts['loaiDichVu'] = new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        labels: result.data.map(d => {
                            if (d.loai === 'DoUong') return '🍹 Đồ uống';
                            if (d.loai === 'DoAn') return '🍔 Đồ ăn';
                            return '📦 Khác';
                        }),
                        datasets: [{
                            data: result.data.map(d => d.doanhThu),
                            backgroundColor: colors,
                            borderWidth: 0
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'bottom'
                            },
                            tooltip: {
                                callbacks: {
                                    label: (context) => {
                                        const label = context.label || '';
                                        const value = this.formatCurrency(context.parsed);
                                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                        const percent = ((context.parsed / total) * 100).toFixed(1);
                                        return `${label}: ${value} (${percent}%)`;
                                    }
                                }
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading loai dich vu:', error);
        }
    },

    // Load doanh thu theo loại bàn
    async loadLoaiBan() {
        try {
            const tuNgay = document.getElementById('tuNgay').value;
            const denNgay = document.getElementById('denNgay').value;

            const response = await fetch(`/ThongKe/GetDoanhThuTheoLoaiBan?tuNgay=${tuNgay}&denNgay=${denNgay}`);
            const result = await response.json();

            if (result.success) {
                const ctx = document.getElementById('chartLoaiBan');

                if (this.charts['loaiBan']) {
                    this.charts['loaiBan'].destroy();
                }

                const colors = [
                    'rgba(102, 126, 234, 0.8)',
                    'rgba(118, 75, 162, 0.8)',
                    'rgba(255, 193, 7, 0.8)',
                    'rgba(40, 167, 69, 0.8)',
                    'rgba(220, 53, 69, 0.8)'
                ];

                this.charts['loaiBan'] = new Chart(ctx, {
                    type: 'pie',
                    data: {
                        labels: result.data.map(d => d.loaiBan),
                        datasets: [{
                            data: result.data.map(d => d.doanhThu),
                            backgroundColor: colors,
                            borderWidth: 0
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'bottom'
                            },
                            tooltip: {
                                callbacks: {
                                    label: (context) => {
                                        const label = context.label || '';
                                        const value = this.formatCurrency(context.parsed);
                                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                        const percent = ((context.parsed / total) * 100).toFixed(1);
                                        return `${label}: ${value} (${percent}%)`;
                                    }
                                }
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading loai ban:', error);
        }
    },

    // Load top khách hàng
    async loadTopKhachHang() {
        try {
            const tuNgay = document.getElementById('tuNgay').value;
            const denNgay = document.getElementById('denNgay').value;

            const response = await fetch(`/ThongKe/GetTopKhachHang?tuNgay=${tuNgay}&denNgay=${denNgay}&top=10`);
            const result = await response.json();

            if (result.success) {
                const tbody = document.querySelector('#tableTopKhachHang tbody');
                tbody.innerHTML = '';

                if (result.data.length === 0) {
                    tbody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 30px; color: #6c757d;">Không có dữ liệu</td></tr>';
                    return;
                }

                result.data.forEach((kh, index) => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td style="font-weight: 600; color: #667eea;">${index + 1}</td>
                        <td style="font-weight: 600;">${kh.tenKH}</td>
                        <td>${kh.sdt}</td>
                        <td style="font-weight: 700; color: #28a745;">${this.formatCurrency(kh.tongChiTieu)}</td>
                        <td style="text-align: center;"><span style="background: #f8f9fa; padding: 4px 12px; border-radius: 12px; font-weight: 600;">${kh.soLanDen} lần</span></td>
                    `;
                    tbody.appendChild(row);
                });
            }
        } catch (error) {
            console.error('Error loading top khach hang:', error);
        }
    },

    // Load so sánh doanh thu
    async loadSoSanh(loai) {
        try {
            // Update active button
            document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active'));
            event.target.classList.add('active');

            const response = await fetch(`/ThongKe/GetSoSanhDoanhThu?loai=${loai}`);
            const result = await response.json();

            if (result.success) {
                const container = document.getElementById('comparisonContainer');
                const data = result.data;

                let title = loai === 'ngay' ? 'Hôm nay' : (loai === 'tuan' ? 'Tuần này' : 'Tháng này');
                let prevTitle = loai === 'ngay' ? 'Hôm qua' : (loai === 'tuan' ? 'Tuần trước' : 'Tháng trước');

                const changeClass = data.tangTruong >= 0 ? 'up' : 'down';
                const changeIcon = data.tangTruong >= 0 ? '📈' : '📉';
                const changeText = data.tangTruong >= 0 ? `+${data.tangTruong}%` : `${data.tangTruong}%`;

                container.innerHTML = `
                    <div class="comparison-card">
                        <h4>${title}</h4>
                        <div class="comparison-value">${this.formatCurrency(data.hienTai)}</div>
                        <span class="comparison-badge ${changeClass}">${changeIcon} ${changeText}</span>
                    </div>
                    <div class="comparison-card">
                        <h4>${prevTitle}</h4>
                        <div class="comparison-value">${this.formatCurrency(data.truoc)}</div>
                    </div>
                    <div class="comparison-card">
                        <h4>Chênh lệch</h4>
                        <div class="comparison-value">${this.formatCurrency(Math.abs(data.hienTai - data.truoc))}</div>
                        <span class="comparison-badge ${changeClass}">${changeText}</span>
                    </div>
                `;
            }
        } catch (error) {
            console.error('Error loading so sanh:', error);
        }
    }
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    ThongKeManager.init();
});