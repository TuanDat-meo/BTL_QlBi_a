/**
 * Module quản lý chỉnh sửa bàn
 */
const EditTableManager = {
    currentTableId: null,

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
     * Tự động cập nhật thời gian
     */
    startTimerUpdate: function () {
        const updateDuration = () => {
            const gioBatDauInput = document.getElementById('gioBatDau');
            if (!gioBatDauInput || !gioBatDauInput.value) return;

            const gioBatDau = new Date(gioBatDauInput.value);
            const now = new Date();
            const duration = (now - gioBatDau) / 1000 / 60; // phút

            const hours = Math.floor(duration / 60);
            const minutes = Math.floor(duration % 60);

            const currentDurationEl = document.getElementById('currentDuration');
            if (currentDurationEl) {
                currentDurationEl.textContent = `${hours}h ${minutes}m`;
            }

            // Tính tiền bàn ước tính (lấy từ data attribute)
            const editSection = document.querySelector('.edit-section');
            const giaGio = editSection ? parseFloat(editSection.getAttribute('data-gia-gio')) || 0 : 0;

            const soPhutLamTron = Math.ceil(duration / 15) * 15;
            const soGio = soPhutLamTron / 60;
            const tienBan = giaGio * soGio;

            const tienBanUocTinhEl = document.getElementById('tienBanUocTinh');
            if (tienBanUocTinhEl) {
                tienBanUocTinhEl.textContent = Math.round(tienBan).toLocaleString('vi-VN') + ' đ';
            }

            // Tính tổng tiền
            const tienDichVuEl = document.getElementById('tienDichVu');
            const tienDichVu = tienDichVuEl ? parseFloat(tienDichVuEl.value) || 0 : 0;

            const tongTien = Math.ceil((tienBan + tienDichVu) / 1000) * 1000;
            const tongTienUocTinhEl = document.getElementById('tongTienUocTinh');
            if (tongTienUocTinhEl) {
                tongTienUocTinhEl.textContent = tongTien.toLocaleString('vi-VN') + ' đ';
            }
        };

        // Update immediately
        updateDuration();

        // Update every second
        this.timerInterval = setInterval(updateDuration, 1000);
    },

    /**
     * Dừng timer update
     */
    stopTimerUpdate: function () {
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
        }
    },

    /**
     * Tăng số lượng dịch vụ
     */
    increaseQuantity: async function (chiTietId, maBan) {
        const input = document.getElementById(`qty-${chiTietId}`);
        if (!input) return;

        const currentQty = parseInt(input.value);
        if (currentQty >= 99) return;

        try {
            const response = await fetch('/QLBan/CapNhatSoLuongDichVu', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    id: chiTietId,
                    soLuong: currentQty + 1
                })
            });

            const result = await response.json();
            if (result.success) {
                input.value = currentQty + 1;

                const totalEl = document.getElementById(`total-${chiTietId}`);
                if (totalEl) {
                    totalEl.textContent = result.thanhTien.toLocaleString('vi-VN') + ' đ';
                }

                if (window.Toast) Toast.success('Đã cập nhật số lượng');

                // Reload modal to update totals
                setTimeout(() => {
                    this.show(maBan);
                }, 500);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra');
        }
    },

    /**
     * Giảm số lượng dịch vụ
     */
    decreaseQuantity: async function (chiTietId, maBan) {
        const input = document.getElementById(`qty-${chiTietId}`);
        if (!input) return;

        const currentQty = parseInt(input.value);
        if (currentQty <= 1) return;

        try {
            const response = await fetch('/QLBan/CapNhatSoLuongDichVu', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    id: chiTietId,
                    soLuong: currentQty - 1
                })
            });

            const result = await response.json();
            if (result.success) {
                input.value = currentQty - 1;

                const totalEl = document.getElementById(`total-${chiTietId}`);
                if (totalEl) {
                    totalEl.textContent = result.thanhTien.toLocaleString('vi-VN') + ' đ';
                }

                if (window.Toast) Toast.success('Đã cập nhật số lượng');

                // Reload modal to update totals
                setTimeout(() => {
                    this.show(maBan);
                }, 500);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra');
        }
    },

    /**
     * Xóa dịch vụ
     */
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
                // Reload the edit panel
                setTimeout(() => {
                    this.show(maBan);
                }, 300);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
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
                this.close();

                // Reload table detail if available
                if (window.TableManager && window.TableManager.showDetail) {
                    setTimeout(() => {
                        window.TableManager.showDetail(maBan);
                    }, 300);
                }
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi lưu');
        }
    },

    /**
     * Đóng panel
     */
    close: function () {
        this.stopTimerUpdate();

        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            modalOverlay.classList.remove('active');
            setTimeout(() => {
                modalOverlay.innerHTML = '';
            }, 300);
        }

        this.currentTableId = null;
    }
};

// Export global functions for HTML onclick handlers
window.EditTableManager = EditTableManager;

window.increaseServiceQty = function (chiTietId, maBan) {
    EditTableManager.increaseQuantity(chiTietId, maBan);
};

window.decreaseServiceQty = function (chiTietId, maBan) {
    EditTableManager.decreaseQuantity(chiTietId, maBan);
};

console.log('✅ EditTableManager loaded');