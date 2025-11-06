/**
 * Module quản lý CRUD bàn bi-a
 */
const BanBiaManager = {
    /**
     * Mở modal thêm bàn
     */
    openAddModal: async function () {
        try {
            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found');
                return;
            }

            // Show loading
            modalOverlay.innerHTML = `
                <div class="modal-content ban-modal">
                    <div class="loading-state" style="text-align: center; padding: 40px;">
                        <div class="spinner" style="margin: 0 auto 20px;"></div>
                        <p>Đang tải form...</p>
                    </div>
                </div>
            `;
            modalOverlay.classList.add('active');

            const response = await fetch('/QLBan/FormThemBan');
            if (!response.ok) throw new Error('Không thể tải form');

            const html = await response.text();

            modalOverlay.innerHTML = `
                <div class="modal-content ban-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Add form loaded');
        } catch (error) {
            console.error('Error loading add form:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content ban-modal">
                        <div class="error-state" style="text-align: center; padding: 40px;">
                            <div class="error-icon" style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                            <p style="color: #dc3545;">Không thể tải form thêm bàn</p>
                            <button class="btn btn-primary" onclick="BanBiaManager.closeModal()">Đóng</button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) Toast.error('Không thể tải form');
        }
    },

    /**
     * Mở modal chỉnh sửa bàn
     */
    openEditModal: async function (maBan) {
        try {
            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found');
                return;
            }

            // Show loading
            modalOverlay.innerHTML = `
                <div class="modal-content ban-modal">
                    <div class="loading-state" style="text-align: center; padding: 40px;">
                        <div class="spinner" style="margin: 0 auto 20px;"></div>
                        <p>Đang tải thông tin...</p>
                    </div>
                </div>
            `;
            modalOverlay.classList.add('active');

            const response = await fetch(`/QLBan/FormChinhSuaBanBia?maBan=${maBan}`);
            if (!response.ok) throw new Error('Không thể tải form');

            const html = await response.text();

            modalOverlay.innerHTML = `
                <div class="modal-content ban-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Edit form loaded');
        } catch (error) {
            console.error('Error loading edit form:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content ban-modal">
                        <div class="error-state" style="text-align: center; padding: 40px;">
                            <div class="error-icon" style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                            <p style="color: #dc3545;">Không thể tải form chỉnh sửa</p>
                            <button class="btn btn-primary" onclick="BanBiaManager.closeModal()">Đóng</button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) Toast.error('Không thể tải form');
        }
    },

    /**
     * Submit thêm bàn
     */
    submitAdd: async function () {
        try {
            const form = document.getElementById('formThemBan');
            if (!form) return;

            // Validate
            const tenBan = document.getElementById('tenBan')?.value?.trim();
            const maLoai = document.getElementById('maLoai')?.value;
            const maKhuVuc = document.getElementById('maKhuVuc')?.value;

            if (!tenBan || !maLoai || !maKhuVuc) {
                if (window.Toast) Toast.error('Vui lòng điền đầy đủ thông tin bắt buộc');
                return;
            }

            // Create FormData
            const formData = new FormData(form);

            if (window.Loading) Loading.show();

            const response = await fetch('/QLBan/ThemBan', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (window.Loading) Loading.hide();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                this.closeModal();
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            if (window.Loading) Loading.hide();
            console.error('Error adding table:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi thêm bàn');
        }
    },

    /**
     * Submit chỉnh sửa bàn
     */
    submitEdit: async function (maBan) {
        try {
            const form = document.getElementById('formChinhSuaBan');
            if (!form) return;

            // Validate
            const tenBan = document.getElementById('tenBan')?.value?.trim();
            const maLoai = document.getElementById('maLoai')?.value;
            const maKhuVuc = document.getElementById('maKhuVuc')?.value;

            if (!tenBan || !maLoai || !maKhuVuc) {
                if (window.Toast) Toast.error('Vui lòng điền đầy đủ thông tin bắt buộc');
                return;
            }

            // Create FormData
            const formData = new FormData(form);

            if (window.Loading) Loading.show();

            const response = await fetch('/QLBan/CapNhatBan', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (window.Loading) Loading.hide();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                this.closeModal();
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            if (window.Loading) Loading.hide();
            console.error('Error updating table:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi cập nhật');
        }
    },

    /**
     * Xóa bàn
     */
    deleteBan: async function (maBan) {
        if (!confirm('⚠️ BẠN CÓ CHẮC MUỐN XÓA BÀN NÀY?\n\nHành động này không thể hoàn tác!')) {
            return;
        }

        try {
            if (window.Loading) Loading.show();

            const response = await fetch('/QLBan/XoaBan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ maBan: maBan })
            });

            const result = await response.json();

            if (window.Loading) Loading.hide();

            if (result.success) {
                if (window.Toast) Toast.success(result.message);
                this.closeModal();
                setTimeout(() => location.reload(), 1000);
            } else {
                if (window.Toast) Toast.error(result.message);
            }
        } catch (error) {
            if (window.Loading) Loading.hide();
            console.error('Error deleting table:', error);
            if (window.Toast) Toast.error('Có lỗi xảy ra khi xóa bàn');
        }
    },

    /**
     * Preview hình ảnh
     */
    previewImage: function (input) {
        if (input.files && input.files[0]) {
            const file = input.files[0];

            // Validate file size (5MB)
            if (file.size > 5 * 1024 * 1024) {
                if (window.Toast) Toast.error('Kích thước file không được vượt quá 5MB');
                input.value = '';
                return;
            }

            // Validate file type
            if (!file.type.startsWith('image/')) {
                if (window.Toast) Toast.error('Vui lòng chọn file hình ảnh');
                input.value = '';
                return;
            }

            const reader = new FileReader();

            reader.onload = function (e) {
                const placeholder = document.getElementById('uploadPlaceholder');
                const preview = document.getElementById('imagePreview');
                const previewImg = document.getElementById('previewImg');

                if (placeholder && preview && previewImg) {
                    placeholder.style.display = 'none';
                    preview.style.display = 'block';
                    previewImg.src = e.target.result;
                }
            };

            reader.readAsDataURL(file);
        }
    },

    /**
     * Xóa preview hình ảnh
     */
    removeImage: function () {
        const fileInput = document.getElementById('hinhAnh') || document.getElementById('hinhAnhMoi');
        const placeholder = document.getElementById('uploadPlaceholder');
        const preview = document.getElementById('imagePreview');

        if (fileInput) fileInput.value = '';
        if (placeholder) placeholder.style.display = 'block';
        if (preview) preview.style.display = 'none';
    },

    /**
     * Đóng modal
     */
    closeModal: function () {
        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            modalOverlay.classList.remove('active');
            setTimeout(() => {
                modalOverlay.innerHTML = '';
            }, 300);
        }
    }
};

// Add CSS for edit button on table card
(function () {
    if (document.getElementById('ban-bia-manager-styles')) return;

    const style = document.createElement('style');
    style.id = 'ban-bia-manager-styles';
    style.textContent = `
        .btn-edit-table {
            position: absolute;
            top: 50px;
            right: 10px;
            width: 36px;
            height: 36px;
            background: rgba(255, 255, 255, 0.95);
            border: 2px solid #3b82f6;
            border-radius: 50%;
            cursor: pointer;
            font-size: 16px;
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 11;
            transition: all 0.3s;
            box-shadow: 0 2px 8px rgba(59, 130, 246, 0.3);
        }

        .btn-edit-table:hover {
            background: #3b82f6;
            transform: scale(1.1);
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.5);
        }

        .ban-modal {
            width: 100%;
            max-width: 700px;
            padding: 0;
        }

        @media (max-width: 768px) {
            .btn-edit-table {
                width: 32px;
                height: 32px;
                font-size: 14px;
            }

            .ban-modal {
                max-width: 100%;
                max-height: 95vh;
                margin: 10px;
            }
        }
    `;
    document.head.appendChild(style);
})();

// Export to global scope
window.BanBiaManager = BanBiaManager;

console.log('✅ BanBiaManager loaded');