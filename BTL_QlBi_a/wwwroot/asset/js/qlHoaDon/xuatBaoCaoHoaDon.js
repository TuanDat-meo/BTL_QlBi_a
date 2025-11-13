function exportInvoices() {
    alert('Xuất báo cáo hóa đơn');
    // Lấy giá trị lọc hiện tại
    const status = document.querySelector('.filter-btn[data-status].active')?.dataset.status || 'all';
    const dateFrom = document.getElementById('dateFrom').value;
    const dateTo = document.getElementById('dateTo').value;
    const search = document.getElementById('searchInvoices').value;

    const url = new URL('/HoaDon/ExportToExcel', window.location.origin);

    url.searchParams.append('status', status);
    if (dateFrom) url.searchParams.append('dateFrom', dateFrom);
    if (dateTo) url.searchParams.append('dateTo', dateTo);
    if (search) url.searchParams.append('search', search);

    window.location.href = url.toString();
}