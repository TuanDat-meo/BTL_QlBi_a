// ==================== TABLE DIAGRAM MANAGER - FIXED ====================
const TableDiagramManager = {
    currentFloor: 'Tầng 1',
    isEditMode: false,
    isDragging: false,
    currentDragElement: null,
    offsetX: 0,
    offsetY: 0,
    tables: [],
    floorDimensions: {
        'Tầng 1': { width: 20, height: 8 },
        'Tầng 2': { width: 20, height: 8 },
        'VIP': { width: 15, height: 6 }
    },

    // Mở modal sơ đồ
    openDiagram: async function () {
        try {
            console.log('🔍 Opening diagram...');
            const tenNhom = this.getUserRole();
            const isAdmin = tenNhom === 'Admin' || tenNhom === 'Quản lý';

            // Load dữ liệu bàn
            await this.loadTables();

            if (this.tables.length === 0) {
                console.warn('⚠️ No tables loaded!');
                if (window.Toast) Toast.warning('Không có dữ liệu bàn');
                return;
            }

            console.log('✅ Loaded tables:', this.tables.length);

            const modalContent = `
                <div class="diagram-modal">
                    <div class="diagram-header">
                        <h3>🗺️ Sơ đồ bàn bi-a</h3>
                        <button class="close-modal-btn" onclick="TableDiagramManager.closeDiagram()">✕</button>
                    </div>

                    <div class="diagram-controls">
                        <div class="floor-tabs">
                            <button class="floor-tab active" data-floor="Tầng 1" onclick="TableDiagramManager.switchFloor('Tầng 1', event)">
                                Tầng 1
                            </button>
                            <button class="floor-tab" data-floor="Tầng 2" onclick="TableDiagramManager.switchFloor('Tầng 2', event)">
                                Tầng 2
                            </button>
                            <button class="floor-tab" data-floor="VIP" onclick="TableDiagramManager.switchFloor('VIP', event)">
                                VIP
                            </button>
                        </div>

                        ${isAdmin ? `
                            <div class="edit-controls">
                                <button class="btn btn-primary" onclick="TableDiagramManager.toggleEditMode()" id="editModeBtn">
                                    ✏️ Chỉnh sửa
                                </button>
                                <button class="btn btn-success" style="display: none;" id="saveDiagramBtn" onclick="TableDiagramManager.saveDiagram()">
                                    💾 Lưu thay đổi
                                </button>
                                <button class="btn btn-secondary" style="display: none;" id="cancelEditBtn" onclick="TableDiagramManager.cancelEdit()">
                                    ❌ Hủy
                                </button>
                            </div>
                        ` : ''}
                    </div>

                    <div class="diagram-body">
                        <div class="diagram-canvas-wrapper">
                            <div class="diagram-canvas" id="diagramCanvas">
                                <div class="loading-indicator">Đang tải sơ đồ...</div>
                            </div>
                            <div class="diagram-legend">
                                <div class="legend-item">
                                    <span class="legend-color available"></span> Trống
                                </div>
                                <div class="legend-item">
                                    <span class="legend-color occupied"></span> Đang chơi
                                </div>
                                <div class="legend-item">
                                    <span class="legend-color reserved"></span> Đã đặt
                                </div>
                            </div>
                        </div>

                        ${isAdmin && this.isEditMode ? `
                            <div class="edit-instructions">
                                <h4>💡 Hướng dẫn:</h4>
                                <ul>
                                    <li>Kéo thả bàn để di chuyển vị trí</li>
                                    <li>Click vào bàn để xem chi tiết</li>
                                    <li>Nhấn "Lưu thay đổi" để cập nhật</li>
                                </ul>
                            </div>
                        ` : ''}
                    </div>
                </div>
            `;

            const modalOverlay = document.getElementById('modalOverlay');
            if (modalOverlay) {
                modalOverlay.innerHTML = modalContent;
                modalOverlay.classList.add('active');

                // Render sơ đồ tầng đầu tiên
                setTimeout(() => this.renderDiagram(), 100);
            }

        } catch (error) {
            console.error('❌ Error opening diagram:', error);
            if (window.Toast) Toast.error('Không thể mở sơ đồ bàn: ' + error.message);
        }
    },

    // Load danh sách bàn
    loadTables: async function () {
        try {
            console.log('📡 Loading tables from API...');
            const response = await fetch('/QLBan/LayDanhSachBan');

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            this.tables = await response.json();
            console.log('✅ Tables loaded:', this.tables);

            // Kiểm tra và gán vị trí mặc định nếu thiếu
            this.tables = this.tables.map((table, index) => {
                if (!table.viTriX || !table.viTriY || table.viTriX === 0 || table.viTriY === 0) {
                    console.warn(`⚠️ Table ${table.tenBan} missing position, assigning default`);
                    // Gán vị trí mặc định theo index
                    const row = Math.floor(index / 4);
                    const col = index % 4;
                    table.viTriX = 10 + (col * 25);
                    table.viTriY = 10 + (row * 30);
                }
                return table;
            });

        } catch (error) {
            console.error('❌ Error loading tables:', error);
            this.tables = [];
            throw error;
        }
    },

    // Chuyển tầng
    switchFloor: function (floor, event) {
        console.log('🔄 Switching to floor:', floor);
        this.currentFloor = floor;

        // Update active tab
        document.querySelectorAll('.floor-tab').forEach(tab => {
            tab.classList.remove('active');
        });
        if (event && event.target) {
            event.target.classList.add('active');
        }

        this.renderDiagram();
    },

    // Render sơ đồ
    renderDiagram: function () {
        const canvas = document.getElementById('diagramCanvas');
        if (!canvas) {
            console.error('❌ Canvas not found!');
            return;
        }

        console.log(`🎨 Rendering diagram for ${this.currentFloor}`);

        const floorTables = this.tables.filter(t => t.khuVuc === this.currentFloor);
        console.log(`Found ${floorTables.length} tables for ${this.currentFloor}`);

        const dimensions = this.floorDimensions[this.currentFloor] || { width: 20, height: 8 };

        // Set canvas dimensions
        canvas.style.width = `${dimensions.width * 40}px`;
        canvas.style.height = `${dimensions.height * 40}px`;
        canvas.style.position = 'relative';
        canvas.innerHTML = '';

        if (floorTables.length === 0) {
            canvas.innerHTML = `
                <div class="empty-floor">
                    <div style="text-align: center; padding: 40px;">
                        <div style="font-size: 48px; margin-bottom: 16px;">🎱</div>
                        <h3 style="margin: 0 0 8px 0;">Không có bàn</h3>
                        <p style="color: #6b7280; margin: 0;">Khu vực ${this.currentFloor} chưa có bàn nào</p>
                    </div>
                </div>
            `;
            return;
        }

        // Render tables
        floorTables.forEach(table => {
            const tableElement = this.createTableElement(table);
            canvas.appendChild(tableElement);
        });

        // Add grid lines if in edit mode
        if (this.isEditMode) {
            this.addGridLines(canvas, dimensions);
        }

        console.log('✅ Diagram rendered successfully');
    },

    // Tạo element bàn
    createTableElement: function (table) {
        const div = document.createElement('div');
        div.className = `diagram-table ${table.trangThai.toLowerCase()}`;
        div.setAttribute('data-table-id', table.maBan);

        // Position
        const x = parseFloat(table.viTriX) || 10;
        const y = parseFloat(table.viTriY) || 10;

        div.style.left = `${x}%`;
        div.style.top = `${y}%`;
        div.style.position = 'absolute';

        // Status class
        const statusClass = this.getStatusClass(table.trangThai);

        div.innerHTML = `
            <div class="table-icon ${statusClass}">
                🎱
            </div>
            <div class="table-label">${table.tenBan}</div>
            ${table.khuVuc === 'VIP' ? '<span class="vip-mark">⭐</span>' : ''}
            ${this.isEditMode ? '<span class="drag-handle">⋮⋮</span>' : ''}
        `;

        // Events
        if (this.isEditMode) {
            div.style.cursor = 'move';
            div.addEventListener('mousedown', (e) => this.startDrag(e, div));
        } else {
            div.style.cursor = 'pointer';
            div.addEventListener('click', () => {
                console.log('🖱️ Clicked table:', table.tenBan);
                TableDiagramManager.closeDiagram();
                setTimeout(() => {
                    if (typeof TableManager !== 'undefined' && TableManager.showDetail) {
                        TableManager.showDetail(table.maBan);
                    }
                }, 300);
            });
        }

        return div;
    },

    // Get status class
    getStatusClass: function (status) {
        const statusMap = {
            'Trong': 'available',
            'DangChoi': 'occupied',
            'DaDat': 'reserved',
            'BaoTri': 'maintenance'
        };
        return statusMap[status] || 'available';
    },

    // Add grid lines
    addGridLines: function (canvas, dimensions) {
        const gridOverlay = document.createElement('div');
        gridOverlay.className = 'grid-overlay';
        gridOverlay.style.position = 'absolute';
        gridOverlay.style.top = '0';
        gridOverlay.style.left = '0';
        gridOverlay.style.width = '100%';
        gridOverlay.style.height = '100%';
        gridOverlay.style.pointerEvents = 'none';
        gridOverlay.style.zIndex = '1';

        // Vertical lines
        for (let i = 0; i <= dimensions.width; i += 2) {
            const line = document.createElement('div');
            line.className = 'grid-line-v';
            line.style.position = 'absolute';
            line.style.left = `${(i / dimensions.width) * 100}%`;
            line.style.width = '1px';
            line.style.height = '100%';
            line.style.background = 'rgba(0,0,0,0.05)';
            gridOverlay.appendChild(line);
        }

        // Horizontal lines
        for (let i = 0; i <= dimensions.height; i += 2) {
            const line = document.createElement('div');
            line.className = 'grid-line-h';
            line.style.position = 'absolute';
            line.style.top = `${(i / dimensions.height) * 100}%`;
            line.style.height = '1px';
            line.style.width = '100%';
            line.style.background = 'rgba(0,0,0,0.05)';
            gridOverlay.appendChild(line);
        }

        canvas.appendChild(gridOverlay);
    },

    // Toggle edit mode
    toggleEditMode: function () {
        this.isEditMode = !this.isEditMode;

        const editBtn = document.getElementById('editModeBtn');
        const saveBtn = document.getElementById('saveDiagramBtn');
        const cancelBtn = document.getElementById('cancelEditBtn');

        if (this.isEditMode) {
            if (editBtn) editBtn.style.display = 'none';
            if (saveBtn) saveBtn.style.display = 'inline-block';
            if (cancelBtn) cancelBtn.style.display = 'inline-block';
        } else {
            if (editBtn) editBtn.style.display = 'inline-block';
            if (saveBtn) saveBtn.style.display = 'none';
            if (cancelBtn) cancelBtn.style.display = 'none';
        }

        this.renderDiagram();
    },

    // Drag functionality
    startDrag: function (e, element) {
        if (!this.isEditMode) return;

        this.isDragging = true;
        this.currentDragElement = element;

        const rect = element.getBoundingClientRect();
        const parentRect = element.parentElement.getBoundingClientRect();

        this.offsetX = e.clientX - rect.left;
        this.offsetY = e.clientY - rect.top;

        element.classList.add('dragging');

        document.addEventListener('mousemove', this.drag.bind(this));
        document.addEventListener('mouseup', this.stopDrag.bind(this));

        e.preventDefault();
    },

    drag: function (e) {
        if (!this.isDragging || !this.currentDragElement) return;

        const element = this.currentDragElement;
        const parent = element.parentElement;
        const parentRect = parent.getBoundingClientRect();

        let x = e.clientX - parentRect.left - this.offsetX;
        let y = e.clientY - parentRect.top - this.offsetY;

        // Constrain to parent boundaries
        x = Math.max(0, Math.min(x, parentRect.width - element.offsetWidth));
        y = Math.max(0, Math.min(y, parentRect.height - element.offsetHeight));

        // Convert to percentage
        const xPercent = (x / parentRect.width) * 100;
        const yPercent = (y / parentRect.height) * 100;

        element.style.left = `${xPercent}%`;
        element.style.top = `${yPercent}%`;
    },

    stopDrag: function () {
        if (this.currentDragElement) {
            this.currentDragElement.classList.remove('dragging');
        }

        this.isDragging = false;
        this.currentDragElement = null;

        document.removeEventListener('mousemove', this.drag);
        document.removeEventListener('mouseup', this.stopDrag);
    },

    // Save diagram
    saveDiagram: async function () {
        try {
            const canvas = document.getElementById('diagramCanvas');
            if (!canvas) return;

            const tables = canvas.querySelectorAll('.diagram-table');
            const updates = [];

            tables.forEach(table => {
                const maBan = parseInt(table.getAttribute('data-table-id'));
                const left = parseFloat(table.style.left);
                const top = parseFloat(table.style.top);

                updates.push({
                    maBan: maBan,
                    viTriX: left,
                    viTriY: top
                });
            });

            console.log('💾 Saving positions:', updates);

            const response = await fetch('/QLBan/CapNhatViTriBan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updates)
            });

            const result = await response.json();

            if (result.success) {
                if (window.Toast) Toast.success('Đã lưu vị trí bàn');
                this.toggleEditMode();
                await this.loadTables(); // Reload data
            } else {
                if (window.Toast) Toast.error(result.message || 'Lỗi khi lưu');
            }

        } catch (error) {
            console.error('❌ Error saving diagram:', error);
            if (window.Toast) Toast.error('Lỗi khi lưu sơ đồ');
        }
    },

    // Cancel edit
    cancelEdit: function () {
        if (confirm('Hủy các thay đổi chưa lưu?')) {
            this.toggleEditMode();
            this.renderDiagram();
        }
    },

    // Close diagram
    closeDiagram: function () {
        this.isEditMode = false;
        this.isDragging = false;
        this.currentDragElement = null;

        const modalOverlay = document.getElementById('modalOverlay');
        if (modalOverlay) {
            modalOverlay.classList.remove('active');
            setTimeout(() => {
                modalOverlay.innerHTML = '';
            }, 300);
        }
    },

    // Get user role
    getUserRole: function () {
        const grid = document.querySelector('[data-role]');
        return grid ? grid.getAttribute('data-role') : 'Nhân viên';
    }
};

// Export
window.TableDiagramManager = TableDiagramManager;

console.log('✅ TableDiagramManager loaded successfully');