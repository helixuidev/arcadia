// Arcadia DataGrid — JS interop

export function downloadCsv(csv, filename) {
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename || 'export.csv';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

// Download binary blob (Excel, etc.)
export function downloadBlob(bytes, filename, mimeType) {
    const blob = new Blob([new Uint8Array(bytes)], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

// Copy text to clipboard
export function copyToClipboard(text) {
    if (navigator.clipboard) {
        return navigator.clipboard.writeText(text);
    }
    // Fallback for older browsers
    const ta = document.createElement('textarea');
    ta.value = text;
    ta.style.position = 'fixed';
    ta.style.opacity = '0';
    document.body.appendChild(ta);
    ta.select();
    document.execCommand('copy');
    document.body.removeChild(ta);
}

// State persistence (localStorage)
export function saveState(key, json) {
    try { localStorage.setItem('arcadia-grid-' + key, json); } catch {}
}
export function loadState(key) {
    try { return localStorage.getItem('arcadia-grid-' + key); } catch { return null; }
}

// Print grid in a new window — uses DOM construction instead of document.write
export function printGrid(html) {
    const win = window.open('', '_blank', 'width=900,height=600');
    if (!win) return;
    const doc = win.document;
    doc.open();
    // Build the page structure via DOM API
    const style = doc.createElement('style');
    style.textContent = `
      body { font-family: system-ui, sans-serif; margin: 20px; }
      table { page-break-inside: auto; border-collapse: collapse; width: 100%; }
      th, td { border: 1px solid #ddd; padding: 6px 10px; text-align: left; font-size: 13px; }
      th { background: #f5f5f5; font-weight: 600; }
      tr { page-break-inside: avoid; }
      @media print { body { margin: 0; } }
    `;
    doc.head.appendChild(style);
    doc.title = 'Print Grid';
    // Parse HTML safely via DOMParser to strip scripts/event handlers
    const parsed = new DOMParser().parseFromString(html, 'text/html');
    parsed.querySelectorAll('script,iframe,object,embed').forEach(el => el.remove());
    parsed.querySelectorAll('*').forEach(el => {
        for (const attr of [...el.attributes]) {
            if (attr.name.startsWith('on') || attr.value.trim().toLowerCase().startsWith('javascript:')) {
                el.removeAttribute(attr.name);
            }
        }
    });
    // Adopt sanitized nodes into the print window
    for (const node of [...parsed.body.childNodes]) {
        doc.body.appendChild(doc.adoptNode(node));
    }
    doc.close();
    win.focus();
    win.print();
}

// Infinite scroll — IntersectionObserver on sentinel element
export function observeInfiniteScroll(sentinelEl, dotNetRef) {
    const observer = new IntersectionObserver(entries => {
        if (entries[0].isIntersecting) {
            dotNetRef.invokeMethodAsync('OnInfiniteScrollTrigger');
        }
    }, { threshold: 0.1 });
    observer.observe(sentinelEl);
    return observer;
}

// Column resize via pointer events
export function initResizeHandles(tableElement, minWidth, dotNetRef) {
    if (!tableElement) return;
    const min = minWidth || 50;
    const headers = tableElement.querySelectorAll('th .arcadia-grid__resize-handle');

    headers.forEach(handle => {
        handle.addEventListener('pointerdown', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const th = handle.closest('th');
            if (!th) return;
            const startX = e.clientX;
            const startWidth = th.offsetWidth;
            handle.classList.add('arcadia-grid__resize-handle--active');
            document.body.style.cursor = 'col-resize';
            document.body.style.userSelect = 'none';

            const idx = Array.from(th.parentElement.children).indexOf(th);
            const table = th.closest('table');

            const onMove = (e2) => {
                const newWidth = Math.max(min, startWidth + (e2.clientX - startX));
                th.style.width = newWidth + 'px';
                th.style.minWidth = newWidth + 'px';
                if (table) {
                    table.querySelectorAll('tbody tr, tfoot tr').forEach(tr => {
                        const cell = tr.children[idx];
                        if (cell) {
                            cell.style.width = newWidth + 'px';
                            cell.style.minWidth = newWidth + 'px';
                        }
                    });
                }
            };

            const onUp = () => {
                const finalWidth = th.offsetWidth;
                handle.classList.remove('arcadia-grid__resize-handle--active');
                document.body.style.cursor = '';
                document.body.style.userSelect = '';
                document.removeEventListener('pointermove', onMove);
                document.removeEventListener('pointerup', onUp);

                // Report the new width back to .NET for state persistence
                if (dotNetRef) {
                    try {
                        dotNetRef.invokeMethodAsync('OnColumnResized', idx, finalWidth + 'px');
                    } catch (err) {
                        // Circuit may be disconnected
                    }
                }
            };

            document.addEventListener('pointermove', onMove);
            document.addEventListener('pointerup', onUp);
        });
    });
}
