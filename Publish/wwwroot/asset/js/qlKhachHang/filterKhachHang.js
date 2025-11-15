function filterCustomers(rank) {
    const cards = document.querySelectorAll('#customersGrid .table-card');
    const buttons = document.querySelectorAll('.filter-btn[data-filter]');

    buttons.forEach(btn => btn.classList.remove('active'));
    document.querySelector('.filter-btn[data-filter="' + rank + '"]')?.classList.add('active');

    const searchValue = document.getElementById('searchCustomers').value.toLowerCase();

    cards.forEach(card => {
        const cardRank = card.dataset.rank;
        const customerInfo = card.textContent.toLowerCase();

        const rankMatch = (rank === 'all' || cardRank === rank);
        const searchMatch = (searchValue === '' || customerInfo.includes(searchValue));

        card.style.display = (rankMatch && searchMatch) ? 'block' : 'none';
    });
    updateCustomerSummary();
}

function searchCustomers() {
    filterCustomers(document.querySelector('.filter-btn[data-filter].active')?.dataset.filter || 'all');
}

function updateCustomerSummary() {
    const cards = document.querySelectorAll('#customersGrid .table-card');

    let visibleCount = 0;
    let totalDiem = 0;
    let totalChiTieu = 0;

    cards.forEach(card => {
        if (card.style.display !== 'none') {
            visibleCount++;
            totalDiem += parseFloat(card.dataset.diem || 0);
            totalChiTieu += parseFloat(card.dataset.chitieu || 0);
        }
    });

    // Cập nhật HTML
    document.getElementById('summaryTongKhachHang').innerText = visibleCount + ' khách';
    document.getElementById('summaryTongDiem').innerText = totalDiem.toLocaleString('vi-VN') + ' điểm';
    document.getElementById('summaryTongChiTieu').innerText = totalChiTieu.toLocaleString('vi-VN') + ' đ';
}