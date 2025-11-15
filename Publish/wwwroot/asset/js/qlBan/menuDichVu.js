const MenuDichVu = {
    currentCategory: 'all',
    currentTableId: null,

    /**
     * Khởi tạo MenuDichVu
     */
    init: function () {
        console.log('🎯 MenuDichVu initialized');

        // Lấy mã bàn từ nhiều nguồn
        this.currentTableId = this.getTableId();
        console.log('📌 Current table ID:', this.currentTableId);

        // Reset category về "Tất cả"
        this.currentCategory = 'all';

        // Apply initial filter
        this.applyFilters();
    },

    /**
     * Lấy mã bàn từ nhiều nguồn
     */
    getTableId: function () {
        // 1. Từ TableManager
        if (typeof TableManager !== 'undefined' && TableManager.currentTableId) {
            return TableManager.currentTableId;
        }

        // 2. Từ data attribute của modal
        const modal = document.querySelector('[data-table-id]');
        if (modal) {
            return parseInt(modal.getAttribute('data-table-id'));
        }

        // 3. Từ global variable
        if (typeof window.currentTableId !== 'undefined') {
            return window.currentTableId;
        }

        console.warn('⚠️ Cannot determine table ID');
        return null;
    },

    /**
     * Lọc theo danh mục
     */
    filterByCategory: function (category, event) {
        console.log('🔖 Filtering by category:', category);

        this.currentCategory = category;

        // Update active button
        const buttons = document.querySelectorAll('.menu-category-btn');
        buttons.forEach(btn => {
            btn.classList.remove('active');
        });

        if (event && event.target) {
            event.target.classList.add('active');
        }

        // Apply filters
        this.applyFilters();
    },

    /**
     * Tìm kiếm dịch vụ
     */
    searchServices: function () {
        const searchInput = document.getElementById('menuSearch');
        if (!searchInput) return;

        const searchTerm = searchInput.value.toLowerCase().trim();
        console.log('🔍 Searching for:', searchTerm);

        this.applyFilters(searchTerm);
    },

    /**
     * Áp dụng các bộ lọc (danh mục + tìm kiếm)
     */
    applyFilters: function (searchTerm = '') {
        const menuItems = document.querySelectorAll('.menu-item');
        let visibleCount = 0;

        menuItems.forEach(item => {
            const category = item.getAttribute('data-category') || '';
            const serviceName = item.querySelector('.menu-item-name')?.textContent.toLowerCase() || '';
            const serviceDesc = item.querySelector('.menu-item-desc')?.textContent.toLowerCase() || '';

            // Check category filter
            const categoryMatch = this.currentCategory === 'all' || category === this.currentCategory;

            // Check search filter
            const searchMatch = searchTerm === '' ||
                serviceName.includes(searchTerm) ||
                serviceDesc.includes(searchTerm);

            if (categoryMatch && searchMatch) {
                item.style.display = 'block';
                visibleCount++;
            } else {
                item.style.display = 'none';
            }
        });

        console.log(`✅ ${visibleCount} services visible`);

        // Show empty message if no services
        this.toggleEmptyMessage(visibleCount === 0);
    },

    /**
     * Hiển thị/ẩn thông báo không có dịch vụ
     */
    toggleEmptyMessage: function (show) {
        const menuBody = document.querySelector('.menu-body');
        if (!menuBody) return;

        let emptyMsg = menuBody.querySelector('.menu-no-results');

        if (show && !emptyMsg) {
            emptyMsg = document.createElement('div');
            emptyMsg.className = 'menu-no-results';
            emptyMsg.innerHTML = `
                <div class="menu-empty-icon">🔍</div>
                <p>Không tìm thấy dịch vụ phù hợp</p>
            `;
            menuBody.appendChild(emptyMsg);
        } else if (!show && emptyMsg) {
            emptyMsg.remove();
        }
    },

    /**
     * Tăng số lượng
     */
    increaseQuantity: function (serviceId) {
        const input = document.getElementById(`qty-${serviceId}`);
        if (!input) return;

        let currentValue = parseInt(input.value) || 1;
        const maxValue = parseInt(input.max) || 99;

        if (currentValue < maxValue) {
            input.value = currentValue + 1;
            console.log(`➕ Increased quantity for service ${serviceId}: ${input.value}`);
        }
    },

    /**
     * Giảm số lượng
     */
    decreaseQuantity: function (serviceId) {
        const input = document.getElementById(`qty-${serviceId}`);
        if (!input) return;

        let currentValue = parseInt(input.value) || 1;
        const minValue = parseInt(input.min) || 1;

        if (currentValue > minValue) {
            input.value = currentValue - 1;
            console.log(`➖ Decreased quantity for service ${serviceId}: ${input.value}`);
        }
    },

    /**
     * Thêm dịch vụ - GỌI TRỰC TIẾP API
     */
    addService: async function (serviceId) {
        console.log('📝 Adding service:', serviceId);

        // Validate table ID
        this.currentTableId = this.getTableId();

        if (!this.currentTableId) {
            if (window.Toast) {
                Toast.error('Không xác định được bàn');
            } else {
                alert('Không xác định được bàn');
            }
            return;
        }

        // Get quantity
        const input = document.getElementById(`qty-${serviceId}`);
        const quantity = input ? parseInt(input.value) || 1 : 1;

        console.log(`🎯 Adding ${quantity}x service ${serviceId} to table ${this.currentTableId}`);

        try {
            // GỌI TRỰC TIẾP API
            const response = await fetch('/QLBan/ThemDichVu', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    maBan: this.currentTableId,
                    maDV: serviceId,
                    soLuong: quantity
                })
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) {
                    Toast.success(result.message || 'Thêm dịch vụ thành công');
                } else {
                    alert(result.message || 'Thêm dịch vụ thành công');
                }

                // Reset quantity to 1
                if (input) {
                    input.value = 1;
                }

                // Đóng modal sau 500ms
                setTimeout(() => {
                    if (typeof TableManager !== 'undefined' && typeof TableManager.closeModal === 'function') {
                        TableManager.closeModal();
                    }
                }, 500);

                // Reload chi tiết bàn để hiển thị dịch vụ vừa thêm
                setTimeout(async () => {
                    if (typeof TableManager !== 'undefined' && typeof TableManager.loadTableDetail === 'function') {
                        await TableManager.loadTableDetail(this.currentTableId);
                    } else {
                        // Fallback: reload trực tiếp
                        this.reloadTableDetail(this.currentTableId);
                    }
                }, 600);

            } else {
                if (window.Toast) {
                    Toast.error(result.message || 'Không thể thêm dịch vụ');
                } else {
                    alert(result.message || 'Không thể thêm dịch vụ');
                }
            }

        } catch (error) {
            console.error('❌ Error adding service:', error);
            if (window.Toast) {
                Toast.error('Lỗi kết nối server');
            } else {
                alert('Lỗi kết nối server');
            }
        }
    },

    /**
     * Reload chi tiết bàn (fallback method)
     */
    reloadTableDetail: async function (tableId) {
        try {
            const response = await fetch(`/QLBan/ChiTietBan?maBan=${tableId}`);
            const html = await response.text();

            const detailPanel = document.getElementById('detailPanel');
            if (detailPanel) {
                detailPanel.innerHTML = html;
                console.log('✅ Reloaded table detail');
            }
        } catch (error) {
            console.error('❌ Error reloading table detail:', error);
        }
    },

    /**
     * Reset form
     */
    reset: function () {
        console.log('🔄 Resetting menu');

        this.currentCategory = 'all';

        // Reset search input
        const searchInput = document.getElementById('menuSearch');
        if (searchInput) {
            searchInput.value = '';
        }

        // Reset category buttons
        const buttons = document.querySelectorAll('.menu-category-btn');
        buttons.forEach(btn => {
            btn.classList.remove('active');
            if (btn.getAttribute('data-category') === 'all') {
                btn.classList.add('active');
            }
        });

        // Reset all quantity inputs
        const quantityInputs = document.querySelectorAll('.quantity-input');
        quantityInputs.forEach(input => {
            input.value = 1;
        });

        // Apply filters
        this.applyFilters();
    }
};

/**
 * Khởi tạo MenuDichVu khi modal được load
 */
function initMenuDichVu() {
    if (typeof MenuDichVu === 'undefined') {
        console.error('MenuDichVu not found!');
        return;
    }

    const menuModal = document.querySelector('.menu-header');
    if (menuModal) {
        console.log('Initializing MenuDichVu for modal');
        setTimeout(() => {
            MenuDichVu.init();
        }, 100);
    }
}

// Listen for modal content changes
if (typeof MutationObserver !== 'undefined') {
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.addedNodes.length) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) { // Element node
                        const menu = node.querySelector ? node.querySelector('.menu-header') : null;
                        if (menu || (node.classList && node.classList.contains('menu-header'))) {
                            initMenuDichVu();
                        }
                    }
                });
            }
        });
    });

    // Observe modal overlay
    document.addEventListener('DOMContentLoaded', function () {
        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            observer.observe(modalOverlay, {
                childList: true,
                subtree: true
            });
        }

        // Check if modal is already loaded
        initMenuDichVu();
    });
}

// Export to global scope
window.MenuDichVu = MenuDichVu;
window.initMenuDichVu = initMenuDichVu;