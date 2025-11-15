// scheduleModal.js - Modal lịch làm việc nâng cao

// ===========================
// GLOBAL VARIABLES
// ===========================
let currentScheduleWeek = new Date();
let scheduleModalData = {};
let allScheduleEmployees = [];

// ===========================
// OPEN SCHEDULE MODAL
// ===========================

function openScheduleModal() {
    // Create modal overlay
    const modal = document.createElement('div');
    modal.id = 'scheduleModal';
    modal.className = 'schedule-modal-overlay';
    modal.innerHTML = generateScheduleModalHTML();

    document.body.appendChild(modal);

    // Load employees and schedule
    loadScheduleData();

    // Show modal with animation
    setTimeout(() => {
        modal.classList.add('active');
    }, 10);
}

function generateScheduleModalHTML() {
    return `
        <div class="schedule-modal-backdrop" onclick="closeScheduleModal()"></div>
        <div class="schedule-modal-container">
            <!-- Header -->
            <div class="schedule-modal-header">
                <div class="header-left">
                    <h2>📅 Lịch làm việc tuần</h2>
                    <div class="week-selector">
                        <button class="btn-week-nav" onclick="changeScheduleWeek(-1)" title="Tuần trước">
                            ◀️
                        </button>
                        <span class="week-display" id="scheduleWeekDisplay">Đang tải...</span>
                        <button class="btn-week-nav" onclick="changeScheduleWeek(1)" title="Tuần sau">
                            ▶️
                        </button>
                    </div>
                </div>
                <div class="header-right">
                    <button class="btn-action btn-add" onclick="openAddScheduleForm()">
                        <span>➕</span>
                        <span>Thêm lịch</span>
                    </button>
                    <button class="btn-action btn-export" onclick="exportScheduleToExcel()">
                        <span>📥</span>
                        <span>Xuất Excel</span>
                    </button>
                    <button class="btn-close-schedule" onclick="closeScheduleModal()" title="Đóng">
                        ✕
                    </button>
                </div>
            </div>

            <!-- Main Content -->
            <div class="schedule-modal-content">
                <!-- Timeline (Left Side) -->
                <div class="schedule-timeline">
                    <div class="timeline-header">
                        <div class="timeline-title">Khung giờ</div>
                    </div>
                    <div class="timeline-slots">
                        <div class="timeline-slot morning">
                            <div class="slot-icon">🌅</div>
                            <div class="slot-info">
                                <div class="slot-label">Ca sáng</div>
                                <div class="slot-time">06:00 - 14:00</div>
                            </div>
                            <div class="slot-count" id="morningCount">0</div>
                        </div>
                        <div class="timeline-slot evening">
                            <div class="slot-icon">🌙</div>
                            <div class="slot-info">
                                <div class="slot-label">Ca tối</div>
                                <div class="slot-time">14:00 - 22:00</div>
                            </div>
                            <div class="slot-count" id="eveningCount">0</div>
                        </div>
                    </div>
                    
                    <!-- Statistics -->
                    <div class="timeline-stats">
                        <div class="stat-item">
                            <div class="stat-icon">👥</div>
                            <div class="stat-value" id="totalEmployees">0</div>
                            <div class="stat-label">Nhân viên</div>
                        </div>
                        <div class="stat-item">
                            <div class="stat-icon">📊</div>
                            <div class="stat-value" id="totalShifts">0</div>
                            <div class="stat-label">Ca làm</div>
                        </div>
                    </div>
                </div>

                <!-- Schedule Grid (Right Side) -->
                <div class="schedule-grid-wrapper">
                    <div class="schedule-grid" id="scheduleGridContent">
                        <div class="loading-state">
                            <div class="loading-spinner">⏳</div>
                            <p>Đang tải lịch làm việc...</p>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Legend -->
            <div class="schedule-modal-footer">
                <div class="legend-items">
                    <div class="legend-item">
                        <span class="legend-badge morning">🌅</span>
                        <span>Ca sáng (6h-14h)</span>
                    </div>
                    <div class="legend-item">
                        <span class="legend-badge evening">🌙</span>
                        <span>Ca tối (14h-22h)</span>
                    </div>
                    <div class="legend-item">
                        <span class="legend-badge highlight">⭐</span>
                        <span>Hôm nay</span>
                    </div>
                </div>
            </div>
        </div>
    `;
}

// ===========================
// CLOSE MODAL
// ===========================

function closeScheduleModal() {
    const modal = document.getElementById('scheduleModal');
    if (modal) {
        modal.classList.remove('active');
        setTimeout(() => {
            modal.remove();
        }, 300);
    }
}

// ===========================
// DATA LOADING
// ===========================

async function loadScheduleData() {
    try {
        // Load all employees
        const empResponse = await fetch('/NhanVien/GetAllEmployees');
        if (empResponse.ok) {
            allScheduleEmployees = await empResponse.json();
        }

        // Load schedule for current week
        await loadWeekScheduleData();

    } catch (error) {
        console.error('Error loading schedule data:', error);
        showScheduleError('Không thể tải dữ liệu lịch làm việc');
    }
}

async function loadWeekScheduleData() {
    const gridContent = document.getElementById('scheduleGridContent');
    if (!gridContent) return;

    try {
        // Show loading
        gridContent.innerHTML = `
            <div class="loading-state">
                <div class="loading-spinner">⏳</div>
                <p>Đang tải lịch làm việc...</p>
            </div>
        `;

        // Get week range
        const weekStart = getWeekStartDate(currentScheduleWeek);
        const weekEnd = new Date(weekStart);
        weekEnd.setDate(weekEnd.getDate() + 6);

        // Update week display
        updateWeekDisplay(weekStart, weekEnd);

        // Fetch schedule data from server
        const response = await fetch(
            `/NhanVien/GetWeekSchedule?startDate=${formatDateForAPI(weekStart)}&endDate=${formatDateForAPI(weekEnd)}`
        );

        if (!response.ok) {
            throw new Error('Failed to load schedule');
        }

        scheduleModalData = await response.json();

        // Render schedule grid
        renderScheduleGrid(weekStart);

        // Update statistics
        updateScheduleStatistics();

    } catch (error) {
        console.error('Error loading week schedule:', error);
        showScheduleError('Không thể tải lịch làm việc');
    }
}

function renderScheduleGrid(weekStart) {
    const gridContent = document.getElementById('scheduleGridContent');
    if (!gridContent) return;

    // Generate days array
    const days = [];
    for (let i = 0; i < 7; i++) {
        const date = new Date(weekStart);
        date.setDate(date.getDate() + i);
        days.push(date);
    }

    // Generate time slots
    const timeSlots = [];
    for (let hour = 7; hour <= 22; hour++) {
        timeSlots.push(hour);
    }

    let html = '<div class="schedule-table-hourly">';

    // Header row - Days of week
    html += '<div class="schedule-header-row-hourly">';
    html += '<div class="time-column-header"></div>'; // Empty corner cell

    days.forEach(date => {
        const isToday = isDateToday(date);
        const dayName = date.toLocaleDateString('vi-VN', { weekday: 'short' });
        const dayNum = date.getDate();
        const monthNum = date.getMonth() + 1;

        html += `
            <div class="day-header-hourly ${isToday ? 'today' : ''}">
                <div class="day-number">${dayNum}</div>
                <div class="day-name-small">${dayName}</div>
            </div>
        `;
    });
    html += '</div>';

    // Time slots rows
    timeSlots.forEach(hour => {
        const isPM = hour >= 12;
        const displayHour = hour > 12 ? hour - 12 : hour;
        const ampm = isPM ? 'pm' : 'am';

        html += '<div class="schedule-time-row">';
        html += `<div class="time-label">${displayHour} ${ampm}</div>`;

        days.forEach(date => {
            const dateKey = formatDateForAPI(date);
            const isToday = isDateToday(date);

            // Check which shift this hour belongs to
            const shift = hour < 14 ? 'morning' : 'evening';
            const employees = scheduleModalData[dateKey]?.[shift] || [];

            // Only show employees at start of shift
            const showEmployees = (shift === 'morning' && hour === 7) || (shift === 'evening' && hour === 14);

            html += `
                <div class="schedule-cell-hourly ${isToday ? 'today-cell' : ''} ${shift}-shift-cell"
                     data-date="${dateKey}"
                     data-shift="${shift}"
                     data-hour="${hour}"
                     onclick="openDayScheduleDetail('${dateKey}', '${shift}', '${date.toLocaleDateString('vi-VN')}')">
                    ${showEmployees ? renderScheduleCellContentCompact(employees, shift) : ''}
                </div>
            `;
        });

        html += '</div>';
    });

    html += '</div>';
    gridContent.innerHTML = html;
}
function renderScheduleCellContentCompact(employees, shift) {
    if (employees.length === 0) {
        return '';
    }

    let html = '<div class="cell-employees-compact">';

    // Show first employee name only
    if (employees.length > 0) {
        html += `<div class="emp-name-compact">${employees[0].tenNV}</div>`;
    }

    // Show count if more than 1
    if (employees.length > 1) {
        html += `<div class="emp-count-compact">+${employees.length - 1}</div>`;
    }

    html += '</div>';
    return html;
}
function renderScheduleCellContent(employees, shift) {
    if (employees.length === 0) {
        return `
            <div class="empty-cell">
                <span class="add-icon">➕</span>
                <span class="add-text">Thêm</span>
            </div>
        `;
    }

    let html = '<div class="cell-employees">';

    // Show first 3 employees
    const displayCount = Math.min(3, employees.length);

    for (let i = 0; i < displayCount; i++) {
        const emp = employees[i];
        html += `
            <div class="employee-card-mini ${shift}">
                <div class="emp-avatar-mini">${getInitials(emp.tenNV)}</div>
                <div class="emp-details-mini">
                    <div class="emp-name-mini">${emp.tenNV}</div>
                    <div class="emp-phone-mini">📱 ${emp.sdt}</div>
                </div>
            </div>
        `;
    }

    // Show "more" indicator
    if (employees.length > 3) {
        html += `
            <div class="more-employees-indicator">
                <span>+${employees.length - 3} khác</span>
            </div>
        `;
    }

    html += '</div>';
    return html;
}

// ===========================
// DAY DETAIL MODAL
// ===========================

function openDayScheduleDetail(dateKey, shift, dateDisplay) {
    const employees = scheduleModalData[dateKey]?.[shift] || [];
    const shiftName = shift === 'morning' ? '🌅 Ca sáng (6h-14h)' : '🌙 Ca tối (14h-22h)';

    const detailContent = `
        <div class="day-schedule-detail">
            <h3>${shiftName}</h3>
            <p class="detail-date">${dateDisplay}</p>

            <div class="detail-employee-list">
                ${employees.length > 0 ? renderDetailEmployeeList(employees, dateKey, shift) : `
                    <div class="empty-detail">
                        <div class="empty-icon">👥</div>
                        <p>Chưa có nhân viên được phân công</p>
                        <button class="btn-detail-add" onclick="closeModal(); setTimeout(() => openAddScheduleForm('${dateKey}', '${shift}'), 300)">
                            ➕ Thêm nhân viên
                        </button>
                    </div>
                `}
            </div>

            <div class="detail-actions">
                <button class="btn btn-primary" onclick="closeModal(); setTimeout(() => openAddScheduleForm('${dateKey}', '${shift}'), 300)">
                    ➕ Thêm nhân viên
                </button>
                <button class="btn btn-secondary" onclick="closeModal()">
                    ✕ Đóng
                </button>
            </div>
        </div>
    `;

    openModal(`Chi tiết lịch làm việc`, detailContent);
}

function renderDetailEmployeeList(employees, dateKey, shift) {
    return employees.map(emp => `
        <div class="detail-employee-item">
            <div class="detail-emp-avatar">${getInitials(emp.tenNV)}</div>
            <div class="detail-emp-info">
                <div class="detail-emp-name">${emp.tenNV}</div>
                <div class="detail-emp-role">${emp.tenNhom || 'Nhân viên'}</div>
                <div class="detail-emp-phone">📱 ${emp.sdt}</div>
            </div>
            <div class="detail-emp-actions">
                <button class="btn-icon-action" 
                        onclick="removeEmployeeFromSchedule(${emp.maNV}, '${dateKey}', '${shift}')"
                        title="Xóa khỏi ca">
                    🗑️
                </button>
            </div>
        </div>
    `).join('');
}

// ===========================
// ADD SCHEDULE FORM
// ===========================

function openAddScheduleForm(preselectedDate = null, preselectedShift = null) {
    const formContent = `
        <div class="add-schedule-form">
            <h3>➕ Thêm lịch làm việc</h3>

            <div class="form-group">
                <label>Chọn ngày: <span style="color: red;">*</span></label>
                <input type="date" 
                       id="scheduleDate" 
                       class="form-control"
                       value="${preselectedDate || ''}"
                       min="${formatDateForInput(new Date())}"
                       required>
            </div>

            <div class="form-group">
                <label>Ca làm việc: <span style="color: red;">*</span></label>
                <select id="scheduleShift" class="form-control" required>
                    <option value="">-- Chọn ca --</option>
                    <option value="morning" ${preselectedShift === 'morning' ? 'selected' : ''}>🌅 Ca sáng (6h-14h)</option>
                    <option value="evening" ${preselectedShift === 'evening' ? 'selected' : ''}>🌙 Ca tối (14h-22h)</option>
                </select>
            </div>

            <div class="form-group">
                <label>Chọn nhân viên: <span style="color: red;">*</span></label>
                <div class="search-box-schedule">
                    <input type="text" 
                           id="searchScheduleEmployee" 
                           class="form-control"
                           placeholder="🔍 Tìm kiếm nhân viên..."
                           oninput="filterScheduleEmployees()">
                </div>
                <div class="employee-select-list-schedule" id="employeeSelectListSchedule">
                    ${renderEmployeeSelectList()}
                </div>
            </div>

            <div class="form-actions">
                <button class="btn btn-primary" onclick="submitAddSchedule()">
                    ✅ Thêm lịch
                </button>
                <button class="btn btn-secondary" onclick="closeModal()">
                    ❌ Hủy
                </button>
            </div>
        </div>
    `;

    openModal('Thêm lịch làm việc', formContent);
}

function renderEmployeeSelectList() {
    return allScheduleEmployees.map(emp => `
        <div class="employee-select-item-schedule" data-search="${emp.tenNV.toLowerCase()} ${emp.sdt}">
            <label class="checkbox-label-schedule">
                <input type="checkbox" value="${emp.maNV}">
                <div class="emp-select-info">
                    <div class="emp-select-avatar">${getInitials(emp.tenNV)}</div>
                    <div class="emp-select-details">
                        <div class="emp-select-name">${emp.tenNV}</div>
                        <div class="emp-select-meta">
                            <span class="emp-select-role">${emp.tenNhom || 'Nhân viên'}</span>
                            <span class="emp-select-phone">📱 ${emp.sdt}</span>
                        </div>
                    </div>
                </div>
            </label>
        </div>
    `).join('');
}

function filterScheduleEmployees() {
    const searchTerm = document.getElementById('searchScheduleEmployee')?.value.toLowerCase() || '';
    const items = document.querySelectorAll('.employee-select-item-schedule');

    items.forEach(item => {
        const searchData = item.getAttribute('data-search');
        item.style.display = searchData.includes(searchTerm) ? '' : 'none';
    });
}

async function submitAddSchedule() {
    const date = document.getElementById('scheduleDate')?.value;
    const shift = document.getElementById('scheduleShift')?.value;
    const checkboxes = document.querySelectorAll('#employeeSelectListSchedule input[type="checkbox"]:checked');
    const employeeIds = Array.from(checkboxes).map(cb => parseInt(cb.value));

    if (!date || !shift) {
        alert('⚠️ Vui lòng điền đầy đủ thông tin');
        return;
    }

    if (employeeIds.length === 0) {
        alert('⚠️ Vui lòng chọn ít nhất 1 nhân viên');
        return;
    }

    try {
        const response = await fetch('/NhanVien/AddSchedule', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                date: date,
                shift: shift,
                employeeIds: employeeIds
            })
        });

        if (!response.ok) throw new Error('Failed to add schedule');

        const result = await response.json();

        if (result.success) {
            alert('✅ Thêm lịch làm việc thành công!');
            closeModal();
            await loadWeekScheduleData();
        } else {
            alert('❌ ' + (result.message || 'Có lỗi xảy ra'));
        }

    } catch (error) {
        console.error('Error adding schedule:', error);
        alert('❌ Không thể thêm lịch. Vui lòng thử lại.');
    }
}

async function removeEmployeeFromSchedule(empId, dateKey, shift) {
    if (!confirm('Xác nhận xóa nhân viên khỏi ca làm việc này?')) {
        return;
    }

    try {
        const response = await fetch('/NhanVien/RemoveFromSchedule', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                employeeId: empId,
                date: dateKey,
                shift: shift
            })
        });

        if (!response.ok) throw new Error('Failed to remove');

        const result = await response.json();

        if (result.success) {
            alert('✅ Đã xóa nhân viên khỏi ca');
            closeModal();
            await loadWeekScheduleData();
        } else {
            alert('❌ ' + (result.message || 'Có lỗi xảy ra'));
        }

    } catch (error) {
        console.error('Error removing employee:', error);
        alert('❌ Không thể xóa. Vui lòng thử lại.');
    }
}

// ===========================
// NAVIGATION & UTILITIES
// ===========================

function changeScheduleWeek(delta) {
    currentScheduleWeek.setDate(currentScheduleWeek.getDate() + (delta * 7));
    loadWeekScheduleData();
}

function getWeekStartDate(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Monday
    return new Date(d.setDate(diff));
}

function updateWeekDisplay(startDate, endDate) {
    const display = document.getElementById('scheduleWeekDisplay');
    if (display) {
        const startStr = `${startDate.getDate()}/${startDate.getMonth() + 1}`;
        const endStr = `${endDate.getDate()}/${endDate.getMonth() + 1}/${endDate.getFullYear()}`;
        display.textContent = `${startStr} - ${endStr}`;
    }
}

function updateScheduleStatistics() {
    let morningTotal = 0;
    let eveningTotal = 0;
    const uniqueEmployees = new Set();

    Object.keys(scheduleModalData).forEach(dateKey => {
        const morning = scheduleModalData[dateKey]?.morning || [];
        const evening = scheduleModalData[dateKey]?.evening || [];

        morningTotal += morning.length;
        eveningTotal += evening.length;

        morning.forEach(emp => uniqueEmployees.add(emp.maNV));
        evening.forEach(emp => uniqueEmployees.add(emp.maNV));
    });

    // Update counts
    const morningCount = document.getElementById('morningCount');
    const eveningCount = document.getElementById('eveningCount');
    const totalEmployees = document.getElementById('totalEmployees');
    const totalShifts = document.getElementById('totalShifts');

    if (morningCount) morningCount.textContent = morningTotal;
    if (eveningCount) eveningCount.textContent = eveningTotal;
    if (totalEmployees) totalEmployees.textContent = uniqueEmployees.size;
    if (totalShifts) totalShifts.textContent = morningTotal + eveningTotal;
}

function isDateToday(date) {
    const today = new Date();
    return date.getDate() === today.getDate() &&
        date.getMonth() === today.getMonth() &&
        date.getFullYear() === today.getFullYear();
}

function formatDateForAPI(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function formatDateForInput(date) {
    return formatDateForAPI(date);
}

function getInitials(name) {
    if (!name) return '?';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
        return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
}

function showScheduleError(message) {
    const gridContent = document.getElementById('scheduleGridContent');
    if (gridContent) {
        gridContent.innerHTML = `
            <div class="error-state">
                <div class="error-icon">⚠️</div>
                <p>${message}</p>
                <button class="btn-retry" onclick="loadWeekScheduleData()">
                    🔄 Thử lại
                </button>
            </div>
        `;
    }
}

function exportScheduleToExcel() {
    alert('⚠️ Chức năng xuất Excel đang được phát triển');
}

// ===========================
// GLOBAL EXPORTS
// ===========================
window.openScheduleModal = openScheduleModal;
window.closeScheduleModal = closeScheduleModal;
window.changeScheduleWeek = changeScheduleWeek;
window.openDayScheduleDetail = openDayScheduleDetail;
window.openAddScheduleForm = openAddScheduleForm;
window.submitAddSchedule = submitAddSchedule;
window.removeEmployeeFromSchedule = removeEmployeeFromSchedule;
window.filterScheduleEmployees = filterScheduleEmployees;
window.exportScheduleToExcel = exportScheduleToExcel;