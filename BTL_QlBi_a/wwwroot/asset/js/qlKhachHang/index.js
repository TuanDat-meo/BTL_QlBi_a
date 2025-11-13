
// Xuat Bao Cao Excel
function exportCustomers() {

    const rank = document.querySelector('.filter-btn[data-filter].active')?.dataset.filter || 'all';
    const search = document.getElementById('searchCustomers').value;

    const url = new URL('/KhachHang/ExportKhachHangToExcel', window.location.origin);

    url.searchParams.append('rank', rank);
    if (search)
        url.searchParams.append('search', search);

    window.location.href = url.toString();
}