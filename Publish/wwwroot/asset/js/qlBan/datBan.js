const ReservationManager = {
    selectedTableData: null,
    currentAreaFilter: 'all',

    // Đặt hằng số cho giờ giới hạn
    START_TIME_MIN: 8, // 8h sáng
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

        const today = now.toISOString().split('T')[0];
        if (dateInput) {
            dateInput.min = today;
            dateInput.value = today;
        }

        let defaultStart = new Date(now.getTime());
        let defaultEnd = new Date(now.getTime());

        const currentHour = now.getHours();
        const currentMinute = now.getMinutes();

        if (currentHour === this.LATE_HOUR_LIMIT) {
            if (window.Toast) {
                Toast.warning('Hiện tại đã là 1h sáng. Vui lòng đặt bàn cho ngày tiếp theo!');
            }
            const tomorrow = new Date(now);
            tomorrow.setDate(tomorrow.getDate() + 1);
            const tomorrowDate = tomorrow.toISOString().split('T')[0];
            if (dateInput) dateInput.value = tomorrowDate;

            defaultStart.setDate(defaultStart.getDate() + 1);
            defaultStart.setHours(this.START_TIME_MIN, 0, 0, 0);
        } else if (currentHour >= this.END_TIME_MAX && currentHour < this.START_TIME_MIN) {
            defaultStart.setHours(this.START_TIME_MIN, 0, 0, 0);
        } else {
            let nextHour = currentHour;
            let nextMinute = 0;

            if (currentMinute > 30) {
                nextHour += 1;
                nextMinute = 0;
            } else if (currentMinute > 0) {
                nextMinute = 30;
            } else {
                nextHour += 1;
                nextMinute = 0;
            }

            if (nextHour > 23) {
                nextHour = nextHour % 24;
                defaultStart.setDate(defaultStart.getDate() + 1);
            }

            defaultStart.setHours(nextHour, nextMinute, 0, 0);
        }

        defaultEnd.setTime(defaultStart.getTime());
        defaultEnd.setHours(defaultEnd.getHours() + 2);

        if (startTimeInput && endTimeInput) {
            startTimeInput.value = defaultStart.toTimeString().slice(0, 5);
            endTimeInput.value = defaultEnd.toTimeString().slice(0, 5);
        }

        this.updateTimeLimits();
        this.calculateDuration();
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

        startTimeInput.min = '';

        if (dateInput === today) {
            const currentHour = now.getHours();
            const currentMinute = now.getMinutes();

            if (currentHour === this.LATE_HOUR_LIMIT) {
                document.getElementById('reservationDate').value = tomorrowDate;
                this.updateTimeLimits();
                if (window.Toast) {
                    Toast.warning('Hiện tại đã là 1h sáng. Vui lòng đặt bàn cho ngày tiếp theo!');
                }
                return;
            }

            if (currentHour >= this.END_TIME_MAX && currentHour < this.START_TIME_MIN) {
                startTimeInput.min = `${this.START_TIME_MIN.toString().padStart(2, '0')}:00`;
            } else if (currentHour >= this.START_TIME_MIN || currentHour < this.END_TIME_MAX) {
                let minStartHour = currentHour;
                let minStartMinute;

                if (currentMinute > 30) {
                    minStartHour += 1;
                    minStartMinute = 0;
                } else if (currentMinute > 0) {
                    minStartMinute = 30;
                } else {
                    minStartHour += 1;
                    minStartMinute = 0;
                }

                if (minStartHour > 23) {
                    minStartHour = minStartHour % 24;
                }

                if (minStartHour === this.LATE_HOUR_LIMIT && minStartMinute >= 0) {
                    document.getElementById('reservationDate').value = tomorrowDate;
                    this.updateTimeLimits();
                    return;
                }

                startTimeInput.min = `${minStartHour.toString().padStart(2, '0')}:${minStartMinute.toString().padStart(2, '0')}`;
            }
        } else {
            startTimeInput.min = '07:00';
        }

        this.updateEndTimeMin();
    },

    updateEndTimeMin: function () {
        const startTime = document.getElementById('startTime')?.value;
        if (!startTime) return;

        const [hours, minutes] = startTime.split(':').map(Number);
        const start = new Date(new Date().setHours(hours, minutes, 0, 0));
        start.setMinutes(start.getMinutes() + 30);

        const minEndTime = start.toTimeString().slice(0, 5);
        const endTimeInput = document.getElementById('endTime');

        if (endTimeInput) {
            endTimeInput.min = minEndTime;

            if (!endTimeInput.value || endTimeInput.value < minEndTime) {
                start.setHours(hours + 2, minutes, 0);
                let newEndTime = start.toTimeString().slice(0, 5);

                const endHour24 = hours + 2;
                if (endHour24 >= this.END_TIME_MAX + 24) {
                    newEndTime = '02:00';
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
                this.updateTimeLimits();
                this.calculateDuration();
                this.loadAvailableTables();
            });
        }

        if (startTimeInput) {
            startTimeInput.addEventListener('change', () => {
                this.updateEndTimeMin();
                this.calculateDuration();
                this.loadAvailableTables();
            });
        }

        if (endTimeInput) {
            endTimeInput.addEventListener('change', () => {
                this.calculateDuration();
                this.loadAvailableTables();
            });
        }
    },

    loadAvailableTables: async function () {
        const date = document.getElementById('reservationDate')?.value;
        const startTime = document.getElementById('startTime')?.value;
        const endTime = document.getElementById('endTime')?.value;

        if (!date || !startTime || !endTime) {
            console.log('⚠️ Chưa đủ thông tin để load bàn');
            return;
        }

        if (!this.validateTimeRange(date, startTime, endTime)) {
            return;
        }

        try {
            console.log(`🔍 Loading available tables: ${date} ${startTime} - ${endTime}`);

            const response = await fetch(`/QLBan/LayBanTrongTheoGio?ngayDat=${date}&gioBatDau=${startTime}&gioKetThuc=${endTime}`);
            const result = await response.json();

            if (result.success) {
                this.updateTableDiagram(result.data);
                console.log(`✅ Loaded ${result.soLuong} available tables`);
            } else {
                console.error('❌ Failed to load tables:', result.message);
                if (window.Toast) {
                    Toast.error(result.message);
                }
            }
        } catch (error) {
            console.error('❌ Error loading tables:', error);
            if (window.Toast) {
                Toast.error('Không thể tải danh sách bàn');
            }
        }
    },

    updateTableDiagram: function (availableTables) {
        const diagram = document.getElementById('tableDiagram');
        if (!diagram) return;

        diagram.innerHTML = '';

        if (availableTables.length === 0) {
            diagram.innerHTML = `
                <div class="no-tables-available" style="grid-column: 1 / -1;">
                    <div class="no-tables-icon">😞</div>
                    <p>Không có bàn trống trong khung giờ này</p>
                    <small style="color: #6c757d;">Vui lòng chọn thời gian khác</small>
                </div>
            `;
            return;
        }

        availableTables.forEach(ban => {
            const tableDiv = document.createElement('div');
            tableDiv.className = 'diagram-table-compact';
            tableDiv.setAttribute('data-table-id', ban.maBan);
            tableDiv.setAttribute('data-area', ban.khuVuc);
            tableDiv.setAttribute('data-name', ban.tenBan);
            tableDiv.setAttribute('data-price', ban.giaGio);
            tableDiv.setAttribute('data-type', ban.loaiBan);
            tableDiv.onclick = () => this.selectTable(ban.maBan);
            tableDiv.title = `${ban.tenBan} - ${ban.loaiBan} - ${ban.giaGio.toLocaleString('vi-VN')}đ/h`;

            tableDiv.innerHTML = `
                <div class="table-name-sm">${ban.tenBan}</div>
                <div class="table-price-sm">${ban.giaGio.toLocaleString('vi-VN')}đ</div>
                ${ban.khuVuc === 'VIP' ? '<span class="vip-badge-sm">⭐</span>' : ''}
            `;

            diagram.appendChild(tableDiv);
        });

        this.applyFilters();
    },

    validateTimeRange: function (date, startTime, endTime) {
        const start = new Date(`${date}T${startTime}`);
        let end = new Date(`${date}T${endTime}`);

        const [startHours] = startTime.split(':').map(Number);
        const [endHours] = endTime.split(':').map(Number);

        // ✅ FIX: Chỉ cộng ngày khi giờ kết thúc < giờ bắt đầu (qua đêm)
        if (endHours < startHours) {
            end.setDate(end.getDate() + 1);
        }

        const durationMs = end - start;

        if (durationMs <= 0) {
            return false;
        }

        if (startHours === this.LATE_HOUR_LIMIT) {
            return false;
        }

        // ✅ FIX: Kiểm tra giờ kết thúc không vượt quá 2h sáng (chỉ khi qua đêm)
        if (endHours < startHours && endHours > this.END_TIME_MAX) {
            return false;
        }

        return true;
    },

    filterTablesByArea: function (area, event) {
        console.log('Filtering by area:', area);
        this.currentAreaFilter = area;

        const buttons = document.querySelectorAll('.area-btn-sm');
        buttons.forEach(btn => btn.classList.remove('active'));

        if (event && event.target) {
            event.target.classList.add('active');
        }

        this.applyFilters();
    },

    searchTables: function () {
        const searchInput = document.getElementById('tableSearch');
        if (!searchInput) return;

        const searchTerm = searchInput.value.toLowerCase().trim();
        console.log('🔍 Searching for:', searchTerm);

        this.applyFilters(searchTerm);
    },

    applyFilters: function (searchTerm = '') {
        const tables = document.querySelectorAll('.diagram-table-compact');
        let visibleCount = 0;

        tables.forEach(table => {
            const tableArea = table.getAttribute('data-area') || '';
            const tableName = table.getAttribute('data-name')?.toLowerCase() || '';
            const tableType = table.getAttribute('data-type')?.toLowerCase() || '';

            const areaMatch = this.currentAreaFilter === 'all' || tableArea === this.currentAreaFilter;
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
        this.toggleEmptyMessage(visibleCount === 0);
    },

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

    selectTable: function (tableId) {
        console.log('✅ Selecting table:', tableId);

        const allTables = document.querySelectorAll('.diagram-table-compact');
        allTables.forEach(table => table.classList.remove('selected'));

        const selectedTable = document.querySelector(`.diagram-table-compact[data-table-id="${tableId}"]`);
        if (!selectedTable) {
            console.error('❌ Table not found:', tableId);
            if (window.Toast) {
                Toast.error('Không tìm thấy bàn');
            }
            return;
        }

        selectedTable.classList.add('selected');

        const tableName = selectedTable.getAttribute('data-name');
        const tableType = selectedTable.getAttribute('data-type');
        const tablePrice = parseFloat(selectedTable.getAttribute('data-price')) || 0;

        this.selectedTableData = {
            id: tableId,
            name: tableName,
            type: tableType,
            price: tablePrice
        };

        console.log('📋 Selected table data:', this.selectedTableData);

        const reservationTableInput = document.getElementById('reservationTable');
        if (reservationTableInput) {
            reservationTableInput.value = tableId;
        }

        this.updateSelectedInfo();
        this.calculateDuration();

        const timeSection = document.querySelector('.reservation-step:nth-child(2)');
        if (timeSection) {
            setTimeout(() => {
                timeSection.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            }, 100);
        }
    },

    updateSelectedInfo: function () {
        const infoPanel = document.getElementById('selectedTableInfo');
        const infoName = document.getElementById('selectedTableName');

        if (!infoPanel || !infoName || !this.selectedTableData) return;

        infoName.textContent = `${this.selectedTableData.name} - ${this.selectedTableData.type} - ${this.selectedTableData.price.toLocaleString('vi-VN')}đ/h`;
        infoPanel.style.display = 'flex';
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

        const start = new Date(`${date}T${startTime}`);
        let end = new Date(`${date}T${endTime}`);

        const [startHours] = startTime.split(':').map(Number);
        const [endHours] = endTime.split(':').map(Number);

        // ✅ FIX: Chỉ cộng ngày khi giờ kết thúc < giờ bắt đầu
        if (endHours < startHours) {
            end.setDate(end.getDate() + 1);
        }

        const now = new Date();
        const today = now.toISOString().split('T')[0];

        if (date === today && start < now && startHours >= this.START_TIME_MIN) {
            if (window.Toast) {
                Toast.error('Giờ bắt đầu không được là giờ của quá khứ!');
            }
            if (durationDisplay) durationDisplay.style.display = 'none';
            return;
        }

        const durationMs = end - start;

        if (startHours < this.START_TIME_MIN && startHours >= this.END_TIME_MAX) {
            if (window.Toast) {
                Toast.error(`Giờ bắt đầu phải từ ${this.START_TIME_MIN}:00 sáng!`);
            }
            if (durationDisplay) durationDisplay.style.display = 'none';
            return;
        }

        if (startHours === this.LATE_HOUR_LIMIT) {
            if (window.Toast) {
                Toast.error('Không được đặt bàn sau 01:00 sáng!');
            }
            if (durationDisplay) durationDisplay.style.display = 'none';
            return;
        }

        // ✅ FIX: Kiểm tra giờ kết thúc chỉ khi qua đêm
        if (endHours < startHours && endHours > this.END_TIME_MAX) {
            if (window.Toast) {
                Toast.error(`Giờ kết thúc không được vượt quá ${this.END_TIME_MAX.toString().padStart(2, '0')}:00 sáng hôm sau!`);
            }
            if (durationDisplay) durationDisplay.style.display = 'none';
            return;
        }

        if (durationMs <= 0) {
            if (durationDisplay) {
                durationDisplay.style.display = 'none';
            }
            if (window.Toast) {
                Toast.warning('Giờ kết thúc phải sau giờ bắt đầu!');
            }
            return;
        }

        const hours = durationMs / (1000 * 60 * 60);
        const roundedHours = Math.ceil(hours * 4) / 4;

        this.updateDurationDisplay(roundedHours);
        this.updateEstimatedCost(roundedHours);

        if (durationDisplay) {
            durationDisplay.style.display = 'flex';
        }
    },

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

    validateForm: function () {
        const errors = [];

        if (!this.selectedTableData) {
            errors.push('Vui lòng chọn bàn');
        }

        const date = document.getElementById('reservationDate')?.value;
        const startTime = document.getElementById('startTime')?.value;
        const endTime = document.getElementById('endTime')?.value;

        if (!date || !startTime || !endTime) {
            errors.push('Vui lòng chọn đầy đủ thời gian');
        }

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

    reset: function () {
        console.log('🔄 Resetting reservation form');

        this.selectedTableData = null;
        this.currentAreaFilter = 'all';

        const allTables = document.querySelectorAll('.diagram-table-compact');
        allTables.forEach(table => table.classList.remove('selected'));

        const infoPanel = document.getElementById('selectedTableInfo');
        if (infoPanel) {
            infoPanel.style.display = 'none';
        }

        const form = document.getElementById('reservationForm');
        if (form) {
            form.reset();
        }

        const areaButtons = document.querySelectorAll('.area-btn-sm');
        areaButtons.forEach(btn => {
            btn.classList.remove('active');
            if (btn.textContent.includes('Tất cả')) {
                btn.classList.add('active');
            }
        });

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

if (typeof MutationObserver !== 'undefined') {
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.addedNodes.length) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) {
                        const modal = node.querySelector ? node.querySelector('.reservation-modal') : null;
                        if (modal || (node.classList && node.classList.contains('reservation-modal'))) {
                            initReservationModal();
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

        initReservationModal();
    });
}

window.ReservationManager = ReservationManager;
window.initReservationModal = initReservationModal;