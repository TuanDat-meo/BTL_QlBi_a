/**
 * Module quản lý chỉnh sửa bàn - FIXED VERSION
 * Sửa lỗi: Cập nhật số lượng và xóa dịch vụ hoạt động chính xác
 */
const EditTableManager = {
    currentTableId: null,
    timerInterval: null,
    giaGio: 0,

    /**
     * Hiển thị panel chỉnh sửa
     */
    show: async function (maBan) {
        try {
            this.currentTableId = maBan;

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

            // Lấy giá giờ từ data attribute
            const editSection = document.querySelector('.edit-section');
            if (editSection) {
                this.giaGio = parseFloat(editSection.getAttribute('data-gia-gio')) || 0;
                console.log('💰 Giá giờ:', this.giaGio);
            }

            // Initialize timer update
            this.startTimerUpdate();

            console.log('✅ Edit panel loaded successfully');
        } catch (error) {
            console.error('❌ Error loading edit panel:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content edit-modal">
                        <div class="error-state" style="text-align: center; padding: 40px;">
                            <div class="error-icon" style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                            <p style="color: #dc3545; font-weight: 600;">Không thể tải panel chỉnh sửa</p>
                            <button class="btn btn-primary" onclick="EditTableManager.close()">Đóng</button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) Toast.error('Không thể tải panel chỉnh sửa');
        }
    },

    /**
     * Tự động cập nhật thời gian và tiền
     */
    startTimerUpdate: function () {
        // Dừng timer cũ nếu có
        this.stopTimerUpdate();

        const updateDuration = () => {
            const gioBatDauInput = document.getElementById('gioBatDau');
            if (!gioBatDauInput || !gioBatDauInput.value) return;

            const gioBatDau = new Date(gioBatDauInput.value);
            const now = new Date();
            const durationMinutes = (now - gioBatDau) / 1000 / 60; // phút

            // Hiển thị thời gian chơi
            const hours = Math.floor(durationMinutes / 60);
            const minutes = Math.floor(durationMinutes % 60);

            const currentDurationEl = document.getElementById('currentDuration');
            if (currentDurationEl) {
                currentDurationEl.textContent = `${hours} giờ ${minutes} phút`;
            }

            // Tính tiền bàn (thời gian thực, không làm tròn)
            const soGioThucTe = durationMinutes / 60;
            const tienBanThucTe = this.giaGio * soGioThucTe;

            const tienBanUocTinhEl = document.getElementById('tienBanUocTinh');
            if (tienBanUocTinhEl) {
                tienBanUocTinhEl.textContent = Math.round(tienBanThucTe).toLocaleString('vi-VN') + ' đ';
            }

            // Tính tổng tiền
            this.updateTotalAmount();
        };

        // Update immediately
        updateDuration();

        // Update every second
        this.timerInterval = setInterval(updateDuration, 1000);
        console.log('✅ Timer started');
    },

    updateTotalAmount: function () {
        const tienBanEl = document.getElementById('tienBanUocTinh');
        const tongTienEl = document.getElementById('tongTienUocTinh');

        // Lấy tiền dịch vụ và giảm giá bằng ID (đã được reloadServiceList cập nhật HTML)
        const tienDichVuEl = document.getElementById('tienDichVuHienThi');
        const giamGiaEl = document.getElementById('giamGiaHienThi');

        if (!tienBanEl || !tongTienEl || !tienDichVuEl || !giamGiaEl) {
            console.warn('⚠️ Missing required elements for total calculation. Aborting updateTotalAmount.');
            return;
        }

        // Parse tiền bàn (lấy từ giá trị hiển thị hiện tại của timer)
        const tienBanText = tienBanEl.textContent.replace(/[^0-9]/g, '');
        const tienBan = parseFloat(tienBanText) || 0;

        // Parse tiền dịch vụ từ ID mới
        // Giá trị của tienDichVuEl đã được cập nhật từ newBillSummary trong reloadServiceList
        const tienDichVuText = tienDichVuEl.textContent.replace(/[^0-9]/g, '');
        const tienDichVu = parseFloat(tienDichVuText) || 0;

        // Parse giảm giá từ ID mới (Bỏ qua dấu trừ)
        // Giá trị của giamGiaEl đã được cập nhật từ newBillSummary trong reloadServiceList
        const giamGiaText = giamGiaEl.textContent.replace(/[^0-9]/g, '');
        const giamGia = parseFloat(giamGiaText) || 0;

        // Tính tổng và làm tròn lên nghìn
        const tongTienTruocLamTron = tienBan + tienDichVu - giamGia;
        const tongTien = Math.ceil(tongTienTruocLamTron / 1000) * 1000;

        console.log(`Calc: Bàn=${tienBan.toLocaleString()}, DV=${tienDichVu.toLocaleString()}, GG=${giamGia.toLocaleString()}, Tổng=${tongTien.toLocaleString()}`);

        tongTienEl.textContent = tongTien.toLocaleString('vi-VN') + ' đ';
    },

    // ...
    /**
     * Reload danh sách dịch vụ (không reload toàn bộ panel)
     */
    // THAY ĐỔI: Thêm tham số maBan
    reloadServiceList: async function (maBan) {
        try {
            console.log('🔄 Reloading service list only for table:', maBan);

            // THAY ĐỔI: Sử dụng maBan truyền vào hoặc this.currentTableId
            const tableId = maBan || this.currentTableId;

            if (!tableId) {
                throw new Error('Mã bàn không xác định (tableId is null)');
            }

            // GỌI API VỚI tableId đã xác định
            const response = await fetch(`/QLBan/PanelChinhSuaBan?maBan=${tableId}`);

            if (!response.ok) {
                // Ghi log chi tiết lỗi 404
                console.error('Lỗi tải panel:', response.status, response.statusText);
                throw new Error('Không thể tải dịch vụ. Lỗi HTTP: ' + response.status);
            }

            const html = await response.text();

            // ... (Phần parse HTML và cập nhật DOM giữ nguyên) ...
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;

            const newServiceList = tempDiv.querySelector('.service-list-edit');
            const newBillSummary = tempDiv.querySelector('.bill-summary');

            const currentServiceList = document.querySelector('.service-list-edit');
            const currentBillSummary = document.querySelector('.bill-summary');

            if (newServiceList && currentServiceList) {
                currentServiceList.innerHTML = newServiceList.innerHTML;
                console.log('✅ Service list updated');
            }

            if (newBillSummary && currentBillSummary) {
                currentBillSummary.innerHTML = newBillSummary.innerHTML;
                console.log('✅ Bill summary updated');
            }

            // Cập nhật lại tổng tiền (sẽ đọc giá trị mới từ bill summary)
            this.updateTotalAmount();

        } catch (error) {
            console.error('❌ Error reloading service list:', error);
            if (window.Toast) Toast.error('Không thể cập nhật danh sách dịch vụ');
        }
    },
    /**
     * Dừng timer update
     */
    stopTimerUpdate: function () {
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
            console.log('⏹️ Timer stopped');
        }
    },

    /**
     * Tăng số lượng dịch vụ
     */
    increaseQuantity: async function (chiTietId, maBan) {
        console.log('➕ Increase quantity called:', chiTietId, maBan);

        const input = document.getElementById(`qty-${chiTietId}`);
        if (!input) {
            console.error('❌ Input not found for chiTietId:', chiTietId);
            return;
        }

        let currentValue = parseInt(input.value) || 1;
        const maxValue = parseInt(input.max) || 99;

        if (currentValue >= maxValue) {
            if (window.Toast) Toast.warning('Đã đạt số lượng tối đa');
            return;
        }

        const newValue = currentValue + 1;

        try {
            if (window.Loading) window.Loading.show();

            const response = await fetch('/QLBan/CapNhatSoLuongDichVu', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    id: chiTietId,
                    soLuong: newValue
                })
            });

            const result = await response.json();

            if (window.Loading) window.Loading.hide();

            if (result.success) {
                console.log(`✅ Increased quantity for detail ${chiTietId}: ${newValue}`);

                // Cập nhật input
                input.value = newValue;

                // Cập nhật thành tiền của item này
                const totalEl = document.getElementById(`total-${chiTietId}`);
                if (totalEl && result.thanhTien) {
                    totalEl.textContent = result.thanhTien.toLocaleString('vi-VN') + ' đ';
                }
                
                await this.reloadServiceList(maBan);

                if (window.Toast) Toast.success('Đã tăng số lượng');
            } else {
                console.error('❌ Failed to increase:', result.message);
                if (window.Toast) Toast.error(result.message || 'Không thể cập nhật số lượng');
            }
        } catch (error) {
            if (window.Loading) window.Loading.hide();
            console.error('❌ Error updating quantity:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi cập nhật');
        }
    },

    /**
     * Giảm số lượng dịch vụ
     */
    decreaseQuantity: async function (chiTietId, maBan) {
        console.log('➖ Decrease quantity called:', chiTietId, maBan);

        const input = document.getElementById(`qty-${chiTietId}`);
        if (!input) {
            console.error('❌ Input not found for chiTietId:', chiTietId);
            return;
        }

        let currentValue = parseInt(input.value) || 1;
        const minValue = parseInt(input.min) || 1;

        if (currentValue <= minValue) {
            if (window.Toast) Toast.warning('Số lượng tối thiểu là 1');
            return;
        }

        const newValue = currentValue - 1;

        try {
            if (window.Loading) window.Loading.show();

            const response = await fetch('/QLBan/CapNhatSoLuongDichVu', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    id: chiTietId,
                    soLuong: newValue
                })
            });

            const result = await response.json();

            if (window.Loading) window.Loading.hide();

            if (result.success) {
                console.log(`✅ Decreased quantity for detail ${chiTietId}: ${newValue}`);

                // Cập nhật input
                input.value = newValue;

                // Cập nhật thành tiền của item này
                const totalEl = document.getElementById(`total-${chiTietId}`);
                if (totalEl && result.thanhTien) {
                    totalEl.textContent = result.thanhTien.toLocaleString('vi-VN') + ' đ';
                }

                // Reload danh sách dịch vụ để cập nhật tổng tiền
                await this.reloadServiceList(maBan);

                if (window.Toast) Toast.success('Đã giảm số lượng');
            } else {
                console.error('❌ Failed to decrease:', result.message);
                if (window.Toast) Toast.error(result.message || 'Không thể cập nhật số lượng');
            }
        } catch (error) {
            if (window.Loading) window.Loading.hide();
            console.error('❌ Error updating quantity:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi cập nhật');
        }
    },

    /**
     * Xóa dịch vụ
     */
    removeService: async function (chiTietId, maBan) {
        console.log('🗑️ Remove service called:', chiTietId, maBan);

        if (!confirm('Bạn có chắc muốn xóa dịch vụ này?')) {
            return;
        }

        try {
            if (window.Loading) window.Loading.show();

            const response = await fetch('/QLBan/XoaDichVu', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id: chiTietId })
            });

            const result = await response.json();

            if (window.Loading) window.Loading.hide();

            if (result.success) {
                console.log('✅ Service removed successfully');
                if (window.Toast) Toast.success(result.message || 'Xóa dịch vụ thành công');

                // Reload danh sách dịch vụ
                await this.reloadServiceList(maBan);
            } else {
                console.error('❌ Failed to remove:', result.message);
                if (window.Toast) Toast.error(result.message || 'Không thể xóa dịch vụ');
            }
        } catch (error) {
            if (window.Loading) window.Loading.hide();
            console.error('❌ Error removing service:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi xóa');
        }
    },

    /**
     * Lưu thay đổi giờ bắt đầu
     */
    save: async function (maBan) {
        try {
            const gioBatDau = document.getElementById('gioBatDau')?.value;

            if (!gioBatDau) {
                if (window.Toast) Toast.error('Vui lòng chọn giờ bắt đầu');
                return;
            }

            console.log('💾 Saving start time:', gioBatDau);

            if (window.Loading) window.Loading.show();

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

            if (window.Loading) window.Loading.hide();

            if (result.success) {
                console.log('✅ Start time saved successfully');
                if (window.Toast) Toast.success(result.message || 'Lưu thành công');

                // Chỉ cập nhật lại timer, không reload panel
                this.startTimerUpdate();
            } else {
                console.error('❌ Failed to save:', result.message);
                if (window.Toast) Toast.error(result.message || 'Không thể lưu');
            }
        } catch (error) {
            if (window.Loading) window.Loading.hide();
            console.error('❌ Error saving:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi lưu');
        }
    },

    /**
     * Đóng panel
     */
    close: function () {
        console.log('🔒 Closing edit panel');
        this.stopTimerUpdate();

        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            modalOverlay.classList.remove('active');
            setTimeout(() => {
                modalOverlay.innerHTML = '';
            }, 300);
        }

        this.currentTableId = null;
        this.giaGio = 0;
    }
};

// Export global functions for HTML onclick handlers
window.EditTableManager = EditTableManager;

// Compatibility functions (nếu view sử dụng tên khác)
window.increaseServiceQty = function (chiTietId, maBan) {
    console.log('🔗 increaseServiceQty called (legacy)');
    EditTableManager.increaseQuantity(chiTietId, maBan);
};

window.decreaseServiceQty = function (chiTietId, maBan) {
    console.log('🔗 decreaseServiceQty called (legacy)');
    EditTableManager.decreaseQuantity(chiTietId, maBan);
};

console.log('✅ EditTableManager loaded and ready (FIXED VERSION - v2)');