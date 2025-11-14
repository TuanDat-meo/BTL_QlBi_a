// Quản lý lương - asset/js/qlNhanVien/salary.js

// Load salary history
async function loadSalaryHistory(maNV) {
    const container = document.getElementById('salaryHistoryList');
    if (!container) return;

    try {
        container.innerHTML = '<div style="text-align: center; padding: 20px;">Đang tải...</div>';

        const response = await fetch(`/NhanVien/GetSalaryHistory/${maNV}`);
        if (!response.ok) throw new Error('Failed to load salary history');

        const salaries = await response.json();

        if (salaries.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <div class="empty-icon">💰</div>
                    <div class="empty-text">Chưa có dữ liệu lương</div>
                </div>
            `;
            return;
        }

        let html = '';
        salaries.forEach(salary => {
            html += `
                <div class="salary-history-item">
                    <div class="salary-month">
                        <span class="month-label">Tháng ${salary.thang}/${salary.nam}</span>
                        <span class="salary-date">${new Date(salary.ngayTinh).toLocaleDateString('vi-VN')}</span>
                    </div>
                    <div class="salary-breakdown-detail">
                        <div class="salary-row">
                            <span>Lương cơ bản:</span>
                            <span>${salary.luongCoBan.toLocaleString('vi-VN')}đ</span>
                        </div>
                        <div class="salary-row">
                            <span>Phụ cấp:</span>
                            <span>${salary.phuCap.toLocaleString('vi-VN')}đ</span>
                        </div>
                        <div class="salary-row">
                            <span>Thưởng:</span>
                            <span style="color: #28a745;">+${salary.thuong.toLocaleString('vi-VN')}đ</span>
                        </div>
                        <div class="salary-row">
                            <span>Phạt:</span>
                            <span style="color: #dc3545;">-${salary.phat.toLocaleString('vi-VN')}đ</span>
                        </div>
                        <div class="salary-row">
                            <span>Tổng giờ:</span>
                            <span>${salary.tongGio}h</span>
                        </div>
                        <div class="salary-row total">
                            <span>Tổng lương:</span>
                            <span>${salary.tongLuong.toLocaleString('vi-VN')}đ</span>
                        </div>
                    </div>
                </div>
            `;
        });

        container.innerHTML = html;

    } catch (error) {
        console.error('Error loading salary history:', error);
        container.innerHTML = `
            <div class="empty-state">
                <div class="empty-icon">⚠️</div>
                <div class="empty-text">Không thể tải dữ liệu lương</div>
            </div>
        `;
    }
}

// Calculate salary for current month
async function calculateSalary(maNV) {
    if (!window.canManage) {
        alert('Bạn không có quyền tính lương');
        return;
    }

    const now = new Date();
    const month = now.getMonth() + 1;
    const year = now.getFullYear();

    if (!confirm(`Tính lương tháng ${month}/${year} cho nhân viên này?`)) {
        return;
    }

    try {
        const response = await fetch('/NhanVien/CalculateSalary', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ MaNV: maNV, Month: month, Year: year })
        });

        if (!response.ok) throw new Error('Failed to calculate salary');

        const result = await response.json();
        alert('Tính lương thành công!');

        // Reload detail
        if (window.currentEmployeeId === maNV) {
            showEmployeeDetail(maNV);
        }
    } catch (error) {
        console.error('Error calculating salary:', error);
        alert('Không thể tính lương. Vui lòng thử lại.');
    }
}

// Open salary detail modal
function openSalaryDetailModal(maNV) {
    const content = `
        <div class="salary-detail-modal">
            <div class="form-group">
                <label class="form-label">Chọn năm:</label>
                <select class="form-control" id="salaryYear" onchange="loadSalaryHistory(${maNV})">
                    <option value="${new Date().getFullYear()}">${new Date().getFullYear()}</option>
                    <option value="${new Date().getFullYear() - 1}">${new Date().getFullYear() - 1}</option>
                    <option value="${new Date().getFullYear() - 2}">${new Date().getFullYear() - 2}</option>
                </select>
            </div>

            <div id="salaryHistoryList" class="salary-history-list">
                <div style="text-align: center; padding: 20px;">Đang tải...</div>
            </div>

            <div class="action-buttons" style="margin-top: 20px;">
                ${window.canManage ? `
                    <button class="btn btn-primary" onclick="calculateSalary(${maNV})">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <line x1="12" y1="1" x2="12" y2="23"></line>
                            <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"></path>
                        </svg>
                        Tính lương tháng này
                    </button>
                ` : ''}
                <button class="btn btn-secondary" onclick="closeModal()">Đóng</button>
            </div>
        </div>
    `;

    openModal('Chi tiết lương', content);
    loadSalaryHistory(maNV);
}

// Add CSS styles
const style = document.createElement('style');
style.textContent = `
    .salary-history-list {
        max-height: 500px;
        overflow-y: auto;
    }

    .salary-history-item {
        background: #f8f9fa;
        border-radius: 10px;
        padding: 15px;
        margin-bottom: 12px;
        border-left: 4px solid #28a745;
    }

    .salary-month {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 12px;
        padding-bottom: 10px;
        border-bottom: 2px solid #e9ecef;
    }

    .month-label {
        font-weight: 700;
        font-size: 15px;
        color: #1a1a2e;
    }

    .salary-date {
        font-size: 12px;
        color: #6c757d;
    }

    .salary-breakdown-detail {
        display: flex;
        flex-direction: column;
        gap: 8px;
    }

    .salary-row {
        display: flex;
        justify-content: space-between;
        font-size: 13px;
        padding: 4px 0;
    }

    .salary-row.total {
        margin-top: 8px;
        padding-top: 8px;
        border-top: 2px solid #e9ecef;
        font-weight: 700;
        font-size: 15px;
        color: #28a745;
    }
`;
document.head.appendChild(style);