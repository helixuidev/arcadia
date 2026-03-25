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

// Column resize via pointer events
export function initResizeHandles(tableElement, minWidth) {
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
                handle.classList.remove('arcadia-grid__resize-handle--active');
                document.body.style.cursor = '';
                document.body.style.userSelect = '';
                document.removeEventListener('pointermove', onMove);
                document.removeEventListener('pointerup', onUp);
            };

            document.addEventListener('pointermove', onMove);
            document.addEventListener('pointerup', onUp);
        });
    });
}
