// Quản lý chấm công
function openAttendanceModal(maNV) {
    const content = `
        <div class="attendance-modal">
            <div class="info-card" style="margin-bottom: 20px; background: linear-gradient(135deg, #667eea, #764ba2); color: white;">
                <h3 style="margin: 0 0 10px 0;">Chấm công hôm nay</h3>
                <p style="margin: 0; font-size: 14px; opacity: 0.9;">
                    ${new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
                </p>
            </div>

            <div class="form-group">
                <label class="form-label">Phương thức chấm công:</label>
                <select class="form-control" id="attendanceMethod" onchange="changeAttendanceMethod()">
                    <option value="manual">Thủ công</option>
                    <option value="faceid">Face ID</option>
                    <option value="fingerprint">Vân tay</option>
                </select>
            </div>

            <!-- Manual Input -->
            <div id="manualSection">
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label">Giờ vào *</label>
                        <input type="time" class="form-control" id="gioVao" value="${new Date().toTimeString().slice(0, 5)}">
                    </div>
                    <div class="form-group">
                        <label class="form-label">Giờ ra</label>
                        <input type="time" class="form-control" id="gioRa">
                    </div>
                </div>
                <div class="form-group">
                    <label class="form-label">Ghi chú</label>
                    <textarea class="form-control" id="ghiChu" rows="3" placeholder="Ghi chú về ca làm việc..."></textarea>
                </div>
            </div>

            <!-- Face ID Section -->
            <div id="faceidSection" style="display: none;">
                <div class="face-capture-section">
                    <div class="face-preview">
                        <video id="attendanceFaceVideo" autoplay></video>
                        <canvas id="attendanceFaceCanvas" style="display: none;"></canvas>
                        <div class="face-overlay"></div>
                    </div>
                    <p id="faceidStatus" style="text-align: center; margin-top: 15px; color: #667eea; font-weight: 600;">
                        Đang khởi động camera...
                    </p>
                </div>
            </div>

            <!-- Fingerprint Section -->
            <div id="fingerprintSection" style="display: none;">
                <div class="fingerprint-section">
                    <div class="fingerprint-animation">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M2 12C2 6.5 6.5 2 12 2a10 10 0 0 1 8 4"></path>
                            <path d="M5 19.5C5.5 18 6 15 6 12c0-.7.12-1.37.34-2"></path>
                            <path d="M17.29 21.02c.12-.6.43-2.3.5-3.02"></path>
                            <path d="M12 10a2 2 0 0 0-2 2c0 1.02-.1 2.51-.26 4"></path>
                            <path d="M8.65 22c.21-.66.45-1.32.57-2"></path>
                            <path d="M14 13.12c0 2.38 0 6.38-1 8.88"></path>
                            <path d="M2 16h.01"></path>
                            <path d="M21.8 16c.2-2 .131-5.354 0-6"></path>
                            <path d="M9 6.8a6 6 0 0 1 9 5.2c0 .47 0 1.17-.02 2"></path>
                        </svg>
                    </div>
                    <p id="fingerprintStatus" style="text-align: center; margin-top: 15px; color: #667eea; font-weight: 600;">
                        Đặt ngón tay lên máy quét...
                    </p>
                    <button class="btn btn-primary" onclick="scanFingerprint(${maNV})" style="width: 100%; margin-top: 15px;">
                        Quét vân tay
                    </button>
                </div>
            </div>

            <div class="info-card" style="margin-top: 20px;">
                <h4 style="margin-bottom: 10px;">Lưu ý:</h4>
                <ul style="font-size: 12px; color: #6c757d; margin: 0; padding-left: 20px;">
                    <li>Giờ chuẩn vào: 7:00 - Trễ sau 7:15</li>
                    <li>Giờ chuẩn ra: 15:00 (ca sáng), 23:00 (ca chiều), 03:00 (ca tối)</li>
                    <li>Chấm công vào đầu ca, chấm công ra cuối ca</li>
                    <li>Nếu quên chấm công, liên hệ quản lý để điều chỉnh</li>
                </ul>
            </div>

            <div class="action-buttons" style="margin-top: 20px;">
                <button class="btn btn-primary" onclick="submitAttendance(${maNV})" id="submitAttendanceBtn">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                    Chấm công
                </button>
                <button class="btn btn-secondary" onclick="closeAttendanceModal()">Đóng</button>
            </div>
        </div>
    `;

    openModal('Chấm công', content);
}

function changeAttendanceMethod() {
    const method = document.getElementById('attendanceMethod').value;

    document.getElementById('manualSection').style.display = method === 'manual' ? 'block' : 'none';
    document.getElementById('faceidSection').style.display = method === 'faceid' ? 'block' : 'none';
    document.getElementById('fingerprintSection').style.display = method === 'fingerprint' ? 'block' : 'none';

    if (method === 'faceid') {
        startAttendanceFaceCamera();
    } else {
        stopAttendanceFaceCamera();
    }

    if (method === 'fingerprint') {
        initFingerprintReader();
    }
}

// Face ID attendance
let attendanceFaceStream = null;

async function startAttendanceFaceCamera() {
    const video = document.getElementById('attendanceFaceVideo');
    const statusEl = document.getElementById('faceidStatus');

    try {
        attendanceFaceStream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: 'user' }
        });

        video.srcObject = attendanceFaceStream;
        video.onloadedmetadata = () => {
            video.play();
            statusEl.textContent = '✓ Đã sẵn sàng. Đặt khuôn mặt vào khung';
            statusEl.style.color = '#28a745';

            // Auto detect after 2 seconds
            setTimeout(() => detectAttendanceFace(), 2000);
        };
    } catch (error) {
        console.error('Camera error:', error);
        statusEl.textContent = '✗ Không thể truy cập camera';
        statusEl.style.color = '#dc3545';
    }
}

function stopAttendanceFaceCamera() {
    if (attendanceFaceStream) {
        attendanceFaceStream.getTracks().forEach(track => track.stop());
        attendanceFaceStream = null;
    }
}

async function detectAttendanceFace() {
    const video = document.getElementById('attendanceFaceVideo');
    const canvas = document.getElementById('attendanceFaceCanvas');
    const statusEl = document.getElementById('faceidStatus');

    if (!video || !canvas) return;

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    const context = canvas.getContext('2d');
    context.drawImage(video, 0, 0);

    statusEl.textContent = '✓ Đã phát hiện khuôn mặt. Đang xác thực...';
    statusEl.style.color = '#667eea';

    // Simulate face recognition
    setTimeout(() => {
        statusEl.textContent = '✓ Xác thực thành công!';
        statusEl.style.color = '#28a745';

        document.getElementById('submitAttendanceBtn').disabled = false;
    }, 1500);
}

function closeAttendanceModal() {
    stopAttendanceFaceCamera();
    closeModal();
}

// Fingerprint attendance
function initFingerprintReader() {
    const statusEl = document.getElementById('fingerprintStatus');
    statusEl.textContent = 'Sẵn sàng quét vân tay';
    statusEl.style.color = '#667eea';
}

async function scanFingerprint(maNV) {
    const statusEl = document.getElementById('fingerprintStatus');

    try {
        statusEl.textContent = 'Đang quét...';
        statusEl.style.color = '#667eea';

        // Simulate fingerprint scan
        await new Promise(resolve => setTimeout(resolve, 2000));

        // In production, integrate with actual fingerprint device
        const mockFingerprint = 'FP_' + maNV + '_' + Date.now();

        statusEl.textContent = '✓ Quét thành công!';
        statusEl.style.color = '#28a745';

        document.getElementById('submitAttendanceBtn').disabled = false;

    } catch (error) {
        console.error('Fingerprint error:', error);
        statusEl.textContent = '✗ Quét thất bại. Vui lòng thử lại';
        statusEl.style.color = '#dc3545';
    }
}

// Submit attendance
async function submitAttendance(maNV) {
    const method = document.getElementById('attendanceMethod').value;

    let data = {
        MaNV: maNV,
        XacThucBang: method === 'manual' ? 'Thủ công' : method === 'faceid' ? 'FaceID' : 'Vân tay',
        GioVao: null,
        GioRa: null,
        GhiChu: ''
    };

    if (method === 'manual') {
        const gioVao = document.getElementById('gioVao').value;
        const gioRa = document.getElementById('gioRa').value;
        const ghiChu = document.getElementById('ghiChu').value;

        if (!gioVao) {
            alert('Vui lòng nhập giờ vào');
            return;
        }

        const today = new Date();
        data.GioVao = new Date(today.toDateString() + ' ' + gioVao);
        if (gioRa) {
            data.GioRa = new Date(today.toDateString() + ' ' + gioRa);
        }
        data.GhiChu = ghiChu;
    } else {
        // For Face ID and Fingerprint, use current time
        data.GioVao = new Date();
    }

    try {
        const response = await fetch('/NhanVien/CheckAttendance', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (!response.ok) throw new Error('Failed to submit attendance');

        const result = await response.json();
        alert(result.message || 'Chấm công thành công!');

        closeAttendanceModal();

        // Reload attendance history if showing
        if (document.getElementById('tab-attendance')?.classList.contains('active')) {
            loadAttendanceHistory(maNV);
        }

    } catch (error) {
        console.error('Error submitting attendance:', error);
        alert('Không thể chấm công. Vui lòng thử lại.');
    }
}

// Load attendance history
async function loadAttendanceHistory(maNV) {
    const container = document.getElementById('attendanceList');
    if (!container) return;

    try {
        container.innerHTML = '<div style="text-align: center; padding: 20px;">Đang tải...</div>';

        const response = await fetch(`/NhanVien/GetAttendanceHistory/${maNV}`);
        if (!response.ok) throw new Error('Failed to load attendance history');

        const records = await response.json();

        if (records.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <div class="empty-icon">📋</div>
                    <div class="empty-text">Chưa có dữ liệu chấm công</div>
                </div>
            `;
            return;
        }

        let html = '';
        records.forEach(record => {
            const date = new Date(record.ngay);
            const gioVao = record.gioVao ? new Date(record.gioVao).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '--:--';
            const gioRa = record.gioRa ? new Date(record.gioRa).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '--:--';

            let statusClass = 'on-time';
            let statusText = 'Đúng giờ';

            if (record.trangThai === 'DiTre') {
                statusClass = 'late';
                statusText = 'Đi trễ';
            } else if (record.trangThai === 'Vang') {
                statusClass = 'absent';
                statusText = 'Vắng';
            } else if (record.trangThai === 'VeSom') {
                statusClass = 'late';
                statusText = 'Về sớm';
            }

            html += `
                <div class="attendance-item ${statusClass}">
                    <div class="attendance-date">
                        ${date.toLocaleDateString('vi-VN', { weekday: 'short', day: '2-digit', month: '2-digit', year: 'numeric' })}
                    </div>
                    <div class="attendance-time">
                        <span>Vào: ${gioVao}</span>
                        <span>Ra: ${gioRa}</span>
                        <span>Tổng: ${record.soGioLam || 0}h</span>
                    </div>
                    <span class="attendance-status ${statusClass}">${statusText}</span>
                    ${record.ghiChu ? `<div style="font-size: 11px; color: #6c757d; margin-top: 5px;">${record.ghiChu}</div>` : ''}
                </div>
            `;
        });

        container.innerHTML = html;

    } catch (error) {
        console.error('Error loading attendance history:', error);
        container.innerHTML = `
            <div class="empty-state">
                <div class="empty-icon">⚠️</div>
                <div class="empty-text">Không thể tải dữ liệu chấm công</div>
            </div>
        `;
    }
}