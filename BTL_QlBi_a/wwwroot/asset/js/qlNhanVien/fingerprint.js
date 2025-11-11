// Fingerprint Management
function openFingerprintModal(maNV) {
    if (!window.canManage) {
        alert('Bạn không có quyền cập nhật vân tay');
        return;
    }

    const content = `
        <div class="fingerprint-modal">
            <div class="fingerprint-section">
                <div class="fingerprint-animation" id="fingerprintAnimation">
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
                <p id="fingerprintRegisterStatus" style="text-align: center; margin-top: 15px; color: #667eea; font-weight: 600;">
                    Sẵn sàng đăng ký vân tay
                </p>
                <div id="fingerprintProgress" style="display: none; margin-top: 20px;">
                    <div style="background: #e9ecef; border-radius: 10px; height: 10px; overflow: hidden;">
                        <div id="progressBar" style="background: linear-gradient(90deg, #667eea, #764ba2); height: 100%; width: 0%; transition: width 0.3s;"></div>
                    </div>
                    <p style="text-align: center; margin-top: 10px; font-size: 12px; color: #6c757d;">
                        Đã quét <span id="scanCount">0</span>/3 lần
                    </p>
                </div>
            </div>

            <div class="info-card" style="margin-top: 20px;">
                <h4 style="margin-bottom: 10px;">Hướng dẫn:</h4>
                <ul style="font-size: 12px; color: #6c757d;">
                    <li>Làm sạch ngón tay và bề mặt máy quét</li>
                    <li>Đặt ngón tay lên máy quét khi có hướng dẫn</li>
                    <li>Giữ ngón tay ổn định trong 2-3 giây</li>
                    <li>Quét 3 lần để đảm bảo độ chính xác</li>
                    <li>Nên đăng ký 2 ngón tay khác nhau (ngón trỏ và ngón cái)</li>
                </ul>
            </div>

            <div class="action-buttons" style="margin-top: 20px;">
                <button class="btn btn-primary" id="startScanBtn" onclick="startFingerprintRegistration(${maNV})">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="12" r="10"></circle>
                        <polyline points="12 6 12 12 16 14"></polyline>
                    </svg>
                    Bắt đầu quét
                </button>
                <button class="btn btn-secondary" onclick="closeModal()">Đóng</button>
            </div>
        </div>
    `;

    openModal('Đăng ký vân tay', content);
}

let fingerprintScanCount = 0;
let fingerprintData = [];

async function startFingerprintRegistration(maNV) {
    const statusEl = document.getElementById('fingerprintRegisterStatus');
    const progressEl = document.getElementById('fingerprintProgress');
    const startBtn = document.getElementById('startScanBtn');
    const animation = document.getElementById('fingerprintAnimation');

    // Reset
    fingerprintScanCount = 0;
    fingerprintData = [];

    // Show progress
    progressEl.style.display = 'block';
    startBtn.disabled = true;

    // Start animation
    animation.style.animation = 'pulse 1.5s infinite';

    // Scan 3 times
    for (let i = 0; i < 3; i++) {
        try {
            statusEl.textContent = `Đặt ngón tay lên máy quét (lần ${i + 1}/3)`;
            statusEl.style.color = '#667eea';

            // Simulate fingerprint scan
            // In production, integrate with actual fingerprint device API
            await simulateFingerprintScan();

            fingerprintScanCount++;
            document.getElementById('scanCount').textContent = fingerprintScanCount;
            document.getElementById('progressBar').style.width = (fingerprintScanCount / 3 * 100) + '%';

            // Generate mock fingerprint data
            fingerprintData.push({
                scan: i + 1,
                data: 'FP_' + maNV + '_' + Date.now() + '_' + i,
                quality: 85 + Math.random() * 15
            });

            statusEl.textContent = `✓ Quét lần ${i + 1} thành công`;
            statusEl.style.color = '#28a745';

            if (i < 2) {
                await new Promise(resolve => setTimeout(resolve, 1000));
            }

        } catch (error) {
            console.error('Fingerprint scan error:', error);
            statusEl.textContent = `✗ Quét lần ${i + 1} thất bại. Vui lòng thử lại`;
            statusEl.style.color = '#dc3545';
            startBtn.disabled = false;
            animation.style.animation = '';
            return;
        }
    }

    // Processing
    statusEl.textContent = 'Đang xử lý dữ liệu vân tay...';
    statusEl.style.color = '#667eea';

    try {
        // Generate fingerprint hash
        const fingerprintHash = await generateFingerprintHash(fingerprintData);

        // Save to server
        const response = await fetch('/NhanVien/UpdateFingerprint', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                MaNV: maNV,
                FingerprintHash: fingerprintHash
            })
        });

        if (!response.ok) throw new Error('Failed to save fingerprint');

        statusEl.textContent = '✓ Đăng ký vân tay thành công!';
        statusEl.style.color = '#28a745';
        animation.style.animation = '';

        setTimeout(() => {
            closeModal();
            alert('Đã đăng ký vân tay thành công!');

            // Reload detail if showing
            if (window.currentEmployeeId === maNV) {
                showEmployeeDetail(maNV);
            }
        }, 1500);

    } catch (error) {
        console.error('Error saving fingerprint:', error);
        statusEl.textContent = '✗ Không thể lưu vân tay. Vui lòng thử lại';
        statusEl.style.color = '#dc3545';
        startBtn.disabled = false;
        animation.style.animation = '';
    }
}

function simulateFingerprintScan() {
    return new Promise((resolve, reject) => {
        // Simulate device communication delay
        const delay = 2000 + Math.random() * 1000;

        setTimeout(() => {
            // 95% success rate simulation
            if (Math.random() > 0.05) {
                resolve();
            } else {
                reject(new Error('Poor fingerprint quality'));
            }
        }, delay);
    });
}

async function generateFingerprintHash(data) {
    // In production, use proper fingerprint encoding algorithm
    // For demo, generate a hash from the scan data
    const dataString = JSON.stringify(data);
    const encoder = new TextEncoder();
    const dataBuffer = encoder.encode(dataString);

    const hashBuffer = await crypto.subtle.digest('SHA-256', dataBuffer);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

    return hashHex;
}

// Fingerprint verification (for attendance)
async function verifyFingerprint(maNV) {
    try {
        const statusEl = document.getElementById('fingerprintStatus');
        statusEl.textContent = 'Đang quét vân tay...';
        statusEl.style.color = '#667eea';

        // Simulate fingerprint scan
        await simulateFingerprintScan();

        // Generate hash from current scan
        const scanData = [{
            scan: 1,
            data: 'FP_VERIFY_' + Date.now(),
            quality: 90
        }];
        const scannedHash = await generateFingerprintHash(scanData);

        // Verify with server
        const response = await fetch('/NhanVien/VerifyFingerprint', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                MaNV: maNV,
                ScannedHash: scannedHash
            })
        });

        if (!response.ok) throw new Error('Verification failed');

        const result = await response.json();

        if (result.success) {
            statusEl.textContent = '✓ Xác thực thành công!';
            statusEl.style.color = '#28a745';
            return true;
        } else {
            statusEl.textContent = '✗ Xác thực thất bại. Vân tay không khớp';
            statusEl.style.color = '#dc3545';
            return false;
        }

    } catch (error) {
        console.error('Fingerprint verification error:', error);
        const statusEl = document.getElementById('fingerprintStatus');
        if (statusEl) {
            statusEl.textContent = '✗ Không thể xác thực vân tay';
            statusEl.style.color = '#dc3545';
        }
        return false;
    }
}

// CSS animation for pulse effect
const style = document.createElement('style');
style.textContent = `
    @keyframes pulse {
        0%, 100% {
            transform: scale(1);
            opacity: 1;
        }
        50% {
            transform: scale(1.1);
            opacity: 0.8;
        }
    }
`;
document.head.appendChild(style);