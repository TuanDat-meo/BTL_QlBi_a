const ReservationManager = {
    selectedTableData: null,
    currentAreaFilter: 'all',

    // Đặt hằng số cho giờ giới hạn
    START_TIME_MIN: 7, // 7h sáng
    END_TIME_MAX: 2,   // 2h sáng hôm sau (26:00)
    LATE_HOUR_LIMIT: 1, // 1h sáng (không cho phép đặt BẮT ĐẦU từ 1h)

    /**
     * Khởi tạo form đặt bàn
     */
    init: function () {
        console.log('🎯 ReservationManager initialized');

        const now = new Date();
        const dateInput = document.getElementById('reservationDate');
        const startTimeInput = document.getElementById('startTime');
        const endTimeInput = document.getElementById('endTime');

        // --- 1. Set minimum date to today
        const today = now.toISOString().split('T')[0];
        if (dateInput) {
            dateInput.min = today;
            dateInput.value = today;
        }

        // --- 2. Calculate initial start/end time based on current time and rules
        let defaultStart = new Date(now.getTime());
        let defaultEnd = new Date(now.getTime());

        // Lấy giờ hiện tại (0-23)
        const currentHour = now.getHours();
        const currentMinute = now.getMinutes();

        // 💡 Logic: Giờ bắt đầu mặc định là giờ tiếp theo, làm tròn lên giờ tròn hoặc nửa giờ (00 hoặc 30)

        // Nếu hiện tại là 1h (LATE_HOUR_LIMIT), không cho đặt (buộc chuyển sang ngày mai)
        if (currentHour === this.LATE_HOUR_LIMIT) {
            if (window.Toast) {
                Toast.warning('Hiện tại đã là 1h sáng. Vui lòng đặt bàn cho ngày tiếp theo!');
            }
            // Tự động chuyển ngày đặt sang ngày mai
            const tomorrow = new Date(now);
            tomorrow.setDate(tomorrow.getDate() + 1);
            const tomorrowDate = tomorrow.toISOString().split('T')[0];
            if (dateInput) dateInput.value = tomorrowDate;

            // Giờ mặc định là 7h sáng ngày mai
            defaultStart.setDate(defaultStart.getDate() + 1);
            defaultStart.setHours(this.START_TIME_MIN, 0, 0, 0);
        }
        // Nếu đang trong khoảng 2h sáng đến 7h sáng, mặc định bắt đầu là 7h sáng hôm nay
        else if (currentHour >= this.END_TIME_MAX && currentHour < this.START_TIME_MIN) {
            defaultStart.setHours(this.START_TIME_MIN, 0, 0, 0);
        }
        // Các giờ còn lại (không phải quá khứ, không phải 1h, không phải khung nghỉ)
        else {
            // Tính giờ tiếp theo:
            let nextHour = currentHour;
            let nextMinute = 0;

            if (currentMinute > 30) {
                // Ví dụ 10:45 -> 11:00
                nextHour += 1;
                nextMinute = 0;
            } else if (currentMinute > 0) {
                // Ví dụ 10:15 -> 10:30
                nextMinute = 30;
            } else {
                // Ví dụ 10:00 -> 11:00 (mặc định cho đặt sau 1h)
                nextHour += 1;
                nextMinute = 0;
            }

            // Xử lý trường hợp 23:30 -> 00:00 hôm sau
            if (nextHour > 23) {
                nextHour = nextHour % 24;
                defaultStart.setDate(defaultStart.getDate() + 1); // Chuyển sang ngày tiếp theo nếu > 23h
            }

            defaultStart.setHours(nextHour, nextMinute, 0, 0);
        }

        // Giờ kết thúc là giờ bắt đầu + 2 giờ
        defaultEnd.setTime(defaultStart.getTime());
        defaultEnd.setHours(defaultEnd.getHours() + 2);

        // Giới hạn giờ kết thúc không vượt quá 2h sáng hôm sau (xử lý logic sau)
        // Hiện tại chỉ set value cho input

        if (startTimeInput && endTimeInput) {
            startTimeInput.value = defaultStart.toTimeString().slice(0, 5);
            // Giữ lại logic của bạn về end time
            endTimeInput.value = defaultEnd.toTimeString().slice(0, 5);
        }

        // Cập nhật min/max time
        this.updateTimeLimits();

        // Calculate initial duration
        this.calculateDuration();

        // Setup event listeners
        this.setupEventListeners();
    },

    updateTimeLimits: function () {
        const dateInput = document.getElementById('reservationDate')?.value;
        const startTimeInput = document.getElementById('startTime');
        const endTimeInput = document.getElementById('endTime');
        const now = new Date();
        const today = now.toISOString().split('T')[0];
        const tomorrow = new Date(now);
        tomorrow.setDate(tomorrow.getDate() + 1);
        const tomorrowDate = tomorrow.toISOString().split('T')[0];

        if (!dateInput || !startTimeInput || !endTimeInput) return;

        // Xóa giới hạn min/max trước
        startTimeInput.min = '';

        // Cập nhật min time cho startTime (chỉ áp dụng nếu chọn ngày hôm nay)
        if (dateInput === today) {
            const currentHour = now.getHours();
            const currentMinute = now.getMinutes();

            // 1. Nếu đang trong khung 1h sáng, TỰ ĐỘNG chuyển sang ngày mai và thoát.
            // Điều kiện này sẽ xử lý khi người dùng chọn lại ngày hôm nay trong khi đang là 1h.
            if (currentHour === this.LATE_HOUR_LIMIT) {
                document.getElementById('reservationDate').value = tomorrowDate;
                this.updateTimeLimits(); // Đệ quy để cập nhật lại theo ngày mai
                if (window.Toast) {
                    Toast.warning('Hiện tại đã là 1h sáng. Vui lòng đặt bàn cho ngày tiếp theo!');
                }
                return;
            }

            // 2. Nếu đang trong khoảng 2h sáng đến 7h sáng, min là 7h sáng
            if (currentHour >= this.END_TIME_MAX && currentHour < this.START_TIME_MIN) {
                startTimeInput.min = `${this.START_TIME_MIN.toString().padStart(2, '0')}:00`;
            }
            // 3. Nếu đang trong khung giờ hoạt động (7h sáng - 23h59) hoặc 0h sáng
            else if (currentHour >= this.START_TIME_MIN || currentHour < this.END_TIME_MAX) {
                let minStartHour = currentHour;
                let minStartMinute;

                // Làm tròn giờ hiện tại lên nửa giờ tiếp theo
                if (currentMinute > 30) {
                    minStartHour += 1;
                    minStartMinute = 0;
                } else if (currentMinute > 0) {
                    minStartMinute = 30;
                } else {
                    // Nếu là giờ tròn, giờ bắt đầu tối thiểu là giờ tiếp theo
                    minStartHour += 1;
                    minStartMinute = 0;
                }

                // Xử lý trường hợp 23h30 hoặc 0h30 (sau khi đã cộng 1h)
                if (minStartHour > 23) {
                    minStartHour = minStartHour % 24;
                }

                // Kiểm tra giới hạn 1h sáng (không cho đặt BẮT ĐẦU từ 1h)
                if (minStartHour === this.LATE_HOUR_LIMIT && minStartMinute >= 0) {
                    // Nếu thời gian tối thiểu tính ra là 1hxx, ta phải chuyển sang ngày mai
                    document.getElementById('reservationDate').value = tomorrowDate;
                    this.updateTimeLimits(); // Đệ quy để cập nhật lại theo ngày mai
                    return;
                }

                startTimeInput.min = `${minStartHour.toString().padStart(2, '0')}:${minStartMinute.toString().padStart(2, '0')}`;
            }

        } else {
            // Nếu chọn ngày khác (tương lai), min là 07:00
            startTimeInput.min = '07:00';
        }

        // Đảm bảo updateEndTimeMin được gọi để set min cho endTime
        this.updateEndTimeMin();
    },
    updateEndTimeMin: function () {
        const startTime = document.getElementById('startTime')?.value;
        if (!startTime) return;

        const [hours, minutes] = startTime.split(':').map(Number);

        // Tính toán giờ tối thiểu: Giờ bắt đầu + 30 phút
        const start = new Date(new Date().setHours(hours, minutes, 0, 0));
        start.setMinutes(start.getMinutes() + 30);

        const minEndTime = start.toTimeString().slice(0, 5);
        const endTimeInput = document.getElementById('endTime');

        if (endTimeInput) {
            endTimeInput.min = minEndTime;

            // Auto-set end time if not set or invalid
            if (!endTimeInput.value || endTimeInput.value < minEndTime) {
                // Đặt mặc định là giờ bắt đầu + 2 giờ
                start.setHours(hours + 2, minutes, 0);
                let newEndTime = start.toTimeString().slice(0, 5);

                // Giới hạn max end time (2h sáng hôm sau)
                // Cần kiểm tra xem giờ kết thúc (dạng 24h) có vượt quá 26h (2h sáng hôm sau) không
                const endHour24 = hours + 2;
                if (endHour24 >= this.END_TIME_MAX + 24) {
                    newEndTime = '02:00'; // Đặt tối đa là 02:00 sáng
                }
                endTimeInput.value = newEndTime;
            }
        }

        this.calculateDuration();
    },
    setupEventListeners: function () {
        const dateInput = document.getElementById('reservationDate');
        const startTimeInput = document.getElementById('startTime');
        const endTimeInput = document.getElementById('endTime');

        if (dateInput) {
            dateInput.addEventListener('change', () => {
                this.updateTimeLimits(); // Cập nhật giới hạn giờ khi ngày thay đổi
                this.calculateDuration();
            });
        }

        if (startTimeInput) {
            startTimeInput.addEventListener('change', () => {
                this.updateEndTimeMin();
                this.calculateDuration(); // Thêm calculateDuration để check giới hạn
            });
        }

        if (endTimeInput) {
            endTimeInput.addEventListener('change', () => this.calculateDuration());
        }
    },
    filterTablesByArea: function (area, event) {
        console.log('Filtering by area:', area);

        this.currentAreaFilter = area;

        // Update active button
        const buttons = document.querySelectorAll('.area-btn-sm');
        buttons.forEach(btn => {
            btn.classList.remove('active');
        });

        if (event && event.target) {
            event.target.classList.add('active');
        }

        // Filter tables
        this.applyFilters();
    },

    /**
     * Tìm kiếm bàn theo tên
     */
    searchTables: function () {
        const searchInput = document.getElementById('tableSearch');
        if (!searchInput) return;

        const searchTerm = searchInput.value.toLowerCase().trim();
        console.log('🔍 Searching for:', searchTerm);

        this.applyFilters(searchTerm);
    },

    /**
     * Áp dụng các bộ lọc (khu vực + tìm kiếm)
     */
    applyFilters: function (searchTerm = '') {
        const tables = document.querySelectorAll('.diagram-table-compact');
        let visibleCount = 0;

        tables.forEach(table => {
            const tableArea = table.getAttribute('data-area') || '';
            const tableName = table.getAttribute('data-name')?.toLowerCase() || '';
            const tableType = table.getAttribute('data-type')?.toLowerCase() || '';

            // Check area filter
            const areaMatch = this.currentAreaFilter === 'all' || tableArea === this.currentAreaFilter;

            // Check search filter
            const searchMatch = searchTerm === '' ||
                tableName.includes(searchTerm) ||
                tableType.includes(searchTerm);

            if (areaMatch && searchMatch) {
                table.style.display = 'block';
                visibleCount++;
            } else {
                table.style.display = 'none';
            }
        });

        console.log(`✅ ${visibleCount} tables visible`);

        // Show empty message if no tables
        this.toggleEmptyMessage(visibleCount === 0);
    },

    /**
     * Hiển thị/ẩn thông báo không có bàn
     */
    toggleEmptyMessage: function (show) {
        const diagram = document.getElementById('tableDiagram');
        if (!diagram) return;

        let emptyMsg = diagram.querySelector('.no-tables-found');

        if (show && !emptyMsg) {
            emptyMsg = document.createElement('div');
            emptyMsg.className = 'no-tables-found';
            emptyMsg.innerHTML = `
                <div class="no-tables-icon">🔍</div>
                <p>Không tìm thấy bàn phù hợp</p>
            `;
            diagram.appendChild(emptyMsg);
        } else if (!show && emptyMsg) {
            emptyMsg.remove();
        }
    },

    /**
     * Chọn bàn
     */
    selectTable: function (tableId) {
        console.log('✅ Selecting table:', tableId);

        // Remove previous selection
        const allTables = document.querySelectorAll('.diagram-table-compact');
        allTables.forEach(table => {
            table.classList.remove('selected');
        });

        // Find and select the clicked table
        const selectedTable = document.querySelector(`.diagram-table-compact[data-table-id="${tableId}"]`);
        if (!selectedTable) {
            console.error('❌ Table not found:', tableId);
            if (window.Toast) {
                Toast.error('Không tìm thấy bàn');
            }
            return;
        }

        selectedTable.classList.add('selected');

        // Get table data from attributes
        const tableName = selectedTable.getAttribute('data-name');
        const tableType = selectedTable.getAttribute('data-type');
        const tablePrice = parseFloat(selectedTable.getAttribute('data-price')) || 0;

        // Store selected table data
        this.selectedTableData = {
            id: tableId,
            name: tableName,
            type: tableType,
            price: tablePrice
        };

        console.log('📋 Selected table data:', this.selectedTableData);

        // Update hidden input
        const reservationTableInput = document.getElementById('reservationTable');
        if (reservationTableInput) {
            reservationTableInput.value = tableId;
        }

        // Show selected info
        this.updateSelectedInfo();

        // Recalculate cost
        this.calculateDuration();

        // Scroll to time selection
        const timeSection = document.querySelector('.reservation-step:nth-child(2)');
        if (timeSection) {
            setTimeout(() => {
                timeSection.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            }, 100);
        }
    },

    /**
     * Cập nhật thông tin bàn đã chọn
     */
    updateSelectedInfo: function () {
        const infoPanel = document.getElementById('selectedTableInfo');
        const infoName = document.getElementById('selectedTableName');

        if (!infoPanel || !infoName || !this.selectedTableData) return;

        infoName.textContent = `${this.selectedTableData.name} - ${this.selectedTableData.type} - ${this.selectedTableData.price.toLocaleString('vi-VN')}đ/h`;
        infoPanel.style.display = 'flex';
    },

    /**
     * Cập nhật giờ kết thúc tối thiểu
     */
    updateEndTimeMin: function () {
        const startTime = document.getElementById('startTime')?.value;
        if (!startTime) return;

        const [hours, minutes] = startTime.split(':');
        const start = new Date();
        start.setHours(parseInt(hours), parseInt(minutes), 0);
        start.setMinutes(start.getMinutes() + 30); // Minimum 30 minutes

        const minEndTime = start.toTimeString().slice(0, 5);
        const endTimeInput = document.getElementById('endTime');

        if (endTimeInput) {
            endTimeInput.min = minEndTime;

            // Auto-set end time if not set or invalid
            if (!endTimeInput.value || endTimeInput.value < minEndTime) {
                start.setHours(parseInt(hours) + 2, parseInt(minutes), 0); // Add 2 hours
                endTimeInput.value = start.toTimeString().slice(0, 5);
            }
        }

        this.calculateDuration();
    },

    calculateDuration: function () {
        const date = document.getElementById('reservationDate')?.value;
        const startTime = document.getElementById('startTime')?.value;
        const endTime = document.getElementById('endTime')?.value;
        const durationDisplay = document.getElementById('durationDisplay');

        if (!date || !startTime || !endTime) {
            if (durationDisplay) {
                durationDisplay.style.display = 'none';
            }
            return;
        }

        // --- 1. Tạo đối tượng ngày giờ
        const start = new Date(`${date}T${startTime}`);
        let end = new Date(`${date}T${endTime}`);

        // --- 2. Xử lý trường hợp giờ kết thúc sang ngày hôm sau
        const [startHours] = startTime.split(':').map(Number);
        const [endHours] = endTime.split(':').map(Number);

        // Nếu giờ kết thúc (0h, 1h, 2h) nhỏ hơn giờ bắt đầu (7h-23h)
        if (endHours >= 0 && endHours < this.START_TIME_MIN && startHours >= this.START_TIME_MIN) {
            end.setDate(end.getDate() + 1); // Tăng ngày lên 1
        }

        // 💡 Thêm kiểm tra nếu người dùng chọn ngày hôm nay và giờ bắt đầu đã là giờ quá khứ
        const now = new Date();
        const today = now.toISOString().split('T')[0];

        // Nếu ngày đặt là hôm nay VÀ giờ bắt đầu đã trôi qua
        if (date === today && start < now && startHours >= this.START_TIME_MIN) {
            if (window.Toast) {
                Toast.error('Giờ bắt đầu không được là giờ của quá khứ!');
            }
            if (durationDisplay) durationDisplay.style.display = 'none';
            return;
        }


        const durationMs = end - start;

        // --- 3. Kiểm tra giới hạn thời gian (7h sáng - 2h sáng hôm sau)

        // a. Kiểm tra giờ bắt đầu tối thiểu (7h sáng)
        if (startHours < this.START_TIME_MIN && startHours >= this.END_TIME_MAX) {
            if (window.Toast) {
                Toast.error(`Giờ bắt đầu phải từ ${this.START_TIME_MIN}:00 sáng!`);
            }
            if (durationDisplay) durationDisplay.style.display = 'none';
            return;
        }

        // 💡 Thêm kiểm tra KHÔNG cho đặt bàn bắt đầu từ 1h sáng
        if (startHours === this.LATE_HOUR_LIMIT) {
            if (window.Toast) {
                Toast.error('Không được đặt bàn sau 01:00 sáng!');
            }
            if (durationDisplay) durationDisplay.style.display = 'none';
            return;
        }

        // b. Kiểm tra giờ kết thúc tối đa (2h sáng hôm sau)
        // Nếu giờ kết thúc nằm giữa 2h và 7h sáng
        if (endHours > this.END_TIME_MAX && endHours < this.START_TIME_MIN) {
            if (window.Toast) {
                Toast.error(`Giờ kết thúc không được vượt quá ${this.END_TIME_MAX.toString().padStart(2, '0')}:00 sáng hôm sau!`);
            }
            if (durationDisplay) durationDisplay.style.display = 'none';
            return;
        }

        // c. Kiểm tra thời lượng
        if (durationMs <= 0) {
            if (durationDisplay) {
                durationDisplay.style.display = 'none';
            }
            if (window.Toast) {
                Toast.warning('Giờ kết thúc phải sau giờ bắt đầu!');
            }
            return;
        }

        // --- 4. Tính toán và hiển thị
        const hours = durationMs / (1000 * 60 * 60);
        const roundedHours = Math.ceil(hours * 4) / 4; // Round to 15 minutes

        // Display duration
        this.updateDurationDisplay(roundedHours);

        // Calculate and display cost
        this.updateEstimatedCost(roundedHours);

        if (durationDisplay) {
            durationDisplay.style.display = 'flex';
        }
    },

    /**
     * Cập nhật hiển thị thời gian
     */
    updateDurationDisplay: function (hours) {
        const durationText = document.getElementById('durationText');
        if (!durationText) return;

        if (hours < 1) {
            durationText.textContent = `${Math.round(hours * 60)} phút`;
        } else {
            const h = Math.floor(hours);
            const m = Math.round((hours - h) * 60);
            durationText.textContent = m > 0 ? `${h} giờ ${m} phút` : `${h} giờ`;
        }
    },

    /**
     * Cập nhật chi phí dự kiến
     */
    updateEstimatedCost: function (hours) {
        const estimatedCostEl = document.getElementById('estimatedCost');
        if (!estimatedCostEl) return;

        if (this.selectedTableData) {
            const estimatedCost = Math.round(this.selectedTableData.price * hours / 1000) * 1000;
            estimatedCostEl.textContent = estimatedCost.toLocaleString('vi-VN') + 'đ';
        } else {
            estimatedCostEl.textContent = 'Vui lòng chọn bàn';
        }
    },

    /**
     * Validate form trước khi submit
     */
    validateForm: function () {
        const errors = [];

        // Check table selection
        if (!this.selectedTableData) {
            errors.push('Vui lòng chọn bàn');
        }

        // Check date and time
        const date = document.getElementById('reservationDate')?.value;
        const startTime = document.getElementById('startTime')?.value;
        const endTime = document.getElementById('endTime')?.value;

        if (!date || !startTime || !endTime) {
            errors.push('Vui lòng chọn đầy đủ thời gian');
        }

        // Check customer info
        const customerName = document.getElementById('customerName')?.value?.trim();
        const customerPhone = document.getElementById('customerPhone')?.value?.trim();

        if (!customerName) {
            errors.push('Vui lòng nhập tên khách hàng');
        }

        if (!customerPhone) {
            errors.push('Vui lòng nhập số điện thoại');
        } else if (!/^[0-9]{10,11}$/.test(customerPhone)) {
            errors.push('Số điện thoại không hợp lệ');
        }

        // Show errors if any
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
     * Reset form
     */
    reset: function () {
        console.log('🔄 Resetting reservation form');

        this.selectedTableData = null;
        this.currentAreaFilter = 'all';

        // Clear selection
        const allTables = document.querySelectorAll('.diagram-table-compact');
        allTables.forEach(table => {
            table.classList.remove('selected');
        });

        // Hide selected info
        const infoPanel = document.getElementById('selectedTableInfo');
        if (infoPanel) {
            infoPanel.style.display = 'none';
        }

        // Reset form
        const form = document.getElementById('reservationForm');
        if (form) {
            form.reset();
        }

        // Reset area filter
        const areaButtons = document.querySelectorAll('.area-btn-sm');
        areaButtons.forEach(btn => {
            btn.classList.remove('active');
            if (btn.textContent.includes('Tất cả')) {
                btn.classList.add('active');
            }
        });

        // Reinitialize
        this.init();
    }
};
function initReservationModal() {
    if (typeof ReservationManager === 'undefined') {
        console.error('ReservationManager not found!');
        return;
    }

    const reservationModal = document.querySelector('.reservation-modal');
    if (reservationModal) {
        console.log('Initializing ReservationManager for modal');
        setTimeout(() => {
            ReservationManager.init();
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
                        const modal = node.querySelector ? node.querySelector('.reservation-modal') : null;
                        if (modal || (node.classList && node.classList.contains('reservation-modal'))) {
                            initReservationModal();
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
        initReservationModal();
    });
}

window.ReservationManager = ReservationManager;
window.initReservationModal = initReservationModal;