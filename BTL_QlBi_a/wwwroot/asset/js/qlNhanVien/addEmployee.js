// addEmployeePanel.js - Side Panel System for Add Employee
// Thay thế modal bằng side panel để tránh xung đột với camera

// ===========================
// GLOBAL VARIABLES
// ===========================
let addEmployeeStream = null;
let capturedFaceBlob = null;
let isAddCameraActive = false;

// ===========================
// PANEL MANAGEMENT
// ===========================

async function openAddEmployeePanel() {
    try {
        console.log('📂 Opening add employee panel...');

        // Fetch form HTML
        const response = await fetch('/NhanVien/FormThemNhanVien');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const html = await response.text();

        // Create panel if not exists
        let panel = document.getElementById('addEmployeePanel');
        if (!panel) {
            panel = document.createElement('div');
            panel.id = 'addEmployeePanel';
            panel.className = 'side-panel';
            document.body.appendChild(panel);
        }

        // Set content
        panel.innerHTML = `
            <div class="side-panel-overlay" onclick="closeAddEmployeePanel()"></div>
            <div class="side-panel-content">
                <div class="side-panel-header">
                    <h3 class="side-panel-title">
                        <span class="title-icon">➕</span>
                        <span>Thêm nhân viên mới</span>
                    </h3>
                    <button class="side-panel-close" onclick="closeAddEmployeePanel()">
                        <span>✕</span>
                    </button>
                </div>
                <div class="side-panel-body">
                    ${html}
                </div>
            </div>
        `;

        // Show panel with animation
        setTimeout(() => {
            panel.classList.add('active');
        }, 10);

        // Reset state
        addEmployeeStream = null;
        capturedFaceBlob = null;
        isAddCameraActive = false;

        // Initialize after panel is shown
        setTimeout(() => {
            initializeCurrencyInputs();
        }, 300);

        console.log('✅ Add employee panel opened');
    } catch (error) {
        console.error('❌ Error opening add employee panel:', error);
        showNotification('❌ Không thể mở form thêm nhân viên: ' + error.message, 'error');
    }
}

function closeAddEmployeePanel() {
    console.log('🛑 Closing add employee panel...');

    const panel = document.getElementById('addEmployeePanel');
    if (!panel) return;

    // Cleanup camera
    stopAddEmployeeCamera();
    deleteCapturedFace();

    // Hide with animation
    panel.classList.remove('active');

    // Remove after animation
    setTimeout(() => {
        panel.remove();
    }, 300);

    console.log('✅ Add employee panel closed');
}

// ===========================
// CAMERA MANAGEMENT
// ===========================

async function startAddEmployeeCamera() {
    console.log('📸 Starting camera...');

    const video = document.getElementById('addEmployeeVideo');
    const previewContainer = document.getElementById('facePreviewContainer');
    const faceStatus = document.getElementById('faceStatus');
    const btnStart = document.getElementById('btnStartCamera');
    const btnStop = document.getElementById('btnStopCamera');
    const btnCapture = document.getElementById('btnCaptureFace');

    if (!video || !previewContainer || !faceStatus) {
        console.error('❌ Required elements not found');
        alert('⚠️ Lỗi: Không tìm thấy các thành phần cần thiết');
        return;
    }

    // Stop existing camera
    if (addEmployeeStream) {
        console.log('🛑 Stopping existing camera...');
        stopAddEmployeeCamera();
        await new Promise(resolve => setTimeout(resolve, 500));
    }

    try {
        // Check browser support
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            throw new Error('Trình duyệt không hỗ trợ camera');
        }

        // Update status
        faceStatus.innerHTML = '<span class="status-icon">🔄</span><span>Đang khởi động camera...</span>';
        faceStatus.className = 'biometric-status info';

        // Request camera
        const constraints = {
            video: {
                facingMode: 'user',
                width: { ideal: 640, max: 1280 },
                height: { ideal: 480, max: 720 }
            },
            audio: false
        };

        console.log('📹 Requesting camera access...');
        const stream = await navigator.mediaDevices.getUserMedia(constraints);

        addEmployeeStream = stream;
        video.srcObject = stream;
        isAddCameraActive = true;

        console.log('✅ Camera stream obtained');

        // Wait for video to be ready
        video.onloadedmetadata = () => {
            video.play().then(() => {
                console.log('✅ Video playing');

                // Update UI
                previewContainer.style.display = 'block';
                btnStart.style.display = 'none';
                btnStop.style.display = 'inline-flex';
                btnCapture.style.display = 'inline-flex';

                faceStatus.innerHTML = '<span class="status-icon">✅</span><span>Camera đã sẵn sàng. Đặt khuôn mặt vào khung hình và nhấn "Chụp ảnh"</span>';
                faceStatus.className = 'biometric-status success';

            }).catch(err => {
                console.error('❌ Error playing video:', err);
                throw new Error('Không thể phát video từ camera');
            });
        };

    } catch (error) {
        console.error('❌ Camera error:', error);
        handleCameraError(error, faceStatus);
    }
}

function stopAddEmployeeCamera() {
    console.log('🛑 Stopping camera...');

    const video = document.getElementById('addEmployeeVideo');
    const previewContainer = document.getElementById('facePreviewContainer');
    const faceStatus = document.getElementById('faceStatus');
    const btnStart = document.getElementById('btnStartCamera');
    const btnStop = document.getElementById('btnStopCamera');
    const btnCapture = document.getElementById('btnCaptureFace');

    // Stop all tracks
    if (addEmployeeStream) {
        addEmployeeStream.getTracks().forEach(track => {
            track.stop();
            console.log('✓ Stopped camera track:', track.label);
        });
        addEmployeeStream = null;
    }

    // Clear video
    if (video) {
        video.srcObject = null;
        video.pause();
    }

    // Reset UI
    if (previewContainer) previewContainer.style.display = 'none';
    if (btnStart) btnStart.style.display = 'inline-flex';
    if (btnStop) btnStop.style.display = 'none';
    if (btnCapture) btnCapture.style.display = 'none';

    if (faceStatus) {
        faceStatus.innerHTML = '<span class="status-icon">ℹ️</span><span>Nhấn "Bật Camera" để chụp ảnh Face ID</span>';
        faceStatus.className = 'biometric-status info';
    }

    isAddCameraActive = false;
    console.log('✅ Camera stopped');
}

function handleCameraError(error, statusEl) {
    let errorMessage = 'Không thể truy cập camera';

    if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {
        errorMessage = 'Vui lòng cấp quyền truy cập camera';
    } else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {
        errorMessage = 'Không tìm thấy camera';
    } else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {
        errorMessage = 'Camera đang được sử dụng bởi ứng dụng khác';
    } else if (error.message) {
        errorMessage = error.message;
    }

    if (statusEl) {
        statusEl.innerHTML = `<span class="status-icon">❌</span><span>${errorMessage}</span>`;
        statusEl.className = 'biometric-status error';
    }

    alert(`❌ ${errorMessage}\n\nVui lòng:\n- Kiểm tra quyền truy cập camera\n- Đóng các ứng dụng khác đang dùng camera\n- Thử tải lại trang`);
}

// ===========================
// FACE CAPTURE
// ===========================

function captureAddEmployeeFace() {
    console.log('📸 Capturing face...');

    const video = document.getElementById('addEmployeeVideo');
    const canvas = document.getElementById('addEmployeeCanvas');
    const capturedContainer = document.getElementById('capturedFaceContainer');
    const capturedImage = document.getElementById('capturedFaceImage');
    const faceStatus = document.getElementById('faceStatus');

    if (!video || !canvas) {
        console.error('❌ Video or canvas not found');
        alert('⚠️ Lỗi: Không tìm thấy video hoặc canvas');
        return;
    }

    // Check if video is ready
    if (!video.videoWidth || !video.videoHeight || video.readyState !== video.HAVE_ENOUGH_DATA) {
        alert('⚠️ Camera chưa sẵn sàng. Vui lòng đợi vài giây và thử lại.');
        return;
    }

    try {
        // Set canvas size
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        // Draw video frame
        const context = canvas.getContext('2d');
        context.drawImage(video, 0, 0, canvas.width, canvas.height);

        console.log(`✓ Captured frame: ${canvas.width}x${canvas.height}`);

        // Convert to blob
        canvas.toBlob(blob => {
            if (blob) {
                capturedFaceBlob = blob;

                // Display captured image
                const imageUrl = URL.createObjectURL(blob);
                if (capturedImage) {
                    capturedImage.src = imageUrl;
                }
                if (capturedContainer) {
                    capturedContainer.style.display = 'block';
                }

                // Stop camera
                stopAddEmployeeCamera();

                // Update status
                if (faceStatus) {
                    faceStatus.innerHTML = '<span class="status-icon">✅</span><span>Đã chụp ảnh Face ID thành công!</span>';
                    faceStatus.className = 'biometric-status success';
                }

                console.log('✅ Face captured, blob size:', blob.size);
                showNotification('✅ Đã chụp ảnh Face ID thành công!', 'success');
            } else {
                throw new Error('Không thể tạo blob từ canvas');
            }
        }, 'image/jpeg', 0.95);

    } catch (error) {
        console.error('❌ Error capturing face:', error);
        alert('❌ Lỗi khi chụp ảnh: ' + error.message);
    }
}

function deleteCapturedFace() {
    console.log('🗑️ Deleting captured face...');

    const capturedContainer = document.getElementById('capturedFaceContainer');
    const capturedImage = document.getElementById('capturedFaceImage');
    const faceStatus = document.getElementById('faceStatus');

    // Clear blob
    capturedFaceBlob = null;

    // Clear image
    if (capturedImage) {
        if (capturedImage.src) {
            URL.revokeObjectURL(capturedImage.src);
        }
        capturedImage.src = '';
    }

    // Hide container
    if (capturedContainer) {
        capturedContainer.style.display = 'none';
    }

    // Reset status
    if (faceStatus) {
        faceStatus.innerHTML = '<span class="status-icon">ℹ️</span><span>Nhấn "Bật Camera" để chụp ảnh Face ID</span>';
        faceStatus.className = 'biometric-status info';
    }

    console.log('✅ Captured face deleted');
}

// ===========================
// FORM HANDLING
// ===========================

function initializeCurrencyInputs() {
    const currencyInputs = document.querySelectorAll('.currency-input');

    currencyInputs.forEach(input => {
        input.addEventListener('input', function (e) {
            let value = e.target.value.replace(/[^\d]/g, '');
            if (value === '') {
                e.target.value = '0';
                return;
            }
            value = parseInt(value, 10).toString();
            e.target.value = formatCurrency(value);
        });

        input.addEventListener('blur', function (e) {
            if (e.target.value === '') {
                e.target.value = '0';
            }
        });

        if (input.value) {
            input.value = formatCurrency(input.value.replace(/[^\d]/g, ''));
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
// SUBMIT FORM
// ===========================

async function submitAddEmployee() {
    const btnSubmit = document.getElementById('btnSubmitAddEmployee');

    try {
        console.log('📤 Submitting form...');

        // Get form values
        const tenNV = document.getElementById('addTenNV')?.value.trim();
        const sdt = document.getElementById('addSDT')?.value.trim();
        const email = document.getElementById('addEmail')?.value.trim();
        const maNhom = document.getElementById('addMaNhom')?.value;
        const luongCoBan = parseCurrency(document.getElementById('addLuongCoBan')?.value || '0');
        const phuCap = parseCurrency(document.getElementById('addPhuCap')?.value || '0');
        const matKhau = document.getElementById('addMatKhau')?.value;
        const matKhauConfirm = document.getElementById('addMatKhauConfirm')?.value;

        // Validate
        if (!tenNV) {
            alert('⚠️ Vui lòng nhập tên nhân viên');
            document.getElementById('addTenNV')?.focus();
            return;
        }

        if (!sdt) {
            alert('⚠️ Vui lòng nhập số điện thoại');
            document.getElementById('addSDT')?.focus();
            return;
        }

        if (!/^[0-9]{10,11}$/.test(sdt)) {
            alert('⚠️ Số điện thoại phải là 10-11 chữ số');
            document.getElementById('addSDT')?.focus();
            return;
        }

        if (!maNhom) {
            alert('⚠️ Vui lòng chọn nhóm quyền');
            document.getElementById('addMaNhom')?.focus();
            return;
        }

        if (!matKhau) {
            alert('⚠️ Vui lòng nhập mật khẩu');
            document.getElementById('addMatKhau')?.focus();
            return;
        }

        if (matKhau !== matKhauConfirm) {
            alert('⚠️ Mật khẩu xác nhận không khớp');
            document.getElementById('addMatKhauConfirm')?.focus();
            return;
        }

        if (matKhau.length < 6) {
            alert('⚠️ Mật khẩu phải có ít nhất 6 ký tự');
            document.getElementById('addMatKhau')?.focus();
            return;
        }

        // Disable button
        if (btnSubmit) {
            btnSubmit.disabled = true;
            btnSubmit.innerHTML = '<span class="btn-icon">⏳</span><span>Đang xử lý...</span>';
        }

        // Create FormData
        const formData = new FormData();
        formData.append('TenNV', tenNV);
        formData.append('SDT', sdt);
        formData.append('Email', email || '');
        formData.append('MaNhom', maNhom);
        formData.append('LuongCoBan', luongCoBan);
        formData.append('PhuCap', phuCap);
        formData.append('MatKhau', matKhau);

        // Add face image
        if (capturedFaceBlob) {
            formData.append('FaceIDAnh', capturedFaceBlob, 'face.jpg');
            console.log('📸 Including face image, size:', capturedFaceBlob.size);
        }

        // Submit
        const response = await fetch('/NhanVien/ThemNhanVien', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error(`Server error: ${response.status}`);
        }

        const result = await response.json();
        console.log('📥 Response:', result);

        if (result.success) {
            showNotification('✅ ' + result.message, 'success');

            // Cleanup
            stopAddEmployeeCamera();
            deleteCapturedFace();

            // Close panel and reload
            setTimeout(() => {
                closeAddEmployeePanel();
                location.reload();
            }, 1500);
        } else {
            showNotification('❌ ' + result.message, 'error');

            if (btnSubmit) {
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = '<span class="btn-icon">✅</span><span>Thêm nhân viên</span>';
            }
        }

    } catch (error) {
        console.error('❌ Error:', error);
        showNotification('❌ Không thể thêm nhân viên. Vui lòng thử lại.', 'error');

        if (btnSubmit) {
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = '<span class="btn-icon">✅</span><span>Thêm nhân viên</span>';
        }
    }
}

// ===========================
// UTILITIES
// ===========================

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 16px 24px;
        background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#667eea'};
        color: white;
        border-radius: 12px;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
        z-index: 10000;
        animation: slideInRight 0.4s ease;
        font-weight: 600;
        max-width: 400px;
        font-size: 14px;
    `;
    notification.textContent = message;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.4s ease';
        setTimeout(() => notification.remove(), 400);
    }, 3000);
}

// ===========================
// GLOBAL EXPORTS
// ===========================
window.openAddEmployeePanel = openAddEmployeePanel;
window.closeAddEmployeePanel = closeAddEmployeePanel;
window.startAddEmployeeCamera = startAddEmployeeCamera;
window.stopAddEmployeeCamera = stopAddEmployeeCamera;
window.captureAddEmployeeFace = captureAddEmployeeFace;
window.deleteCapturedFace = deleteCapturedFace;
window.submitAddEmployee = submitAddEmployee;

console.log('✅ addEmployeePanel.js loaded');