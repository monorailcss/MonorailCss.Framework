// Glue for the WebAssembly playground island (docs/MonorailCss.Docs.Client/PlaygroundIsland.razor).
// Only the playground page uses this; it is inert everywhere else.
(function () {
    let themeObserver = null;

    window.monorailPlayground = {
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
