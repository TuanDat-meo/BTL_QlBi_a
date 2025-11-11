// Quản lý phân quyền - asset/js/qlNhanVien/permission.js

let currentPermissions = {};

async function openPermissionModal() {
    if (!window.canManage) {
        alert('Bạn không có quyền quản lý phân quyền');
        return;
    }

    try {
        const response = await fetch('/NhanVien/GetPermissions');
        if (!response.ok) throw new Error('Failed to load permissions');

        const roles = await response.json();

        // Store current permissions
        roles.forEach(role => {
            currentPermissions[role.maNhom] = role.phanQuyens.map(p => p.maCN);
        });

        let html = '<div class="permission-modal"><div class="permission-roles">';

        roles.forEach(role => {
            const isAdmin = role.tenNhom === 'Admin';
            const permissions = role.phanQuyens || [];

            html += `
                <div class="role-card">
                    <div class="role-header">
                        <span class="role-name">${role.tenNhom}</span>
                        ${isAdmin ?
                    '<span style="font-size: 12px; color: #6c757d;">Toàn quyền</span>' :
                    `<button class="btn btn-sm btn-primary" onclick="saveRolePermissions(${role.maNhom})">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path>
                                </svg>
                                Lưu
                            </button>`
                }
                    </div>
                    ${isAdmin ?
                    '<p style="font-size: 13px; color: #6c757d; padding: 10px;">Admin có quyền truy cập tất cả chức năng</p>' :
                    `<div class="permission-list" id="permissions-${role.maNhom}">
                            ${generatePermissionCheckboxes(role.maNhom, permissions)}
                        </div>`
                }
                </div>
            `;
        });

        html += `
            </div>
            <div class="action-buttons" style="margin-top: 20px;">
                <button class="btn btn-secondary" onclick="closeModal()">Đóng</button>
            </div>
        </div>`;

        openModal('Quản lý phân quyền', html);
    } catch (error) {
        console.error('Error loading permissions:', error);
        alert('Không thể tải danh sách phân quyền');
    }
}

function generatePermissionCheckboxes(roleId, currentPerms) {
    // Define all available permissions
    const allPermissions = [
        { id: 'VIEW_TABLE', name: 'Xem bàn', group: 'Quản lý bàn' },
        { id: 'MANAGE_TABLE', name: 'Quản lý bàn (CRUD)', group: 'Quản lý bàn' },
        { id: 'VIEW_ORDER', name: 'Xem đơn hàng', group: 'Đơn hàng' },
        { id: 'MANAGE_ORDER', name: 'Quản lý đơn hàng', group: 'Đơn hàng' },
        { id: 'VIEW_CUSTOMER', name: 'Xem khách hàng', group: 'Khách hàng' },
        { id: 'MANAGE_CUSTOMER', name: 'Quản lý khách hàng', group: 'Khách hàng' },
        { id: 'VIEW_EMPLOYEE', name: 'Xem nhân viên', group: 'Nhân viên' },
        { id: 'MANAGE_EMPLOYEE', name: 'Quản lý nhân viên', group: 'Nhân viên' },
        { id: 'ATTENDANCE', name: 'Chấm công', group: 'Nhân viên' },
        { id: 'VIEW_SALARY', name: 'Xem lương', group: 'Nhân viên' },
        { id: 'MANAGE_SALARY', name: 'Quản lý lương', group: 'Nhân viên' },
        { id: 'VIEW_PRODUCT', name: 'Xem dịch vụ', group: 'Dịch vụ' },
        { id: 'MANAGE_PRODUCT', name: 'Quản lý dịch vụ', group: 'Dịch vụ' },
        { id: 'VIEW_INVENTORY', name: 'Xem kho', group: 'Kho hàng' },
        { id: 'MANAGE_INVENTORY', name: 'Quản lý kho', group: 'Kho hàng' },
        { id: 'VIEW_REPORT', name: 'Xem báo cáo', group: 'Báo cáo' },
        { id: 'MANAGE_REPORT', name: 'Xuất báo cáo', group: 'Báo cáo' },
        { id: 'PAYMENT', name: 'Thanh toán', group: 'Thanh toán' },
        { id: 'MANAGE_SETTINGS', name: 'Cài đặt hệ thống', group: 'Hệ thống' }
    ];

    const currentPermIds = currentPerms.map(p => p.maCN);

    // Group permissions
    const groupedPermissions = {};
    allPermissions.forEach(perm => {
        if (!groupedPermissions[perm.group]) {
            groupedPermissions[perm.group] = [];
        }
        groupedPermissions[perm.group].push(perm);
    });

    let html = '';

    Object.keys(groupedPermissions).forEach(group => {
        html += `<div class="permission-group-section">
            <h5 class="permission-group-title">${group}</h5>
            <div class="permission-items">`;

        groupedPermissions[group].forEach(perm => {
            const checked = currentPermIds.includes(perm.id) ? 'checked' : '';
            html += `
                <div class="permission-checkbox">
                    <input type="checkbox" 
                           id="perm-${roleId}-${perm.id}" 
                           value="${perm.id}"
                           ${checked}
                           onchange="updatePermissionState(${roleId}, '${perm.id}', this.checked)">
                    <label for="perm-${roleId}-${perm.id}">${perm.name}</label>
                </div>
            `;
        });

        html += `</div></div>`;
    });

    return html;
}

function updatePermissionState(roleId, permId, checked) {
    if (!currentPermissions[roleId]) {
        currentPermissions[roleId] = [];
    }

    if (checked) {
        if (!currentPermissions[roleId].includes(permId)) {
            currentPermissions[roleId].push(permId);
        }
    } else {
        currentPermissions[roleId] = currentPermissions[roleId].filter(p => p !== permId);
    }
}

async function saveRolePermissions(roleId) {
    try {
        const permissions = currentPermissions[roleId] || [];

        const response = await fetch('/NhanVien/UpdatePermissions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                MaNhom: roleId,
                Permissions: permissions
            })
        });

        if (!response.ok) throw new Error('Failed to update permissions');

        const result = await response.json();
        alert(result.message || 'Cập nhật phân quyền thành công!');

        // Reload permissions
        openPermissionModal();
    } catch (error) {
        console.error('Error updating permissions:', error);
        alert('Không thể cập nhật phân quyền. Vui lòng thử lại.');
    }
}

// Add CSS for permission modal
const permissionStyle = document.createElement('style');
permissionStyle.textContent = `
    .permission-modal {
        max-width: 900px;
    }

    .permission-roles {
        display: grid;
        gap: 20px;
    }

    .role-card {
        background: #f8f9fa;
        border-radius: 12px;
        padding: 20px;
        border-left: 4px solid #667eea;
    }

    .role-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 15px;
        padding-bottom: 12px;
        border-bottom: 2px solid #e9ecef;
    }

    .role-name {
        font-size: 18px;
        font-weight: 700;
        color: #1a1a2e;
    }

    .permission-group-section {
        margin-bottom: 15px;
    }

    .permission-group-title {
        font-size: 13px;
        font-weight: 700;
        color: #667eea;
        margin-bottom: 10px;
        text-transform: uppercase;
        letter-spacing: 0.5px;
    }

    .permission-items {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
        gap: 10px;
        padding-left: 10px;
    }

    .permission-checkbox {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 8px 12px;
        background: white;
        border-radius: 6px;
        cursor: pointer;
        transition: all 0.3s;
    }

    .permission-checkbox:hover {
        background: #e8ecff;
        transform: translateX(2px);
    }

    .permission-checkbox input[type="checkbox"] {
        width: 18px;
        height: 18px;
        cursor: pointer;
        accent-color: #667eea;
    }

    .permission-checkbox label {
        font-size: 13px;
        cursor: pointer;
        user-select: none;
        margin: 0;
        flex: 1;
    }

    @media (max-width: 768px) {
        .permission-items {
            grid-template-columns: 1fr;
        }
    }
`;
document.head.appendChild(permissionStyle);