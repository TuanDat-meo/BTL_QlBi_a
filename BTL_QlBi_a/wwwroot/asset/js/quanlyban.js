const TableManager = {
    currentArea: 'all',
    currentStatus: 'all',
    currentTableType: 'all',
    currentTableId: null,
    isShowingMenu: false,

    // Hiển thị chi tiết bàn vào panel bên phải
    showDetail: async function (maBan) {
        try {
            console.log('Loading detail for table:', maBan);

            const detailPanel = document.getElementById('detailPanel');
            const container = document.querySelector('.container');

            if (!detailPanel) {
                console.error('Detail panel not found!');
                return;
            }

            // Đóng modal nếu đang mở
            this.closeModal();

            this.currentTableId = maBan;

            if (container) {
                container.classList.add('with-detail');
            }

            detailPanel.innerHTML = `
                <div class="loading-state" style="text-align: center; padding: 40px;">
                    <div class="spinner" style="margin: 0 auto 20px;"></div>
                    <p>Đang tải thông tin...</p>
                </div>
            `;

            const response = await fetch(`/QLBan/ChiTietBan?maBan=${maBan}`, {
                method: 'GET',
                headers: {
                    'Accept': 'text/html'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const html = await response.text();
            detailPanel.innerHTML = html;

            document.querySelectorAll('.table-card').forEach(card => {
                card.classList.remove('selected');
            });

            const selectedCard = document.querySelector(`[data-table-id="${maBan}"]`);
            if (selectedCard) {
                selectedCard.classList.add('selected');
            }

            const rightPanel = document.querySelector('.right-panel');
            if (rightPanel) {
                rightPanel.style.display = 'flex';
                rightPanel.style.visibility = 'visible';

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
                    <div class="error-state" style="text-align: center; padding: 40px;">
                        <div class="error-icon" style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                        <p>Không thể tải chi tiết bàn</p>
                        <p style="font-size: 12px; color: #999; margin: 10px 0;">${error.message}</p>
                        <button class="btn btn-primary" onclick="location.reload()">Tải lại</button>
                    </div>
                `;
            }

            if (window.Toast) {
                Toast.error('Không thể tải chi tiết bàn');
            }
        }
    },

    // Hiển thị menu dịch vụ trong modal
    addService: async function (maBan) {
        try {
            console.log('Opening service menu for table:', maBan);
            this.currentTableId = maBan;

            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found!');
                return;
            }

            // Show loading
            modalOverlay.innerHTML = `
                <div class="modal-content service-modal">
                    <div class="loading-state" style="text-align: center; padding: 40px;">
                        <div class="spinner" style="margin: 0 auto 20px;"></div>
                        <p>Đang tải menu...</p>
                    </div>
                </div>
            `;
            modalOverlay.classList.add('active');

            const response = await fetch('/QLBan/LayDanhSachDichVu', {
                method: 'GET',
                headers: {
                    'Accept': 'text/html'
                }
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Server response:', errorText);
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const html = await response.text();

            // Wrap content in modal structure
            modalOverlay.innerHTML = `
                <div class="modal-content service-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Menu loaded successfully');
        } catch (error) {
            console.error('❌ Error loading menu:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content service-modal">
                        <div class="error-state" style="text-align: center; padding: 40px;">
                            <div class="error-icon" style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                            <p style="color: #dc3545; font-weight: 600;">Không thể tải menu dịch vụ</p>
                            <p style="font-size: 12px; color: #999; margin: 10px 0;">${error.message}</p>
                            <button class="btn btn-primary" onclick="TableManager.closeModal()">Đóng</button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) {
                Toast.error('Không thể tải menu. Vui lòng thử lại.');
            }
        }
    },

    // Đóng menu dịch vụ
    closeServiceMenu: function () {
        this.closeModal();
    },

    // Tăng số lượng
    increaseQuantity: function (maDV) {
        const input = document.getElementById(`qty-${maDV}`);
        if (input) {
            const currentValue = parseInt(input.value) || 1;
            if (currentValue < 99) {
                input.value = currentValue + 1;
            }
        }
    },

    // Giảm số lượng
    decreaseQuantity: function (maDV) {
        const input = document.getElementById(`qty-${maDV}`);
        if (input) {
            const currentValue = parseInt(input.value) || 1;
            if (currentValue > 1) {
                input.value = currentValue - 1;
            }
        }
    },

    // Xác nhận thêm dịch vụ
    confirmAddService: async function (maDV) {
        if (!this.currentTableId) {
            if (window.Toast) Toast.error('Không xác định được bàn');
            return;
        }

        const input = document.getElementById(`qty-${maDV}`);
        const soLuong = parseInt(input?.value) || 1;

        try {
            const response = await fetch('/QLBan/ThemDichVu', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    maBan: this.currentTableId,
                    maDV: maDV,
                    soLuong: soLuong
                })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                this.closeModal();
                setTimeout(() => {
                    this.showDetail(this.currentTableId);
                }, 300);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi thêm dịch vụ');
        }
    },

    // Bắt đầu chơi
    start: async function (maBan) {
        const customerId = prompt('Nhập mã khách hàng (để trống nếu là khách lẻ):');
        const maKH = customerId && customerId.trim() !== '' ? parseInt(customerId) : null;

        try {
            const response = await fetch('/QLBan/BatDauChoi', {
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
            const response = await fetch('/QLBan/KetThucChoi', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ maBan })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                if (result.hoaDonId) {
                    setTimeout(() => {
                        this.showPaymentPanel(result.hoaDonId);
                    }, 500);
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

    // Hiển thị panel thanh toán trong modal
    showPaymentPanel: async function (hoaDonId) {
        try {
            console.log('Loading payment panel for invoice:', hoaDonId);

            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found');
                return;
            }

            // Show loading state
            modalOverlay.innerHTML = `
                <div class="modal-content payment-modal">
                    <div class="loading-state" style="text-align: center; padding: 40px;">
                        <div class="spinner" style="margin: 0 auto 20px;"></div>
                        <p>Đang tải thông tin thanh toán...</p>
                    </div>
                </div>
            `;
            modalOverlay.classList.add('active');

            const response = await fetch(`/QLBan/PanelThanhToan?maHD=${hoaDonId}`, {
                method: 'GET',
                headers: {
                    'Accept': 'text/html'
                }
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Server error:', response.status, errorText);
                throw new Error(`HTTP ${response.status}`);
            }

            const html = await response.text();

            // Wrap content in modal structure
            modalOverlay.innerHTML = `
                <div class="modal-content payment-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Payment panel loaded successfully');
        } catch (error) {
            console.error('❌ Error loading payment panel:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content payment-modal">
                        <div class="error-state" style="text-align: center; padding: 40px;">
                            <div class="error-icon" style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                            <h4 style="color: #dc3545; margin-bottom: 10px;">Không thể tải panel thanh toán</h4>
                            <p style="font-size: 14px; color: #6c757d; margin: 10px 0;">${error.message}</p>
                            <button class="btn btn-primary" onclick="TableManager.closeModal()">Đóng</button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) {
                Toast.error('Không thể tải panel thanh toán. Vui lòng thử lại.');
            }
        }
    },

    // Xác nhận thanh toán
    confirmPayment: async function (hoaDonId, phuongThuc) {
        try {
            const tienKhachDua = document.getElementById('tienKhachDua')?.value || 0;

            const response = await fetch('/QLBan/XacNhanThanhToan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    maHD: hoaDonId,
                    phuongThucThanhToan: phuongThuc,
                    tienKhachDua: parseFloat(tienKhachDua)
                })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                this.closeModal();
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi thanh toán');
        }
    },

    // Xác nhận đặt bàn
    confirmReservation: async function (maBan) {
        if (!confirm('Xác nhận khách hàng đã đến?')) {
            return;
        }

        try {
            const response = await fetch('/QLBan/XacNhanDatBan', {
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

    // Chỉnh sửa bàn - hiển thị trong modal
    editTable: async function (maBan) {
        try {
            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found');
                return;
            }

            // Show loading
            modalOverlay.innerHTML = `
                <div class="modal-content edit-modal">
                    <div class="loading-state" style="text-align: center; padding: 40px;">
                        <div class="spinner" style="margin: 0 auto 20px;"></div>
                        <p>Đang tải thông tin...</p>
                    </div>
                </div>
            `;
            modalOverlay.classList.add('active');

            const response = await fetch(`/QLBan/PanelChinhSuaBan?maBan=${maBan}`);
            if (!response.ok) throw new Error('Không thể tải panel chỉnh sửa');

            const html = await response.text();

            // Wrap content in modal structure
            modalOverlay.innerHTML = `
                <div class="modal-content edit-modal">
                    ${html}
                </div>
            `;

        } catch (error) {
            console.error('Error:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content edit-modal">
                        <div class="error-state" style="text-align: center; padding: 40px;">
                            <div class="error-icon" style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                            <p style="color: #dc3545; font-weight: 600;">Không thể tải panel chỉnh sửa</p>
                            <button class="btn btn-primary" onclick="TableManager.closeModal()">Đóng</button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) Toast.error('Không thể tải panel chỉnh sửa');
        }
    },

    // Lưu chỉnh sửa bàn
    saveEditTable: async function (maBan) {
        try {
            const gioBatDau = document.getElementById('gioBatDau')?.value;

            const response = await fetch('/QLBan/LuuChinhSuaBan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    maBan: maBan,
                    gioBatDau: gioBatDau
                })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                this.closeModal();
                setTimeout(() => {
                    this.showDetail(maBan);
                }, 300);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi lưu');
        }
    },

    // Xóa dịch vụ
    removeService: async function (chiTietId, maBan) {
        if (!confirm('Bạn có chắc muốn xóa dịch vụ này?')) {
            return;
        }

        try {
            const response = await fetch('/QLBan/XoaDichVu', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id: chiTietId })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                // Reload the edit panel in modal
                setTimeout(() => {
                    this.editTable(maBan);
                }, 300);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi xóa');
        }
    },

    // Xóa bàn (hủy đặt bàn)
    deleteTable: async function (maBan) {
        if (!confirm('Bạn có chắc muốn hủy đặt bàn này?')) {
            return;
        }

        try {
            const response = await fetch('/QLBan/HuyDatBan', {
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

    // Lọc theo khu vực
    filterByArea: function (area, event) {
        console.log('🔍 Filter by area:', area);
        this.currentArea = area;
        this.applyFilters();

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

    // Lọc theo trạng thái
    filterByStatus: function (status, event) {
        console.log('🔍 Filter by status:', status);
        this.currentStatus = status;
        this.applyFilters();

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

    // Lọc theo loại bàn
    filterByTableType: function (tableType, event) {
        console.log('🔍 Filter by table type:', tableType);
        this.currentTableType = tableType;
        this.applyFilters();

        const typeFilters = document.getElementById('tableTypeFilters');
        if (typeFilters) {
            typeFilters.querySelectorAll('.filter-btn').forEach(btn => {
                btn.classList.remove('active');
            });
        }

        if (event && event.target) {
            event.target.classList.add('active');
        }
    },

    // Áp dụng bộ lọc
    applyFilters: function () {
        console.log('🎯 Applying filters - Area:', this.currentArea, 'Status:', this.currentStatus, 'Type:', this.currentTableType);

        const cards = document.querySelectorAll('.table-card');
        let visibleCount = 0;

        cards.forEach(card => {
            const cardArea = card.getAttribute('data-area');
            const cardStatus = card.getAttribute('data-status');
            const cardType = card.getAttribute('data-table-type');

            const areaMatch = this.currentArea === 'all' || cardArea === this.currentArea;
            const statusMatch = this.currentStatus === 'all' || cardStatus === this.currentStatus;
            const typeMatch = this.currentTableType === 'all' || cardType === this.currentTableType;

            if (areaMatch && statusMatch && typeMatch) {
                card.style.display = 'block';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        console.log(`✅ Visible: ${visibleCount}, Total: ${cards.length}`);

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

    // Tìm kiếm
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
            this.applyFilters();
            return;
        }

        cards.forEach(card => {
            const tableName = card.querySelector('.table-name')?.textContent.toLowerCase() || '';
            const cardArea = card.getAttribute('data-area');
            const cardStatus = card.getAttribute('data-status');
            const cardType = card.getAttribute('data-table-type');

            const searchMatch = tableName.includes(searchValue);
            const areaMatch = this.currentArea === 'all' || cardArea === this.currentArea;
            const statusMatch = this.currentStatus === 'all' || cardStatus === this.currentStatus;
            const typeMatch = this.currentTableType === 'all' || cardType === this.currentTableType;

            if (searchMatch && areaMatch && statusMatch && typeMatch) {
                card.style.display = 'block';
                foundCount++;
            } else {
                card.style.display = 'none';
            }
        });

        console.log(`✅ Found ${foundCount} cards matching "${searchValue}"`);
    },

    // Mở modal đặt bàn
    openReservationModal: async function () {
        try {
            const response = await fetch('/QLBan/PanelDatBan');
            if (!response.ok) throw new Error('Không thể tải form đặt bàn');

            const html = await response.text();
            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = html;
                modalOverlay.classList.add('active');
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Không thể tải form đặt bàn');
        }
    },

    confirmReservationBooking: async function () {
        try {
            const maBan = document.getElementById('reservationTable')?.value;
            const date = document.getElementById('reservationDate')?.value;
            const startTime = document.getElementById('startTime')?.value;
            const endTime = document.getElementById('endTime')?.value;
            const tenKhach = document.getElementById('customerName')?.value;
            const sdt = document.getElementById('customerPhone')?.value;
            const email = document.getElementById('customerEmail')?.value;
            const soNguoi = document.getElementById('numberOfPeople')?.value;
            const ghiChu = document.getElementById('reservationNote')?.value;

            // Validation
            if (!maBan || !date || !startTime || !endTime || !tenKhach || !sdt) {
                if (window.Toast) Toast.error('Vui lòng điền đầy đủ thông tin bắt buộc');
                return;
            }

            // Tạo datetime string
            const gioBatDau = `${date}T${startTime}`;
            const gioKetThuc = `${date}T${endTime}`;

            // Kiểm tra giờ kết thúc phải sau giờ bắt đầu
            const start = new Date(gioBatDau);
            const end = new Date(gioKetThuc);

            // Handle case where end time is past midnight
            if (end <= start) {
                end.setDate(end.getDate() + 1);
            }

            if (end <= start) {
                if (window.Toast) Toast.error('Giờ kết thúc phải sau giờ bắt đầu');
                return;
            }

            const response = await fetch('/QLBan/TaoDatBan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    maBan: parseInt(maBan),
                    thoiGianDat: gioBatDau,
                    gioKetThuc: end.toISOString().slice(0, 19),
                    tenKhach: tenKhach,
                    sdt: sdt,
                    email: email,
                    soNguoi: parseInt(soNguoi) || 1,
                    ghiChu: ghiChu
                })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                this.closeModal();
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi đặt bàn');
        }
    },

    // Đóng modal
    closeModal: function () {
        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            modalOverlay.classList.remove('active');
            setTimeout(() => {
                modalOverlay.innerHTML = '';
            }, 300);
        }
    }
};

// Enhanced styles
(function () {
    if (document.getElementById('table-manager-styles')) {
        return;
    }

    const style = document.createElement('style');
    style.id = 'table-manager-styles';
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

        .table-card.selected {
            border: 3px solid #3b82f6 !important;
            box-shadow: 0 8px 20px rgba(59, 130, 246, 0.4) !important;
            transform: translateY(-5px);
        }

        .loading-state {
            text-align: center;
            padding: 40px 20px;
            color: #6c757d;
        }

        .spinner {
            width: 40px;
            height: 40px;
            margin: 0 auto 20px;
            border: 4px solid #f3f4f6;
            border-top-color: #3b82f6;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        .error-state {
            text-align: center;
            padding: 40px 20px;
            color: #dc3545;
        }

        .error-icon {
            font-size: 48px;
            margin-bottom: 20px;
        }

        .right-panel {
            display: flex !important;
            visibility: visible !important;
        }

        .panel-body {
            display: block !important;
        }

        /* Modal Overlay */
        .modal-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.6);
            z-index: 2000;
            align-items: center;
            justify-content: center;
            padding: 20px;
            animation: fadeIn 0.3s ease;
        }

        .modal-overlay.active {
            display: flex;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
            }
            to {
                opacity: 1;
            }
        }

        /* Modal Content Variants */
        .modal-content {
            background: white;
            border-radius: 16px;
            max-height: 90vh;
            overflow-y: auto;
            box-shadow: 0 10px 40px rgba(0,0,0,0.3);
            animation: slideUp 0.3s ease;
            position: relative;
        }

        @keyframes slideUp {
            from {
                transform: translateY(50px);
                opacity: 0;
            }
            to {
                transform: translateY(0);
                opacity: 1;
            }
        }

        /* Service Modal - Wider */
        .service-modal {
            width: 100%;
            max-width: 700px;
        }

        /* Payment Modal - Medium */
        .payment-modal {
            width: 100%;
            max-width: 600px;
            padding: 0;
        }

        /* Edit Modal - Medium */
        .edit-modal {
            width: 100%;
            max-width: 600px;
            padding: 0;
        }

        /* Close button for modals */
        .btn-close-modal {
            position: absolute;
            top: 15px;
            right: 15px;
            width: 36px;
            height: 36px;
            border-radius: 50%;
            border: none;
            background: #f8f9fa;
            cursor: pointer;
            font-size: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s;
            z-index: 10;
        }

        .btn-close-modal:hover {
            background: #e9ecef;
            transform: rotate(90deg);
        }

        /* Responsive */
        @media (max-width: 768px) {
            .modal-content {
                max-width: 100%;
                max-height: 95vh;
                margin: 10px;
            }

            .service-modal,
            .payment-modal,
            .edit-modal {
                max-width: 100%;
            }
        }
    `;
    document.head.appendChild(style);
})();

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function () {
    console.log('=== TableManager Initialization ===');
    console.log('Detail panel:', document.getElementById('detailPanel'));
    console.log('Right panel:', document.querySelector('.right-panel'));
    console.log('Table cards:', document.querySelectorAll('.table-card').length);

    // Tự động load chi tiết bàn đầu tiên
    const firstCard = document.querySelector('.table-card');
    if (firstCard) {
        const tableId = firstCard.getAttribute('data-table-id');
        if (tableId) {
            console.log('Auto-loading first table:', tableId);
            setTimeout(() => {
                TableManager.showDetail(parseInt(tableId));
            }, 500);
        }
    }

    // Apply initial filters
    TableManager.applyFilters();
});

// Export
window.TableManager = TableManager;

console.log('✅ TableManager script loaded successfully');