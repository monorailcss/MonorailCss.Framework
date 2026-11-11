// Glue for the WebAssembly playground island (docs/MonorailCss.Docs.Client/PlaygroundIsland.razor).
// Only the playground page uses this; it is inert everywhere else.
(function () {
    let themeObserver = null;

    // ---- draggable pane splitters ------------------------------------------------
    // A handle is any [data-pg-gutter="<name>"] element sitting between two tracks of its
    // parent grid. Dragging writes the split as an fr pair (--pg-<name>-a / -b) on the parent,
    // which the grid templates in PlaygroundIsland.razor read; Monaco's automaticLayout follows
    // the resize by itself. Delegated from the document so it survives Blazor re-renders, and
    // pointer capture keeps the drag alive while the pointer is over the preview iframe.
    const MIN_SPLIT = 0.15;
    const wideLayout = window.matchMedia('(min-width: 64rem)');

    // The main handle splits left/right when the panes sit side by side (lg+), top/bottom when
    // they stack; the editor-stack handle is always top/bottom.
    const isHorizontalSplit = (gutter) => gutter.dataset.pgGutter === 'main' && wideLayout.matches;

    function setSplit(gutter, ratio) {
        const r = Math.min(1 - MIN_SPLIT, Math.max(MIN_SPLIT, ratio));
        const name = gutter.dataset.pgGutter;
        gutter.parentElement.style.setProperty(`--pg-${name}-a`, `${r}fr`);
        gutter.parentElement.style.setProperty(`--pg-${name}-b`, `${1 - r}fr`);
        gutter.setAttribute('aria-valuenow', String(Math.round(r * 100)));
    }

    function resetSplit(gutter) {
        const name = gutter.dataset.pgGutter;
        gutter.parentElement.style.removeProperty(`--pg-${name}-a`);
        gutter.parentElement.style.removeProperty(`--pg-${name}-b`);
        gutter.removeAttribute('aria-valuenow');
    }

    const gutterOf = (e) => (e.target instanceof Element ? e.target.closest('[data-pg-gutter]') : null);

    document.addEventListener('pointerdown', (e) => {
        const gutter = gutterOf(e);
        if (!gutter || e.button !== 0) return;
        e.preventDefault();
        gutter.setPointerCapture(e.pointerId);
        document.body.classList.add('pg-dragging');
    });

    document.addEventListener('pointermove', (e) => {
        const gutter = gutterOf(e);
        if (!gutter || !gutter.hasPointerCapture(e.pointerId)) return;
        const rect = gutter.parentElement.getBoundingClientRect();
        setSplit(gutter, isHorizontalSplit(gutter)
            ? (e.clientX - rect.left) / rect.width
            : (e.clientY - rect.top) / rect.height);
    });

    const endDrag = (e) => {
        const gutter = gutterOf(e);
        if (gutter && gutter.hasPointerCapture(e.pointerId)) gutter.releasePointerCapture(e.pointerId);
        document.body.classList.remove('pg-dragging');
    };
    document.addEventListener('pointerup', endDrag);
    document.addEventListener('pointercancel', endDrag);

    document.addEventListener('dblclick', (e) => {
        const gutter = gutterOf(e);
        if (gutter) resetSplit(gutter);
    });

    // Keyboard: arrow keys nudge the focused handle along its axis, Home resets.
    document.addEventListener('keydown', (e) => {
        const gutter = gutterOf(e);
        if (!gutter) return;
        const horizontal = isHorizontalSplit(gutter);
        const step = e.key === (horizontal ? 'ArrowLeft' : 'ArrowUp') ? -0.02
            : e.key === (horizontal ? 'ArrowRight' : 'ArrowDown') ? 0.02 : 0;
        if (step !== 0) {
            e.preventDefault();
            setSplit(gutter, (Number(gutter.getAttribute('aria-valuenow') ?? 50) / 100) + step);
        } else if (e.key === 'Home') {
            e.preventDefault();
            resetSplit(gutter);
        }
    });

    // The main handle changes axis with the layout; keep its ARIA orientation honest.
    const syncOrientation = () => document.querySelectorAll('[data-pg-gutter="main"]').forEach((g) =>
        g.setAttribute('aria-orientation', wideLayout.matches ? 'vertical' : 'horizontal'));
    wideLayout.addEventListener('change', syncOrientation);
    document.addEventListener('DOMContentLoaded', syncOrientation);

    window.monorailPlayground = {
        // User Timing mark for a playground milestone (see MarkAsync in PlaygroundIsland.razor).
        mark: (name) => performance.mark(name),

        // Push {html, css} into the sandboxed preview iframe.
        postPreviewMessage: (element, data) => {
            if (element && element.contentWindow) {
                element.contentWindow.postMessage(data, '*');
            }
        },

        // The docs toggle theme by stamping/removing `.dark` on <html> (see App.razor).
        isDark: () => document.documentElement.classList.contains('dark'),

        // Report light/dark changes to the island so the Monaco editors recolour with the page.
        // Returns the current theme so the caller can pick the initial editor theme.
        observeTheme: (dotnetRef) => {
            const root = document.documentElement;
            let last = root.classList.contains('dark');
            themeObserver = new MutationObserver(() => {
                const now = root.classList.contains('dark');
                if (now !== last) {
                    last = now;
                    dotnetRef.invokeMethodAsync('OnHostThemeChanged', now ? 'dark' : 'light');
                }
            });
            themeObserver.observe(root, { attributes: true, attributeFilter: ['class'] });
            return last ? 'dark' : 'light';
        },

        disposeTheme: () => {
            if (themeObserver) {
                themeObserver.disconnect();
                themeObserver = null;
            }
        }
    };
})();
