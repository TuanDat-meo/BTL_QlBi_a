/**
 * Module quản lý thanh toán
 * Xử lý các thao tác thanh toán hóa đơn
 */
const PaymentManager = {
    currentInvoiceId: null,
    currentPaymentMethod: 'TienMat',
    totalAmount: 0,

    /**
     * Hàm định dạng số: nhập 1000000 -> trả về "1.000.000"
     */
    formatNumber: function (numString) {
        let rawValue = numString.replace(/[^0-9]/g, '');
        if (rawValue === '') return '';

        let number = parseInt(rawValue, 10);
        return number.toLocaleString('vi-VN', { maximumFractionDigits: 0 });
    },

    /**
     * Hàm chuyển chuỗi định dạng về số thuần
     */
    parseFormattedNumber: function (formattedString) {
        if (!formattedString) return 0;
        const rawValue = formattedString.toString().replace(/[^0-9]/g, '');
        return parseFloat(rawValue) || 0;
    },

    /**
     * Khởi tạo PaymentManager
     */
    init: function () {
        console.log('🎯 PaymentManager initialized');

        const invoiceIdInput = document.getElementById('invoiceId');
        const totalAmountEl = document.getElementById('totalAmount');

        if (invoiceIdInput) {
            this.currentInvoiceId = parseInt(invoiceIdInput.value);
        }

        if (totalAmountEl) {
            // Lấy tổng tiền từ data-amount attribute
            this.totalAmount = parseFloat(totalAmountEl.getAttribute('data-amount')) || 0;
        }

        console.log('Invoice ID:', this.currentInvoiceId);
        console.log('Total Amount:', this.totalAmount);

        // Set default payment method
        this.currentPaymentMethod = 'TienMat';

        // Show default panel
        this.showPanel('cashPaymentPanel');

        // Setup auto-fill for cash payment
        this.setupAutoFill();
    },

    /**
     * Auto-fill tiền khách đưa = tổng tiền
     */
    setupAutoFill: function () {
        const tienKhachDuaInput = document.getElementById('tienKhachDua');
        if (tienKhachDuaInput && this.totalAmount > 0) {
            // Auto-fill với giá trị tổng tiền đã format
            tienKhachDuaInput.value = this.formatNumber(this.totalAmount.toString());
            this.calculateChange();
        }
    },

    /**
     * Chọn phương thức thanh toán
     */
    selectPaymentMethod: function (method, buttonElement) {
        console.log('💳 Selected payment method:', method);

        this.currentPaymentMethod = method;

        // Update hidden input
        const methodInput = document.getElementById('selectedPaymentMethod');
        if (methodInput) {
            methodInput.value = method;
        }

        // Update active button
        const allButtons = document.querySelectorAll('.payment-btn');
        allButtons.forEach(btn => btn.classList.remove('active'));

        if (buttonElement) {
            buttonElement.classList.add('active');
        }

        // Hide all payment panels
        this.hideAllPaymentPanels();

        // Show appropriate panel
        switch (method) {
            case 'TienMat':
                this.showPanel('cashPaymentPanel');
                break;
            case 'QRCode':
                this.showPanel('qrPaymentPanel');
                break;
            case 'ChuyenKhoan':
            case 'The':
                this.showPanel('bankPaymentPanel');
                break;
        }
    },

    /**
     * Ẩn tất cả panel thanh toán
     */
    hideAllPaymentPanels: function () {
        const panels = ['cashPaymentPanel', 'qrPaymentPanel', 'bankPaymentPanel'];
        panels.forEach(panelId => {
            const panel = document.getElementById(panelId);
            if (panel) {
                panel.style.display = 'none';
            }
        });
    },

    /**
     * Hiển thị panel
     */
    showPanel: function (panelId) {
        const panel = document.getElementById(panelId);
        if (panel) {
            panel.style.display = 'block';
        }
    },

    /**
     * Định dạng số tiền và tính tiền thối
     */
    formatAndCalculateChange: function (inputElement) {
        const formattedValue = this.formatNumber(inputElement.value);
        inputElement.value = formattedValue;
        this.calculateChange();
    },

    /**
     * Tính tiền thối
     */
    calculateChange: function () {
        const tienKhachDuaInput = document.getElementById('tienKhachDua');
        const changeDisplay = document.getElementById('changeDisplay');
        const changeAmount = document.getElementById('changeAmount');

        if (!tienKhachDuaInput || !changeDisplay || !changeAmount) return;

        // Parse số từ input đã format
        const tienKhachDua = this.parseFormattedNumber(tienKhachDuaInput.value);
        const tienThoi = tienKhachDua - this.totalAmount;

        if (tienKhachDua > 0 && tienThoi >= 0) {
            changeDisplay.style.display = 'flex';
            changeAmount.textContent = this.formatNumber(tienThoi.toString()) + ' đ';
            changeAmount.style.color = '#28a745';
        } else if (tienKhachDua > 0 && tienThoi < 0) {
            changeDisplay.style.display = 'flex';
            changeAmount.textContent = 'Chưa đủ: ' + this.formatNumber(Math.abs(tienThoi).toString()) + ' đ';
            changeAmount.style.color = '#dc3545';
        } else {
            changeDisplay.style.display = 'none';
        }
    },

    /**
     * Validate form thanh toán
     */
    validatePayment: function () {
        const errors = [];

        if (!this.currentPaymentMethod) {
            errors.push('Vui lòng chọn phương thức thanh toán');
        }

        // Validate cash payment
        if (this.currentPaymentMethod === 'TienMat') {
            const tienKhachDuaInput = document.getElementById('tienKhachDua');
            const tienKhachDua = this.parseFormattedNumber(tienKhachDuaInput?.value);

            if (tienKhachDua <= 0) {
                errors.push('Vui lòng nhập số tiền khách đưa');
            } else if (tienKhachDua < this.totalAmount) {
                errors.push('Số tiền khách đưa không đủ');
            }
        }

        // Validate bank transfer
        if (this.currentPaymentMethod === 'ChuyenKhoan' || this.currentPaymentMethod === 'The') {
            const transactionCode = document.getElementById('transactionCode')?.value?.trim();
            if (!transactionCode) {
                errors.push('Vui lòng nhập mã giao dịch');
            }
        }

        // Show errors
        if (errors.length > 0) {
            if (window.Toast) {
                errors.forEach(error => Toast.error(error));
            } else {
                alert(errors.join('\n'));
            }
            return false;
        }

        return true;
    },

    /**
     * Map JavaScript payment method to C# enum value
     */
    mapPaymentMethodToEnum: function (method) {
        const mapping = {
            'TienMat': 'Tiền mặt',
            'ChuyenKhoan': 'Chuyển khoản',
            'QRCode': 'QR Code'
        };
        return mapping[method] || 'Tiền mặt';
    },

    /**
     * Xác nhận thanh toán
     */
    confirmPayment: async function () {
        console.log('💰 Confirming payment...');

        // Validate
        if (!this.validatePayment()) {
            return;
        }

        // Validate invoice ID
        if (!this.currentInvoiceId) {
            if (window.Toast) {
                Toast.error('Không xác định được hóa đơn');
            } else {
                alert('Không xác định được hóa đơn');
            }
            return;
        }

        // Prepare payment data
        const tienKhachDuaInput = document.getElementById('tienKhachDua');
        const tienKhachDua = this.currentPaymentMethod === 'TienMat'
            ? this.parseFormattedNumber(tienKhachDuaInput?.value)
            : this.totalAmount;

        const transactionCode = document.getElementById('transactionCode')?.value?.trim() || null;

        const paymentData = {
            MaHD: this.currentInvoiceId,
            PhuongThucThanhToan: this.mapPaymentMethodToEnum(this.currentPaymentMethod),
            TienKhachDua: tienKhachDua,
            MaGiaoDichQR: this.currentPaymentMethod !== 'TienMat' ? transactionCode : null
        };

        console.log('Payment data:', paymentData);

        try {
            const response = await fetch('/QLBan/XacNhanThanhToan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(paymentData)
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) {
                    Toast.success(result.message || 'Thanh toán thành công!');
                } else {
                    alert(result.message || 'Thanh toán thành công!');
                }

                // Close modal and reload
                if (typeof TableManager !== 'undefined' && typeof TableManager.closeModal === 'function') {
                    TableManager.closeModal();
                }

                setTimeout(() => {
                    location.reload();
                }, 1000);
            } else {
                if (window.Toast) {
                    Toast.error(result.message || 'Thanh toán thất bại!');
                } else {
                    alert(result.message || 'Thanh toán thất bại!');
                }
            }
        } catch (error) {
            console.error('❌ Payment error:', error);
            if (window.Toast) {
                Toast.error('Có lỗi xảy ra: ' + error.message);
            } else {
                alert('Có lỗi xảy ra: ' + error.message);
            }
        }
    },

    /**
     * Show payment panel
     */
    show: async function (maHD) {
        console.log('📄 Loading payment panel for invoice:', maHD);

        if (!maHD) {
            console.error('❌ Invalid invoice ID');
            if (window.Toast) {
                Toast.error('Không xác định được mã hóa đơn');
            }
            return;
        }

        this.currentInvoiceId = maHD;
        await this.loadPaymentPanel(maHD);
    },

    /**
     * Load payment panel content
     */
    loadPaymentPanel: async function (maHD) {
        try {
            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('❌ Modal overlay not found');
                return;
            }

            // Show loading
            modalOverlay.innerHTML = `
                <div class="modal-content payment-modal">
                    <div class="loading-state" style="text-align: center; padding: 40px;">
                        <div class="spinner" style="margin: 0 auto 20px;"></div>
                        <p>Đang tải thông tin thanh toán...</p>
                    </div>
                </div>
            `;
            modalOverlay.classList.add('active');

            const response = await fetch(`/QLBan/PanelThanhToan?maHD=${maHD}`, {
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
                <div class="modal-content payment-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Payment panel loaded successfully');

            // Initialize PaymentManager after DOM is injected
            setTimeout(() => {
                if (window.initPaymentPanel) {
                    window.initPaymentPanel();
                }
            }, 100);

        } catch (error) {
            console.error('❌ Error loading payment panel:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content payment-modal">
                        <div class="error-state" style="text-align: center; padding: 40px;">
                            <div class="error-icon" style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                            <p style="color: #dc3545; font-weight: 600;">Không thể tải panel thanh toán</p>
                            <p style="font-size: 12px; color: #999; margin: 10px 0;">${error.message}</p>
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

    /**
     * Reset payment manager
     */
    reset: function () {
        console.log('🔄 Resetting PaymentManager');

        this.currentInvoiceId = null;
        this.currentPaymentMethod = 'TienMat';
        this.totalAmount = 0;

        const tienKhachDuaInput = document.getElementById('tienKhachDua');
        if (tienKhachDuaInput) {
            tienKhachDuaInput.value = '';
        }

        const transactionCodeInput = document.getElementById('transactionCode');
        if (transactionCodeInput) {
            transactionCodeInput.value = '';
        }

        const changeDisplay = document.getElementById('changeDisplay');
        if (changeDisplay) {
            changeDisplay.style.display = 'none';
        }

        this.selectPaymentMethod('TienMat');
    }
};

/**
 * Khởi tạo PaymentManager khi panel được load
 */
function initPaymentPanel() {
    if (typeof PaymentManager === 'undefined') {
        console.error('PaymentManager not found!');
        return;
    }

    const paymentPanel = document.querySelector('.payment-panel-wrapper');
    if (paymentPanel) {
        console.log('Initializing PaymentManager for payment panel');
        setTimeout(() => {
            PaymentManager.init();
        }, 100);
    }
}

// Listen for modal content changes
if (typeof MutationObserver !== 'undefined') {
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.addedNodes.length) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) {
                        const paymentPanel = node.querySelector ? node.querySelector('.payment-panel-wrapper') : null;
                        if (paymentPanel || (node.classList && node.classList.contains('payment-panel-wrapper'))) {
                            initPaymentPanel();
                        }
                    }
                });
            }
        });
    });

    document.addEventListener('DOMContentLoaded', function () {
        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            observer.observe(modalOverlay, {
                childList: true,
                subtree: true
            });
        }

        initPaymentPanel();
    });
}

// Export to global scope
window.PaymentManager = PaymentManager;
window.initPaymentPanel = initPaymentPanel;

console.log('✅ PaymentManager script loaded successfully');