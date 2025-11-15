// attendance.js - FIXED Face Recognition System

// ===========================
// GLOBAL VARIABLES
// ===========================
let currentAttendanceMonth = new Date().getMonth() + 1;
let currentAttendanceYear = new Date().getFullYear();
let attendanceFaceStream = null;
let todayAttendanceRecord = null;
let recognizedEmployee = null;
let currentMethod = 'faceid';
let faceDetectionInterval = null;
let isRecognizing = false;

// ===========================
// MODAL MANAGEMENT
// ===========================

async function openAttendanceModal() {
    try {
        const response = await fetch(`/NhanVien/GetAttendanceModal`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const html = await response.text();
        openModal('🕐 Chấm công', html);

        // Reset
        recognizedEmployee = null;
        todayAttendanceRecord = null;
        currentMethod = 'faceid';
        isRecognizing = false;

        // Auto-select Face ID method after modal is fully loaded
        setTimeout(() => {
            selectAttendanceMethod('faceid');
        }, 500);
    } catch (error) {
        console.error('❌ Error opening attendance modal:', error);
        showNotification('❌ Không thể mở form chấm công: ' + error.message, 'error');
    }
}

function closeAttendanceModal() {
    console.log('🛑 Closing attendance modal and cleaning up...');

    // Stop camera and clear intervals
    stopFaceCamera();

    // Reset all data
    resetAttendanceState();

    // Close modal
    closeModal();

    console.log('✅ Attendance modal closed and cleaned up');
}
function resetAttendanceState() {
    console.log('🔄 Resetting attendance state...');

    // Clear recognition data
    recognizedEmployee = null;
    todayAttendanceRecord = null;
    isRecognizing = false;
    currentMethod = 'faceid';

    // Clear UI elements if they exist
    const employeeInfo = document.getElementById('recognizedEmployeeInfo');
    if (employeeInfo) {
        employeeInfo.style.display = 'none';
        employeeInfo.innerHTML = '';
    }

    const statusEl = document.getElementById('attendanceStatus');
    if (statusEl) {
        statusEl.innerHTML = `
            <div class="status-info info">
                <span class="status-icon">👤</span>
                <div class="status-content">
                    <div class="status-title">Hệ thống chấm công</div>
                    <div class="status-subtitle">Chọn phương thức chấm công</div>
                </div>
            </div>
        `;
    }

    const submitBtn = document.getElementById('submitAttendanceBtn');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="btn-icon">⏰</span><span>Vui lòng nhận diện</span>';
    }

    const ghiChu = document.getElementById('ghiChu');
    if (ghiChu) {
        ghiChu.value = '';
    }

    const manualInput = document.getElementById('manualMaNV');
    if (manualInput) {
        manualInput.value = '';
    }

    const manualInfo = document.getElementById('manualEmployeeInfo');
    if (manualInfo) {
        manualInfo.style.display = 'none';
        manualInfo.innerHTML = '';
    }
}
// ===========================
// METHOD SELECTION
// ===========================

function selectAttendanceMethod(method) {
    console.log('📋 Selecting method:', method);
    currentMethod = method;

    // Update button states
    document.querySelectorAll('.method-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    const methodBtn = document.querySelector(`[data-method="${method}"]`);
    if (methodBtn) methodBtn.classList.add('active');

    // Show/hide sections
    document.querySelectorAll('.method-section').forEach(section => {
        section.classList.remove('active');
    });
    const methodSection = document.getElementById(`${method}Section`);
    if (methodSection) methodSection.classList.add('active');

    // Reset data when switching methods
    recognizedEmployee = null;
    todayAttendanceRecord = null;
    isRecognizing = false;

    const submitBtn = document.getElementById('submitAttendanceBtn');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = `<span class="btn-icon">⏰</span><span>${method === 'faceid' ? 'Đang khởi động...' : 'Vui lòng nhập mã NV'}</span>`;
    }

    // Handle method-specific logic
    if (method === 'faceid') {
        setTimeout(() => startFaceCamera(), 300);
    } else {
        stopFaceCamera();
        setTimeout(() => {
            const manualInput = document.getElementById('manualMaNV');
            if (manualInput) manualInput.focus();
        }, 100);
    }
}
// ===========================
// CAMERA & FACE RECOGNITION
// ===========================

async function startFaceCamera() {
    console.log('📸 Starting face camera...');

    const video = document.getElementById('attendanceFaceVideo');
    const statusEl = document.getElementById('faceidStatus');

    if (!video || !statusEl) {
        console.error('❌ Video or status element not found');
        return;
    }

    try {
        // Check browser support
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            throw new Error('Trình duyệt không hỗ trợ camera');
        }

        statusEl.innerHTML = '<span class="status-spinner">🔄</span><span>Đang khởi động camera...</span>';
        statusEl.className = 'biometric-status info';

        // Request camera
        const constraints = {
            video: {
                facingMode: 'user',
                width: { ideal: 640, max: 1280 },
                height: { ideal: 480, max: 720 }
            },
            audio: false
        };

        attendanceFaceStream = await navigator.mediaDevices.getUserMedia(constraints);
        video.srcObject = attendanceFaceStream;

        video.onloadedmetadata = () => {
            video.play().then(() => {
                console.log('✅ Camera started successfully');
                statusEl.innerHTML = '<span class="status-icon">✅</span><span>Camera đã sẵn sàng. Đặt khuôn mặt vào khung hình</span>';
                statusEl.className = 'biometric-status success';

                // Start face detection after camera is ready
                setTimeout(() => {
                    startContinuousFaceDetection();
                }, 2000);
            }).catch(err => {
                console.error('❌ Error playing video:', err);
                statusEl.innerHTML = '<span class="status-icon">❌</span><span>Lỗi phát video</span>';
                statusEl.className = 'biometric-status error';
            });
        };

    } catch (error) {
        console.error('❌ Camera error:', error);
        handleCameraError(error, statusEl);
    }
}
function handleCameraError(error, statusEl) {
    let errorMessage = 'Không thể truy cập camera';

    if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {
        errorMessage = 'Vui lòng cấp quyền truy cập camera';
    } else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {
        errorMessage = 'Không tìm thấy camera';
    } else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {
        errorMessage = 'Camera đang được sử dụng bởi ứng dụng khác';
    }

    statusEl.innerHTML = `<span class="status-icon">❌</span><span>${errorMessage}</span>`;
    statusEl.className = 'biometric-status error';

    // Suggest manual method
    setTimeout(() => {
        if (confirm(`${errorMessage}\n\nBạn có muốn chuyển sang chấm công thủ công?`)) {
            openManualAttendanceModal();
        }
    }, 1000);
}

function handleCameraError(error, statusEl) {
    let errorMessage = 'Không thể truy cập camera';

    if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {
        errorMessage = 'Vui lòng cấp quyền truy cập camera';
    } else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {
        errorMessage = 'Không tìm thấy camera';
    } else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {
        errorMessage = 'Camera đang được sử dụng bởi ứng dụng khác';
    }

    statusEl.innerHTML = `<span class="status-icon">❌</span><span>${errorMessage}</span>`;
    statusEl.className = 'biometric-status error';

    // Suggest manual method
    setTimeout(() => {
        if (confirm(`${errorMessage}\n\nBạn có muốn chuyển sang chấm công thủ công?`)) {
            openManualAttendanceModal();
        }
    }, 1000);
}

function stopFaceCamera() {
    console.log('🛑 Stopping face camera...');

    // Clear detection interval
    if (faceDetectionInterval) {
        clearInterval(faceDetectionInterval);
        faceDetectionInterval = null;
        console.log('✓ Cleared face detection interval');
    }

    // Stop video stream
    if (attendanceFaceStream) {
        attendanceFaceStream.getTracks().forEach(track => {
            track.stop();
            console.log('✓ Stopped camera track:', track.label);
        });
        attendanceFaceStream = null;
        console.log('✓ Camera stream released');
    }

    // Clear video element
    const video = document.getElementById('attendanceFaceVideo');
    if (video) {
        video.srcObject = null;
        video.pause();
        console.log('✓ Video element cleared');
    }

    // Reset recognition flag
    isRecognizing = false;

    console.log('✅ Camera fully stopped and cleaned up');
}
function startContinuousFaceDetection() {
    console.log('🔍 Starting continuous face detection...');

    // Clear any existing interval
    if (faceDetectionInterval) {
        clearInterval(faceDetectionInterval);
    }

    // Detect face every 3 seconds
    faceDetectionInterval = setInterval(() => {
        if (attendanceFaceStream && !recognizedEmployee && !isRecognizing) {
            detectAndRecognizeFace();
        }
    }, 3000);

    // Run first detection immediately
    setTimeout(() => {
        if (attendanceFaceStream && !recognizedEmployee && !isRecognizing) {
            detectAndRecognizeFace();
        }
    }, 500);
}

async function detectAndRecognizeFace() {
    if (isRecognizing) {
        console.log('⏳ Already recognizing, skipping...');
        return;
    }

    const video = document.getElementById('attendanceFaceVideo');
    const canvas = document.getElementById('attendanceFaceCanvas');
    const statusEl = document.getElementById('faceidStatus');

    if (!video || !canvas || !statusEl) {
        console.error('❌ Required elements not found');
        return;
    }

    // Check if video is ready
    if (video.readyState !== video.HAVE_ENOUGH_DATA) {
        console.log('⏳ Video not ready yet');
        return;
    }

    isRecognizing = true;

    try {
        console.log('📸 Capturing face image...');

        // Set canvas size
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        // Draw video frame to canvas
        const context = canvas.getContext('2d');
        context.drawImage(video, 0, 0);

        statusEl.innerHTML = '<span class="status-spinner">🔍</span><span>Đang nhận diện khuôn mặt...</span>';
        statusEl.className = 'biometric-status processing';

        // Convert canvas to blob
        const blob = await new Promise((resolve, reject) => {
            canvas.toBlob(blob => {
                if (blob) {
                    resolve(blob);
                } else {
                    reject(new Error('Failed to create blob'));
                }
            }, 'image/jpeg', 0.95);
        });

        console.log('📤 Sending image to server...');

        // Create form data
        const formData = new FormData();
        formData.append('faceImage', blob, 'face.jpg');

        // Send to server for recognition
        const response = await fetch('/NhanVien/RecognizeFaceForAttendance', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error(`Server error: ${response.status}`);
        }

        const result = await response.json();
        console.log('📥 Server response:', result);

        if (result.success && result.employee) {
            // ✅ Face recognized successfully
            console.log('✅ Face recognized:', result.employee.tenNV);

            // Stop further detection
            if (faceDetectionInterval) {
                clearInterval(faceDetectionInterval);
                faceDetectionInterval = null;
            }

            recognizedEmployee = result.employee;
            todayAttendanceRecord = result.todayAttendance;

            statusEl.innerHTML = `
                <span class="status-icon">✅</span>
                <span>Xác thực thành công! Chào ${result.employee.tenNV}</span>
            `;
            statusEl.className = 'biometric-status success';

            // Update UI
            updateAttendanceUIForEmployee(result.employee, result.todayAttendance);

            // Play success sound
            playSuccessBeep();

        } else {
            // ⚠️ Face not recognized
            console.log('⚠️ Face not recognized');
            statusEl.innerHTML = '<span class="status-icon">⚠️</span><span>Chưa nhận diện được. Vui lòng đặt khuôn mặt vào khung...</span>';
            statusEl.className = 'biometric-status info';
        }

    } catch (error) {
        console.error('❌ Face recognition error:', error);
        statusEl.innerHTML = '<span class="status-icon">❌</span><span>Lỗi nhận diện. Đang thử lại...</span>';
        statusEl.className = 'biometric-status error';
    } finally {
        isRecognizing = false;
    }
}

// ===========================
// UI UPDATES
// ===========================

function updateAttendanceUIForEmployee(employee, todayAttendance) {
    console.log('🔄 Updating UI for employee:', employee.tenNV);

    const statusEl = document.getElementById('attendanceStatus');
    const submitBtn = document.getElementById('submitAttendanceBtn');
    const employeeInfoEl = document.getElementById('recognizedEmployeeInfo');

    // Show employee info
    if (employeeInfoEl && employee.tenNV) {
        employeeInfoEl.innerHTML = `
            <div class="employee-recognized">
                <div class="employee-avatar">
                    ${employee.faceIDAnh ?
                `<img src="/asset/img/${employee.faceIDAnh}" alt="${employee.tenNV}">` :
                `<div class="avatar-placeholder">👤</div>`
            }
                </div>
                <div class="employee-details">
                    <div class="employee-name">${employee.tenNV}</div>
                    <div class="employee-role">${employee.tenNhom || 'Nhân viên'} • Ca ${employee.caMacDinh || 'N/A'}</div>
                </div>
            </div>
        `;
        employeeInfoEl.style.display = 'block';
    }

    if (!submitBtn || !statusEl) return;

    if (!todayAttendance || !todayAttendance.gioVao) {
        // Chưa check-in
        statusEl.innerHTML = `
            <div class="status-info pending">
                <span class="status-icon">⏰</span>
                <div class="status-content">
                    <div class="status-title">Chưa chấm công hôm nay</div>
                    <div class="status-subtitle">Sẵn sàng Check-in</div>
                </div>
            </div>
        `;

        submitBtn.innerHTML = `
            <span class="btn-icon">⏰</span>
            <span>Check-in</span>
        `;
        submitBtn.classList.add('btn-checkin');
        submitBtn.classList.remove('btn-checkout');
        submitBtn.disabled = false;

    } else if (todayAttendance.gioVao && !todayAttendance.gioRa) {
        // Đã check-in, chưa check-out
        const gioVao = formatTime(todayAttendance.gioVao);

        statusEl.innerHTML = `
            <div class="status-info success">
                <span class="status-icon">✅</span>
                <div class="status-content">
                    <div class="status-title">Đã Check-in</div>
                    <div class="status-time">${gioVao}</div>
                </div>
            </div>
        `;

        submitBtn.innerHTML = `
            <span class="btn-icon">🚪</span>
            <span>Check-out</span>
        `;
        submitBtn.classList.remove('btn-checkin');
        submitBtn.classList.add('btn-checkout');
        submitBtn.disabled = false;

    } else if (todayAttendance.gioVao && todayAttendance.gioRa) {
        // Đã hoàn thành
        const gioVao = formatTime(todayAttendance.gioVao);
        const gioRa = formatTime(todayAttendance.gioRa);

        statusEl.innerHTML = `
            <div class="status-info complete">
                <span class="status-icon">✅</span>
                <div class="status-content">
                    <div class="status-title">Đã hoàn thành ca làm việc</div>
                    <div class="status-detail">
                        <span>Vào: ${gioVao}</span>
                        <span class="divider">•</span>
                        <span>Ra: ${gioRa}</span>
                        <span class="divider">•</span>
                        <span class="total-hours">${todayAttendance.soGioLam?.toFixed(1) || 0}h</span>
                    </div>
                </div>
            </div>
        `;

        submitBtn.disabled = true;
        submitBtn.innerHTML = `
            <span class="btn-icon">✅</span>
            <span>Đã chấm công đầy đủ</span>
        `;
        submitBtn.classList.add('disabled');
    }
}

function formatTime(dateTimeString) {
    const date = new Date(dateTimeString);
    return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

// ===========================
// SUBMIT ATTENDANCE
// ===========================

async function submitAttendance() {
    if (!recognizedEmployee) {
        showNotification('❌ Vui lòng nhận diện trước khi chấm công', 'error');
        return;
    }

    const submitBtn = document.getElementById('submitAttendanceBtn');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="btn-spinner">⏳</span><span>Đang xử lý...</span>';
    }

    const isCheckIn = !todayAttendanceRecord || !todayAttendanceRecord.gioVao;

    const data = {
        MaNV: recognizedEmployee.maNV,
        IsCheckIn: isCheckIn,
        XacThucBang: currentMethod === 'faceid' ? 2 : 0, // 0=ThuCong, 2=FaceID
        GhiChu: document.getElementById('ghiChu')?.value || ''
    };

    try {
        console.log('📤 Submitting attendance:', data);

        const response = await fetch('/NhanVien/CheckAttendance', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (!response.ok) throw new Error('Failed to submit attendance');

        const result = await response.json();
        console.log('📥 Submit result:', result);

        if (result.success) {
            const actionType = isCheckIn ? 'Check-in' : 'Check-out';
            const now = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

            showNotification(`✅ ${actionType} thành công lúc ${now}!`, 'success');

            // Close modal after success
            setTimeout(() => {
                closeAttendanceModal();

                // Reload attendance if on detail page
                if (window.currentEmployeeId === recognizedEmployee.maNV) {
                    const attendanceTab = document.getElementById('tab-attendance');
                    if (attendanceTab && attendanceTab.classList.contains('active')) {
                        loadAttendanceHistory(recognizedEmployee.maNV);
                    }
                }
            }, 1500);
        } else {
            showNotification('❌ ' + (result.message || 'Không thể chấm công'), 'error');

            if (submitBtn) {
                submitBtn.disabled = false;
                const actionText = isCheckIn ? 'Check-in' : 'Check-out';
                submitBtn.innerHTML = `<span class="btn-icon">${actionText === 'Check-in' ? '⏰' : '🚪'}</span><span>${actionText}</span>`;
            }
        }

    } catch (error) {
        console.error('❌ Error submitting attendance:', error);
        showNotification('❌ Không thể chấm công. Vui lòng thử lại.', 'error');

        if (submitBtn) {
            submitBtn.disabled = false;
            const actionText = (!todayAttendanceRecord || !todayAttendanceRecord.gioVao) ? 'Check-in' : 'Check-out';
            submitBtn.innerHTML = `<span class="btn-icon">${actionText === 'Check-in' ? '⏰' : '🚪'}</span><span>${actionText}</span>`;
        }
    }
}

// ===========================
// MANUAL ATTENDANCE
// ===========================

async function loadEmployeeInfo(maNV) {
    if (!maNV) {
        recognizedEmployee = null;
        todayAttendanceRecord = null;

        const submitBtn = document.getElementById('submitAttendanceBtn');
        const infoDiv = document.getElementById('manualEmployeeInfo');

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="btn-icon">⏰</span><span>Vui lòng nhập mã NV</span>';
        }

        if (infoDiv) {
            infoDiv.style.display = 'none';
        }

        return;
    }

    const infoDiv = document.getElementById('manualEmployeeInfo');
    const submitBtn = document.getElementById('submitAttendanceBtn');

    try {
        // Show loading
        if (infoDiv) {
            infoDiv.style.display = 'block';
            infoDiv.innerHTML = '<div style="text-align: center; padding: 10px;">⏳ Đang tải...</div>';
        }

        // Get today's attendance
        const todayResponse = await fetch(`/NhanVien/GetTodayAttendance?maNV=${maNV}`);
        const todayData = await todayResponse.json();

        const todayAttendance = todayData.success ? todayData : null;

        // Get employee info
        const empResponse = await fetch(`/NhanVien/ChiTietNhanVien?maNV=${maNV}`);

        if (!empResponse.ok) {
            throw new Error('Không tìm thấy nhân viên');
        }

        const empHtml = await empResponse.text();

        // Parse employee name
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = empHtml;
        const empName = tempDiv.querySelector('h3')?.textContent || `Nhân viên #${maNV}`;

        // Show info
        if (infoDiv) {
            infoDiv.style.display = 'block';
            infoDiv.innerHTML = `
                <div class="employee-name">${empName}</div>
                <div class="employee-role">Đã xác thực</div>
            `;
        }

        // Set recognized employee
        recognizedEmployee = {
            maNV: parseInt(maNV),
            tenNV: empName
        };
        todayAttendanceRecord = todayAttendance;

        // Update UI
        updateAttendanceUIForEmployee(recognizedEmployee, todayAttendance);

    } catch (error) {
        console.error('Error loading employee:', error);

        if (infoDiv) {
            infoDiv.style.display = 'block';
            infoDiv.innerHTML = `
                <div style="text-align: center; padding: 10px; color: #dc3545;">
                    ❌ ${error.message}
                </div>
            `;
        }

        showNotification('❌ ' + error.message, 'error');

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="btn-icon">⏰</span><span>Mã NV không hợp lệ</span>';
        }
    }
}

// ===========================
// HELPERS
// ===========================

function playSuccessBeep() {
    try {
        const audio = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBSuBzvLZiTUIGWi77eeeTRAMUKfj8LZjHAY4ktfzyn0xBSp+zPLaizsIHWS07uiiUBELTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwI=');
        audio.play().catch(e => console.log('🔇 Sound play failed:', e));
    } catch (e) {
        console.log('🔇 Sound not supported');
    }
}

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
// ATTENDANCE HISTORY
// ===========================

async function changeMonth(delta) {
    currentAttendanceMonth += delta;

    if (currentAttendanceMonth > 12) {
        currentAttendanceMonth = 1;
        currentAttendanceYear++;
    } else if (currentAttendanceMonth < 1) {
        currentAttendanceMonth = 12;
        currentAttendanceYear--;
    }

    const monthDisplay = document.getElementById('currentMonth');
    if (monthDisplay) {
        monthDisplay.textContent = `Tháng ${currentAttendanceMonth}/${currentAttendanceYear}`;
    }

    const maNV = window.currentEmployeeId;
    if (maNV) {
        await loadAttendanceHistory(maNV);
    }
}

async function loadAttendanceHistory(maNV) {
    const container = document.getElementById('attendanceList');
    if (!container) return;

    try {
        container.innerHTML = '<div class="loading-spinner">⏳ Đang tải dữ liệu...</div>';

        const response = await fetch(`/NhanVien/GetAttendanceHistory/${maNV}?month=${currentAttendanceMonth}&year=${currentAttendanceYear}`);
        if (!response.ok) throw new Error('Failed to load attendance');

        const records = await response.json();

        if (records.length === 0) {
            container.innerHTML = `
                <div class="empty-state-small">
                    <div class="empty-icon">📅</div>
                    <p>Chưa có dữ liệu chấm công tháng ${currentAttendanceMonth}/${currentAttendanceYear}</p>
                </div>
            `;
            updateAttendanceStats(0, 0, 0);
            return;
        }

        const totalDays = records.length;
        const totalHours = records.reduce((sum, r) => sum + (r.soGioLam || 0), 0);
        const lateDays = records.filter(r => r.trangThai === 'DiTre').length;

        updateAttendanceStats(totalDays, totalHours, lateDays);

        let html = '<div class="attendance-items">';

        records.forEach(record => {
            const date = new Date(record.ngay);
            const dayOfWeek = date.toLocaleDateString('vi-VN', { weekday: 'long' });
            const dateStr = date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });

            const gioVao = record.gioVao ? new Date(record.gioVao).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '--:--';
            const gioRa = record.gioRa ? new Date(record.gioRa).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '--:--';

            const statusInfo = getStatusInfo(record.trangThai);

            html += `
                <div class="attendance-item ${statusInfo.class}">
                    <div class="attendance-date-section">
                        <div class="day-number">${date.getDate()}</div>
                        <div class="day-info">
                            <div class="day-name">${dayOfWeek}</div>
                            <div class="date-full">${dateStr}</div>
                        </div>
                    </div>
                    
                    <div class="attendance-time-section">
                        <div class="time-item">
                            <span class="time-icon">⏰</span>
                            <div class="time-info">
                                <span class="time-label">Check-in</span>
                                <span class="time-value ${record.gioVao ? 'has-time' : 'no-time'}">${gioVao}</span>
                            </div>
                        </div>
                        
                        <div class="time-divider">→</div>
                        
                        <div class="time-item">
                            <span class="time-icon">🚪</span>
                            <div class="time-info">
                                <span class="time-label">Check-out</span>
                                <span class="time-value ${record.gioRa ? 'has-time' : 'no-time'}">${gioRa}</span>
                            </div>
                        </div>
                        
                        <div class="time-divider">→</div>
                        
                        <div class="time-item total">
                            <span class="time-icon">⏱️</span>
                            <div class="time-info">
                                <span class="time-label">Tổng</span>
                                <span class="time-value">${record.soGioLam ? record.soGioLam.toFixed(1) + 'h' : '0h'}</span>
                            </div>
                        </div>
                    </div>
                    
                    <div class="attendance-status-section">
                        <span class="status-badge ${statusInfo.class}">
                            <span class="status-icon">${statusInfo.icon}</span>
                            <span class="status-text">${statusInfo.text}</span>
                        </span>
                        ${record.xacThucBang ? `
                            <div class="auth-method">${getAuthIcon(record.xacThucBang)} ${record.xacThucBang}</div>
                        ` : ''}
                    </div>
                    
                    ${record.ghiChu ? `
                        <div class="attendance-note">
                            <span class="note-icon">📝</span>
                            <span class="note-text">${record.ghiChu}</span>
                        </div>
                    ` : ''}
                </div>
            `;
        });

        html += '</div>';
        container.innerHTML = html;

    } catch (error) {
        console.error('Error loading attendance:', error);
        container.innerHTML = `
            <div class="error-state">
                <div class="error-icon">⚠️</div>
                <p>Không thể tải dữ liệu chấm công</p>
                <button class="btn-retry" onclick="loadAttendanceHistory(${maNV})">🔄 Thử lại</button>
            </div>
        `;
    }
}

function updateAttendanceStats(days, hours, late) {
    const statDays = document.getElementById('statDays');
    const statHours = document.getElementById('statHours');
    const statLate = document.getElementById('statLate');

    if (statDays) statDays.textContent = days;
    if (statHours) statHours.textContent = hours.toFixed(1);
    if (statLate) statLate.textContent = late;
}

function getStatusInfo(status) {
    const statusMap = {
        'DungGio': { icon: '✅', text: 'Đúng giờ', class: 'status-success' },
        'DiTre': { icon: '⚠️', text: 'Đi trễ', class: 'status-warning' },
        'VeSom': { icon: '⏰', text: 'Về sớm', class: 'status-info' },
        'Vang': { icon: '❌', text: 'Vắng', class: 'status-danger' },
        'CaThem': { icon: '⭐', text: 'Ca thêm', class: 'status-primary' }
    };

    return statusMap[status] || { icon: '📝', text: status, class: 'status-default' };
}

function getAuthIcon(method) {
    const iconMap = {
        'ThuCong': '✍️',
        'FaceID': '📸',
        'VanTay': '👆'
    };
    return iconMap[method] || '📝';
}

// ===========================
// MANUAL ATTENDANCE MODAL
// ===========================

async function openManualAttendanceModal() {
    try {
        const response = await fetch('/NhanVien/GetManualAttendanceModal');

        if (!response.ok) {
            throw new Error('Failed to load form');
        }

        const html = await response.text();
        openModal('✍️ Chấm công thủ công', html);

        setTimeout(() => {
            setupManualAttendanceListeners();
        }, 300);

    } catch (error) {
        console.error('Error:', error);
        showNotification('❌ Không thể mở form chấm công', 'error');
    }
}

function setupManualAttendanceListeners() {
    const input = document.getElementById('manualSDT');
    if (input) {
        input.addEventListener('input', debounce(async function (e) {
            const sdt = e.target.value.trim();

            if (!sdt || sdt.length < 10) {
                hideManualEmployeeInfo();
                return;
            }

            if (!/^[0-9]{10,11}$/.test(sdt)) {
                hideManualEmployeeInfo();
                return;
            }

            await loadManualEmployeeInfoByPhone(sdt);
        }, 500));
    }
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Global variables for manual attendance
let currentManualEmployee = null;
let currentManualAttendance = null;

async function loadManualEmployeeInfoByPhone(sdt) {
    const infoBox = document.getElementById('manualEmployeeInfo');
    const statusBox = document.getElementById('manualAttendanceStatus');
    const btnCheckIn = document.getElementById('btnCheckIn');
    const btnCheckOut = document.getElementById('btnCheckOut');

    try {
        infoBox.style.display = 'block';
        infoBox.innerHTML = '<div style="text-align: center;">⏳ Đang tìm nhân viên...</div>';

        const searchResponse = await fetch(`/NhanVien/SearchEmployeeByPhone?sdt=${encodeURIComponent(sdt)}`);

        if (!searchResponse.ok) {
            throw new Error('Không tìm thấy nhân viên với số điện thoại này');
        }

        const employeeData = await searchResponse.json();

        if (!employeeData.success || !employeeData.employee) {
            throw new Error('Không tìm thấy nhân viên với số điện thoại này');
        }

        const employee = employeeData.employee;

        if (employee.trangThai !== 'DangLam') {
            throw new Error('Nhân viên này đã nghỉ việc');
        }

        const todayResponse = await fetch(`/NhanVien/GetTodayAttendance?maNV=${employee.maNV}`);
        const todayData = await todayResponse.json();

        currentManualEmployee = {
            maNV: employee.maNV,
            tenNV: employee.tenNV,
            sdt: employee.sdt,
            tenNhom: employee.tenNhom,
            caMacDinh: employee.caMacDinh
        };
        currentManualAttendance = todayData.success ? todayData : null;

        infoBox.innerHTML = `
            <div style="display: flex; align-items: center; gap: 15px;">
                <div style="width: 50px; height: 50px; background: linear-gradient(135deg, #667eea, #764ba2); border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; font-size: 20px; font-weight: 700;">
                    ${getInitials(employee.tenNV)}
                </div>
                <div style="flex: 1;">
                    <div class="emp-name">${employee.tenNV}</div>
                    <div class="emp-role">${employee.tenNhom} • Ca ${employee.caMacDinh}</div>
                    <div style="font-size: 12px; opacity: 0.8; margin-top: 2px;">📱 ${employee.sdt}</div>
                </div>
                <div style="font-size: 24px;">✅</div>
            </div>
        `;

        statusBox.style.display = 'block';

        if (!currentManualAttendance || !currentManualAttendance.gioVao) {
            statusBox.className = 'attendance-status-box status-pending';
            statusBox.innerHTML = `
                <div style="font-weight: 700; margin-bottom: 5px;">⏰ Chưa chấm công hôm nay</div>
                <div style="font-size: 13px;">Sẵn sàng check-in</div>
            `;
            btnCheckIn.disabled = false;
            btnCheckOut.disabled = true;
        } else if (currentManualAttendance.gioVao && !currentManualAttendance.gioRa) {
            const gioVao = new Date(currentManualAttendance.gioVao);
            const now = new Date();
            const workingHours = ((now - gioVao) / (1000 * 60 * 60)).toFixed(1);

            statusBox.className = 'attendance-status-box status-active';
            statusBox.innerHTML = `
                <div style="font-weight: 700; margin-bottom: 8px;">✅ Đã check-in</div>
                <div style="font-size: 13px;">
                    Giờ vào: ${gioVao.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} • 
                    Đang làm: ${workingHours}h
                </div>
            `;
            btnCheckIn.disabled = true;
            btnCheckOut.disabled = false;
        } else {
            const gioVao = new Date(currentManualAttendance.gioVao);
            const gioRa = new Date(currentManualAttendance.gioRa);
            statusBox.className = 'attendance-status-box status-complete';
            statusBox.innerHTML = `
                <div style="font-weight: 700; margin-bottom: 8px;">✅ Đã hoàn thành ca làm việc</div>
                <div style="font-size: 13px;">
                    ${gioVao.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} → 
                    ${gioRa.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} • 
                    ${currentManualAttendance.soGioLam?.toFixed(1)}h
                </div>
            `;
            btnCheckIn.disabled = true;
            btnCheckOut.disabled = true;
        }

    } catch (error) {
        console.error('Error:', error);
        infoBox.style.display = 'block';
        infoBox.innerHTML = `
            <div style="text-align: center; padding: 15px;">
                <div style="font-size: 32px; margin-bottom: 10px;">❌</div>
                <div style="color: #dc3545; font-weight: 600;">${error.message}</div>
            </div>
        `;
        if (statusBox) statusBox.style.display = 'none';
        if (btnCheckIn) btnCheckIn.disabled = true;
        if (btnCheckOut) btnCheckOut.disabled = true;
    }
}

function getInitials(name) {
    if (!name) return '?';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
        return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
}

function hideManualEmployeeInfo() {
    const infoBox = document.getElementById('manualEmployeeInfo');
    const statusBox = document.getElementById('manualAttendanceStatus');
    const btnCheckIn = document.getElementById('btnCheckIn');
    const btnCheckOut = document.getElementById('btnCheckOut');

    if (infoBox) infoBox.style.display = 'none';
    if (statusBox) statusBox.style.display = 'none';
    if (btnCheckIn) btnCheckIn.disabled = true;
    if (btnCheckOut) btnCheckOut.disabled = true;

    currentManualEmployee = null;
    currentManualAttendance = null;
}

async function submitManualAttendance(isCheckIn) {
    if (!currentManualEmployee) {
        showNotification('❌ Vui lòng nhập số điện thoại nhân viên', 'error');
        return;
    }

    const ghiChu = document.getElementById('manualGhiChu')?.value || '';
    const btnCheckIn = document.getElementById('btnCheckIn');
    const btnCheckOut = document.getElementById('btnCheckOut');

    if (btnCheckIn) btnCheckIn.disabled = true;
    if (btnCheckOut) btnCheckOut.disabled = true;

    try {
        const response = await fetch('/NhanVien/ManualCheckAttendance', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                MaNV: currentManualEmployee.maNV,
                IsCheckIn: isCheckIn,
                XacThucBang: 0, // ThuCong
                GhiChu: ghiChu
            })
        });

        const result = await response.json();

        if (result.success) {
            const actionType = isCheckIn ? 'Check-in' : 'Check-out';
            const now = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

            showNotification(`✅ ${actionType} thành công lúc ${now}!`, 'success');

            setTimeout(() => {
                closeModal();

                if (window.currentEmployeeId === currentManualEmployee.maNV) {
                    const attendanceTab = document.getElementById('tab-attendance');
                    if (attendanceTab && attendanceTab.classList.contains('active')) {
                        loadAttendanceHistory(currentManualEmployee.maNV);
                    }
                }
            }, 1500);
        } else {
            showNotification('❌ ' + (result.message || 'Không thể chấm công'), 'error');

            if (isCheckIn && btnCheckIn) btnCheckIn.disabled = false;
            if (!isCheckIn && btnCheckOut) btnCheckOut.disabled = false;
        }

    } catch (error) {
        console.error('Error:', error);
        showNotification('❌ Không thể chấm công. Vui lòng thử lại.', 'error');

        if (isCheckIn && btnCheckIn) btnCheckIn.disabled = false;
        if (!isCheckIn && btnCheckOut) btnCheckOut.disabled = false;
    }
}

// ===========================
// GLOBAL EXPORTS
// ===========================
window.openAttendanceModal = openAttendanceModal;
window.closeAttendanceModal = closeAttendanceModal;
window.selectAttendanceMethod = selectAttendanceMethod;
window.submitAttendance = submitAttendance;
window.changeMonth = changeMonth;
window.loadAttendanceHistory = loadAttendanceHistory;
window.loadEmployeeInfo = loadEmployeeInfo;
window.openManualAttendanceModal = openManualAttendanceModal;
window.loadManualEmployeeInfoByPhone = loadManualEmployeeInfoByPhone;
window.submitManualAttendance = submitManualAttendance;

console.log('✅ Attendance.js loaded - Face Recognition System Ready');