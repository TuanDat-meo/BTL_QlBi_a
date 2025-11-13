async function viewCustomer(maKH) {
    const detailPanel = document.getElementById('detailPanel');
    if (!detailPanel) {
        console.error('Không tìm thấy #detailPanel');
        return;
    }

    detailPanel.innerHTML = '<div class="empty-text">Đang tải chi tiết khách hàng...</div>';

    const rightPanel = document.querySelector('.right-panel');
    if (rightPanel) {
        rightPanel.classList.add('active');
    }

    try {
        const response = await fetch('/KhachHang/ChiTietKhachHang?maKH=' + maKH);

        if (!response.ok) {
            detailPanel.innerHTML = '<div class="empty-state">Lỗi khi tải dữ liệu khách hàng.</div>';
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // Đưa HTML vào panel
        const html = await response.text();
        detailPanel.innerHTML = html;

    } catch (error) {
        console.error('Lỗi fetch ChiTietKhachHang:', error);
        detailPanel.innerHTML = '<div class="empty-state">Không thể tải chi tiết.</div>';
    }
}

// DELETLE KHACH HANG
async function deleteCustomer(maKH) {
    if (!confirm('Bạn có chắc chắn muốn xóa khách hàng này không? Hành động này không thể hoàn tác.')) {
        return;
    }

    try {
        const formData = new FormData();
        formData.append('maKH', maKH);

        const response = await fetch('/KhachHang/XoaKhachHang', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            alert('Đã xóa thành công!');
            // closeRightPanel();
            window.location.reload();
        }
        else {
            alert('Không thể xóa: ' + result.message);
        }
    }
    catch (error) {
        console.error('Lỗi:', error);
        alert('Đã xảy ra lỗi khi xóa.');
    }

}

// KHOI PHUC. KHACH HANG`
async function restoreCustomer(maKH) {
    if (!confirm('Bạn có muốn khôi phục khách hàng này không?')) return;

    try {
        const formData = new FormData();
        formData.append('maKH', maKH);

        const response = await fetch('/KhachHang/KhoiPhucKhachHang', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            alert('Khôi phục thành công!');
            // closeRightPanel();
            window.location.reload(); // Tải lại để thấy khách hàng quay về danh sách chính
        } else {
            alert('Lỗi: ' + result.message);
        }
    } catch (error) {
        console.error(error);
        alert('Lỗi kết nối.');
    }
}