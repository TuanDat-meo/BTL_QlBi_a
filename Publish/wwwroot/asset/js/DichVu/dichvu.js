// DichVuManager - Quản lý dịch vụ (Isolated version)
const DichVuManager = {
    isEditMode: false,
    currentMaDV: null,

    // Mở modal thêm mới
    openAddModal() {
        this.isEditMode = false;
        this.currentMaDV = null;

        document.getElementById('modalTitle').textContent = '➕ Thêm dịch vụ mới';
        document.getElementById('btnSubmitText').textContent = 'Thêm dịch vụ';
        document.getElementById('formDichVu').reset();
        document.getElementById('maDV').value = '';

        this.removeImage();

        const modal = document.getElementById('addServiceModal');
        modal.classList.add('show');
        modal.style.display = 'flex';

        // Ngăn scroll body
        document.body.style.overflow = 'hidden';
    },

    // Mở modal chỉnh sửa
    async openEditModal(maDV) {
        this.isEditMode = true;
        this.currentMaDV = maDV;

        document.getElementById('modalTitle').textContent = '✏️ Chỉnh sửa dịch vụ';
        document.getElementById('btnSubmitText').textContent = 'Cập nhật';

        try {
            const response = await fetch(`/DichVu/LayThongTin?maDV=${maDV}`);
            const result = await response.json();

            if (result.success) {
                const data = result.data;
                document.getElementById('maDV').value = data.maDV;
                document.getElementById('tenDV').value = data.tenDV;
                document.getElementById('loai').value = data.loai;
                document.getElementById('gia').value = data.gia;
                document.getElementById('donVi').value = data.donVi;
                document.getElementById('trangThai').value = data.trangThai;
                document.getElementById('moTa').value = data.moTa || '';

                if (data.maHang) {
                    document.getElementById('maHang').value = data.maHang;
                }

                // Hiển thị ảnh nếu có
                if (data.hinhAnh) {
                    const previewImg = document.getElementById('previewImg');
                    let imagePath = data.hinhAnh;

                    // Xử lý đường dẫn ảnh
                    if (imagePath.startsWith('/')) {
                        imagePath = imagePath.substring(1);
                    }
                    if (imagePath.startsWith('img/')) {
                        imagePath = imagePath.substring(4);
                    }

                    previewImg.src = `/asset/img/${imagePath}`;
                    document.getElementById('uploadPlaceholder').style.display = 'none';
                    document.getElementById('imagePreview').style.display = 'block';
                }

                const modal = document.getElementById('addServiceModal');
                modal.classList.add('show');
                modal.style.display = 'flex';

                // Ngăn scroll body
                document.body.style.overflow = 'hidden';
            } else {
                alert(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Không thể tải thông tin dịch vụ!');
        }
    },

    // Đóng modal
    closeModal() {
        const modal = document.getElementById('addServiceModal');
        modal.classList.remove('show');

        // Animation đóng
        setTimeout(() => {
            modal.style.display = 'none';
            document.getElementById('formDichVu').reset();
            this.removeImage();

            // Cho phép scroll body lại
            document.body.style.overflow = '';
        }, 300);
    },

    // Preview ảnh
    previewImage(input) {
        if (input.files && input.files[0]) {
            const file = input.files[0];

            // Kiểm tra kích thước file (max 5MB)
            if (file.size > 5 * 1024 * 1024) {
                alert('Kích thước file không được vượt quá 5MB!');
                input.value = '';
                return;
            }

            // Kiểm tra định dạng
            if (!file.type.match('image.*')) {
                alert('Vui lòng chọn file hình ảnh!');
                input.value = '';
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                const previewImg = document.getElementById('previewImg');
                previewImg.src = e.target.result;
                document.getElementById('uploadPlaceholder').style.display = 'none';
                document.getElementById('imagePreview').style.display = 'block';
            };
            reader.readAsDataURL(file);
        }
    },

    // Xóa ảnh
    removeImage() {
        document.getElementById('hinhAnhFile').value = '';
        document.getElementById('previewImg').src = '';
        document.getElementById('uploadPlaceholder').style.display = 'block';
        document.getElementById('imagePreview').style.display = 'none';
    },

    // Submit form
    async submitForm() {
        const form = document.getElementById('formDichVu');
        const formData = new FormData(form);

        // Validation
        const tenDV = document.getElementById('tenDV').value.trim();
        const loai = document.getElementById('loai').value;
        const gia = document.getElementById('gia').value;
        const trangThai = document.getElementById('trangThai').value;

        if (!tenDV) {
            alert('Vui lòng nhập tên dịch vụ!');
            document.getElementById('tenDV').focus();
            return;
        }

        if (!loai) {
            alert('Vui lòng chọn loại dịch vụ!');
            document.getElementById('loai').focus();
            return;
        }

        if (!gia || gia <= 0) {
            alert('Vui lòng nhập giá hợp lệ!');
            document.getElementById('gia').focus();
            return;
        }

        if (!trangThai) {
            alert('Vui lòng chọn trạng thái!');
            document.getElementById('trangThai').focus();
            return;
        }

        const url = this.isEditMode ? '/DichVu/SuaDichVu' : '/DichVu/ThemDichVu';
        const btnSubmit = document.getElementById('btnSubmit');
        const btnText = document.getElementById('btnSubmitText');
        const originalText = btnText.textContent;

        try {
            btnSubmit.disabled = true;
            btnText.textContent = 'Đang xử lý...';

            console.log('Submitting to:', url);
            console.log('FormData entries:');
            for (let pair of formData.entries()) {
                console.log(pair[0] + ':', pair[1]);
            }

            const response = await fetch(url, {
                method: 'POST',
                body: formData
            });

            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers.get('content-type'));

            // Kiểm tra response có phải JSON không
            const contentType = response.headers.get('content-type');
            if (!contentType || !contentType.includes('application/json')) {
                const text = await response.text();
                console.error('Response is not JSON:', text);
                alert('Lỗi server: Response không phải JSON. Kiểm tra console để xem chi tiết.');
                return;
            }

            const result = await response.json();
            console.log('Result:', result);

            if (result.success) {
                alert(result.message);
                this.closeModal();

                // Reload sau khi animation đóng modal hoàn tất
                setTimeout(() => {
                    location.reload();
                }, 350);
            } else {
                alert(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Có lỗi xảy ra! Kiểm tra console để xem chi tiết.');
        } finally {
            btnSubmit.disabled = false;
            btnText.textContent = originalText;
        }
    },

    // Xóa dịch vụ
    async deleteService(maDV, tenDV) {
        if (!confirm(`Bạn có chắc chắn muốn xóa dịch vụ "${tenDV}"?`)) {
            return;
        }

        try {
            const response = await fetch('/DichVu/XoaDichVu', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ maDV: maDV })
            });

            const result = await response.json();

            if (result.success) {
                alert(result.message);
                location.reload();
            } else {
                alert(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Có lỗi xảy ra! Vui lòng thử lại.');
        }
    }
};

// Đóng modal khi click ESC
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const modal = document.getElementById('addServiceModal');
        if (modal && modal.classList.contains('show')) {
            DichVuManager.closeModal();
        }
    }
});

// Ngăn modal đóng khi click vào content
document.addEventListener('DOMContentLoaded', function () {
    const modalContent = document.querySelector('.dichvu-modal-content');
    if (modalContent) {
        modalContent.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    }
});