// Common utilities và shared functions

// API Helper
const API = {
    async post(url, data) {
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: { 
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify(data)
            });
            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },

    async get(url) {
        try {
            const response = await fetch(url);
            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }
};

// Toast Notification
const Toast = {
    show(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 20px;
            background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#17a2b8'};
            color: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.2);
            z-index: 10000;
            animation: slideIn 0.3s ease;
        `;
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    },

    success(message) {
        this.show(message, 'success');
    },

    error(message) {
        this.show(message, 'error');
    },

    info(message) {
        this.show(message, 'info');
    }
};

// Add CSS for toast animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Format date
function formatDate(date, format = 'dd/MM/yyyy HH:mm') {
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    
    return format
        .replace('dd', day)
        .replace('MM', month)
        .replace('yyyy', year)
        .replace('HH', hours)
        .replace('mm', minutes);
}

// Confirm dialog
function confirmDialog(message, onConfirm) {
    const content = `
        <div style="padding: 20px;">
            <p style="font-size: 16px; margin-bottom: 20px;">${message}</p>
            <div style="display: flex; gap: 10px; justify-content: flex-end;">
                <button class="btn btn-secondary" onclick="closeModal()">Hủy</button>
                <button class="btn btn-primary" onclick="confirmAction()">Xác nhận</button>
            </div>
        </div>
    `;
    
    window.confirmAction = function() {
        onConfirm();
        closeModal();
    };
    
    openModal('Xác nhận', content);
}

// Loading overlay
const Loading = {
    show() {
        let overlay = document.getElementById('loadingOverlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'loadingOverlay';
            overlay.innerHTML = '<div class="spinner"></div>';
            overlay.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0,0,0,0.5);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 9999;
            `;
            document.body.appendChild(overlay);
        }
        overlay.style.display = 'flex';
    },

    hide() {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.style.display = 'none';
        }
    }
};

// Add spinner CSS
const spinnerStyle = document.createElement('style');
spinnerStyle.textContent = `
    .spinner {
        border: 4px solid #f3f3f3;
        border-top: 4px solid #3498db;
        border-radius: 50%;
        width: 40px;
        height: 40px;
        animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
    }
`;
document.head.appendChild(spinnerStyle);

async function saveCustomerForm(event) {
    event.preventDefault(); // Ngăn form submit theo cách cũ

    const form = document.getElementById('customerForm');
    const formData = new FormData(form);

    try {
        const response = await fetch('/Home/LuuKhachHang', {
            method: 'POST',
            body: formData,
            // Header AntiForgeryToken (nếu bạn dùng .NET 6+ thì ko cần)
            // headers: {
            //     'RequestVerificationToken': formData.get('__RequestVerificationToken')
            // }
        });

        const result = await response.json();

        if (result.success) {
            alert('Lưu thành công!');
            closeModal(); // Đóng modal
            location.reload(); // TẢI LẠI TRANG để cập nhật danh sách
        } else {
            alert('Đã xảy ra lỗi: ' + result.message);
        }
    } catch (error) {
        console.error('Lỗi khi submit form:', error);
        alert('Lỗi kết nối. Không thể lưu.');
    }
}


// Export to window for global access
window.API = API;
window.Toast = Toast;
window.formatCurrency = formatCurrency;
window.formatDate = formatDate;
window.confirmDialog = confirmDialog;
window.Loading = Loading;

console.log('✅ Common utilities loaded');