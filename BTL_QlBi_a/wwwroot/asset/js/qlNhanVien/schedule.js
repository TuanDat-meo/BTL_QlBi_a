// Quản lý lịch làm việc
let currentDate = new Date();
let scheduleData = {};

function openScheduleModal(maNV) {
    const content = `
        <div class="schedule-modal">
            <div class="calendar-header">
                <button class="btn btn-secondary" onclick="schedulePreviousMonth()">‹ Tháng trước</button>
                <h4 id="scheduleMonth">${getMonthYearText(currentDate)}</h4>
                <button class="btn btn-secondary" onclick="scheduleNextMonth()">Tháng sau ›</button>
            </div>

            <div class="calendar-grid" id="scheduleCalendar">
                ${generateScheduleCalendar(maNV)}
            </div>

            <div class="info-card" style="margin-top: 20px;">
                <h4 style="margin-bottom: 10px;">Hướng dẫn:</h4>
                <ul style="font-size: 12px; color: #6c757d;">
                    <li>Click vào ngày để chọn/bỏ chọn ca làm việc</li>
                    <li>Chỉ có thể thay đổi lịch ít nhất 1 ngày trước</li>
                    <li>Ngày màu vàng: Chưa xếp lịch</li>
                    <li>Ngày màu xanh: Đã có lịch</li>
                </ul>
            </div>

            <div class="form-group" style="margin-top: 15px;">
                <label class="form-label">Ca làm việc mặc định cho các ngày đã chọn:</label>
                <select class="form-control" id="defaultShift">
                    <option value="Sáng">Sáng (7h - 15h)</option>
                    <option value="Chiều">Chiều (15h - 23h)</option>
                    <option value="Tối">Tối (19h - 3h)</option>
                </select>
            </div>

            <div class="action-buttons" style="margin-top: 20px;">
                <button class="btn btn-primary" onclick="saveSchedule(${maNV})">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path>
                    </svg>
                    Lưu lịch
                </button>
                <button class="btn btn-secondary" onclick="closeModal()">Đóng</button>
            </div>
        </div>
    `;

    openModal('Xếp lịch làm việc', content);
    loadScheduleData(maNV);
}

function getMonthYearText(date) {
    return `Tháng ${date.getMonth() + 1}/${date.getFullYear()}`;
}

function generateScheduleCalendar(maNV) {
    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();

    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startDayOfWeek = firstDay.getDay();

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    let html = `
        <div class="calendar-day-header">CN</div>
        <div class="calendar-day-header">T2</div>
        <div class="calendar-day-header">T3</div>
        <div class="calendar-day-header">T4</div>
        <div class="calendar-day-header">T5</div>
        <div class="calendar-day-header">T6</div>
        <div class="calendar-day-header">T7</div>
    `;

    // Empty cells before first day
    for (let i = 0; i < startDayOfWeek; i++) {
        html += '<div class="calendar-day other-month"></div>';
    }

    // Days of month
    for (let day = 1; day <= daysInMonth; day++) {
        const date = new Date(year, month, day);
        date.setHours(0, 0, 0, 0);

        const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
        const isToday = date.getTime() === today.getTime();
        const isPast = date < today;
        const canEdit = !isPast && date.getTime() !== today.getTime();

        let classes = ['calendar-day'];
        if (isToday) classes.push('today');
        if (!canEdit) classes.push('disabled');

        html += `
            <div class="${classes.join(' ')}" 
                 data-date="${dateStr}" 
                 ${canEdit ? `onclick="toggleScheduleDay('${dateStr}')"` : ''}
                 style="${!canEdit ? 'cursor: not-allowed; opacity: 0.5;' : ''}">
                <div>${day}</div>
                <div class="shift-indicator" id="shift-${dateStr}"></div>
            </div>
        `;
    }

    return html;
}

async function loadScheduleData(maNV) {
    try {
        const year = currentDate.getFullYear();
        const month = currentDate.getMonth() + 1;

        const response = await fetch(`/NhanVien/GetSchedule/${maNV}?year=${year}&month=${month}`);
        if (!response.ok) throw new Error('Failed to load schedule');

        scheduleData = await response.json();

        // Update calendar with schedule data
        Object.keys(scheduleData).forEach(dateStr => {
            const shift = scheduleData[dateStr];
            const indicator = document.getElementById(`shift-${dateStr}`);
            const dayEl = document.querySelector(`[data-date="${dateStr}"]`);

            if (indicator && shift) {
                indicator.textContent = shift;
                indicator.className = 'shift-indicator ' +
                    (shift === 'Sáng' ? 'morning' : shift === 'Chiều' ? 'afternoon' : 'evening');
                dayEl?.classList.add('has-shift');
            } else if (dayEl && !dayEl.classList.contains('disabled')) {
                dayEl.classList.add('no-shift');
            }
        });
    } catch (error) {
        console.error('Error loading schedule:', error);
    }
}

function toggleScheduleDay(dateStr) {
    const dayEl = document.querySelector(`[data-date="${dateStr}"]`);
    const indicator = document.getElementById(`shift-${dateStr}`);

    if (!dayEl || !indicator) return;

    if (scheduleData[dateStr]) {
        // Remove schedule
        delete scheduleData[dateStr];
        indicator.textContent = '';
        indicator.className = 'shift-indicator';
        dayEl.classList.remove('has-shift');
        dayEl.classList.add('no-shift');
    } else {
        // Add schedule with default shift
        const defaultShift = document.getElementById('defaultShift')?.value || 'Sáng';
        scheduleData[dateStr] = defaultShift;
        indicator.textContent = defaultShift;
        indicator.className = 'shift-indicator ' +
            (defaultShift === 'Sáng' ? 'morning' : defaultShift === 'Chiều' ? 'afternoon' : 'evening');
        dayEl.classList.add('has-shift');
        dayEl.classList.remove('no-shift');
    }
}

async function saveSchedule(maNV) {
    try {
        const response = await fetch('/NhanVien/SaveSchedule', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                MaNV: maNV,
                Schedule: scheduleData
            })
        });

        if (!response.ok) throw new Error('Failed to save schedule');

        alert('Lưu lịch làm việc thành công!');
        closeModal();

        // Reload schedule calendar if on that tab
        if (document.getElementById('tab-schedule')?.classList.contains('active')) {
            loadScheduleCalendar(maNV);
        }
    } catch (error) {
        console.error('Error saving schedule:', error);
        alert('Không thể lưu lịch. Vui lòng thử lại.');
    }
}

function schedulePreviousMonth() {
    currentDate.setMonth(currentDate.getMonth() - 1);
    document.getElementById('scheduleMonth').textContent = getMonthYearText(currentDate);
    document.getElementById('scheduleCalendar').innerHTML = generateScheduleCalendar(window.currentEmployeeId);
    loadScheduleData(window.currentEmployeeId);
}

function scheduleNextMonth() {
    currentDate.setMonth(currentDate.getMonth() + 1);
    document.getElementById('scheduleMonth').textContent = getMonthYearText(currentDate);
    document.getElementById('scheduleCalendar').innerHTML = generateScheduleCalendar(window.currentEmployeeId);
    loadScheduleData(window.currentEmployeeId);
}

// Load schedule calendar in detail panel
async function loadScheduleCalendar(maNV) {
    const container = document.getElementById('calendarGrid');
    if (!container) return;

    try {
        const year = new Date().getFullYear();
        const month = new Date().getMonth() + 1;

        const response = await fetch(`/NhanVien/GetSchedule/${maNV}?year=${year}&month=${month}`);
        if (!response.ok) throw new Error('Failed to load schedule');

        const data = await response.json();

        // Generate calendar
        const firstDay = new Date(year, month - 1, 1);
        const lastDay = new Date(year, month, 0);
        const daysInMonth = lastDay.getDate();
        const startDayOfWeek = firstDay.getDay();

        let html = `
            <div class="calendar-day-header">CN</div>
            <div class="calendar-day-header">T2</div>
            <div class="calendar-day-header">T3</div>
            <div class="calendar-day-header">T4</div>
            <div class="calendar-day-header">T5</div>
            <div class="calendar-day-header">T6</div>
            <div class="calendar-day-header">T7</div>
        `;

        for (let i = 0; i < startDayOfWeek; i++) {
            html += '<div class="calendar-day other-month"></div>';
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const dateStr = `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const shift = data[dateStr];

            let classes = ['calendar-day'];
            if (shift) classes.push('has-shift');
            else classes.push('no-shift');

            html += `
                <div class="${classes.join(' ')}">
                    <div>${day}</div>
                    ${shift ? `<div class="shift-indicator ${shift === 'Sáng' ? 'morning' : shift === 'Chiều' ? 'afternoon' : 'evening'}">${shift}</div>` : ''}
                </div>
            `;
        }

        container.innerHTML = html;
    } catch (error) {
        console.error('Error loading schedule calendar:', error);
    }
}

function previousMonth() {
    currentDate.setMonth(currentDate.getMonth() - 1);
    document.getElementById('currentMonth').textContent = getMonthYearText(currentDate);
    loadScheduleCalendar(window.currentEmployeeId);
}

function nextMonth() {
    currentDate.setMonth(currentDate.getMonth() + 1);
    document.getElementById('currentMonth').textContent = getMonthYearText(currentDate);
    loadScheduleCalendar(window.currentEmployeeId);
}