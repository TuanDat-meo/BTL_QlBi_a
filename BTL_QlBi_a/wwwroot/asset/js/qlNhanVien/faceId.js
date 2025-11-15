//// =====================================================
//// FACE ID ATTENDANCE - CAMERA & RECOGNITION MANAGER
//// =====================================================

//let faceStream = null;
//let faceVideo = null;
//let faceCanvas = null;
//let faceContext = null;
//let recognitionInterval = null;
//let isProcessing = false;

//// =====================================================
//// CAMERA INITIALIZATION
//// =====================================================

//async function startFaceCamera() {
//    console.log('📸 Starting face camera...');

//    faceVideo = document.getElementById('attendanceFaceVideo');
//    faceCanvas = document.getElementById('attendanceFaceCanvas');

//    if (!faceVideo || !faceCanvas) {
//        console.error('❌ Video or Canvas element not found');
//        updateFaceidStatus('error', '❌ Không tìm thấy camera element');
//        return;
//    }

//    faceContext = faceCanvas.getContext('2d');

//    try {
//        updateFaceidStatus('info', '📸 Đang yêu cầu quyền camera...');

//        // Request camera permission with constraints
//        const constraints = {
//            video: {
//                width: { ideal: 640 },
//                height: { ideal: 480 },
//                facingMode: 'user' // Front camera
//            },
//            audio: false
//        };

//        faceStream = await navigator.mediaDevices.getUserMedia(constraints);

//        faceVideo.srcObject = faceStream;

//        // Wait for video to be ready
//        faceVideo.onloadedmetadata = () => {
//            console.log('✅ Camera stream ready');
//            faceVideo.play();

//            // Set canvas size to match video
//            faceCanvas.width = faceVideo.videoWidth;
//            faceCanvas.height = faceVideo.videoHeight;

//            updateFaceidStatus('success', '✅ Camera đã sẵn sàng - Đang nhận diện...');

//            // Start face recognition
//            startFaceRecognition();
//        };

//    } catch (error) {
//        console.error('❌ Camera error:', error);
//        handleCameraError(error);
//    }
//}

//function handleCameraError(error) {
//    let message = '❌ Không thể truy cập camera: ';

//    if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {
//        message += 'Vui lòng cấp quyền truy cập camera';
//    } else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {
//        message += 'Không tìm thấy camera';
//    } else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {
//        message += 'Camera đang được sử dụng bởi ứng dụng khác';
//    } else {
//        message += error.message;
//    }

//    updateFaceidStatus('error', message);

//    // Show instructions
//    showCameraErrorInstructions();
//}

//function showCameraErrorInstructions() {
//    const statusDiv = document.getElementById('faceidStatus');
//    if (statusDiv) {
//        statusDiv.innerHTML = `
//            <span class="status-icon">⚠️</span>
//            <div style="flex: 1;">
//                <div style="font-weight: 600; margin-bottom: 5px;">Camera không khả dụng</div>
//                <div style="font-size: 12px; opacity: 0.8;">
//                    • Kiểm tra camera đã kết nối<br>
//                    • Cho phép truy cập camera trong trình duyệt<br>
//                    • Đóng các ứng dụng khác đang dùng camera
//                </div>
//                <button onclick="retryCamera()" style="margin-top: 10px; padding: 5px 15px; background: #667eea; color: white; border: none; border-radius: 5px; cursor: pointer;">
//                    🔄 Thử lại
//                </button>
//            </div>
//        `;
//    }
//}

//function retryCamera() {
//    stopFaceCamera();
//    setTimeout(() => startFaceCamera(), 500);
//}

//function stopFaceCamera() {
//    console.log('🛑 Stopping face camera...');

//    // Stop recognition
//    if (recognitionInterval) {
//        clearInterval(recognitionInterval);
//        recognitionInterval = null;
//    }

//    // Stop video stream
//    if (faceStream) {
//        faceStream.getTracks().forEach(track => track.stop());
//        faceStream = null;
//    }

//    // Clear video
//    if (faceVideo) {
//        faceVideo.srcObject = null;
//    }

//    isProcessing = false;
//}

//// =====================================================
//// FACE RECOGNITION
//// =====================================================

//function startFaceRecognition() {
//    console.log('🔍 Starting face recognition...');

//    // Auto-capture every 2 seconds
//    recognitionInterval = setInterval(() => {
//        if (!isProcessing && faceVideo && faceVideo.readyState === faceVideo.HAVE_ENOUGH_DATA) {
//            captureAndRecognizeFace();
//        }
//    }, 2000);
//}

//async function captureAndRecognizeFace() {
//    if (isProcessing) return;

//    try {
//        isProcessing = true;
//        updateFaceidStatus('processing', '🔄 Đang nhận diện khuôn mặt...');

//        // Capture frame from video
//        faceContext.drawImage(faceVideo, 0, 0, faceCanvas.width, faceCanvas.height);

//        // Convert to blob
//        const blob = await new Promise(resolve => faceCanvas.toBlob(resolve, 'image/jpeg', 0.8));

//        // Send to server for recognition
//        const formData = new FormData();
//        formData.append('faceImage', blob, 'face.jpg');

//        const response = await fetch('/NhanVien/RecognizeFace', {
//            method: 'POST',
//            body: formData
//        });

//        if (!response.ok) {
//            throw new Error('Recognition failed: ' + response.statusText);
//        }

//        const result = await response.json();

//        if (result.success && result.employee) {
//            // Face recognized successfully
//            handleFaceRecognized(result.employee, result.todayAttendance);
//        } else {
//            // Face not recognized
//            updateFaceidStatus('info', '👤 Đang tìm kiếm khuôn mặt...');
//        }

//    } catch (error) {
//        console.error('❌ Recognition error:', error);
//        updateFaceidStatus('error', '❌ Lỗi nhận diện: ' + error.message);
//    } finally {
//        isProcessing = false;
//    }
//}

//function handleFaceRecognized(employee, todayAttendance) {
//    console.log('✅ Face recognized:', employee);

//    // Stop auto-recognition
//    if (recognitionInterval) {
//        clearInterval(recognitionInterval);
//        recognitionInterval = null;
//    }

//    // Store recognized employee
//    window.recognizedEmployee = employee;
//    window.todayAttendanceRecord = todayAttendance;

//    // Update UI
//    updateFaceidStatus('success', `✅ Nhận diện thành công: ${employee.tenNV || 'NV #' + employee.maNV}`);

//    // Show employee info
//    showRecognizedEmployeeInfo(employee);

//    // Update attendance UI
//    updateAttendanceUIForEmployee(employee, todayAttendance);

//    // Play success sound (optional)
//    playSuccessSound();
//}

//function showRecognizedEmployeeInfo(employee) {
//    const infoDiv = document.getElementById('recognizedEmployeeInfo');
//    if (!infoDiv) return;

//    infoDiv.innerHTML = `
//        <div class="employee-recognized">
//            <div class="employee-avatar">
//                ${employee.anhDaiDien ?
//            `<img src="${employee.anhDaiDien}" alt="${employee.tenNV}">` :
//            `<div class="avatar-placeholder">👤</div>`
//        }
//            </div>
//            <div class="employee-details">
//                <div class="employee-name">${employee.tenNV || 'Nhân viên #' + employee.maNV}</div>
//                <div class="employee-role">${employee.tenNhom || employee.chucVu || 'Nhân viên'}</div>
//            </div>
//        </div>
//    `;

//    infoDiv.style.display = 'block';
//}

//// =====================================================
//// UI UPDATES
//// =====================================================

//function updateFaceidStatus(type, message) {
//    const statusDiv = document.getElementById('faceidStatus');
//    if (!statusDiv) return;

//    // Remove all status classes
//    statusDiv.className = 'biometric-status';

//    // Add new status class
//    statusDiv.classList.add(type);

//    // Update icon based on type
//    let icon = '📸';
//    if (type === 'success') icon = '✅';
//    else if (type === 'processing') icon = '🔄';
//    else if (type === 'error') icon = '❌';
//    else if (type === 'info') icon = '👤';

//    statusDiv.innerHTML = `
//        <span class="status-icon ${type === 'processing' ? 'status-spinner' : ''}">${icon}</span>
//        <span>${message}</span>
//    `;
//}

//function updateAttendanceUIForEmployee(employee, todayAttendance) {
//    console.log('🔄 Updating attendance UI:', { employee, todayAttendance });

//    const statusSection = document.getElementById('attendanceStatus');
//    const submitBtn = document.getElementById('submitAttendanceBtn');

//    if (!submitBtn) return;

//    let statusHtml = '';
//    let buttonHtml = '';
//    let isDisabled = false;

//    if (!todayAttendance) {
//        // No attendance today - allow check-in
//        statusHtml = `
//            <div class="status-info pending">
//                <span class="status-icon">⏰</span>
//                <div class="status-content">
//                    <div class="status-title">Chưa chấm công hôm nay</div>
//                    <div class="status-subtitle">Nhấn nút bên dưới để Check-in</div>
//                </div>
//            </div>
//        `;

//        buttonHtml = `
//            <span class="btn-icon">🟢</span>
//            <span>Check-in</span>
//        `;

//        submitBtn.className = 'btn btn-primary btn-checkin';

//    } else if (todayAttendance.gioVao && !todayAttendance.gioRa) {
//        // Already checked in - allow check-out
//        statusHtml = `
//            <div class="status-info success">
//                <span class="status-icon">✅</span>
//                <div class="status-content">
//                    <div class="status-title">Đã Check-in</div>
//                    <div class="status-subtitle">
//                        <span class="status-time">${todayAttendance.gioVao}</span>
//                        <span class="status-detail">
//                            <span class="divider">|</span>
//                            <span>Làm việc: ${todayAttendance.soGioLam || 0}h</span>
//                        </span>
//                    </div>
//                </div>
//            </div>
//        `;

//        buttonHtml = `
//            <span class="btn-icon">🔴</span>
//            <span>Check-out</span>
//        `;

//        submitBtn.className = 'btn btn-primary btn-checkout';

//    } else if (todayAttendance.gioVao && todayAttendance.gioRa) {
//        // Already completed today
//        statusHtml = `
//            <div class="status-info complete">
//                <span class="status-icon">✔️</span>
//                <div class="status-content">
//                    <div class="status-title">Đã hoàn thành chấm công hôm nay</div>
//                    <div class="status-subtitle">
//                        <span>${todayAttendance.gioVao} → ${todayAttendance.gioRa}</span>
//                        <span class="status-detail">
//                            <span class="divider">|</span>
//                            <span class="total-hours">Tổng: ${todayAttendance.soGioLam || 0}h</span>
//                        </span>
//                    </div>
//                </div>
//            </div>
//        `;

//        buttonHtml = `
//            <span class="btn-icon">✔️</span>
//            <span>Đã chấm công xong</span>
//        `;

//        submitBtn.className = 'btn btn-primary disabled';
//        isDisabled = true;
//    }

//    // Update UI
//    if (statusSection) {
//        statusSection.innerHTML = statusHtml;
//    }

//    submitBtn.innerHTML = buttonHtml;
//    submitBtn.disabled = isDisabled;

//    // Add pulse effect if ready
//    if (!isDisabled) {
//        submitBtn.classList.add('ready');
//    }
//}

//// =====================================================
//// HELPER FUNCTIONS
//// =====================================================

//function playSuccessSound() {
//    try {
//        const audio = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBSuBzvLZiTUIGWi77eeeTRAMUKfj8LZjHAY4ktfzyn0xBSp+zPLaizsIHWS07uiiUBELTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwIHWS07uijUBEMTKXh8bllHAU2jdXzzn4yBSh+y/LajDwI=');
//        audio.play().catch(e => console.log('Sound play failed:', e));
//    } catch (e) {
//        console.log('Sound not supported');
//    }
//}

//// =====================================================
//// MANUAL ATTENDANCE
//// =====================================================

//async function loadEmployeeInfo(maNV) {
//    if (!maNV) return;

//    const infoDiv = document.getElementById('manualEmployeeInfo');
//    const submitBtn = document.getElementById('submitAttendanceBtn');

//    try {
//        // Show loading
//        if (infoDiv) {
//            infoDiv.style.display = 'block';
//            infoDiv.innerHTML = '<div style="text-align: center; padding: 15px;">⏳ Đang tải...</div>';
//        }

//        // Fetch employee data
//        const response = await fetch(`/NhanVien/GetEmployeeInfo?maNV=${maNV}`);
//        if (!response.ok) {
//            throw new Error('Không tìm thấy nhân viên');
//        }

//        const employee = await response.json();

//        // Get today's attendance
//        const todayResponse = await fetch(`/NhanVien/GetTodayAttendance?maNV=${maNV}`);
//        const todayAttendance = await todayResponse.json();

//        // Show employee info
//        if (infoDiv) {
//            infoDiv.innerHTML = `
//                <div class="employee-name">👤 ${employee.tenNV || 'NV #' + employee.maNV}</div>
//                <div class="employee-role">${employee.tenNhom || employee.chucVu || 'Nhân viên'}</div>
//            `;
//        }

//        // Set recognized employee
//        window.recognizedEmployee = employee;
//        window.todayAttendanceRecord = todayAttendance;

//        // Update UI
//        updateAttendanceUIForEmployee(employee, todayAttendance);

//    } catch (error) {
//        console.error('Error loading employee:', error);
//        if (infoDiv) {
//            infoDiv.style.display = 'none';
//        }

//        showNotification('❌ ' + error.message, 'error');

//        if (submitBtn) {
//            submitBtn.disabled = true;
//            submitBtn.innerHTML = '<span class="btn-icon">⏰</span><span>Vui lòng nhập mã NV hợp lệ</span>';
//        }
//    }
//}

//// =====================================================
//// CLEANUP ON MODAL CLOSE
//// =====================================================

//function cleanupFaceRecognition() {
//    stopFaceCamera();
//    window.recognizedEmployee = null;
//    window.todayAttendanceRecord = null;
//}

//// Export functions
//window.startFaceCamera = startFaceCamera;
//window.stopFaceCamera = stopFaceCamera;
//window.retryCamera = retryCamera;
//window.loadEmployeeInfo = loadEmployeeInfo;
//window.cleanupFaceRecognition = cleanupFaceRecognition;