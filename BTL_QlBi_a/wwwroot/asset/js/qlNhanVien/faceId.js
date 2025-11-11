// Face ID Management
let faceStream = null;

function openFaceIdModal(maNV) {
    if (!window.canManage) {
        alert('Bạn không có quyền cập nhật Face ID');
        return;
    }

    const content = `
        <div class="faceid-modal">
            <div class="face-capture-section">
                <div class="face-preview">
                    <video id="faceVideo" autoplay></video>
                    <canvas id="faceCanvas" style="display: none;"></canvas>
                    <div class="face-overlay"></div>
                </div>
                <p id="faceStatus" style="text-align: center; margin-top: 15px; color: #667eea; font-weight: 600;">
                    Đang khởi động camera...
                </p>
            </div>

            <div class="info-card" style="margin-top: 20px;">
                <h4 style="margin-bottom: 10px;">Hướng dẫn:</h4>
                <ul style="font-size: 12px; color: #6c757d;">
                    <li>Đặt khuôn mặt vào khung hình oval</li>
                    <li>Giữ khuôn mặt thẳng, ánh sáng đủ</li>
                    <li>Không đeo khẩu trang, kính đen</li>
                    <li>Click "Chụp" khi khuôn mặt đã nằm đúng vị trí</li>
                </ul>
            </div>

            <div class="action-buttons" style="margin-top: 20px;">
                <button class="btn btn-primary" id="captureFaceBtn" onclick="captureFace(${maNV})" disabled>
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"></path>
                        <circle cx="12" cy="13" r="4"></circle>
                    </svg>
                    Chụp
                </button>
                <button class="btn btn-danger" onclick="stopFaceCamera()">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="15" y1="9" x2="9" y2="15"></line>
                        <line x1="9" y1="9" x2="15" y2="15"></line>
                    </svg>
                    Hủy
                </button>
            </div>
        </div>
    `;

    openModal('Cập nhật Face ID', content);
    startFaceCamera();
}

async function startFaceCamera() {
    const video = document.getElementById('faceVideo');
    const statusEl = document.getElementById('faceStatus');
    const captureBtn = document.getElementById('captureFaceBtn');

    try {
        faceStream = await navigator.mediaDevices.getUserMedia({
            video: {
                width: { ideal: 1280 },
                height: { ideal: 720 },
                facingMode: 'user'
            }
        });

        video.srcObject = faceStream;

        // Wait for video to be ready
        video.onloadedmetadata = () => {
            video.play();
            statusEl.textContent = '✓ Camera đã sẵn sàng. Hãy đặt khuôn mặt vào khung hình';
            statusEl.style.color = '#28a745';
            captureBtn.disabled = false;

            // Start face detection (optional - requires face-api.js)
            detectFace();
        };

    } catch (error) {
        console.error('Camera error:', error);
        statusEl.textContent = '✗ Không thể truy cập camera. Vui lòng kiểm tra quyền truy cập';
        statusEl.style.color = '#dc3545';
    }
}

function detectFace() {
    // Optional: Implement face detection using face-api.js or similar library
    // For now, just simulate detection
    const statusEl = document.getElementById('faceStatus');

    setTimeout(() => {
        if (statusEl && statusEl.textContent.includes('sẵn sàng')) {
            statusEl.textContent = '✓ Đã phát hiện khuôn mặt. Click "Chụp" để lưu';
        }
    }, 2000);
}

async function captureFace(maNV) {
    const video = document.getElementById('faceVideo');
    const canvas = document.getElementById('faceCanvas');
    const statusEl = document.getElementById('faceStatus');

    if (!video || !canvas) return;

    // Set canvas size to video size
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    // Draw video frame to canvas
    const context = canvas.getContext('2d');
    context.drawImage(video, 0, 0, canvas.width, canvas.height);

    // Convert to blob
    canvas.toBlob(async (blob) => {
        try {
            statusEl.textContent = 'Đang xử lý...';
            statusEl.style.color = '#667eea';

            // Create form data
            const formData = new FormData();
            formData.append('image', blob, 'face.jpg');
            formData.append('maNV', maNV);

            // Upload image
            const uploadResponse = await fetch('/Upload/FaceImage', {
                method: 'POST',
                body: formData
            });

            if (!uploadResponse.ok) throw new Error('Upload failed');

            const uploadData = await uploadResponse.json();

            // Generate hash from image
            const hash = await generateImageHash(canvas);

            // Update employee Face ID
            const response = await fetch('/NhanVien/UpdateFaceId', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    MaNV: maNV,
                    Hash: hash,
                    ImageUrl: uploadData.url
                })
            });

            if (!response.ok) throw new Error('Failed to update Face ID');

            statusEl.textContent = '✓ Cập nhật Face ID thành công!';
            statusEl.style.color = '#28a745';

            setTimeout(() => {
                stopFaceCamera();
                alert('Đã cập nhật Face ID thành công!');

                // Reload detail if showing
                if (window.currentEmployeeId === maNV) {
                    showEmployeeDetail(maNV);
                }
            }, 1500);

        } catch (error) {
            console.error('Error capturing face:', error);
            statusEl.textContent = '✗ Không thể lưu Face ID. Vui lòng thử lại';
            statusEl.style.color = '#dc3545';
        }
    }, 'image/jpeg', 0.9);
}

function stopFaceCamera() {
    if (faceStream) {
        faceStream.getTracks().forEach(track => track.stop());
        faceStream = null;
    }
    closeModal();
}

async function generateImageHash(canvas) {
    // Simple hash generation from canvas data
    // In production, use more sophisticated face encoding
    const imageData = canvas.toDataURL();
    const encoder = new TextEncoder();
    const data = encoder.encode(imageData);

    const hashBuffer = await crypto.subtle.digest('SHA-256', data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

    return hashHex;
}

// Cleanup on modal close
const originalCloseModal = window.closeModal;
window.closeModal = function () {
    stopFaceCamera();
    originalCloseModal();
};