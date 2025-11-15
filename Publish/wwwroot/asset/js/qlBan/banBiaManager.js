/**
 * Module quản lý CRUD bàn bi-a
 * Updated: Soft delete - ẩn bàn thay vì xóa
 */
const BanBiaManager = {
    /**
     * Mở modal thêm bàn
     */
    openAddModal: async function () {
        try {
            console.log('➕ Opening add table modal...');

            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found');
                return;
            }

            // Show loading
            modalOverlay.innerHTML = `
                <div class="modal-content ban-modal">
                    <div class="loading-state" style="text-align: center; padding: 60px 40px;">
                        <div class="spinner" style="margin: 0 auto 20px; width: 50px; height: 50px; border: 4px solid #f3f4f6; border-top-color: #667eea; border-radius: 50%; animation: spin 1s linear infinite;"></div>
                        <p style="color: #6c757d; font-weight: 500;">Đang tải form thêm bàn...</p>
                    </div>
                </div>
            `;
            modalOverlay.classList.add('active');

            const response = await fetch('/QLBan/FormThemBan');

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const html = await response.text();

            modalOverlay.innerHTML = `
                <div class="modal-content ban-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Add form loaded successfully');
        } catch (error) {
            console.error('❌ Error loading add form:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content ban-modal">
                        <div class="error-state" style="text-align: center; padding: 60px 40px;">
                            <div class="error-icon" style="font-size: 60px; margin-bottom: 20px;">⚠️</div>
                            <h3 style="color: #dc3545; margin-bottom: 10px;">Không thể tải form</h3>
                            <p style="color: #6c757d; font-size: 14px; margin-bottom: 20px;">${error.message}</p>
                            <button class="btn btn-primary" onclick="BanBiaManager.closeModal()" style="padding: 10px 24px; border-radius: 8px; border: none; background: #667eea; color: white; font-weight: 600; cursor: pointer;">
                                Đóng
                            </button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) Toast.error('Không thể tải form thêm bàn');
        }
    },

    /**
     * Mở modal chỉnh sửa bàn
     */
    openEditModal: async function (maBan) {
        try {
            console.log('✏️ Opening edit modal for table:', maBan);

            const modalOverlay = document.getElementById('modalOverlay');
            if (!modalOverlay) {
                console.error('Modal overlay not found');
                return;
            }

            // Show loading
            modalOverlay.innerHTML = `
                <div class="modal-content ban-modal">
                    <div class="loading-state" style="text-align: center; padding: 60px 40px;">
                        <div class="spinner" style="margin: 0 auto 20px; width: 50px; height: 50px; border: 4px solid #f3f4f6; border-top-color: #667eea; border-radius: 50%; animation: spin 1s linear infinite;"></div>
                        <p style="color: #6c757d; font-weight: 500;">Đang tải thông tin bàn...</p>
                    </div>
                </div>
            `;
            modalOverlay.classList.add('active');

            const response = await fetch(`/QLBan/FormChinhSuaBanBia?maBan=${maBan}`);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const html = await response.text();

            modalOverlay.innerHTML = `
                <div class="modal-content ban-modal">
                    ${html}
                </div>
            `;

            console.log('✅ Edit form loaded successfully');
        } catch (error) {
            console.error('❌ Error loading edit form:', error);

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = `
                    <div class="modal-content ban-modal">
                        <div class="error-state" style="text-align: center; padding: 60px 40px;">
                            <div class="error-icon" style="font-size: 60px; margin-bottom: 20px;">⚠️</div>
                            <h3 style="color: #dc3545; margin-bottom: 10px;">Không thể tải thông tin</h3>
                            <p style="color: #6c757d; font-size: 14px; margin-bottom: 20px;">${error.message}</p>
                            <button class="btn btn-primary" onclick="BanBiaManager.closeModal()" style="padding: 10px 24px; border-radius: 8px; border: none; background: #667eea; color: white; font-weight: 600; cursor: pointer;">
                                Đóng
                            </button>
                        </div>
                    </div>
                `;
            }

            if (window.Toast) Toast.error('Không thể tải form chỉnh sửa');
        }
    },

    /**
     * Submit thêm bàn
     */
    submitAdd: async function () {
        try {
            const form = document.getElementById('formThemBan');
            if (!form) {
                throw new Error('Form not found');
            }

            // Validate
            const tenBan = document.getElementById('tenBan')?.value?.trim();
            const maLoai = document.getElementById('maLoai')?.value;
            const maKhuVuc = document.getElementById('maKhuVuc')?.value;

            if (!tenBan) {
                if (window.Toast) Toast.error('Vui lòng nhập tên bàn');
                document.getElementById('tenBan')?.focus();
                return;
            }

            if (!maLoai) {
                if (window.Toast) Toast.error('Vui lòng chọn loại bàn');
                return;
            }

            if (!maKhuVuc) {
                if (window.Toast) Toast.error('Vui lòng chọn khu vực');
                return;
            }

            console.log('💾 Submitting add table form...');

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
                if (window.Toast) Toast.success(result.message || 'Thêm bàn thành công');
                this.closeModal();

                console.log('✅ Table added successfully');

                setTimeout(() => {
                    location.reload();
                }, 800);
            } else {
                console.warn('⚠️ Add table failed:', result.message);
                if (window.Toast) {
                    Toast.error(result.message || 'Không thể thêm bàn');
                } else {
                    alert('❌ Lỗi: ' + (result.message || 'Không thể thêm bàn'));
                }
            }
        } catch (error) {
            if (window.Loading) Loading.hide();
            console.error('❌ Error adding table:', error);

            if (window.Toast) {
                Toast.error('Có lỗi xảy ra khi thêm bàn');
            } else {
                alert('❌ Có lỗi xảy ra: ' + error.message);
            }
        }
    },

    /**
     * Submit chỉnh sửa bàn
     */
    submitEdit: async function (maBan) {
        try {
            const form = document.getElementById('formChinhSuaBan');
            if (!form) {
                throw new Error('Form not found');
            }

            // Validate
            const tenBan = document.getElementById('tenBan')?.value?.trim();
            const maLoai = document.getElementById('maLoai')?.value;
            const maKhuVuc = document.getElementById('maKhuVuc')?.value;

            if (!tenBan) {
                if (window.Toast) Toast.error('Vui lòng nhập tên bàn');
                document.getElementById('tenBan')?.focus();
                return;
            }

            if (!maLoai) {
                if (window.Toast) Toast.error('Vui lòng chọn loại bàn');
                return;
            }

            if (!maKhuVuc) {
                if (window.Toast) Toast.error('Vui lòng chọn khu vực');
                return;
            }

            console.log('💾 Submitting edit table form for table:', maBan);

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
                if (window.Toast) Toast.success(result.message || 'Cập nhật bàn thành công');
                this.closeModal();

                console.log('✅ Table updated successfully');

                setTimeout(() => {
                    location.reload();
                }, 800);
            } else {
                console.warn('⚠️ Update table failed:', result.message);
                if (window.Toast) {
                    Toast.error(result.message || 'Không thể cập nhật bàn');
                } else {
                    alert('❌ Lỗi: ' + (result.message || 'Không thể cập nhật bàn'));
                }
            }
        } catch (error) {
            if (window.Loading) Loading.hide();
            console.error('❌ Error updating table:', error);

            if (window.Toast) {
                Toast.error('Có lỗi xảy ra khi cập nhật');
            } else {
                alert('❌ Có lỗi xảy ra: ' + error.message);
            }
        }
    },

    /**
     * Ẩn bàn (Soft Delete - thay vì xóa vĩnh viễn)
     * Chuyển bàn sang trạng thái "Ngừng phục vụ" (BaoTri)
     */
    deleteBan: async function (maBan) {
        // Hiển thị confirm với thông tin chi tiết
        const confirmMessage = `Bạn có muốn xóa!`;

        if (!confirm(confirmMessage)) {
            console.log('ℹ️ User cancelled hide table action');
            return;
        }

        try {
            console.log('👁️‍🗨️ Hiding table (soft delete):', maBan);

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
                console.log('✅ Table hidden successfully');

                if (window.Toast) {
                    Toast.success(result.message || 'Đã ẩn bàn thành công');
                }

                this.closeModal();

                // Thông báo chi tiết cho người dùng
                setTimeout(() => {
                    const infoMessage = `✅ BÀN ĐÃ ĐƯỢC ẨN THÀNH CÔNG!

📋 Bàn sẽ không hiển thị trong danh sách
💾 Dữ liệu vẫn được lưu trữ an toàn
🔄 Admin có thể khôi phục bàn bất cứ lúc nào
📊 Lịch sử và thông tin liên kết được bảo toàn`;

                    alert(infoMessage);
                    location.reload();
                }, 500);
            } else {
                console.warn('⚠️ Hide table failed:', result.message);

                if (window.Toast) {
                    Toast.error(result.message || 'Không thể ẩn bàn');
                } else {
                    alert('❌ Lỗi: ' + (result.message || 'Không thể ẩn bàn'));
                }
            }
        } catch (error) {
            if (window.Loading) Loading.hide();
            console.error('❌ Error hiding table:', error);

            if (window.Toast) {
                Toast.error('Có lỗi xảy ra khi ẩn bàn');
            } else {
                alert('❌ Có lỗi xảy ra: ' + error.message);
            }
        }
    },

    /**
     * Preview hình ảnh khi upload
     */
    previewImage: function (input) {
        if (!input.files || !input.files[0]) {
            return;
        }

        const file = input.files[0];

        // Validate file size (5MB)
        const maxSize = 5 * 1024 * 1024; // 5MB
        if (file.size > maxSize) {
            console.warn('⚠️ File too large:', (file.size / 1024 / 1024).toFixed(2) + 'MB');

            if (window.Toast) {
                Toast.error('Kích thước ảnh không được vượt quá 5MB');
            } else {
                alert('❌ Kích thước ảnh không được vượt quá 5MB');
            }

            input.value = '';
            return;
        }

        // Validate file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        if (!allowedTypes.includes(file.type.toLowerCase())) {
            console.warn('⚠️ Invalid file type:', file.type);

            if (window.Toast) {
                Toast.error('Chỉ hỗ trợ ảnh JPG, PNG, GIF');
            } else {
                alert('❌ Chỉ hỗ trợ ảnh JPG, PNG, GIF');
            }

            input.value = '';
            return;
        }

        console.log('📷 Previewing image:', file.name, (file.size / 1024).toFixed(2) + 'KB');

        const reader = new FileReader();

        reader.onload = function (e) {
            const placeholder = document.getElementById('uploadPlaceholder');
            const preview = document.getElementById('imagePreview');
            const previewImg = document.getElementById('previewImg');

            if (placeholder && preview && previewImg) {
                placeholder.style.display = 'none';
                preview.style.display = 'block';
                previewImg.src = e.target.result;

                console.log('✅ Image preview loaded');
            }
        };

        reader.onerror = function (error) {
            console.error('❌ Error reading file:', error);

            if (window.Toast) {
                Toast.error('Không thể đọc file ảnh');
            }
        };

        reader.readAsDataURL(file);
    },

    /**
     * Xóa preview hình ảnh
     */
    removeImage: function () {
        console.log('🗑️ Removing image preview');

        const fileInput = document.getElementById('hinhAnh') || document.getElementById('hinhAnhMoi');
        const placeholder = document.getElementById('uploadPlaceholder');
        const preview = document.getElementById('imagePreview');

        if (fileInput) {
            fileInput.value = '';
        }

        if (placeholder) {
            placeholder.style.display = 'block';
        }

        if (preview) {
            preview.style.display = 'none';
        }

        console.log('✅ Image preview removed');
    },

    /**
     * Đóng modal
     */
    closeModal: function () {
        console.log('❌ Closing modal');

        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            modalOverlay.classList.remove('active');

            setTimeout(() => {
                modalOverlay.innerHTML = '';
            }, 300);
        }
    }
};

// Add CSS styles
(function () {
    if (document.getElementById('ban-bia-manager-styles')) {
        return;
    }

    const style = document.createElement('style');
    style.id = 'ban-bia-manager-styles';
    style.textContent = `
        /* Edit button on table card */
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
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(59, 130, 246, 0.3);
        }

        .btn-edit-table:hover {
            background: #3b82f6;
            transform: scale(1.15) rotate(15deg);
            box-shadow: 0 4px 16px rgba(59, 130, 246, 0.5);
        }

        /* Modal sizing */
        .ban-modal {
            width: 100%;
            max-width: 700px;
            padding: 0;
            border-radius: 16px;
            overflow: hidden;
        }

        /* Loading spinner animation */
        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        /* Responsive */
        @media (max-width: 768px) {
            .btn-edit-table {
                width: 32px;
                height: 32px;
                font-size: 14px;
                top: 45px;
                right: 8px;
            }

            .ban-modal {
                max-width: 96%;
                max-height: 92vh;
                margin: 10px;f
                border-radius: 12px;
            }
        }

        @media (max-width: 480px) {
            .btn-edit-table {
                width: 28px;
                height: 28px;
                font-size: 12px;
            }

            .ban-modal {
                max-width: 100%;
                max-height: 95vh;
                margin: 0;
                border-radius: 0;
            }
        }
    `;
    document.head.appendChild(style);
})();

// Export to global scope
window.BanBiaManager = BanBiaManager;

console.log('✅ BanBiaManager loaded successfully (with soft delete support)');