// Quản lý nhân viên
class NhanVienManager {
    constructor() {
        this.currentEmployeeId = null;
        this.initializeFilters();
        this.initializeSearch();
    }

    initializeFilters() {
        // Filter by status
        document.querySelectorAll('.filter-btn[data-filter]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                // Update active state
                e.target.parentElement.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
                e.target.classList.add('active');

                this.filterEmployees();
            });
        });

        // Filter by role
        document.querySelectorAll('.filter-btn[data-role]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.target.parentElement.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
                e.target.classList.add('active');

                this.filterEmployees();
            });
        });
    }

    initializeSearch() {
        const searchInput = document.getElementById('employeeSearch');
        if (searchInput) {
            searchInput.addEventListener('input', () => this.filterEmployees());
        }
    }

    filterEmployees() {
        const statusFilter = document.querySelector('.filter-btn[data-filter].active')?.dataset.filter || 'all';
        const roleFilter = document.querySelector('.filter-btn[data-role].active')?.dataset.role || 'all';
        const searchTerm = document.getElementById('employeeSearch')?.value.toLowerCase() || '';

        const cards = document.querySelectorAll('.employee-card');
        let visibleCount = 0;

        cards.forEach(card => {
            const status = card.dataset.status;
            const role = card.dataset.role;
            const name = card.querySelector('.employee-name')?.textContent.toLowerCase() || '';

            const matchesStatus = statusFilter === 'all' || status === statusFilter;
            const matchesRole = roleFilter === 'all' || role === roleFilter;
            const matchesSearch = name.includes(searchTerm);

            if (matchesStatus && matchesRole && matchesSearch) {
                card.style.display = '';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        // Show/hide empty state
        const emptyState = document.getElementById('emptyState');
        if (emptyState) {
            emptyState.style.display = visibleCount === 0 ? 'block' : 'none';
        }
    }
}

// Show employee detail
async function showEmployeeDetail(maNV) {
    try {
        // Highlight selected card
        document.querySelectorAll('.employee-card').forEach(card => {
            card.classList.remove('active');
        });
        event.currentTarget.classList.add('active');

        // Load detail
        const response = await fetch(`/NhanVien/GetDetail/${maNV}`);
        if (!response.ok) throw new Error('Failed to load employee detail');

        const html = await response.text();
        const detailPanel = document.getElementById('detailPanel');
        if (detailPanel) {
            detailPanel.innerHTML = html;
        }

        // Store current employee ID
        window.currentEmployeeId = maNV;
    } catch (error) {
        console.error('Error loading employee detail:', error);
        alert('Không thể tải thông tin nhân viên. Vui lòng thử lại.');
    }
}

// Add employee
function openAddEmployeeModal() {
    if (!window.canManage) {
        alert('Bạn không có quyền thêm nhân viên');
        return;
    }

    const content = `
        <form id="addEmployeeForm" onsubmit="submitAddEmployee(event)">
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Họ tên *</label>
                    <input type="text" class="form-control" name="TenNV" required>
                </div>
                <div class="form-group">
                    <label class="form-label">Số điện thoại</label>
                    <input type="tel" class="form-control" name="SDT" pattern="[0-9]{10}">
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Chức vụ *</label>
                    <select class="form-control" name="MaNhom" required>
                        <option value="">-- Chọn chức vụ --</option>
                        <option value="1">Admin</option>
                        <option value="2">Quản lý</option>
                        <option value="3">Thu ngân</option>
                        <option value="4">Phục vụ</option>
                    </select>
                </div>
                <div class="form-group">
                    <label class="form-label">Ca làm việc *</label>
                    <select class="form-control" name="CaMacDinh" required>
                        <option value="Sáng">Sáng (7h - 15h)</option>
                        <option value="Chiều">Chiều (15h - 23h)</option>
                        <option value="Tối">Tối (19h - 3h)</option>
                    </select>
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Lương cơ bản *</label>
                    <input type="number" class="form-control" name="LuongCoBan" value="0" required>
                </div>
                <div class="form-group">
                    <label class="form-label">Phụ cấp</label>
                    <input type="number" class="form-control" name="PhuCap" value="0">
                </div>
            </div>
            <div class="form-group">
                <label class="form-label">Mật khẩu *</label>
                <input type="password" class="form-control" name="MatKhau" required minlength="6">
            </div>
            <div class="action-buttons">
                <button type="submit" class="btn btn-primary">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path>
                        <polyline points="17 21 17 13 7 13 7 21"></polyline>
                        <polyline points="7 3 7 8 15 8"></polyline>
                    </svg>
                    Lưu
                </button>
                <button type="button" class="btn btn-secondary" onclick="closeModal()">Hủy</button>
            </div>
        </form>
    `;

    openModal('Thêm nhân viên mới', content);
}

// Submit add employee
async function submitAddEmployee(event) {
    event.preventDefault();

    const formData = new FormData(event.target);
    const data = Object.fromEntries(formData.entries());

    try {
        const response = await fetch('/NhanVien/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (!response.ok) throw new Error('Failed to create employee');

        alert('Thêm nhân viên thành công!');
        closeModal();
        location.reload();
    } catch (error) {
        console.error('Error creating employee:', error);
        alert('Không thể thêm nhân viên. Vui lòng thử lại.');
    }
}

// Edit employee
async function openEditEmployeeModal(maNV) {
    if (!window.canManage) {
        alert('Bạn không có quyền sửa thông tin nhân viên');
        return;
    }

    try {
        const response = await fetch(`/NhanVien/GetEditForm/${maNV}`);
        if (!response.ok) throw new Error('Failed to load edit form');

        const html = await response.text();
        openModal('Sửa thông tin nhân viên', html);
    } catch (error) {
        console.error('Error loading edit form:', error);
        alert('Không thể tải form sửa. Vui lòng thử lại.');
    }
}

// Deactivate employee
async function deactivateEmployee(maNV) {
    if (!window.canManage) {
        alert('Bạn không có quyền thay đổi trạng thái nhân viên');
        return;
    }

    if (!confirm('Bạn có chắc muốn cho nhân viên này nghỉ việc?')) return;

    try {
        const response = await fetch(`/NhanVien/Deactivate/${maNV}`, {
            method: 'POST'
        });

        if (!response.ok) throw new Error('Failed to deactivate employee');

        alert('Đã chuyển trạng thái sang nghỉ việc');
        location.reload();
    } catch (error) {
        console.error('Error deactivating employee:', error);
        alert('Không thể cập nhật trạng thái. Vui lòng thử lại.');
    }
}

// Activate employee
async function activateEmployee(maNV) {
    if (!window.canManage) {
        alert('Bạn không có quyền thay đổi trạng thái nhân viên');
        return;
    }

    if (!confirm('Bạn có chắc muốn cho nhân viên này làm lại?')) return;

    try {
        const response = await fetch(`/NhanVien/Activate/${maNV}`, {
            method: 'POST'
        });

        if (!response.ok) throw new Error('Failed to activate employee');

        alert('Đã chuyển trạng thái sang đang làm');
        location.reload();
    } catch (error) {
        console.error('Error activating employee:', error);
        alert('Không thể cập nhật trạng thái. Vui lòng thử lại.');
    }
}

// Export attendance report
async function exportAttendanceReport() {
    try {
        const month = prompt('Nhập tháng cần xuất báo cáo (MM/YYYY):',
            new Date().getMonth() + 1 + '/' + new Date().getFullYear());

        if (!month) return;

        window.location.href = `/NhanVien/ExportAttendanceReport?month=${month}`;
    } catch (error) {
        console.error('Error exporting report:', error);
        alert('Không thể xuất báo cáo. Vui lòng thử lại.');
    }
}

// Permission management
function openPermissionModal() {
    if (!window.canManage) {
        alert('Bạn không có quyền quản lý phân quyền');
        return;
    }

    const content = `
        <div class="permission-grid">
            <div class="permission-item">
                <span class="permission-label">Admin</span>
                <span>Toàn quyền</span>
            </div>
            <div class="permission-item">
                <span class="permission-label">Quản lý</span>
                <button class="btn btn-secondary btn-sm" onclick="editRolePermissions(2)">Chỉnh sửa</button>
            </div>
            <div class="permission-item">
                <span class="permission-label">Thu ngân</span>
                <button class="btn btn-secondary btn-sm" onclick="editRolePermissions(3)">Chỉnh sửa</button>
            </div>
            <div class="permission-item">
                <span class="permission-label">Phục vụ</span>
                <button class="btn btn-secondary btn-sm" onclick="editRolePermissions(4)">Chỉnh sửa</button>
            </div>
        </div>
        <div class="action-buttons" style="margin-top: 20px;">
            <button class="btn btn-secondary" onclick="closeModal()">Đóng</button>
        </div>
    `;

    openModal('Quản lý phân quyền', content);
}

function editRolePermissions(roleId) {
    // TODO: Implement permission editor
    alert('Chức năng đang phát triển');
}