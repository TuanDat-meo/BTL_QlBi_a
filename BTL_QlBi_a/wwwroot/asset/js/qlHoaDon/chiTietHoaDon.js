async function viewInvoice(maHD) {
    const detailPanel = document.getElementById('detailPanel');
    if (!detailPanel) {
        console.error('Không tìm thấy #detailPanel');
        return;
    }

    detailPanel.innerHTML = '<div class="empty-text">Đang tải chi tiết...</div>';

    const rightPanel = document.querySelector('.right-panel');
    if (rightPanel) {
        rightPanel.classList.add('active');
    }

    try {
        const response = await fetch('/HoaDon/ChiTietHoaDon?maHD=' + maHD); // <-- ĐÃ SỬA

        if (!response.ok) {
            detailPanel.innerHTML = '<div class="empty-state">Lỗi khi tải dữ liệu.</div>';
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const html = await response.text();
        detailPanel.innerHTML = html;

    } catch (error) {
        console.error('Lỗi fetch ChiTietHoaDon:', error);
        detailPanel.innerHTML = '<div class="empty-state">Không thể tải chi tiết.</div>';
    }
}

function printInvoice(maHD) {
    window.open('/HoaDon/InHoaDon?maHD=' + maHD, '_blank');
}