const focusableSelector =
    'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

export function focusFirst(container) {
    if (!container) return;
    const focusable = container.querySelectorAll(focusableSelector);
    // Skip sentinel elements (first real focusable)
    for (const el of focusable) {
        if (!el.classList.contains('helix-focus-sentinel')) {
            el.focus();
            return;
        }
    }
}
