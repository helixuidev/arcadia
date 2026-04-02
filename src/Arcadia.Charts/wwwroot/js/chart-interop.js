/**
 * Arcadia Charts — JS Interop Module
 * Handles: tooltips, resize, export, pan/zoom
 * ~12KB minified. Core SVG rendering stays in C#.
 */

// ── Tooltip ──────────────────────────────────────────

let tooltipEl = null;

function ensureTooltip() {
  if (!tooltipEl) {
    tooltipEl = document.createElement('div');
    tooltipEl.className = 'arcadia-tooltip';
    tooltipEl.style.cssText = `
      position: fixed; z-index: 9999; pointer-events: none;
      background: rgba(15, 10, 40, 0.95); backdrop-filter: blur(8px);
      border: 1px solid rgba(139, 92, 246, 0.3); border-radius: 8px;
      padding: 8px 12px; font-size: 12px; color: #f1f0f9;
      font-family: system-ui, sans-serif; line-height: 1.5;
      box-shadow: 0 4px 20px rgba(0,0,0,0.4);
      opacity: 0; transition: opacity 0.15s ease;
      max-width: 240px;
    `;
    document.body.appendChild(tooltipEl);
  }
  return tooltipEl;
}

// Sanitize HTML: parse via DOMParser, strip scripts and event handler attributes
function sanitizeHtml(html) {
  const doc = new DOMParser().parseFromString(html, 'text/html');
  // Remove <script>, <iframe>, <object>, <embed>, <form> tags
  doc.querySelectorAll('script,iframe,object,embed,form,link[rel="import"]').forEach(el => el.remove());
  // Remove all on* event handler attributes
  doc.querySelectorAll('*').forEach(el => {
    for (const attr of [...el.attributes]) {
      if (attr.name.startsWith('on') || attr.value.trim().toLowerCase().startsWith('javascript:')) {
        el.removeAttribute(attr.name);
      }
    }
  });
  return doc.body.childNodes;
}

export function showTooltip(html, x, y) {
  const el = ensureTooltip();
  el.replaceChildren(...sanitizeHtml(html));
  el.style.opacity = '1';
  
  // Smart positioning — keep within viewport
  const rect = el.getBoundingClientRect();
  const vw = window.innerWidth;
  const vh = window.innerHeight;
  
  let left = x + 12;
  let top = y - 8;
  
  if (left + rect.width > vw - 8) left = x - rect.width - 12;
  if (top + rect.height > vh - 8) top = vh - rect.height - 8;
  if (top < 8) top = 8;
  
  el.style.left = left + 'px';
  el.style.top = top + 'px';
}

export function hideTooltip() {
  if (tooltipEl) tooltipEl.style.opacity = '0';
}

// ── Resize Observer ──────────────────────────────────

const observers = new Map();

export function observeResize(element, dotNetRef) {
  if (!element) return;

  // Clean up any existing observer for this element
  const existing = observers.get(element);
  if (existing) {
    existing.disconnect();
    observers.delete(element);
  }

  const observer = new ResizeObserver(entries => {
    for (const entry of entries) {
      const { width, height } = entry.contentRect;
      try {
        dotNetRef.invokeMethodAsync('OnContainerResized', width, height);
      } catch (e) {
        // DotNetRef was disposed — stop observing
        observer.disconnect();
        observers.delete(element);
      }
    }
  });

  observer.observe(element);
  observers.set(element, observer);
}

export function unobserveResize(element) {
  const observer = observers.get(element);
  if (observer) {
    observer.disconnect();
    observers.delete(element);
  }
}

// ── Export ────────────────────────────────────────────

export function exportSvgAsString(containerEl) {
  const svg = containerEl.querySelector('svg[data-chart]');
  if (!svg) return null;
  const serializer = new XMLSerializer();
  return serializer.serializeToString(svg);
}

export function exportAsPng(containerEl, filename, scale = 2) {
  const svg = containerEl.querySelector('svg[data-chart]');
  if (!svg) return;
  
  const svgData = new XMLSerializer().serializeToString(svg);
  const width = svg.getAttribute('width') || svg.viewBox.baseVal.width || 600;
  const height = svg.getAttribute('height') || svg.viewBox.baseVal.height || 300;
  
  const canvas = document.createElement('canvas');
  canvas.width = width * scale;
  canvas.height = height * scale;
  const ctx = canvas.getContext('2d');
  ctx.scale(scale, scale);
  
  const img = new Image();
  const blob = new Blob([svgData], { type: 'image/svg+xml;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  
  img.onload = () => {
    // White background
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, width, height);
    ctx.drawImage(img, 0, 0, width, height);
    URL.revokeObjectURL(url);
    
    canvas.toBlob(blob => {
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = filename || 'chart.png';
      a.click();
      URL.revokeObjectURL(a.href);
    }, 'image/png');
  };
  
  img.src = url;
}

export function exportAsSvg(containerEl, filename) {
  const svg = containerEl.querySelector('svg[data-chart]');
  if (!svg) return;
  
  const svgData = new XMLSerializer().serializeToString(svg);
  const blob = new Blob([svgData], { type: 'image/svg+xml;charset=utf-8' });
  const a = document.createElement('a');
  a.href = URL.createObjectURL(blob);
  a.download = filename || 'chart.svg';
  a.click();
  URL.revokeObjectURL(a.href);
}

// ── Pan & Zoom ───────────────────────────────────────

const panZoomState = new Map();

export function enablePanZoom(containerEl, dotNetRef, options = {}) {
  const svg = containerEl.querySelector('svg[data-chart]');
  if (!svg) return;
  
  const state = {
    isPanning: false,
    startX: 0, startY: 0,
    viewportX: 0, viewportY: 0,
    zoom: 1,
    minZoom: options.minZoom || 0.5,
    maxZoom: options.maxZoom || 10,
    mode: options.mode || 'x', // 'x', 'y', 'xy'
  };
  
  // Mouse wheel zoom
  svg.addEventListener('wheel', e => {
    e.preventDefault();
    const delta = e.deltaY > 0 ? 0.9 : 1.1;
    const newZoom = Math.min(state.maxZoom, Math.max(state.minZoom, state.zoom * delta));
    
    if (newZoom !== state.zoom) {
      state.zoom = newZoom;
      dotNetRef.invokeMethodAsync('OnZoomChanged', state.zoom, e.offsetX, e.offsetY);
    }
  }, { passive: false });
  
  // Pan via mouse drag — store handlers for cleanup
  const onMouseMove = e => {
    if (!state.isPanning) return;
    const dx = e.clientX - state.startX;
    const dy = e.clientY - state.startY;
    state.startX = e.clientX;
    state.startY = e.clientY;

    if (state.mode === 'x' || state.mode === 'xy') state.viewportX += dx;
    if (state.mode === 'y' || state.mode === 'xy') state.viewportY += dy;

    dotNetRef.invokeMethodAsync('OnPanChanged', state.viewportX, state.viewportY);
  };

  const onMouseUp = () => {
    state.isPanning = false;
    svg.style.cursor = '';
  };

  svg.addEventListener('mousedown', e => {
    if (e.button !== 0) return;
    state.isPanning = true;
    state.startX = e.clientX;
    state.startY = e.clientY;
    svg.style.cursor = 'grabbing';
  });

  window.addEventListener('mousemove', onMouseMove);
  window.addEventListener('mouseup', onMouseUp);
  state._onMouseMove = onMouseMove;
  state._onMouseUp = onMouseUp;
  
  // Touch pinch zoom
  let lastTouchDist = 0;
  svg.addEventListener('touchstart', e => {
    if (e.touches.length === 2) {
      lastTouchDist = Math.hypot(
        e.touches[0].clientX - e.touches[1].clientX,
        e.touches[0].clientY - e.touches[1].clientY
      );
    }
  }, { passive: true });
  
  svg.addEventListener('touchmove', e => {
    if (e.touches.length === 2) {
      const dist = Math.hypot(
        e.touches[0].clientX - e.touches[1].clientX,
        e.touches[0].clientY - e.touches[1].clientY
      );
      const scale = dist / lastTouchDist;
      state.zoom = Math.min(state.maxZoom, Math.max(state.minZoom, state.zoom * scale));
      lastTouchDist = dist;
      dotNetRef.invokeMethodAsync('OnZoomChanged', state.zoom, 0, 0);
    }
  }, { passive: true });
  
  panZoomState.set(containerEl, state);
}

export function disablePanZoom(containerEl) {
  const state = panZoomState.get(containerEl);
  if (state) {
    if (state._onMouseMove) window.removeEventListener('mousemove', state._onMouseMove);
    if (state._onMouseUp) window.removeEventListener('mouseup', state._onMouseUp);
  }
  panZoomState.delete(containerEl);
}

export function resetZoom(containerEl, dotNetRef) {
  const state = panZoomState.get(containerEl);
  if (state) {
    state.zoom = 1;
    state.viewportX = 0;
    state.viewportY = 0;
    dotNetRef.invokeMethodAsync('OnZoomChanged', 1, 0, 0);
    dotNetRef.invokeMethodAsync('OnPanChanged', 0, 0);
  }
}

// ── Slide Animation for Streaming ────────────────────

export function slideChartContent(containerEl, stepWidth, durationMs) {
  const svg = containerEl.querySelector('svg[data-chart]');
  if (!svg) return;
  const group = svg.querySelector('.arcadia-slide-group');
  if (!group) return;

  // Cancel any in-progress animation
  if (group._slideRaf) cancelAnimationFrame(group._slideRaf);

  // Immediately offset right by one step (positions are already at final state)
  group.style.transition = 'none';
  group.style.transform = `translateX(${stepWidth}px)`;

  // Use rAF to ensure the browser paints the offset, then animate to final
  group._slideRaf = requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      group.style.transition = `transform ${durationMs}ms cubic-bezier(0.25, 0.1, 0.25, 1)`;
      group.style.transform = 'translateX(0)';

      // Clean up after animation
      const cleanup = () => {
        group.style.transition = '';
        group.style.transform = '';
        group._slideRaf = null;
        group.removeEventListener('transitionend', cleanup);
      };
      group.addEventListener('transitionend', cleanup, { once: true });

      // Fallback cleanup if transitionend doesn't fire
      setTimeout(cleanup, durationMs + 100);
    });
  });
}

// ── Get Element Bounds ───────────────────────────────

export function getBounds(element) {
  if (!element) return { x: 0, y: 0, width: 0, height: 0 };
  const rect = element.getBoundingClientRect();
  return { x: rect.x, y: rect.y, width: rect.width, height: rect.height };
}
