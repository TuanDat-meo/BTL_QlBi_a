// editEmployee.js - Standalone Side Panel System (No Modal)

// ===========================
// GLOBAL VARIABLES
// ===========================
let editEmployeeStream = null;
let editCapturedFaceBlob = null;
let isEditCameraActive = false;

// ===========================
// SIDE PANEL MANAGEMENT (NO MODAL)
// ===========================

async function openEditEmployeePanel(maNV) {
    try {
        console.log(`📂 Opening edit employee side panel for ID: ${maNV}...`);

        // Close any existing panel first
        closeEditEmployeePanel();

        // Fetch form HTML from server
        const response = await fetch(`/NhanVien/GetEditForm?maNV=${maNV}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const html = await response.text();

        // Create side panel structure (NO MODAL)
        const panel = document.createElement('div');
        panel.id = 'editEmployeeSidePanel';
        panel.className = 'edit-side-panel';
        panel.innerHTML = `
            <div class="edit-panel-overlay" onclick="closeEditEmployeePanel()"></div>
            <div class="edit-panel-container">
                <div class="edit-panel-header">
                    <h3 class="edit-panel-title">
                        <span class="title-icon">✏️</span>
                        <span>Chỉnh sửa nhân viên</span>
                    </h3>
                    <button class="edit-panel-close-btn" onclick="closeEditEmployeePanel()">
                        <span>✕</span>
                    </button>
                </div>
                <div class="edit-panel-body">
                    ${html}
                </div>
            </div>
        `;

        // Append to body
        document.body.appendChild(panel);

        // Trigger animation
        requestAnimationFrame(() => {
            panel.classList.add('active');
        });

        // Reset state
        editEmployeeStream = null;
        editCapturedFaceBlob = null;
        isEditCameraActive = false;

        // Initialize currency inputs after render
        setTimeout(() => {
            initializeEditCurrencyInputs();
        }, 300);

        console.log('✅ Edit employee side panel opened successfully');
    } catch (error) {
        console.error('❌ Error opening edit employee panel:', error);
        showEditNotification('❌ Không thể mở form chỉnh sửa: ' + error.message, 'error');
    }
}

function closeEditEmployeePanel() {
    console.log('🛑 Closing edit employee side panel...');

    const panel = document.getElementById('editEmployeeSidePanel');
    if (!panel) return;

    // Cleanup camera resources
    stopEditEmployeeCamera();
    deleteEditCapturedFace();

    // Remove active class for animation
    panel.classList.remove('active');

    // Remove panel after animation completes
    setTimeout(() => {
        panel.remove();
        console.log('✅ Edit employee side panel removed from DOM');
    }, 300);
}

// Handle ESC key to close panel
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const panel = document.getElementById('editEmployeeSidePanel');
        if (panel && panel.classList.contains('active')) {
            closeEditEmployeePanel();
        }
    }
});

// ===========================
// CAMERA MANAGEMENT
// ===========================

async function startEditEmployeeCamera() {
    console.log('📸 Starting camera for edit employee...');

    const video = document.getElementById('editEmployeeVideo');
    const previewContainer = document.getElementById('editFacePreviewContainer');
    const faceStatus = document.getElementById('editFaceStatus');
    const btnStart = document.getElementById('btnEditStartCamera');
    const btnStop = document.getElementById('btnEditStopCamera');
    const btnCapture = document.getElementById('btnEditCaptureFace');

    if (!video || !previewContainer || !faceStatus) {
        console.error('❌ Required camera elements not found');
        alert('⚠️ Lỗi: Không tìm thấy các thành phần camera');
        return;
    }

    // Stop existing camera if active
    if (editEmployeeStream) {
        console.log('🛑 Stopping existing camera stream...');
        stopEditEmployeeCamera();
        await new Promise(resolve => setTimeout(resolve, 500));
    }

    try {
        // Check browser camera support
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            throw new Error('Trình duyệt không hỗ trợ camera');
        }

        // Update status
        faceStatus.innerHTML = '<span class="status-icon">🔄</span><span>Đang khởi động camera...</span>';
        faceStatus.className = 'face-status info';

        // Request camera access
        const constraints = {
            video: {
                facingMode: 'user',
                width: { ideal: 640, max: 1280 },
                height: { ideal: 480, max: 720 }
            },
            audio: false
        };

        console.log('📹 Requesting camera access with constraints:', constraints);
        const stream = await navigator.mediaDevices.getUserMedia(constraints);

        editEmployeeStream = stream;
        video.srcObject = stream;
        isEditCameraActive = true;

        console.log('✅ Camera stream obtained successfully');

        // Wait for video metadata to load
        video.onloadedmetadata = () => {
            video.play().then(() => {
                console.log('✅ Video playing, dimensions:', video.videoWidth, 'x', video.videoHeight);

                // Update UI
                previewContainer.style.display = 'block';
                btnStart.style.display = 'none';
                btnStop.style.display = 'inline-flex';
                btnCapture.style.display = 'inline-flex';

                faceStatus.innerHTML = '<span class="status-icon">✅</span><span>Camera đã sẵn sàng. Đặt khuôn mặt vào khung hình và nhấn "Chụp ảnh"</span>';
                faceStatus.className = 'face-status success';

            }).catch(err => {
                console.error('❌ Error playing video:', err);
                throw new Error('Không thể phát video từ camera');
            });
        };

        video.onerror = (err) => {
            console.error('❌ Video element error:', err);
            throw new Error('Lỗi video element');
        };

    } catch (error) {
        console.error('❌ Camera error:', error);
        handleEditCameraError(error, faceStatus);
    }
}

function stopEditEmployeeCamera() {
    console.log('🛑 Stopping edit employee camera...');

    const video = document.getElementById('editEmployeeVideo');
    const previewContainer = document.getElementById('editFacePreviewContainer');
    const faceStatus = document.getElementById('editFaceStatus');
    const btnStart = document.getElementById('btnEditStartCamera');
    const btnStop = document.getElementById('btnEditStopCamera');
    const btnCapture = document.getElementById('btnEditCaptureFace');

    // Stop all media tracks
    if (editEmployeeStream) {
        editEmployeeStream.getTracks().forEach(track => {
            track.stop();
            console.log('✓ Stopped track:', track.kind, track.label);
        });
        editEmployeeStream = null;
    }

    // Clear video element
    if (video) {
        video.srcObject = null;
        video.pause();
        video.currentTime = 0;
    }

    // Reset UI elements
    if (previewContainer) previewContainer.style.display = 'none';
    if (btnStart) btnStart.style.display = 'inline-flex';
    if (btnStop) btnStop.style.display = 'none';
    if (btnCapture) btnCapture.style.display = 'none';

    if (faceStatus) {
        faceStatus.innerHTML = '<span class="status-icon">ℹ️</span><span>Nhấn "Bật Camera" để chụp ảnh Face ID mới</span>';
        faceStatus.className = 'face-status info';
    }

    isEditCameraActive = false;
    console.log('✅ Camera stopped and resources released');
}

function handleEditCameraError(error, statusEl) {
    let errorMessage = 'Không thể truy cập camera';
    let suggestions = '';

    if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {
        errorMessage = 'Quyền truy cập camera bị từ chối';
        suggestions = '• Cấp quyền camera cho trình duyệt\n• Kiểm tra cài đặt quyền riêng tư';
    } else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {
        errorMessage = 'Không tìm thấy camera';
        suggestions = '• Kiểm tra kết nối camera\n• Đảm bảo camera được cắm đúng';
    } else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {
        errorMessage = 'Camera đang được sử dụng';
        suggestions = '• Đóng các ứng dụng khác đang dùng camera\n• Thử tải lại trang';
    } else if (error.message) {
        errorMessage = error.message;
    }

    if (statusEl) {
        statusEl.innerHTML = `<span class="status-icon">❌</span><span>${errorMessage}</span>`;
        statusEl.className = 'face-status error';
    }

    alert(`❌ ${errorMessage}\n\n${suggestions || 'Vui lòng thử lại sau'}`);
}

// ===========================
// FACE CAPTURE
// ===========================

function captureEditEmployeeFace() {
    console.log('📸 Capturing employee face...');

    const video = document.getElementById('editEmployeeVideo');
    const canvas = document.getElementById('editEmployeeCanvas');
    const capturedContainer = document.getElementById('editCapturedFaceContainer');
    const capturedImage = document.getElementById('editCapturedFaceImage');
    const faceStatus = document.getElementById('editFaceStatus');

    if (!video || !canvas) {
        console.error('❌ Video or canvas element not found');
        alert('⚠️ Lỗi: Không tìm thấy video hoặc canvas');
        return;
    }

    // Validate video ready state
    if (!video.videoWidth || !video.videoHeight || video.readyState !== video.HAVE_ENOUGH_DATA) {
        alert('⚠️ Camera chưa sẵn sàng. Vui lòng đợi vài giây và thử lại.');
        console.warn('Video not ready:', {
            videoWidth: video.videoWidth,
            videoHeight: video.videoHeight,
            readyState: video.readyState
        });
        return;
    }

    try {
        // Set canvas dimensions to match video
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        // Draw current video frame to canvas
        const context = canvas.getContext('2d');
        context.drawImage(video, 0, 0, canvas.width, canvas.height);

        console.log(`✓ Frame captured: ${canvas.width}x${canvas.height}px`);

        // Convert canvas to blob
        canvas.toBlob(blob => {
            if (blob) {
                editCapturedFaceBlob = blob;

                // Create preview URL
                const imageUrl = URL.createObjectURL(blob);

                if (capturedImage) {
                    capturedImage.src = imageUrl;
                }

                if (capturedContainer) {
                    capturedContainer.style.display = 'block';
                }

                // Stop camera after successful capture
                stopEditEmployeeCamera();

                // Update status
                if (faceStatus) {
                    faceStatus.innerHTML = '<span class="status-icon">✅</span><span>Đã chụp ảnh Face ID mới thành công!</span>';
                    faceStatus.className = 'face-status success';
                }

                console.log('✅ Face captured successfully, blob size:', (blob.size / 1024).toFixed(2), 'KB');
                showEditNotification('✅ Đã chụp ảnh Face ID mới thành công!', 'success');
            } else {
                throw new Error('Không thể tạo blob từ canvas');
            }
        }, 'image/jpeg', 0.95);

    } catch (error) {
        console.error('❌ Error capturing face:', error);
        alert('❌ Lỗi khi chụp ảnh: ' + error.message);
    }
}

function deleteEditCapturedFace() {
    console.log('🗑️ Deleting captured face image...');

    const capturedContainer = document.getElementById('editCapturedFaceContainer');
    const capturedImage = document.getElementById('editCapturedFaceImage');
    const faceStatus = document.getElementById('editFaceStatus');

    // Clear blob
    editCapturedFaceBlob = null;

    // Revoke object URL and clear image
    if (capturedImage && capturedImage.src) {
        URL.revokeObjectURL(capturedImage.src);
        capturedImage.src = '';
    }

    // Hide container
    if (capturedContainer) {
        capturedContainer.style.display = 'none';
    }

    // Reset status message
    if (faceStatus) {
        faceStatus.innerHTML = '<span class="status-icon">ℹ️</span><span>Nhấn "Bật Camera" để chụp ảnh Face ID mới</span>';
        faceStatus.className = 'face-status info';
    }

    console.log('✅ Captured face image deleted');
}

// ===========================
// FACE ID DELETION
// ===========================

function confirmDeleteFaceID() {
    const confirmed = confirm('⚠️ Bạn có chắc muốn xóa Face ID hiện tại không?\n\nFace ID sẽ bị xóa vĩnh viễn khi bạn nhấn "Cập nhật".');

    if (confirmed) {
        const currentFaceDiv = document.querySelector('#editEmployeeSidePanel .current-face-id');
        let noFaceDiv = document.querySelector('#editEmployeeSidePanel .no-face-id');
        const deleteFlagInput = document.getElementById('deleteFaceIDFlag');

        // Hide current face preview
        if (currentFaceDiv) {
            currentFaceDiv.style.display = 'none';
        }

        // Show deletion notice
        if (!noFaceDiv) {
            noFaceDiv = document.createElement('div');
            noFaceDiv.className = 'no-face-id';
            noFaceDiv.innerHTML = '<p>❌ Face ID sẽ bị xóa khi cập nhật</p>';
            if (currentFaceDiv && currentFaceDiv.parentNode) {
                currentFaceDiv.parentNode.insertBefore(noFaceDiv, currentFaceDiv);
            }
        } else {
            noFaceDiv.innerHTML = '<p>❌ Face ID sẽ bị xóa khi cập nhật</p>';
            noFaceDiv.style.display = 'block';
        }

        // Set deletion flag
        if (deleteFlagInput) {
            deleteFlagInput.value = 'true';
        }

        console.log('🗑️ Face ID marked for deletion');
        showEditNotification('✅ Face ID sẽ bị xóa khi bạn nhấn "Cập nhật"', 'info');
    }
}

// ===========================
// FORM HANDLING
// ===========================

function initializeEditCurrencyInputs() {
    const panel = document.getElementById('editEmployeeSidePanel');
    if (!panel) return;

    const currencyInputs = panel.querySelectorAll('.currency-input');
    console.log('💰 Initializing currency inputs:', currencyInputs.length);

    currencyInputs.forEach(input => {
        // Format on input
        input.addEventListener('input', function (e) {
            let value = e.target.value.replace(/[^\d]/g, '');
            if (value === '') {
                e.target.value = '0';
                return;
            }
            value = parseInt(value, 10).toString();
            e.target.value = formatCurrency(value);
        });

        // Ensure valid on blur
        input.addEventListener('blur', function (e) {
            if (e.target.value === '' || e.target.value === '0') {
                e.target.value = '0';
            }
        });

        // Format initial value
        if (input.value) {
            const numericValue = input.value.replace(/[^\d]/g, '');
            input.value = formatCurrency(numericValue);
        }
    });
}

function formatCurrency(value) {
    return value.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

function parseCurrency(formattedValue) {
    return parseInt(formattedValue.replace(/\./g, ''), 10) || 0;
}

// ===========================
// FORM SUBMISSION
// ===========================

async function submitEditEmployee() {
    const btnSubmit = document.getElementById('btnSubmitEditEmployee');

    try {
        console.log('📤 Submitting employee edit form...');

        // Collect form data
        const maNV = document.getElementById('editMaNV')?.value;
        const tenNV = document.getElementById('editTenNV')?.value.trim();
        const sdt = document.getElementById('editSDT')?.value.trim();
        const email = document.getElementById('editEmail')?.value.trim();
        const maNhom = document.getElementById('editMaNhom')?.value;
        const caMacDinh = document.getElementById('editCaMacDinh')?.value;
        const trangThai = document.getElementById('editTrangThai')?.value;
        const luongCoBan = parseCurrency(document.getElementById('editLuongCoBan')?.value || '0');
        const phuCap = parseCurrency(document.getElementById('editPhuCap')?.value || '0');
        const matKhauMoi = document.getElementById('editMatKhauMoi')?.value;
        const matKhauConfirm = document.getElementById('editMatKhauConfirm')?.value;
        const deleteFaceID = document.getElementById('deleteFaceIDFlag')?.value === 'true';

        // Validation
        if (!tenNV) {
            alert('⚠️ Vui lòng nhập tên nhân viên');
            document.getElementById('editTenNV')?.focus();
            return;
        }

        if (!sdt) {
            alert('⚠️ Vui lòng nhập số điện thoại');
            document.getElementById('editSDT')?.focus();
            return;
        }

        if (!/^[0-9]{10,11}$/.test(sdt)) {
            alert('⚠️ Số điện thoại phải là 10-11 chữ số');
            document.getElementById('editSDT')?.focus();
            return;
        }

        if (!maNhom) {
            alert('⚠️ Vui lòng chọn nhóm quyền');
            document.getElementById('editMaNhom')?.focus();
            return;
        }

        // Password validation (if provided)
        if (matKhauMoi) {
            if (matKhauMoi.length < 6) {
                alert('⚠️ Mật khẩu mới phải có ít nhất 6 ký tự');
                document.getElementById('editMatKhauMoi')?.focus();
                return;
            }

            if (matKhauMoi !== matKhauConfirm) {
                alert('⚠️ Mật khẩu xác nhận không khớp');
                document.getElementById('editMatKhauConfirm')?.focus();
                return;
            }
        }

        // Disable submit button
        if (btnSubmit) {
            btnSubmit.disabled = true;
            btnSubmit.innerHTML = '<span class="btn-icon">⏳</span><span>Đang xử lý...</span>';
        }

        // Create FormData
        const formData = new FormData();
        formData.append('MaNV', maNV);
        formData.append('TenNV', tenNV);
        formData.append('SDT', sdt);
        formData.append('Email', email || '');
        formData.append('MaNhom', maNhom);
        formData.append('CaMacDinh', caMacDinh);
        formData.append('TrangThai', trangThai);
        formData.append('LuongCoBan', luongCoBan);
        formData.append('PhuCap', phuCap);
        formData.append('DeleteFaceID', deleteFaceID);

        if (matKhauMoi) {
            formData.append('MatKhauMoi', matKhauMoi);
        }

        // Add captured face image if exists
        if (editCapturedFaceBlob) {
            formData.append('FaceIDAnh', editCapturedFaceBlob, 'face.jpg');
            console.log('📸 Including new face image, size:', (editCapturedFaceBlob.size / 1024).toFixed(2), 'KB');
        }

        // Submit to server
        const response = await fetch('/NhanVien/CapNhatNhanVien', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error(`Server error: ${response.status} ${response.statusText}`);
        }

        const result = await response.json();
        console.log('📥 Server response:', result);

        if (result.success) {
            showEditNotification('✅ ' + result.message, 'success');

            // Cleanup resources
            stopEditEmployeeCamera();
            deleteEditCapturedFace();

            // Close panel and reload page
            setTimeout(() => {
                closeEditEmployeePanel();
                location.reload();
            }, 1500);
        } else {
            showEditNotification('❌ ' + result.message, 'error');

            // Re-enable button
            if (btnSubmit) {
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = '<span class="btn-icon">💾</span><span>Cập nhật</span>';
            }
        }

    } catch (error) {
        console.error('❌ Submit error:', error);
        showEditNotification('❌ Không thể cập nhật nhân viên. Vui lòng thử lại.', 'error');

        // Re-enable button
        if (btnSubmit) {
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = '<span class="btn-icon">💾</span><span>Cập nhật</span>';
        }
    }
}

// ===========================
// NOTIFICATION SYSTEM
// ===========================

function showEditNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.edit-notification');
    existingNotifications.forEach(n => n.remove());

    // Create notification element
    const notification = document.createElement('div');
    notification.className = `edit-notification edit-notification-${type}`;

    const bgColor = type === 'success' ? '#28a745' :
        type === 'error' ? '#dc3545' :
            type === 'warning' ? '#ffc107' : '#667eea';

    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 16px 24px;
        background: ${bgColor};
        color: white;
        border-radius: 12px;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
        z-index: 10001;
        animation: slideInRight 0.4s ease;
        font-weight: 600;
        max-width: 400px;
        font-size: 14px;
        word-wrap: break-word;
    `;
    notification.textContent = message;

    document.body.appendChild(notification);

    // Auto remove after 3 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.4s ease';
        setTimeout(() => notification.remove(), 400);
    }, 3000);
}

// ===========================
// GLOBAL EXPORTS
// ===========================
window.openEditEmployeePanel = openEditEmployeePanel;
window.closeEditEmployeePanel = closeEditEmployeePanel;
window.startEditEmployeeCamera = startEditEmployeeCamera;
window.stopEditEmployeeCamera = stopEditEmployeeCamera;
window.captureEditEmployeeFace = captureEditEmployeeFace;
window.deleteEditCapturedFace = deleteEditCapturedFace;
window.confirmDeleteFaceID = confirmDeleteFaceID;
window.submitEditEmployee = submitEditEmployee;
window.initializeEditCurrencyInputs = initializeEditCurrencyInputs;

console.log('✅ editEmployee.js loaded (Standalone Side Panel - No Modal)');