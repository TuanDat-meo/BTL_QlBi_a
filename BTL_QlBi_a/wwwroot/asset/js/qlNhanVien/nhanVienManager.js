const NhanVienManager = {
    currentFilters: {
        status: 'all',
        role: 'all',
        shift: 'all',
        search: ''
    },

    // Filter methods không cần async
    filterByStatus: function (status, event) {
        if (event) {
            document.querySelectorAll('#page-nhanvien .filter-buttons button').forEach(btn => {
                if (btn.getAttribute('onclick')?.includes('filterByStatus')) {
                    btn.classList.remove('active');
                }
            });
            event.target.classList.add('active');
        }
        this.currentFilters.status = status;
        this.applyFilters();
    },

    filterByRole: function (role, event) {
        if (event) {
            document.querySelectorAll('#page-nhanvien .filter-buttons button').forEach(btn => {
                if (btn.getAttribute('onclick')?.includes('filterByRole')) {
                    btn.classList.remove('active');
                }
            });
            event.target.classList.add('active');
        }
        this.currentFilters.role = role;
        this.applyFilters();
    },

    filterByShift: function (shift, event) {
        if (event) {
            document.querySelectorAll('#page-nhanvien .filter-buttons button').forEach(btn => {
                if (btn.getAttribute('onclick')?.includes('filterByShift')) {
                    btn.classList.remove('active');
                }
            });
            event.target.classList.add('active');
        }
        this.currentFilters.shift = shift;
        this.applyFilters();
    },

    search: function () {
        const searchInput = document.getElementById('searchNhanVien');
        this.currentFilters.search = searchInput.value.toLowerCase().trim();
        this.applyFilters();
    },

    applyFilters: function () {
        const cards = document.querySelectorAll('.employee-card');
        let visibleCount = 0;

        cards.forEach(card => {
            const status = card.getAttribute('data-status');
            const role = card.getAttribute('data-role');
            const shift = card.getAttribute('data-shift');
            const searchText = card.getAttribute('data-search').toLowerCase();

            let show = true;

            if (this.currentFilters.status !== 'all') {
                if (this.currentFilters.status === 'DangLam' && status !== 'DangLam') show = false;
                if (this.currentFilters.status === 'Nghi' && status !== 'Nghi') show = false;
            }

            if (this.currentFilters.role !== 'all' && role !== this.currentFilters.role) {
                show = false;
            }

            if (this.currentFilters.shift !== 'all' && shift !== this.currentFilters.shift) {
                show = false;
            }

            if (this.currentFilters.search && !searchText.includes(this.currentFilters.search)) {
                show = false;
            }

            card.style.display = show ? '' : 'none';
            if (show) visibleCount++;
        });

        this.showNoResultsMessage(visibleCount);
    },

    showNoResultsMessage: function (count) {
        let messageDiv = document.getElementById('noResultsMessage');

        if (count === 0) {
            if (!messageDiv) {
                messageDiv = document.createElement('div');
                messageDiv.id = 'noResultsMessage';
                messageDiv.className = 'empty-state';
                messageDiv.innerHTML = `
                    <div class="empty-icon">🔍</div>
                    <h3>Không tìm thấy nhân viên</h3>
                    <p class="empty-text">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm</p>
                    <button class="btn btn-secondary" onclick="NhanVienManager.resetFilters()" style="margin-top: 15px;">
                        🔄 Đặt lại bộ lọc
                    </button>
                `;
                document.querySelector('.employees-grid')?.insertAdjacentElement('afterend', messageDiv);
            }
            messageDiv.style.display = 'block';
        } else {
            if (messageDiv) {
                messageDiv.style.display = 'none';
            }
        }
    },

    resetFilters: function () {
        this.currentFilters = {
            status: 'all',
            role: 'all',
            shift: 'all',
            search: ''
        };

        document.getElementById('searchNhanVien').value = '';
        document.querySelectorAll('.filter-btn').forEach(btn => {
            btn.classList.remove('active');
            if (btn.textContent.trim() === 'Tất cả') {
                btn.classList.add('active');
            }
        });

        this.applyFilters();
    },

    openAddModal: async function () {
        try {
            const response = await fetch('/NhanVien/FormThemNhanVien');
            if (!response.ok) throw new Error('Failed to load form');

            const html = await response.text();
            openModal('➕ Thêm nhân viên mới', html);
        } catch (error) {
            console.error('Error:', error);
            this.showNotification('❌ Có lỗi khi tải form: ' + error.message, 'error');
        }
    },

    openEditModal: async function (maNV) {
        try {
            const response = await fetch(`/NhanVien/FormChinhSuaNhanVien?maNV=${maNV}`);
            if (!response.ok) throw new Error('Failed to load form');

            const html = await response.text();
            openModal('✏️ Chỉnh sửa nhân viên', html);
        } catch (error) {
            console.error('Error:', error);
            this.showNotification('❌ Có lỗi khi tải form: ' + error.message, 'error');
        }
    },

    viewDetail: async function (maNV) {
        try {
            const response = await fetch(`/NhanVien/ChiTietNhanVien?maNV=${maNV}`);
            if (!response.ok) throw new Error('Failed to load detail');

            const html = await response.text();

            const detailPanel = document.getElementById('detailPanel');
            if (detailPanel) {
                detailPanel.innerHTML = html;
                detailPanel.scrollTop = 0;
            }
        } catch (error) {
            console.error('Error:', error);
            this.showNotification('❌ Có lỗi khi tải chi tiết: ' + error.message, 'error');
        }
    },

    confirmDelete: function (maNV, tenNV) {
        if (confirm(`⚠️ Bạn có chắc chắn muốn xóa nhân viên "${tenNV}"?\n\nLưu ý: Nếu nhân viên có dữ liệu liên quan, tài khoản sẽ chuyển sang trạng thái Nghỉ thay vì xóa hoàn toàn.`)) {
            this.deleteNhanVien(maNV);
        }
    },

    deleteNhanVien: async function (maNV) {
        try {
            const response = await fetch('/NhanVien/XoaNhanVien', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ MaNV: maNV })
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification('✅ ' + result.message, 'success');
                setTimeout(() => location.reload(), 1500);
            } else {
                this.showNotification('❌ ' + result.message, 'error');
            }
        } catch (error) {
            console.error('Error:', error);
            this.showNotification('❌ Có lỗi xảy ra: ' + error.message, 'error');
        }
    },

    viewHistory: async function (maNV) {
        try {
            const response = await fetch(`/NhanVien/LichSuHoatDong?maNV=${maNV}`);
            if (!response.ok) throw new Error('Failed to load history');

            const html = await response.text();
            openModal('📜 Lịch sử hoạt động', html);
        } catch (error) {
            console.error('Error:', error);
            this.showNotification('❌ Có lỗi khi tải lịch sử: ' + error.message, 'error');
        }
    },

    viewAttendance: async function (maNV) {
        try {
            const thang = new Date().getMonth() + 1;
            const nam = new Date().getFullYear();

            const response = await fetch(`/NhanVien/ChamCongNhanVien?maNV=${maNV}&thang=${thang}&nam=${nam}`);
            if (!response.ok) throw new Error('Failed to load attendance');

            const html = await response.text();
            openModal('📅 Chấm công chi tiết', html);
        } catch (error) {
            console.error('Error:', error);
            this.showNotification('❌ Có lỗi khi tải chấm công: ' + error.message, 'error');
        }
    },

    openAttendanceModal: function (maNV) {
        if (typeof openAttendanceModal === 'function') {
            openAttendanceModal(maNV);
        } else {
            this.showNotification('⚠️ Chức năng chấm công chưa được tải', 'warning');
        }
    },

    exportToExcel: function () {
        const visibleCards = Array.from(document.querySelectorAll('.employee-card'))
            .filter(card => card.style.display !== 'none');

        if (visibleCards.length === 0) {
            this.showNotification('❌ Không có dữ liệu để xuất', 'warning');
            return;
        }

        let csv = '\uFEFF';
        csv += 'Mã NV,Họ tên,Số điện thoại,Nhóm quyền,Ca làm việc,Lương cơ bản,Trạng thái\n';

        visibleCards.forEach(card => {
            const maNV = card.getAttribute('data-employee-id');
            const tenNV = card.querySelector('.employee-name')?.textContent.trim() || '';
            const sdt = card.querySelector('.employee-phone')?.textContent.replace('📱', '').trim() || '';
            const nhomQuyen = card.getAttribute('data-role') || '';
            const caLamViec = card.getAttribute('data-shift') || '';
            const luong = card.querySelector('.employee-salary')?.textContent.replace('💵', '').trim() || '';
            const trangThai = card.getAttribute('data-status') === 'DangLam' ? 'Đang làm' : 'Nghỉ việc';

            csv += `#${maNV},"${tenNV}",${sdt},"${nhomQuyen}","${caLamViec}","${luong}","${trangThai}"\n`;
        });

        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);

        link.setAttribute('href', url);
        link.setAttribute('download', `DanhSachNhanVien_${new Date().getTime()}.csv`);
        link.style.visibility = 'hidden';

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        this.showNotification('✅ Xuất Excel thành công!', 'success');
    },

    showNotification: function (message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 20px;
            background: ${type === 'success' ? '#d4edda' : type === 'error' ? '#f8d7da' : type === 'warning' ? '#fff3cd' : '#d1ecf1'};
            color: ${type === 'success' ? '#155724' : type === 'error' ? '#721c24' : type === 'warning' ? '#856404' : '#0c5460'};
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            z-index: 10000;
            animation: slideInRight 0.3s ease;
            font-weight: 600;
            max-width: 350px;
        `;
        notification.textContent = message;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideOutRight 0.3s ease';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
};

// Initialize
document.addEventListener('DOMContentLoaded', function () {
    console.log('✅ NhanVienManager initialized');

    const searchInput = document.getElementById('searchNhanVien');
    if (searchInput) {
        searchInput.addEventListener('input', () => NhanVienManager.search());
    }

    document.addEventListener('keydown', function (e) {
        if (e.ctrlKey && e.key === 'f') {
            e.preventDefault();
            searchInput?.focus();
        }

        if (e.ctrlKey && e.key === 'n') {
            e.preventDefault();
            const addBtn = document.querySelector('[onclick*="openAddModal"]');
            if (addBtn) {
                NhanVienManager.openAddModal();
            }
        }
    });
});

// Export globally
window.NhanVienManager = NhanVienManager;