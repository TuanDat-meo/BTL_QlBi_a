async function saveCustomerForm(event) {
    event.preventDefault(); // Ngăn form submit theo cách cũ

    const form = document.getElementById('customerForm');
    const formData = new FormData(form);

    try {
        const response = await fetch('/KhachHang/LuuKhachHang', {
            method: 'POST',
            body: formData,
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
function openAddCustomerModal() {
    // alert('Mở form thêm khách hàng mới');
    openCustomerModal(0);

}

function editCustomer(maKH) {
    // alert('Sửa thông tin khách hàng: ' + maKH);
    openCustomerModal(maKH);
}

async function openCustomerModal(maKH) {
    let url = '/KhachHang/FormKhachHang';
    let title = '';

    if (maKH > 0) {
        url += '?maKH=' + maKH;
        title = 'Sửa thông tin khách hàng';
    } else {
        title = 'Thêm khách hàng mới';
    }

    try {
        // Hiển thị loading (tạm thời)
        // (Chúng ta gọi hàm 'openModal' có sẵn của bạn)
        openModal(title, '<div class="empty-text">Đang tải...</div>');

        // 1. Gọi Controller để lấy HTML của form
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error('Tải form thất bại');
        }
        const formHtml = await response.text();

        // 2. Lấy được HTML rồi, gọi lại hàm openModal
        //    với nội dung là cái form
        openModal(title, formHtml);

    } catch (error) {
        console.error('Lỗi khi mở modal:', error);
        // Báo lỗi cũng dùng hàm openModal
        openModal('Lỗi', '<div class="empty-state">Không thể tải form.</div>');
    }
}
function previewImage(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById('avatarPreview').src = e.target.result;
        }
        reader.readAsDataURL(input.files[0]);
    }
}
