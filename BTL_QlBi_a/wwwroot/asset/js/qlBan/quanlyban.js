/**
 * Module quản lý bàn billiards
 * Updated: Sử dụng SĐT thay vì mã khách hàng
 */
const TableManager = {
    currentArea: 'all',
    currentStatus: 'all',
    currentTableType: 'all',
    currentTableId: null,

    /**
     * Hiển thị chi tiết bàn vào panel bên phải
     */
    showDetail: async function (maBan) {
        try {
            console.log('📋 Loading detail for table:', maBan);

            const detailPanel = document.getElementById('detailPanel');
            const container = document.querySelector('.container');

            if (!detailPanel) {
                console.error('Detail panel not found!');
                return;
            }

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

    /**
     * Hiển thị menu dịch vụ trong modal
     */
    addService: async function (maBan) {
        try {
            console.log('🍽️ Opening service menu for table:', maBan);
            this.currentTableId = maBan;

            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found!');
                return;
            }

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

            modalOverlay.innerHTML = `
                <div class="modal-content service-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Menu loaded successfully');

            setTimeout(() => {
                if (window.initMenuDichVu) {
                    window.initMenuDichVu();
                }
            }, 100);

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

    /**
     * Bắt đầu chơi - Nhập số điện thoại
     */
    start: async function (maBan) {
        // Tạo HTML modal nhập số điện thoại
        const modalHTML = `
            <div class="modal-content customer-phone-modal" style="max-width: 480px;">
                <div class="modal-header-custom">
                    <h3 style="margin: 0; color: #2c3e50; font-size: 20px;">
                        <span style="font-size: 24px;">📞</span> Thông tin khách hàng
                    </h3>
                    <button class="btn-close-modal" onclick="TableManager.closeModal()">×</button>
                </div>
                
                <div class="modal-body-custom" style="padding: 24px;">
                    <div class="form-group" style="margin-bottom: 16px;">
                        <label style="display: block; margin-bottom: 8px; font-weight: 600; color: #34495e;">
                            Số điện thoại khách hàng:
                        </label>
                        <input 
                            type="text" 
                            id="customerPhoneInput" 
                            class="form-control" 
                            placeholder="Nhập số điện thoại (10-11 số)" 
                            maxlength="11"
                            style="width: 100%; padding: 12px; border: 2px solid #ddd; border-radius: 8px; font-size: 16px;"
                            onkeypress="return event.charCode >= 48 && event.charCode <= 57"
                        >
                        <small style="color: #7f8c8d; display: block; margin-top: 6px;">
                            💡 Để trống nếu là khách vãng lai
                        </small>
                    </div>

                    <div id="customerInfoDisplay" style="display: none; background: #e8f5e9; padding: 14px; border-radius: 8px; margin-top: 16px; border-left: 4px solid #4caf50;">
                        <div style="font-size: 14px; color: #2e7d32;">
                            <div style="font-weight: 600; margin-bottom: 6px;">✅ Khách hàng đã tồn tại:</div>
                            <div id="customerName" style="margin-bottom: 4px;"></div>
                            <div id="customerMembership"></div>
                        </div>
                    </div>

                    <div id="guestInfoDisplay" style="display: none; background: #fff3cd; padding: 14px; border-radius: 8px; margin-top: 16px; border-left: 4px solid #ffc107;">
                        <div style="font-size: 14px; color: #856404;">
                            <div style="font-weight: 600; margin-bottom: 6px;">⚠️ Khách vãng lai</div>
                            <div>Số điện thoại này chưa đăng ký trong hệ thống.</div>
                        </div>
                    </div>
                </div>

                <div class="modal-footer-custom" style="padding: 16px 24px; background: #f8f9fa; border-top: 1px solid #dee2e6; display: flex; gap: 12px;">
                    <button 
                        class="btn btn-secondary" 
                        onclick="TableManager.closeModal()"
                        style="flex: 1; padding: 12px; font-weight: 600;"
                    >
                        Hủy
                    </button>
                    <button 
                        class="btn btn-success" 
                        onclick="TableManager.confirmStart(${maBan})"
                        style="flex: 1; padding: 12px; font-weight: 600;"
                    >
                        ▶️ Bắt đầu
                    </button>
                </div>
            </div>
        `;

        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            modalOverlay.innerHTML = modalHTML;
            modalOverlay.classList.add('active');

            // Focus vào input
            setTimeout(() => {
                const phoneInput = document.getElementById('customerPhoneInput');
                if (phoneInput) {
                    phoneInput.focus();

                    // Thêm event listener để check số điện thoại
                    phoneInput.addEventListener('input', function () {
                        TableManager.checkCustomerPhone(this.value);
                    });

                    // Cho phép Enter để submit
                    phoneInput.addEventListener('keypress', function (e) {
                        if (e.key === 'Enter') {
                            TableManager.confirmStart(maBan);
                        }
                    });
                }
            }, 100);
        }
    },

    /**
     * Kiểm tra số điện thoại khách hàng
     */
    checkCustomerPhone: async function (phoneNumber) {
        const infoDisplay = document.getElementById('customerInfoDisplay');
        const guestDisplay = document.getElementById('guestInfoDisplay');

        // Ẩn tất cả thông báo
        if (infoDisplay) infoDisplay.style.display = 'none';
        if (guestDisplay) guestDisplay.style.display = 'none';

        // Validate số điện thoại (10-11 số)
        if (phoneNumber.length < 10) {
            return;
        }

        try {
            const response = await fetch(`/QLBan/KiemTraKhachHang?sdt=${phoneNumber}`);
            const result = await response.json();

            if (result.success && result.khachHang) {
                // Hiển thị thông tin khách hàng
                if (infoDisplay) {
                    infoDisplay.style.display = 'block';

                    const nameEl = document.getElementById('customerName');
                    const membershipEl = document.getElementById('customerMembership');

                    if (nameEl) {
                        nameEl.textContent = `👤 Tên: ${result.khachHang.tenKH}`;
                    }

                    if (membershipEl) {
                        const membershipBadge = this.getMembershipBadge(result.khachHang.hangTV);
                        membershipEl.innerHTML = `⭐ Hạng: ${membershipBadge}`;
                    }
                }
            } else if (phoneNumber.length >= 10) {
                // Hiển thị thông báo khách vãng lai
                if (guestDisplay) {
                    guestDisplay.style.display = 'block';
                }
            }
        } catch (error) {
            console.error('Error checking customer:', error);
        }
    },

    /**
     * Lấy badge hạng thành viên
     */
    getMembershipBadge: function (hangTV) {
        const badges = {
            'Dong': '<span style="color: #cd7f32; font-weight: 700;">🥉 Đồng</span>',
            'Bac': '<span style="color: #c0c0c0; font-weight: 700;">🥈 Bạc</span>',
            'Vang': '<span style="color: #ffd700; font-weight: 700;">🥇 Vàng</span>',
            'KimCuong': '<span style="color: #00d4ff; font-weight: 700;">💎 Kim Cương</span>'
        };
        return badges[hangTV] || hangTV;
    },

    /**
     * Xác nhận bắt đầu chơi
     */
    confirmStart: async function (maBan) {
        const phoneInput = document.getElementById('customerPhoneInput');
        const sdt = phoneInput ? phoneInput.value.trim() : '';

        // Validate số điện thoại nếu có nhập
        if (sdt && (sdt.length < 10 || sdt.length > 11 || !/^[0-9]+$/.test(sdt))) {
            if (window.Toast) {
                Toast.error('Số điện thoại không hợp lệ (10-11 số)');
            } else {
                alert('Số điện thoại không hợp lệ (10-11 số)');
            }
            return;
        }

        // Xác nhận nếu là khách vãng lai
        if (sdt) {
            const guestDisplay = document.getElementById('guestInfoDisplay');
            if (guestDisplay && guestDisplay.style.display !== 'none') {
                if (!confirm('⚠️ Số điện thoại này chưa đăng ký.\n\nBạn có muốn tiếp tục với khách vãng lai?')) {
                    return;
                }
            }
        }

        try {
            const response = await fetch('/QLBan/BatDauChoi', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    maBan: maBan,
                    sdt: sdt || null
                })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) {
                    Toast.success(result.message || 'Bắt đầu chơi thành công!');
                } else {
                    alert(result.message || 'Bắt đầu chơi thành công!');
                }

                this.closeModal();
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) {
                    Toast.error(result.message || 'Có lỗi xảy ra');
                } else {
                    alert(result.message || 'Có lỗi xảy ra');
                }
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) {
                Toast.error('Có lỗi xảy ra khi bắt đầu chơi');
            } else {
                alert('Có lỗi xảy ra khi bắt đầu chơi');
            }
        }
    },

    /**
     * Kết thúc chơi - Gọi PaymentManager.show()
     */
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
                        if (window.PaymentManager) {
                            window.PaymentManager.show(result.hoaDonId);
                        } else {
                            console.error('PaymentManager not loaded!');
                            alert('Không thể tải module thanh toán. Vui lòng tải lại trang.');
                        }
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

    /**
     * Xác nhận đặt bàn
     */
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

    /**
     * Hủy đặt bàn
     */
    cancelReservation: async function (maBan) {
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

    /**
     * Các phương thức khác giữ nguyên...
     */
    editTable: async function (maBan) {
        try {
            console.log('✏️ Opening edit panel for table:', maBan);

            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found');
                return;
            }

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

            modalOverlay.innerHTML = `
                <div class="modal-content edit-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Edit panel loaded');

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

    /**
     * Đóng modal
     */
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

        /* Customer Phone Modal */
        .customer-phone-modal {
            width: 100%;
            max-width: 480px;
            padding: 0;
        }

        .modal-header-custom {
            padding: 20px 24px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border-radius: 16px 16px 0 0;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .modal-body-custom {
            padding: 24px;
        }

        .modal-footer-custom {
            padding: 16px 24px;
            background: #f8f9fa;
            border-top: 1px solid #dee2e6;
            display: flex;
            gap: 12px;
            border-radius: 0 0 16px 16px;
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
            width: 36px;
            height: 36px;
            border-radius: 50%;
            border: none;
            background: rgba(255, 255, 255, 0.2);
            cursor: pointer;
            font-size: 28px;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s;
            color: white;
            font-weight: 300;
        }

        .btn-close-modal:hover {
            background: rgba(255, 255, 255, 0.3);
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
            .edit-modal,
            .customer-phone-modal {
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

    // Auto-load first table
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

// Export to window
window.TableManager = TableManager;
console.log('✅ TableManager script loaded successfully');