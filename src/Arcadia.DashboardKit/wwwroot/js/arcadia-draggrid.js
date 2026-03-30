// ── ArcadiaDragGrid Engine v4 ──
// Uses explicit grid placement via 2D occupancy grid (like Gridstack).
// Never relies on CSS order or auto-placement — all positions computed.
// Drag lifts item, closestCenter finds target, swap + recompute placement + FLIP.

const controllers = new WeakMap();

export function initialize(gridEl, dotNetRef, options) {
    if (!gridEl) return;
    if (controllers.has(gridEl)) dispose(gridEl);
    controllers.set(gridEl, new DragGridController(gridEl, dotNetRef, options));
}

export function dispose(gridEl) {
    const ctrl = controllers.get(gridEl);
    if (ctrl) { ctrl.destroy(); controllers.delete(gridEl); }
}

export function updateLayout(gridEl, layoutJson) {
    controllers.get(gridEl)?.updateLayout(layoutJson);
}

export function setEditMode(gridEl, enabled) {
    controllers.get(gridEl)?.setEditMode(enabled);
}

export function setWiggleMode(gridEl, enabled) {
    controllers.get(gridEl)?.setWiggleMode(enabled);
}

export function setDragMode(gridEl, mode, longPressDuration) {
    const ctrl = controllers.get(gridEl);
    if (ctrl) {
        const wasLongpress = ctrl.options.dragMode === 'longpress';
        ctrl.options.dragMode = mode;
        ctrl.options.longPressDuration = longPressDuration;

        // Exit wiggle if switching away from longpress
        if (wasLongpress && ctrl._wiggleActive) {
            ctrl._wiggleActive = false;
            ctrl.grid.classList.remove('arcadia-draggrid--wiggle');
            if (ctrl._longPressTimer) { clearTimeout(ctrl._longPressTimer); ctrl._longPressTimer = null; }
        }

        // Update mode classes
        ctrl.grid.classList.remove('arcadia-draggrid--mode-longpress', 'arcadia-draggrid--mode-direct');
        ctrl.grid.classList.add(`arcadia-draggrid--mode-${mode}`);

        // Restore --edit for direct mode if editMode is on
        if (mode === 'direct' && ctrl.options.editMode) {
            ctrl.grid.classList.add('arcadia-draggrid--edit');
        }
    }
}

export function saveLayout(key, json) {
    try { localStorage.setItem(key, json); } catch {}
}

export function loadLayout(key) {
    try { return localStorage.getItem(key); } catch { return null; }
}

// ══════════════════════════════════════════
// 2D OCCUPANCY GRID — computes explicit grid-column-start / grid-row-start
// for every item. This is the core layout engine.
// ══════════════════════════════════════════

function computePlacement(order, itemSpans, cols) {
    // order: array of item IDs in desired display order
    // itemSpans: Map<id, {colSpan, rowSpan}>
    // cols: number of grid columns
    // Returns: Map<id, {col, row}> (1-based for CSS Grid)

    const grid = []; // grid[row][col] = id or null
    const ensure = (n) => { while (grid.length < n) grid.push(new Array(cols).fill(null)); };
    ensure(30);

    const result = new Map();

    for (const id of order) {
        const spans = itemSpans.get(id) || { colSpan: 1, rowSpan: 1 };
        const cs = Math.min(spans.colSpan, cols);
        const rs = spans.rowSpan;
        let placed = false;

        for (let r = 0; !placed; r++) {
            ensure(r + rs);
            for (let c = 0; c <= cols - cs; c++) {
                // Check if all cells in the rectangle are free
                let fits = true;
                for (let dr = 0; dr < rs && fits; dr++) {
                    for (let dc = 0; dc < cs && fits; dc++) {
                        if (grid[r + dr][c + dc] !== null) fits = false;
                    }
                }
                if (fits) {
                    for (let dr = 0; dr < rs; dr++) {
                        for (let dc = 0; dc < cs; dc++) {
                            grid[r + dr][c + dc] = id;
                        }
                    }
                    result.set(id, { col: c + 1, row: r + 1 });
                    placed = true;
                    break; // Exit column loop
                }
            }
        }
    }
    return result;
}

// Apply computed placement to DOM elements
function applyPlacement(gridEl, placement) {
    for (const [id, pos] of placement) {
        const el = gridEl.querySelector(`[data-draggrid-id="${id}"]`);
        if (el) {
            el.style.gridColumnStart = pos.col;
            el.style.gridRowStart = pos.row;
            el.style.order = ''; // clear any stale order
        }
    }
}

// Read colSpan/rowSpan from DOM element
function getSpans(el) {
    const style = el.style;
    const cs = getComputedStyle(el);
    const colMatch = style.gridColumn?.match(/span\s*(\d+)/) || cs.gridColumnEnd?.match(/span\s*(\d+)/);
    const rowMatch = style.gridRow?.match(/span\s*(\d+)/) || cs.gridRowEnd?.match(/span\s*(\d+)/);
    return {
        colSpan: colMatch ? parseInt(colMatch[1]) : 1,
        rowSpan: rowMatch ? parseInt(rowMatch[1]) : 1
    };
}

class DragGridController {
    constructor(gridEl, dotNetRef, options) {
        this.grid = gridEl;
        this.dotNetRef = dotNetRef;
        this.options = options;
        this.dragging = null;
        this.resizing = null;
        this.order = [];
        this.itemSpans = new Map();
        this._highlightedId = null;
        this.reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        this._wiggleActive = false;
        this._longPressTimer = null;

        this._onPointerDown = this._onPointerDown.bind(this);
        this._onPointerMove = this._onPointerMove.bind(this);
        this._onPointerUp = this._onPointerUp.bind(this);
        this._onPointerCancel = this._onPointerCancel.bind(this);

        this.grid.addEventListener('pointerdown', this._onPointerDown);
        document.addEventListener('pointermove', this._onPointerMove);
        document.addEventListener('pointerup', this._onPointerUp);
        document.addEventListener('pointercancel', this._onPointerCancel);

        this._mqListener = (e) => { this.reducedMotion = e.matches; };
        window.matchMedia('(prefers-reduced-motion: reduce)').addEventListener('change', this._mqListener);

        // Build initial order and spans, then apply explicit placement
        this._buildState();
        console.log('DragGrid spans:', [...this.itemSpans.entries()].map(([k,v]) => `${k}:${v.colSpan}x${v.rowSpan}`).join(', '));
        this._applyLayout();
        // Log placement result
        const items = this._getItems();
        console.log('DragGrid placement:', items.map(el => `${el.dataset.draggridId}:col${el.style.gridColumnStart}`).join(', '));
    }

    destroy() {
        this.grid.removeEventListener('pointerdown', this._onPointerDown);
        document.removeEventListener('pointermove', this._onPointerMove);
        document.removeEventListener('pointerup', this._onPointerUp);
        document.removeEventListener('pointercancel', this._onPointerCancel);
        window.matchMedia('(prefers-reduced-motion: reduce)').removeEventListener('change', this._mqListener);
    }

    _buildState() {
        this.order = [];
        this.itemSpans = new Map();
        for (const el of this._getItems()) {
            const id = el.dataset.draggridId;
            this.order.push(id);
            this.itemSpans.set(id, getSpans(el));
        }
    }

    _applyLayout() {
        const placement = computePlacement(this.order, this.itemSpans, this.options.columns);
        applyPlacement(this.grid, placement);
    }

    _getItems() {
        return [...this.grid.querySelectorAll('[data-draggrid-id]')];
    }

    _el(id) {
        return this.grid.querySelector(`[data-draggrid-id="${id}"]`);
    }

    // ══════════════════════════════════════════
    // POINTER EVENTS
    // ══════════════════════════════════════════

    _onPointerDown(e) {
        if (e.target.closest('.arcadia-draggrid__resize-handle')) { this._startResize(e); return; }
        if (e.target.closest('.arcadia-draggrid__remove-btn')) return; // don't drag on remove button
        if (e.target.closest('.arcadia-draggrid__done-btn')) return;

        const item = e.target.closest('[data-draggrid-id]');
        if (!item || item.dataset.draggridLocked === 'true') return;

        // ── Longpress mode: start timer to enter wiggle mode ──
        if (this.options.dragMode === 'longpress' && !this._wiggleActive) {
            e.preventDefault(); // Prevent text selection during long-press
            this._longPressTimer = setTimeout(() => {
                this._longPressTimer = null;
                this.setWiggleMode(true);
                try { this.dotNetRef.invokeMethodAsync('OnWiggleStart'); } catch {}
                // Provide haptic-like feedback via brief scale pulse
                item.style.transition = 'transform 0.15s ease-out';
                item.style.transform = 'scale(1.02)';
                setTimeout(() => {
                    item.style.transform = '';
                    setTimeout(() => { item.style.transition = ''; }, 150);
                }, 150);
            }, this.options.longPressDuration || 500);
            this._longPressStart = { x: e.clientX, y: e.clientY, pointerId: e.pointerId };
            return;
        }

        // ── In longpress mode, only allow drag when wiggling ──
        if (this.options.dragMode === 'longpress' && !this._wiggleActive) return;

        // ── Normal drag (direct mode, or wiggle mode active) ──
        const handle = this.options.dragMode === 'longpress'
            ? item  // In wiggle mode, entire item is draggable
            : e.target.closest('.arcadia-draggrid__handle');
        if (!handle) return;
        if (this.options.dragMode === 'direct' && !this.options.editMode) return;

        e.preventDefault();
        item.setPointerCapture(e.pointerId);

        const rect = item.getBoundingClientRect();
        this.dragging = {
            id: item.dataset.draggridId,
            el: item,
            pointerId: e.pointerId,
            startX: e.clientX, startY: e.clientY,
            offsetX: e.clientX - rect.left, offsetY: e.clientY - rect.top,
            origRect: rect,
            started: false,
            dropTarget: null
        };
    }

    _onPointerMove(e) {
        // Cancel long-press if pointer moved more than 10px
        if (this._longPressTimer && this._longPressStart) {
            if (Math.hypot(e.clientX - this._longPressStart.x, e.clientY - this._longPressStart.y) > 10) {
                clearTimeout(this._longPressTimer);
                this._longPressTimer = null;
            }
        }
        if (this.resizing) { this._handleResizeMove(e); return; }
        if (!this.dragging) return;
        const d = this.dragging;

        if (!d.started) {
            if (Math.hypot(e.clientX - d.startX, e.clientY - d.startY) < 10) return;
            d.started = true;
            this._lift(d);
        }

        d.el.style.left = (e.clientX - d.offsetX) + 'px';
        d.el.style.top = (e.clientY - d.offsetY) + 'px';

        // Only look for drop target after moving a meaningful distance
        const dist = Math.hypot(e.clientX - d.startX, e.clientY - d.startY);
        if (dist > 40) {
            const target = this._closestCenter(e.clientX, e.clientY, d.id);
            d.dropTarget = target;
            this._highlight(target);
        } else {
            d.dropTarget = null;
            this._highlight(null);
        }
    }

    _onPointerUp(e) {
        if (this._longPressTimer) { clearTimeout(this._longPressTimer); this._longPressTimer = null; }
        if (this.resizing) { this._finishResize(e); return; }
        if (!this.dragging) return;
        const d = this.dragging;
        try { d.el.releasePointerCapture(d.pointerId); } catch {}

        if (!d.started) { this.dragging = null; return; }
        this._highlight(null);

        const dropTargetId = d.dropTarget;
        const oldIdx = this.order.indexOf(d.id);
        const newIdx = dropTargetId ? this.order.indexOf(dropTargetId) : oldIdx;

        if (oldIdx >= 0 && newIdx >= 0 && oldIdx !== newIdx) {
            // 1. Snapshot all positions (placeholder keeps grid stable)
            const firstRects = new Map();
            this._getItems().forEach(el => {
                const id = el.dataset.draggridId;
                firstRects.set(id, id === d.id ? d.origRect : el.getBoundingClientRect());
            });

            // 2. Remove placeholder
            if (d.placeholder) { d.placeholder.remove(); d.placeholder = null; }

            // 3. Swap in order array
            this.order[oldIdx] = this.order[newIdx];
            this.order[newIdx] = d.id;

            // 4. Save dragged item's floating position for spring animation
            const floatLeft = parseFloat(d.el.style.left);
            const floatTop = parseFloat(d.el.style.top);

            // 5. Reset dragged item to grid flow
            this._resetItem(d.el);

            // 6. Recompute ALL positions via occupancy grid
            this._applyLayout();
            this.grid.offsetHeight;

            // 7. Notify C# — let Blazor re-render
            try { this.dotNetRef.invokeMethodAsync('OnDropComplete', d.id, newIdx); } catch {}

            // 8. Animate AFTER Blazor re-render settles (2 frames)
            if (!this.reducedMotion) {
                requestAnimationFrame(() => {
                    requestAnimationFrame(() => {
                        // Animate dragged item: jump to floating position, then transition to grid position
                        const newRect = d.el.getBoundingClientRect();
                        if (!isNaN(floatLeft) && !isNaN(floatTop)) {
                            const dx = floatLeft - newRect.left;
                            const dy = floatTop - newRect.top;
                            if (Math.abs(dx) > 2 || Math.abs(dy) > 2) {
                                d.el.style.transform = `translate(${dx}px, ${dy}px)`;
                                d.el.style.transition = 'none';
                                this.grid.offsetHeight; // force layout
                                d.el.style.transition = 'transform 0.45s cubic-bezier(0.22, 1, 0.36, 1)';
                                d.el.style.transform = '';
                                setTimeout(() => { d.el.style.transition = ''; }, 500);
                            }
                        }
                        // FLIP other items from old positions to new
                        this._flipFromSnapshot(firstRects);
                    });
                });
            }
        } else {
            // No swap — animate back to original position
            if (d.placeholder) { d.placeholder.remove(); d.placeholder = null; }
            if (!this.reducedMotion) {
                // Use CSS transition for smooth return
                d.el.style.transition = 'left 0.4s cubic-bezier(0.22, 1, 0.36, 1), top 0.4s cubic-bezier(0.22, 1, 0.36, 1), transform 0.3s ease-out';
                d.el.style.left = d.origRect.left + 'px';
                d.el.style.top = d.origRect.top + 'px';
                d.el.style.transform = 'scale(1)';
                setTimeout(() => this._resetItem(d.el), 450);
            } else {
                this._resetItem(d.el);
            }
        }

        try { this.dotNetRef.invokeMethodAsync('OnDragEnd', d.id); } catch {}
        this.dragging = null;
    }

    _onPointerCancel() {
        if (!this.dragging) return;
        this._highlight(null);
        if (this.dragging.placeholder) this.dragging.placeholder.remove();
        if (this.dragging.started) this._resetItem(this.dragging.el);
        this.dragging = null;
    }

    // ══════════════════════════════════════════
    // LIFT & RESET
    // ══════════════════════════════════════════

    _lift(d) {
        const r = d.origRect;
        const spans = this.itemSpans.get(d.id) || { colSpan: 1, rowSpan: 1 };

        // Placeholder keeps grid stable
        d.placeholder = document.createElement('div');
        d.placeholder.className = 'arcadia-draggrid__placeholder';
        d.placeholder.style.gridColumn = `${d.el.style.gridColumnStart} / span ${spans.colSpan}`;
        d.placeholder.style.gridRow = `${d.el.style.gridRowStart} / span ${spans.rowSpan}`;
        d.el.parentNode.insertBefore(d.placeholder, d.el);

        d.el.classList.add('arcadia-draggrid__item--dragging');
        d.el.style.position = 'fixed';
        d.el.style.width = r.width + 'px';
        d.el.style.height = r.height + 'px';
        d.el.style.left = r.left + 'px';
        d.el.style.top = r.top + 'px';
        d.el.style.zIndex = '9999';
        d.el.style.margin = '0';
        d.el.style.pointerEvents = 'none';
        requestAnimationFrame(() => {
            if (d.el) d.el.style.transform = `scale(${this.options.dragScaleUp || 1.03})`;
        });
        try { this.dotNetRef.invokeMethodAsync('OnDragStart', d.id); } catch {}
    }

    _resetItem(el) {
        el.classList.remove('arcadia-draggrid__item--dragging');
        el.style.position = '';
        el.style.width = '';
        el.style.height = '';
        el.style.left = '';
        el.style.top = '';
        el.style.zIndex = '';
        el.style.transform = '';
        el.style.transition = '';
        el.style.margin = '';
        el.style.pointerEvents = '';
    }

    // ══════════════════════════════════════════
    // COLLISION: pointer-within + closestCenter
    // ══════════════════════════════════════════

    _closestCenter(px, py, excludeId) {
        const candidates = this._getItems().filter(el =>
            el.dataset.draggridId !== excludeId && el.dataset.draggridLocked !== 'true');

        // Pointer inside an item = definite target
        for (const el of candidates) {
            const r = el.getBoundingClientRect();
            if (px >= r.left && px <= r.right && py >= r.top && py <= r.bottom) {
                return el.dataset.draggridId;
            }
        }

        // Fallback: nearest center within 400px
        let closest = null, closestDist = Infinity;
        for (const el of candidates) {
            const r = el.getBoundingClientRect();
            const dist = Math.hypot(px - (r.left + r.width/2), py - (r.top + r.height/2));
            if (dist < closestDist) { closestDist = dist; closest = el.dataset.draggridId; }
        }
        // No distance cap — always return the nearest item
        return closest;
    }

    // ══════════════════════════════════════════
    // DROP TARGET HIGHLIGHT
    // ══════════════════════════════════════════

    _highlight(id) {
        if (this._highlightedId === id) return;
        if (this._highlightedId) {
            const old = this._el(this._highlightedId);
            if (old) old.classList.remove('arcadia-draggrid__item--drop-target');
        }
        this._highlightedId = id;
        if (id) {
            const el = this._el(id);
            if (el) el.classList.add('arcadia-draggrid__item--drop-target');
        }
    }

    // ══════════════════════════════════════════
    // FLIP ANIMATION
    // ══════════════════════════════════════════

    _flipFromSnapshot(firstRects) {
        // Called AFTER Blazor has re-rendered. Read current (final) positions
        // and animate from firstRects to here.
        for (const el of this._getItems()) {
            const id = el.dataset.draggridId;
            const first = firstRects.get(id);
            if (!first) continue;

            const last = el.getBoundingClientRect();
            const dx = first.left - last.left;
            const dy = first.top - last.top;
            if (Math.abs(dx) < 2 && Math.abs(dy) < 2) continue;

            // INVERT — jump to old position
            el.style.transform = `translate(${dx}px, ${dy}px)`;
            el.style.transition = 'none';
        }

        // Force layout with inverse transforms applied
        this.grid.offsetHeight;

        // PLAY — animate back to natural position
        requestAnimationFrame(() => {
            for (const el of this._getItems()) {
                if (!el.style.transform) continue;
                el.style.transition = 'transform 0.45s cubic-bezier(0.22, 1, 0.36, 1)';
                el.style.transform = '';
            }
            // Cleanup after animation
            setTimeout(() => {
                for (const el of this._getItems()) {
                    el.style.transition = '';
                    el.style.transform = '';
                }
            }, 500);
        });
    }

    _flipAll(firstRects) {
        for (const el of this._getItems()) {
            const id = el.dataset.draggridId;
            const first = firstRects.get(id);
            if (!first) continue;

            const last = el.getBoundingClientRect();
            const dx = first.left - last.left;
            const dy = first.top - last.top;
            if (Math.abs(dx) < 1 && Math.abs(dy) < 1) continue;

            el.style.transform = `translate(${dx}px, ${dy}px)`;
            el.style.transition = 'none';

            requestAnimationFrame(() => {
                requestAnimationFrame(() => {
                    el.style.transition = 'transform 0.45s cubic-bezier(0.22, 1, 0.36, 1)';
                    el.style.transform = '';
                    const cleanup = () => { el.style.transition = ''; };
                    el.addEventListener('transitionend', cleanup, { once: true });
                    setTimeout(cleanup, 550);
                });
            });
        }
    }

    // ══════════════════════════════════════════
    // SPRING PHYSICS
    // ══════════════════════════════════════════

    _springAnimate(el, fromX, fromY, onComplete) {
        const k = this.options.springStiffness || 170;
        const c = this.options.springDamping || 26;
        let x = fromX, y = fromY, vx = 0, vy = 0, lastT = performance.now(), rafId;

        const step = (now) => {
            const dt = Math.min((now - lastT) / 1000, 0.032); lastT = now;
            vx += (-k * x - c * vx) * dt; vy += (-k * y - c * vy) * dt;
            x += vx * dt; y += vy * dt;
            el.style.transform = `translate(${x}px, ${y}px)`;
            if (Math.abs(x) < 0.5 && Math.abs(y) < 0.5 && Math.abs(vx) < 20 && Math.abs(vy) < 20) {
                el.style.transform = ''; onComplete(); return;
            }
            rafId = requestAnimationFrame(step);
        };
        rafId = requestAnimationFrame(step);
        setTimeout(() => { if (rafId) { cancelAnimationFrame(rafId); el.style.transform = ''; onComplete(); } }, 1000);
    }

    // ══════════════════════════════════════════
    // RESIZE
    // ══════════════════════════════════════════

    _startResize(e) {
        const handle = e.target.closest('.arcadia-draggrid__resize-handle');
        if (!handle) return;
        const itemId = handle.dataset.draggridResize;
        const item = this._el(itemId);
        if (!item) return;
        e.preventDefault();
        handle.setPointerCapture(e.pointerId);
        const rect = item.getBoundingClientRect();
        const gridRect = this.grid.getBoundingClientRect();
        const cellW = (gridRect.width - (this.options.columns - 1) * this.options.gap) / this.options.columns;
        this.resizing = { id: itemId, el: item, pointerId: e.pointerId,
            startX: e.clientX, startY: e.clientY, startW: rect.width, startH: rect.height,
            cellW, cellH: this.options.rowHeight, gap: this.options.gap };
        item.classList.add('arcadia-draggrid__item--resizing');
    }

    _handleResizeMove(e) {
        const r = this.resizing; if (!r) return;
        const maxCs = this.options.columns; // can't exceed grid width
        r.cs = Math.min(maxCs, Math.max(1, Math.round((r.startW + e.clientX - r.startX) / (r.cellW + r.gap))));
        r.rs = Math.max(1, Math.round((r.startH + e.clientY - r.startY) / (r.cellH + r.gap)));
        // Preview the resize but keep the explicit start position
        const colStart = r.el.style.gridColumnStart || '1';
        const rowStart = r.el.style.gridRowStart || '1';
        r.el.style.gridColumn = `${colStart} / span ${r.cs}`;
        r.el.style.gridRow = `${rowStart} / span ${r.rs}`;
    }

    _finishResize(e) {
        const r = this.resizing; if (!r) return;
        r.el.classList.remove('arcadia-draggrid__item--resizing');
        try { r.el.releasePointerCapture(r.pointerId); } catch {}

        // Snapshot positions before recompute
        const firstRects = new Map();
        this._getItems().forEach(el => {
            firstRects.set(el.dataset.draggridId, el.getBoundingClientRect());
        });

        // Update spans and recompute full layout
        this.itemSpans.set(r.id, { colSpan: r.cs || 1, rowSpan: r.rs || 1 });
        this._applyLayout();
        this.grid.offsetHeight;

        // Notify C# first, then animate after Blazor re-render
        try { this.dotNetRef.invokeMethodAsync('OnResizeComplete', r.id, r.cs || 1, r.rs || 1); } catch {}

        if (!this.reducedMotion) {
            requestAnimationFrame(() => {
                requestAnimationFrame(() => {
                    this._flipFromSnapshot(firstRects);
                });
            });
        }
        this.resizing = null;
    }

    // ══════════════════════════════════════════
    // SYNC
    // ══════════════════════════════════════════

    updateLayout(json) {
        try {
            const layout = JSON.parse(json);
            this.order = layout.items.sort((a, b) => a.order - b.order).map(i => i.id);
            this._applyLayout();
        } catch {}
    }

    setEditMode(enabled) {
        this.options.editMode = enabled;
        this.grid.classList.toggle('arcadia-draggrid--edit', enabled);
    }

    setWiggleMode(enabled) {
        this._wiggleActive = enabled;
        this.grid.classList.toggle('arcadia-draggrid--wiggle', enabled);
        // Only toggle --edit for longpress mode; in direct mode, Blazor controls --edit
        if (this.options.dragMode === 'longpress') {
            this.grid.classList.toggle('arcadia-draggrid--edit', enabled);
        }
        if (!enabled) {
            if (this._longPressTimer) { clearTimeout(this._longPressTimer); this._longPressTimer = null; }
        }
    }
}
