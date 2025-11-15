// Biến timeout để tạo độ trễ
let searchTimeout;

function showLoading() {
    const container = document.querySelector('.loading-container');
    if (!container) return;

    if (container.querySelector('.loading-overlay')) return;

    const overlay = document.createElement('div');
    overlay.className = 'loading-overlay';

    overlay.innerHTML = '<div class="loader"></div>';

    container.appendChild(overlay);
}

function hideLoading() {
    const container = document.querySelector('.loading-container');
    if (!container) return;

    const overlay = container.querySelector('.loading-overlay');
    if (overlay) {
        // Hiệu ứng biến mất từ từ
        overlay.style.transition = 'opacity 0.2s';
        overlay.style.opacity = '0';
        setTimeout(() => overlay.remove(), 200);
    }
}