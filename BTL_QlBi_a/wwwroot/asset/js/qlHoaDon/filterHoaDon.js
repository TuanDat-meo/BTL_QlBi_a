let searchTimeout;

function triggerFilter() {
    // Gọi hàm từ file Loading.js
    if (typeof showLoading === 'function') {
        showLoading();
    }

    clearTimeout(searchTimeout);

    searchTimeout = setTimeout(() => {
        runFilterLogic(); // Chạy logic lọc thật sự

        // Tắt Loading
        if (typeof hideLoading === 'function') {
            hideLoading();
        }
    }, 400); // Độ trễ 0.4s để kịp thấy hiệu ứng
}
function runFilterLogic() {
    const rows = document.querySelectorAll('#invoiceTableBody tr');

    // --- Lấy giá trị hiện tại của các ô input ---
    const activeBtn = document.querySelector('.filter-btn[data-status].active');
    const currentStatus = activeBtn ? activeBtn.dataset.status : 'all';

    const dateFrom = document.getElementById('dateFrom').value;
    const dateTo = document.getElementById('dateTo').value;
    const searchValue = document.getElementById('searchInvoices').value.toLowerCase();

    // --- Duyệt qua từng dòng ---
    rows.forEach(row => {
        const rowStatus = row.dataset.status;
        const rowDate = row.dataset.date;
        const rowText = row.textContent.toLowerCase();

        // 1. Kiểm tra Trạng thái
        const matchStatus = (currentStatus === 'all' || rowStatus === currentStatus);

        // 2. Kiểm tra Ngày
        let matchDate = true;
        if (dateFrom && rowDate < dateFrom) matchDate = false;
        if (dateTo && rowDate > dateTo) matchDate = false;

        // 3. Kiểm tra Tìm kiếm
        const matchSearch = (searchValue === '' || rowText.includes(searchValue));

        // KẾT QUẢ: Phải thỏa mãn CẢ 3 điều kiện
        if (matchStatus && matchDate && matchSearch) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });

    updateSummaryCard();
}
function filterInvoices(status) {
    // Cập nhật nút active
    const buttons = document.querySelectorAll('.filter-btn[data-status]');
    buttons.forEach(btn => btn.classList.remove('active'));
    document.querySelector('.filter-btn[data-status="' + status + '"]')?.classList.add('active');

    // Gọi bộ lọc
    triggerFilter();
}

function filterByDate() {
    triggerFilter();
}

function searchInvoices() {
    triggerFilter();
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