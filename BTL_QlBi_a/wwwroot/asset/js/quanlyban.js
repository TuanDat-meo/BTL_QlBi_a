const TableManager = {
    currentArea: 'all',
    currentStatus: 'all',

    // Hiển thị chi tiết bàn vào panel bên phải
    showDetail: async function (maBan) {
        try {
            console.log('Loading detail for table:', maBan);

            const detailPanel = document.getElementById('detailPanel');
            if (!detailPanel) {
                console.error('Detail panel not found!');
                return;
            }

            // Hiển thị loading
            detailPanel.innerHTML = `
                <div class="loading-state">
                    <div class="spinner"></div>
                    <p>Đang tải thông tin...</p>
                </div>
            `;

            // FIX: Gọi API đúng endpoint
            const response = await fetch(`/Home/ChiTietBan?maBan=${maBan}`, {
                method: 'GET',
                headers: {
                    'Accept': 'text/html'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const html = await response.text();
            console.log('Received HTML length:', html.length);

            // Cập nhật nội dung panel
            detailPanel.innerHTML = html;

            // Highlight card đã chọn
            document.querySelectorAll('.table-card').forEach(card => {
                card.classList.remove('selected');
            });

            const selectedCard = document.querySelector(`[data-table-id="${maBan}"]`);
            if (selectedCard) {
                selectedCard.classList.add('selected');
            }

            // Đảm bảo right-panel hiển thị
            const rightPanel = document.querySelector('.right-panel');
            if (rightPanel) {
                rightPanel.style.display = 'flex';
                rightPanel.style.visibility = 'visible';

                // Scroll panel vào view trên mobile
                if (window.innerWidth <= 1024) {
                    setTimeout(() => {
                        rightPanel.scrollIntoView({
                            behavior: 'smooth',
                            block: 'nearest'
                        });
                    }, 100);
                }
            }

            console.log('✅ Detail loaded successfully');
        } catch (error) {
            console.error('❌ Error loading detail:', error);

            const detailPanel = document.getElementById('detailPanel');
            if (detailPanel) {
                detailPanel.innerHTML = `
                    <div class="error-state">
                        <div class="error-icon">⚠️</div>
                        <p>Không thể tải chi tiết bàn</p>
                        <p style="font-size: 12px; color: #999;">${error.message}</p>
                        <button class="btn btn-primary" onclick="location.reload()">Tải lại</button>
                    </div>
                `;
            }

            if (window.Toast) {
                Toast.error('Không thể tải chi tiết bàn');
            }
        }
    },

    // Bắt đầu chơi
    start: async function (maBan) {
        const customerId = prompt('Nhập mã khách hàng (để trống nếu là khách lẻ):');
        const maKH = customerId && customerId.trim() !== '' ? parseInt(customerId) : null;

        try {
            const response = await fetch('/Home/BatDauChoi', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ maBan, maKH })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi bắt đầu chơi');
        }
    },

    // Kết thúc chơi
    end: async function (maBan) {
        if (!confirm('Bạn có chắc muốn kết thúc và tính tiền cho bàn này?')) {
            return;
        }

        try {
            const response = await fetch('/Home/KetThucChoi', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ maBan })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                // Redirect sang trang thanh toán nếu có
                if (result.hoaDonId) {
                    setTimeout(() => {
                        window.location.href = `/Home/ThanhToan?maHD=${result.hoaDonId}`;
                    }, 1000);
                } else {
                    setTimeout(() => location.reload(), 1000);
                }
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi kết thúc');
        }
    },

    // Thêm dịch vụ
    addService: function (maBan) {
        openAddServiceModal(maBan);
    },

    // Xác nhận đặt bàn
    confirmReservation: async function (maBan) {
        if (!confirm('Xác nhận khách hàng đã đến?')) {
            return;
        }

        try {
            const response = await fetch('/Home/XacNhanDatBan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ maBan })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra');
        }
    },

    // Lọc theo khu vực - FIXED
    filterByArea: function (area, event) {
        console.log('🔍 Filter by area:', area);
        this.currentArea = area;
        this.applyFilters();

        // Cập nhật UI
        const areaFilters = document.getElementById('areaFilters');
        if (areaFilters) {
            areaFilters.querySelectorAll('.filter-btn').forEach(btn => {
                btn.classList.remove('active');
            });
        }

        if (event && event.target) {
            event.target.classList.add('active');
        }
    },

    // Lọc theo trạng thái - FIXED
    filterByStatus: function (status, event) {
        console.log('🔍 Filter by status:', status);
        this.currentStatus = status;
        this.applyFilters();

        // Cập nhật UI
        const statusFilters = document.getElementById('statusFilters');
        if (statusFilters) {
            statusFilters.querySelectorAll('.filter-btn').forEach(btn => {
                btn.classList.remove('active');
            });
        }

        if (event && event.target) {
            event.target.classList.add('active');
        }
    },

    // Áp dụng bộ lọc - IMPROVED
    applyFilters: function () {
        console.log('🎯 Applying filters - Area:', this.currentArea, 'Status:', this.currentStatus);

        const cards = document.querySelectorAll('.table-card');
        let visibleCount = 0;
        let hiddenCount = 0;

        cards.forEach(card => {
            const cardArea = card.getAttribute('data-area');
            const cardStatus = card.getAttribute('data-status');
            const tableName = card.querySelector('.table-name')?.textContent;

            // Debug mỗi card
            // console.log(`Card: ${tableName}, Area: "${cardArea}", Status: "${cardStatus}"`);

            const areaMatch = this.currentArea === 'all' || cardArea === this.currentArea;
            const statusMatch = this.currentStatus === 'all' || cardStatus === this.currentStatus;

            if (areaMatch && statusMatch) {
                card.style.display = 'block';
                visibleCount++;
            } else {
                card.style.display = 'none';
                hiddenCount++;
            }
        });

        console.log(`✅ Visible: ${visibleCount}, Hidden: ${hiddenCount}, Total: ${cards.length}`);

        // Hiển thị thông báo nếu không có kết quả
        const tablesGrid = document.getElementById('tablesGrid');
        if (tablesGrid) {
            let emptyMessage = tablesGrid.querySelector('.filter-empty-message');

            if (visibleCount === 0) {
                if (!emptyMessage) {
                    emptyMessage = document.createElement('div');
                    emptyMessage.className = 'filter-empty-message';
                    emptyMessage.style.cssText = `
                        grid-column: 1 / -1;
                        text-align: center;
                        padding: 60px 20px;
                        color: #6c757d;
                    `;
                    emptyMessage.innerHTML = `
                        <div style="font-size: 48px; margin-bottom: 20px;">🔍</div>
                        <h3>Không tìm thấy bàn</h3>
                        <p>Không có bàn nào phù hợp với bộ lọc hiện tại</p>
                    `;
                    tablesGrid.appendChild(emptyMessage);
                }
            } else if (emptyMessage) {
                emptyMessage.remove();
            }
        }
    },

    // Tìm kiếm - IMPROVED
    search: function () {
        const searchInput = document.getElementById('searchTables');
        if (!searchInput) {
            console.error('Search input not found');
            return;
        }

        const searchValue = searchInput.value.toLowerCase().trim();
        console.log('🔍 Searching for:', searchValue);

        const cards = document.querySelectorAll('.table-card');
        let foundCount = 0;

        if (searchValue === '') {
            // Nếu search rỗng, apply lại filters
            this.applyFilters();
            return;
        }

        cards.forEach(card => {
            const tableName = card.querySelector('.table-name')?.textContent.toLowerCase() || '';
            const cardArea = card.getAttribute('data-area');
            const cardStatus = card.getAttribute('data-status');

            // Kiểm tra search match
            const searchMatch = tableName.includes(searchValue);

            // Kiểm tra filter match
            const areaMatch = this.currentArea === 'all' || cardArea === this.currentArea;
            const statusMatch = this.currentStatus === 'all' || cardStatus === this.currentStatus;

            // Hiển thị nếu match cả 3 điều kiện
            if (searchMatch && areaMatch && statusMatch) {
                card.style.display = 'block';
                foundCount++;
            } else {
                card.style.display = 'none';
            }
        });

        console.log(`✅ Found ${foundCount} cards matching "${searchValue}"`);
    }
};

// Hàm mở modal thêm dịch vụ
function openAddServiceModal(maBan) {
    if (window.Toast) {
        Toast.info('Chức năng thêm dịch vụ chưa được triển khai đầy đủ.');
    } else {
        alert('Chức năng thêm dịch vụ chưa được triển khai đầy đủ.');
    }
}

// Hàm mở modal đặt bàn
function openReservationModal() {
    if (window.Toast) {
        Toast.info('Chức năng đặt bàn chưa được triển khai đầy đủ.');
    } else {
        alert('Chức năng đặt bàn chưa được triển khai đầy đủ.');
    }
}

// --- BẮT ĐẦU PHẦN SỬA LỖI ---
(function () {
    // Kiểm tra nếu thẻ style cho TableManager đã tồn tại để tránh lỗi "already been declared"
    if (document.getElementById('table-manager-styles')) {
        return;
    }

    const style = document.createElement('style');
    style.id = 'table-manager-styles'; // Thêm ID để kiểm tra ở lần load sau
    style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }

    /* ... Giữ nguyên toàn bộ nội dung CSS còn lại ... */

    /* Thêm CSS cho trạng thái SELECTED trong _TableCard.cshtml (Nếu chưa có trong index.css) */
    .table-card.selected {
        border: 3px solid #3b82f6 !important;
        box-shadow: 0 8px 20px rgba(59, 130, 246, 0.4) !important;
        transform: translateY(-5px);
    }
    .table-card.selected::after {
        /* ... Giữ nguyên CSS cho animation pulse ... */
    }

    /* Loading State */
    .loading-state { /* ... */ }
    .spinner { /* ... */ }
    @keyframes spin { /* ... */ }
    .loading-state p { /* ... */ }

    /* Error State */
    .error-state { /* ... */ }
    .error-icon { /* ... */ }
    .error-state p { /* ... */ }

    /* Đảm bảo right-panel luôn hiển thị */
    .right-panel {
        display: flex !important;
        visibility: visible !important;
    }

    .panel-body {
        display: block !important;
    }
    `;
    document.head.appendChild(style);
})();
// --- KẾT THÚC PHẦN SỬA LỖI ---
document.head.appendChild(style);

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function () {
    console.log('=== TableManager Initialization ===');
    console.log('Detail panel:', document.getElementById('detailPanel'));
    console.log('Right panel:', document.querySelector('.right-panel'));
    console.log('Table cards:', document.querySelectorAll('.table-card').length);

    // Debug: Log tất cả data attributes
    document.querySelectorAll('.table-card').forEach((card, index) => {
        if (index < 3) { // Chỉ log 3 card đầu
            console.log(`Card ${index + 1}:`, {
                name: card.querySelector('.table-name')?.textContent,
                area: card.getAttribute('data-area'),
                status: card.getAttribute('data-status'),
                id: card.getAttribute('data-table-id')
            });
        }
    });

    // Tự động load chi tiết bàn đầu tiên (nếu có)
    const firstCard = document.querySelector('.table-card');
    if (firstCard) {
        const tableId = firstCard.getAttribute('data-table-id');
        if (tableId) {
            console.log('Auto-loading first table:', tableId);
            setTimeout(() => {
                TableManager.showDetail(parseInt(tableId));
            }, 500);
        }
    } else {
        console.log('No table cards found on page');
    }

    // Apply initial filters
    TableManager.applyFilters();
});

// Export để sử dụng global
window.TableManager = TableManager;

console.log('✅ TableManager script loaded successfully');