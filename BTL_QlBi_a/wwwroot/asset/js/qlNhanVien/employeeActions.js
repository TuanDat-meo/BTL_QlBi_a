// wwroot/asset/js/qlNhanVien/employeeActions.js

async function openEditEmployeeModal(maNV) {
    try {
        const response = await fetch(`/NhanVien/GetEditForm/${maNV}`);
        if (!response.ok) throw new Error('Failed to load');

        const html = await response.text();
        openModal('Chỉnh sửa nhân viên', html);
    } catch (error) {
        console.error('Error:', error);
        showNotification('Không thể tải form chỉnh sửa', 'error');
    }
}

async function deactivateEmployee(maNV) {
    if (!confirm('Bạn có chắc muốn cho nhân viên này nghỉ việc?')) {
        return;
    }

    try {
        const response = await fetch(`/NhanVien/Deactivate/${maNV}`, {
            method: 'POST'
        });

        const result = await response.json();

        if (result.success) {
            showNotification(result.message || 'Đã chuyển sang trạng thái nghỉ việc', 'success');
            setTimeout(() => location.reload(), 1000);
        } else {
            showNotification(result.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showNotification('Không thể cập nhật trạng thái', 'error');
    }
}

async function activateEmployee(maNV) {
    if (!confirm('Bạn có chắc muốn kích hoạt lại nhân viên này?')) {
        return;
    }

    try {
        const response = await fetch(`/NhanVien/Activate/${maNV}`, {
            method: 'POST'
        });

        const result = await response.json();

        if (result.success) {
            showNotification(result.message || 'Đã kích hoạt lại nhân viên', 'success');
            setTimeout(() => location.reload(), 1000);
        } else {
            showNotification(result.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showNotification('Không thể cập nhật trạng thái', 'error');
    }
}

function openSalaryModal(maNV, tenNV) {
    const currentYear = new Date().getFullYear();

    openModal(`Quản lý lương - ${tenNV}`, `
        <div class="salary-management">
            <div class="salary-controls" style="margin-bottom: 20px;">
                <div class="form-row">
                    <div class="form-group" style="flex: 1;">
                        <label>Năm:</label>
                        <select id="salaryYear" class="form-control" onchange="loadSalaryData(${maNV})">
                            ${Array.from({ length: 5 }, (_, i) => currentYear - i).map(year =>
        `<option value="${year}">${year}</option>`
    ).join('')}
                        </select>
                    </div>
                    <div class="form-group" style="flex: 1;">
                        <label>Tháng:</label>
                        <select id="salaryMonth" class="form-control" onchange="loadSalaryData(${maNV})">
                            ${Array.from({ length: 12 }, (_, i) => i + 1).map(month =>
        `<option value="${month}" ${month === new Date().getMonth() + 1 ? 'selected' : ''}>Tháng ${month}</option>`
    ).join('')}
                        </select>
                    </div>
                </div>
                ${window.canManage ? `
                <button type="button" class="btn btn-primary btn-block" onclick="calculateSalary(${maNV})">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="12" y1="8" x2="12" y2="16"></line>
                        <line x1="8" y1="12" x2="16" y2="12"></line>
                    </svg>
                    Tính lương tự động
                </button>
                ` : ''}
            </div>

            <div id="salaryDetails" style="margin-top: 20px;">
                <p style="text-align: center; color: #6c757d; padding: 20px;">
                    Đang tải dữ liệu...
                </p>
            </div>

            <div id="salaryHistory" style="margin-top: 30px;">
                <h5>Lịch sử lương năm <span id="historyYear">${currentYear}</span></h5>
                <div id="historyList">
                    <!-- History will be loaded here -->
                </div>
            </div>
        </div>
    `);

    // Load initial data
    loadSalaryData(maNV);
}

async function loadSalaryData(maNV) {
    const year = parseInt(document.getElementById('salaryYear').value);
    const month = parseInt(document.getElementById('salaryMonth').value);

    try {
        // Load current month salary
        const response = await fetch(`/NhanVien/GetSalaryHistory/${maNV}?year=${year}`);
        if (!response.ok) throw new Error('Failed to load');

        const data = await response.json();
        const currentSalary = data.find(s => s.thang === month);

        const detailsDiv = document.getElementById('salaryDetails');

        if (currentSalary) {
            detailsDiv.innerHTML = `
                <div class="salary-card">
                    <div class="salary-label">Tổng lương tháng ${month}/${year}</div>
                    <div class="salary-amount">${currentSalary.tongLuong.toLocaleString('vi-VN')}đ</div>
                    <div class="salary-breakdown">
                        <div class="salary-item">
                            <div class="salary-item-label">Lương cơ bản</div>
                            <div class="salary-item-value">${currentSalary.luongCoBan.toLocaleString('vi-VN')}đ</div>
                        </div>
                        <div class="salary-item">
                            <div class="salary-item-label">Phụ cấp</div>
                            <div class="salary-item-value">${currentSalary.phuCap.toLocaleString('vi-VN')}đ</div>
                        </div>
                        <div class="salary-item">
                            <div class="salary-item-label">Thưởng</div>
                            <div class="salary-item-value">+${currentSalary.thuong.toLocaleString('vi-VN')}đ</div>
                        </div>
                        <div class="salary-item">
                            <div class="salary-item-label">Phạt</div>
                            <div class="salary-item-value">-${currentSalary.phat.toLocaleString('vi-VN')}đ</div>
                        </div>
                    </div>
                </div>
                <div style="margin-top: 15px; padding: 15px; background: #f8f9fa; border-radius: 8px;">
                    <p><strong>Tổng giờ làm:</strong> ${currentSalary.tongGio}h</p>
                    <p><strong>Ngày tính lương:</strong> ${new Date(currentSalary.ngayTinh).toLocaleDateString('vi-VN')}</p>
                </div>
            `;
        } else {
            detailsDiv.innerHTML = `
                <div style="text-align: center; padding: 30px; background: #fff3cd; border-radius: 8px;">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#856404" stroke-width="2" style="margin-bottom: 10px;">
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="12" y1="8" x2="12" y2="12"></line>
                        <line x1="12" y1="16" x2="12.01" y2="16"></line>
                    </svg>
                    <p style="margin: 0; color: #856404; font-weight: 600;">Chưa có dữ liệu lương tháng ${month}/${year}</p>
                    ${window.canManage ? '<p style="margin: 10px 0 0 0; color: #856404;">Nhấn "Tính lương tự động" để tính lương</p>' : ''}
                </div>
            `;
        }

        // Update history
        const historyDiv = document.getElementById('historyList');
        document.getElementById('historyYear').textContent = year;

        if (data.length > 0) {
            let historyHtml = '<div style="display: grid; gap: 10px;">';
            data.forEach(salary => {
                historyHtml += `
                    <div class="salary-history-item" style="padding: 12px; background: ${salary.thang === month ? '#e8f5e9' : '#f8f9fa'}; border-radius: 8px; border-left: 4px solid ${salary.thang === month ? '#28a745' : '#6c757d'};">
                        <div style="display: flex; justify-content: space-between; align-items: center;">
                            <div>
                                <strong>Tháng ${salary.thang}/${salary.nam}</strong>
                                <p style="margin: 5px 0 0 0; font-size: 12px; color: #6c757d;">
                                    ${salary.tongGio}h làm việc
                                </p>
                            </div>
                            <div style="text-align: right;">
                                <div style="font-size: 18px; font-weight: 700; color: #28a745;">
                                    ${salary.tongLuong.toLocaleString('vi-VN')}đ
                                </div>
                                <div style="font-size: 11px; color: #6c757d; margin-top: 2px;">
                                    ${new Date(salary.ngayTinh).toLocaleDateString('vi-VN')}
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            });
            historyHtml += '</div>';
            historyDiv.innerHTML = historyHtml;
        } else {
            historyDiv.innerHTML = '<p style="text-align: center; color: #6c757d; padding: 20px;">Chưa có lịch sử lương</p>';
        }

    } catch (error) {
        console.error('Error loading salary:', error);
        document.getElementById('salaryDetails').innerHTML =
            '<p style="text-align: center; color: #dc3545; padding: 20px;">Không thể tải dữ liệu lương</p>';
    }
}

async function calculateSalary(maNV) {
    const year = parseInt(document.getElementById('salaryYear').value);
    const month = parseInt(document.getElementById('salaryMonth').value);

    if (!confirm(`Tính lương tháng ${month}/${year}?\nHệ thống sẽ tự động tính dựa trên dữ liệu chấm công.`)) {
        return;
    }

    try {
        const response = await fetch('/NhanVien/CalculateSalary', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ maNV, month, year })
        });

        const result = await response.json();

        if (result.success) {
            showNotification('Tính lương thành công', 'success');
            loadSalaryData(maNV);
        } else {
            showNotification(result.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        console.error('Error calculating salary:', error);
        showNotification('Không thể tính lương', 'error');
    }
}

// Make functions globally available
window.openEditEmployeeModal = openEditEmployeeModal;
window.deactivateEmployee = deactivateEmployee;
window.activateEmployee = activateEmployee;
window.openSalaryModal = openSalaryModal;
window.loadSalaryData = loadSalaryData;
window.calculateSalary = calculateSalary;