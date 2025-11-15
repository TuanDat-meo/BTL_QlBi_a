function filterInvoices(status) {
    const rows = document.querySelectorAll('#invoiceTableBody tr');
    const buttons = document.querySelectorAll('.filter-btn[data-status]');

    buttons.forEach(btn => btn.classList.remove('active'));
    document.querySelector('.filter-btn[data-status="' + status + '"]')?.classList.add('active');

    rows.forEach(row => {
        const rowStatus = row.dataset.status;
        row.style.display = (status === 'all' || rowStatus === status) ? '' : 'none';
    });
    updateSummaryCard();
}

function filterByDate() {
    const dateFrom = document.getElementById('dateFrom').value;
    const dateTo = document.getElementById('dateTo').value;
    const rows = document.querySelectorAll('#invoiceTableBody tr');

    if (!dateFrom && !dateTo) return;

    rows.forEach(row => {
        const rowDate = row.dataset.date;
        let show = true;

        if (dateFrom && rowDate < dateFrom) show = false;
        if (dateTo && rowDate > dateTo) show = false;

        row.style.display = show ? '' : 'none';
    });

    updateSummaryCard();

}

function searchInvoices() {
    const searchValue = document.getElementById('searchInvoices').value.toLowerCase();
    const rows = document.querySelectorAll('#invoiceTableBody tr');

    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(searchValue) ? '' : 'none';
    });
    updateSummaryCard();

}

function updateSummaryCard() {
    const rows = document.querySelectorAll('#invoiceTableBody tr');

    let visibleCount = 0;
    let totalRevenue = 0;
    let paidCount = 0;

    // Lặp qua tất cả các hàng
    rows.forEach(row => {
        // Chỉ tính toán nếu hàng đang hiển thị
        if (row.style.display !== 'none') {
            visibleCount++;

            // Đọc giá trị tiền từ 'data-tongtien'
            totalRevenue += parseFloat(row.dataset.tongtien || 0);

            // Đọc trạng thái từ 'data-status'
            if (row.dataset.status === 'DaThanhToan') {
                paidCount++;
            }
        }
    });

    // Cập nhật HTML
    document.getElementById('summaryTongHoaDon').innerText = visibleCount + ' hóa đơn';
    document.getElementById('summaryTongDoanhThu').innerText = totalRevenue.toLocaleString('vi-VN') + ' đ';
    document.getElementById('summaryDaThanhToan').innerText = paidCount + ' hóa đơn';
}
document.addEventListener('DOMContentLoaded', function () {
    updateSummaryCard();
})