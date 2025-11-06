/**
 * Module quản lý thanh toán
 */
const PaymentManager = {
    currentInvoiceId: null,
    currentPaymentMethod: 'TienMat',

    /**
     * Hiển thị panel thanh toán
     */
    show: async function (hoaDonId) {
        try {
            console.log('Loading payment panel for invoice:', hoaDonId);

            this.currentInvoiceId = hoaDonId;

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
                            <button class="btn btn-primary" onclick="PaymentManager.close()">Đóng</button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) {
                Toast.error('Không thể tải panel thanh toán. Vui lòng thử lại.');
            }
        }
    },

    /**
     * Chọn phương thức thanh toán
     */
    selectMethod: function (method, button) {
        console.log('Selected payment method:', method);

        this.currentPaymentMethod = method;

        // Update active state
        document.querySelectorAll('.payment-btn').forEach(btn => {
            btn.classList.remove('active');
        });

        if (button) {
            button.classList.add('active');
        }

        // Update hidden input
        const selectedMethodInput = document.getElementById('selectedPaymentMethod');
        if (selectedMethodInput) {
            selectedMethodInput.value = method;
        }

        // Hide all panels
        const cashPanel = document.getElementById('cashPaymentPanel');
        const qrPanel = document.getElementById('qrPaymentPanel');
        const bankPanel = document.getElementById('bankPaymentPanel');

        if (cashPanel) cashPanel.style.display = 'none';
        if (qrPanel) qrPanel.style.display = 'none';
        if (bankPanel) bankPanel.style.display = 'none';

        // Show relevant panel
        if (method === 'TienMat' && cashPanel) {
            cashPanel.style.display = 'block';
        } else if (method === 'QRCode' && qrPanel) {
            qrPanel.style.display = 'block';
        } else if (bankPanel) {
            bankPanel.style.display = 'block';
        }
    },

    /**
     * Tính tiền thối
     */
    calculateChange: function (tongTien) {
        const tienKhachDuaInput = document.getElementById('tienKhachDua');
        if (!tienKhachDuaInput) return;

        const tienKhachDua = parseFloat(tienKhachDuaInput.value) || 0;
        const tienThoi = tienKhachDua - tongTien;

        const changeDisplay = document.getElementById('changeDisplay');
        const changeAmount = document.getElementById('changeAmount');

        if (!changeDisplay || !changeAmount) return;

        if (tienKhachDua >= tongTien && tienKhachDua > 0) {
            changeDisplay.style.display = 'block';
            changeAmount.textContent = tienThoi.toLocaleString('vi-VN') + ' đ';
            changeAmount.style.color = tienThoi >= 0 ? '#28a745' : '#dc3545';
        } else {
            changeDisplay.style.display = 'none';
        }
    },

    /**
     * Xác nhận thanh toán
     */
    confirm: async function (hoaDonId, method) {
        try {
            // Validate payment method
            if (!method) {
                method = this.currentPaymentMethod;
            }

            // Additional validation for cash payment
            if (method === 'TienMat') {
                const tienKhachDuaInput = document.getElementById('tienKhachDua');
                const tienKhachDua = parseFloat(tienKhachDuaInput?.value) || 0;

                // Get total from DOM
                const tongTienText = document.querySelector('.bill-total')?.textContent || '0';
                const tongTien = parseFloat(tongTienText.replace(/[^\d]/g, ''));

                if (tienKhachDua < tongTien) {
                    if (window.Toast) {
                        Toast.error('Số tiền khách đưa không đủ!');
                    } else {
                        alert('Số tiền khách đưa không đủ!');
                    }
                    return;
                }
            }

            // Get transaction data
            const tienKhachDua = document.getElementById('tienKhachDua')?.value || 0;
            const transactionCode = document.getElementById('transactionCode')?.value || '';

            const response = await fetch('/QLBan/XacNhanThanhToan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    maHD: hoaDonId,
                    phuongThucThanhToan: method,
                    tienKhachDua: parseFloat(tienKhachDua),
                    maGiaoDich: transactionCode
                })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                this.close();
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            if (window.Toast) {
                Toast.error('Có lỗi xảy ra khi thanh toán');
            } else {
                alert('Có lỗi xảy ra khi thanh toán');
            }
        }
    },

    /**
     * Đóng panel
     */
    close: function () {
        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            modalOverlay.classList.remove('active');
            setTimeout(() => {
                modalOverlay.innerHTML = '';
            }, 300);
        }

        this.currentInvoiceId = null;
        this.currentPaymentMethod = 'TienMat';
    }
};

// Export global functions for HTML onclick handlers
window.PaymentManager = PaymentManager;

window.selectPaymentMethod = function (method, button) {
    PaymentManager.selectMethod(method, button);
};

window.calculateChange = function (tongTien) {
    PaymentManager.calculateChange(tongTien);
};

window.confirmPayment = function (hoaDonId, method) {
    PaymentManager.confirm(hoaDonId, method);
};

console.log('✅ PaymentManager loaded');