function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('collapsed');
    
    // Lưu trạng thái vào localStorage
    const isCollapsed = sidebar.classList.contains('collapsed');
    localStorage.setItem('sidebarCollapsed', isCollapsed);
}

function showPage(pageId) {
    // Ẩn tất cả các trang
    document.querySelectorAll('.page-section').forEach(section => {
        section.classList.remove('active');
    });
    
    // Hiện trang được chọn
    document.getElementById('page-' + pageId).classList.add('active');
    
    // Cập nhật trạng thái active cho nav-link
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });
    event.currentTarget.classList.add('active');
}

// Khôi phục trạng thái sidebar khi load trang
window.addEventListener('DOMContentLoaded', () => {
    const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
    if (isCollapsed) {
        document.getElementById('sidebar').classList.add('collapsed');
    }
});