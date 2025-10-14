/**
 * Page Manager - Centralized JavaScript functionality
 * Handles theme switching, table of contents, tabs, syntax highlighting, and mobile navigation
 */
class PageManager {
    constructor() {
        this.init();
    }

    init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initializeComponents());
        } else {
            this.initializeComponents();
        }
    }

    initializeComponents() {
        this.themeManager = new ThemeManager();
        this.outlineManager = new OutlineManager();
        this.tabManager = new TabManager();
        this.syntaxHighlighter = new SyntaxHighlighter();
        this.mermaidManager = new MermaidManager();
        this.mobileNavManager = new MobileNavManager();
        this.mainSiteNavManager = new MainSiteNavManager();
        this.sidebarToggleManager = new SidebarToggleManager();
        this.searchManager = new SearchManager();

        // Initialize all components
        this.outlineManager.init();
        this.tabManager.init();
        this.syntaxHighlighter.init();
        this.mermaidManager.init();
        this.mobileNavManager.init();
        this.mainSiteNavManager.init();
        this.sidebarToggleManager.init();
        this.searchManager.init();
    }
}

/**
 * Theme Manager - Handles dark/light theme switching
 */
class ThemeManager {
    constructor() {
        this.bindThemeToggleEvents();
        
        // Make swapTheme globally available for backwards compatibility
        window.swapTheme = this.swapTheme.bind(this);
    }

    bindThemeToggleEvents() {
        // Find all elements with data-theme-toggle attribute
        const themeToggleButtons = document.querySelectorAll('[data-theme-toggle]');
        
        themeToggleButtons.forEach(button => {
            button.addEventListener('click', () => {
                this.swapTheme();
            });
        });
    }

    swapTheme() {
        const isDark = document.documentElement.classList.contains('dark');

        if (isDark) {
            document.documentElement.classList.remove('dark');
            document.documentElement.dataset.theme = 'light';
            localStorage.theme = 'light';
        } else {
            document.documentElement.classList.add('dark');
            document.documentElement.dataset.theme = 'dark';
            localStorage.theme = 'dark';
        }

        // Re-initialize mermaid with new theme
        if (window.pageManager && window.pageManager.mermaidManager) {
            window.pageManager.mermaidManager.reinitializeForTheme();
        }
    }
}

/**
 * Outline Manager - Handles outline navigation and active section highlighting
 */
class OutlineManager {
    constructor() {
        this.outlineLinks = [];
        this.sectionMap = new Map();
        this.sections = [];
        this.isScrolling = false;
        this.scrollTimeout = null;
    }

    init() {
        this.setupOutline();
        if (this.outlineLinks.length > 0) {
            this.setupScrollListener();
            // Initial highlight
            this.updateActiveSection();
        }
    }

    setupOutline() {
        this.outlineLinks = Array.from(document.querySelectorAll('[data-role="page-outline"] ul li a'));

        // Initialize all links and build section map
        this.outlineLinks.forEach(link => {
            link.dataset.selected = 'false';

            const id = this.extractIdFromHref(link.getAttribute('href'));
            if (id) {
                const section = document.getElementById(id);
                if (section) {
                    this.sectionMap.set(section, link);
                    this.sections.push(section);
                }
            }
        });

        // Sort sections by document order
        this.sections.sort((a, b) => {
            const pos = a.compareDocumentPosition(b);
            return pos & Node.DOCUMENT_POSITION_FOLLOWING ? -1 : 1;
        });
    }

    extractIdFromHref(href) {
        return href?.split('#')[1] || null;
    }

    setupScrollListener() {
        // Use passive listener for better performance
        window.addEventListener('scroll', () => {
            if (!this.isScrolling) {
                this.isScrolling = true;
                requestAnimationFrame(() => {
                    this.updateActiveSection();
                    this.isScrolling = false;
                });
            }
        }, { passive: true });
    }

    updateActiveSection() {
        this.resetAllLinks();

        const activeSection = this.findActiveSection();
        if (activeSection) {
            const link = this.sectionMap.get(activeSection);
            if (link) {
                this.activateLink(link);
            }
        }
    }

    findActiveSection() {
        if (this.sections.length === 0) return null;

        const HEADER_OFFSET = 130; // Account for fixed header
        const READING_POSITION = HEADER_OFFSET + 50; // Slightly below header for better UX

        // Find the section that should be highlighted based on scroll position
        let activeSection = null;

        for (let i = this.sections.length - 1; i >= 0; i--) {
            const section = this.sections[i];
            const rect = section.getBoundingClientRect();

            // If section top is at or above our reading position, this is our active section
            if (rect.top <= READING_POSITION) {
                activeSection = section;
                break;
            }
        }

        // If no section is above the reading position, use the first section
        return activeSection || this.sections[0];
    }

    resetAllLinks() {
        this.outlineLinks.forEach(link => {
            link.dataset.selected = 'false';
            link.parentElement?.classList.remove('active');
        });
        this.hideHighlighter();
    }

    activateLink(link) {
        link.dataset.selected = 'true';
        link.parentElement?.classList.add('active');
        this.updateHighlighter(link);
    }

    updateHighlighter(link) {
        const highlighter = document.querySelector('[data-role="page-outline-highlighter"]');
        if (!highlighter || !link) return;

        const linkRect = link.getBoundingClientRect();
        const outlineContainer = document.querySelector('[data-role="page-outline"]');
        if (!outlineContainer) return;

        const containerRect = outlineContainer.getBoundingClientRect();
        
        // Calculate position relative to the outline container
        const top = linkRect.top - containerRect.top;
        const height = linkRect.height;

        // Update highlighter position and visibility
        highlighter.style.top = `${top}px`;
        highlighter.style.height = `${height}px`;
        highlighter.classList.remove('opacity-0');
        highlighter.classList.add('opacity-100');
    }

    hideHighlighter() {
        const highlighter = document.querySelector('[data-role="page-outline-highlighter"]');
        if (highlighter) {
            highlighter.classList.remove('opacity-100');
            highlighter.classList.add('opacity-0');
        }
    }

    destroy() {
        // Clean up scroll listener if needed
        // Note: In practice, this is rarely called as the page manager persists
    }
}

/**
 * Tab Manager - Handles tab navigation and content switching
 */
class TabManager {
    constructor() {
        this.tablists = [];
    }

    init() {
        this.tablists = Array.from(document.querySelectorAll('[role="tablist"]'));
        this.tablists.forEach(tablist => this.setupTablist(tablist));
    }

    setupTablist(tablist) {
        const tablistId = tablist.id;
        if (!tablistId) return;

        const tabs = Array.from(tablist.querySelectorAll('[role="tab"]'));
        if (tabs.length === 0) return;

        // Set up event listeners
        tabs.forEach(tab => {
            tab.addEventListener('click', () => this.activateTab(tab, tabs));
        });

        // Initialize active state
        this.initializeActiveTab(tablist, tabs);
    }

    initializeActiveTab(tablist, tabs) {
        const activeTab = tablist.querySelector('[data="true"]');

        if (!activeTab && tabs.length > 0) {
            this.activateTab(tabs[0], tabs);
        } else if (activeTab) {
            this.showTabContent(activeTab);
        }
    }

    activateTab(selectedTab, allTabs) {
        // Deactivate all tabs
        allTabs.forEach(tab => {
            tab.dataset.selected = 'false';
            tab.setAttribute('data-state', 'inactive');
            tab.setAttribute('tabindex', '-1');
        });

        // Activate the selected tab
        selectedTab.dataset.selected = 'true';
        selectedTab.setAttribute('data-state', 'active');
        selectedTab.setAttribute('tabindex', '0');

        // Show corresponding content
        this.showTabContent(selectedTab);
    }

    showTabContent(tab) {
        const contentId = tab.getAttribute('aria-controls');
        if (!contentId) return;

        const contentPanel = document.getElementById(contentId);
        if (!contentPanel) return;

        // Hide all related content panels
        this.hideRelatedContentPanels(tab);

        // Show the selected content panel
        contentPanel.removeAttribute('hidden');
        contentPanel.dataset.selected = 'true';
    }

    hideRelatedContentPanels(tab) {
        const tabId = tab.id;
        const match = tabId.match(/^tabButton(.*)-\d+$/);

        if (match) {
            const baseId = match[1];
            const allContentPanels = document.querySelectorAll(`[id^="tab-content${baseId}-"]`);

            allContentPanels.forEach(panel => {
                panel.dataset.selected = 'false';
                panel.setAttribute('hidden', '');
            });
        }
    }
}

/**
 * Mermaid Manager - Handles mermaid diagram rendering with theme support
 */
class MermaidManager {
    constructor() {
        this.mermaidLoaded = false;
        this.mermaidInstance = null;
        this.diagrams = [];
        this.renderedDiagrams = []; // Track rendered diagram containers
    }

    async init() {
        this.diagrams = this.findMermaidDiagrams();
        if (this.diagrams.length === 0) return;

        try {
            await this.loadMermaid();
            await this.renderDiagrams();
        } catch (error) {
            console.error('Failed to initialize mermaid:', error);
        }
    }

    findMermaidDiagrams() {
        // Look for code blocks with class 'language-mermaid'
        return Array.from(document.querySelectorAll('code.language-mermaid'));
    }

    async loadMermaid() {
        if (this.mermaidLoaded) return;

        // Dynamically load mermaid from CDN
        this.mermaidInstance = await import('https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs');
        this.mermaidLoaded = true;
        
        this.initializeMermaid();
    }

    initializeMermaid() {
        if (!this.mermaidInstance) return;

        const isDark = document.documentElement.classList.contains('dark');
        const config = this.getMermaidConfig(isDark);
        
        // Use the correct initialization method
        this.mermaidInstance.default.initialize(config);
    }

    getMermaidConfig(isDark) {
        // Helper function to get CSS variables with fallbacks
        function getCSSVariable(variable, fallback) {
            if (typeof window === 'undefined' || typeof document === 'undefined') {
                return fallback;
            }

            const value = getComputedStyle(document.documentElement).getPropertyValue(variable).trim() || fallback;

            if (value.startsWith('oklch(')) {
                let s = oklchToHex(value);
                return s;
            }

            return value;
        }

        // Convert OKLCH string to hex (e.g. "oklch(0.881 0.061 210)" → "#hex")
        function oklchToHex(oklchStr) {
            // Parse the values from the string
            const match = oklchStr.match(/oklch\(\s*([\d.]+)\s+([\d.]+)\s+([\d.]+)\s*\)/);
            if (!match) return '#000000';

            const [_, l, c, h] = match.map(Number);

            // Convert OKLCH to OKLab
            const hRad = (h * Math.PI) / 180; // Correct hue conversion (360° range)
            const a = Math.cos(hRad) * c;
            const b = Math.sin(hRad) * c;

            // Convert OKLab to LMS (cone response)
            const l_lms = l + 0.3963377774 * a + 0.2158037573 * b;
            const m_lms = l - 0.1055613458 * a - 0.0638541728 * b;
            const s_lms = l - 0.0894841775 * a - 1.2914855480 * b;

            // Cube the LMS values to get linear LMS
            const l_linear = Math.pow(l_lms, 3);
            const m_linear = Math.pow(m_lms, 3);
            const s_linear = Math.pow(s_lms, 3);

            // Convert linear LMS to linear RGB
            const r_linear = +4.0767416621 * l_linear - 3.3077115913 * m_linear + 0.2309699292 * s_linear;
            const g_linear = -1.2684380046 * l_linear + 2.6097574011 * m_linear - 0.3413193965 * s_linear;
            const b_linear = -0.0041960863 * l_linear - 0.7034186147 * m_linear + 1.7076147010 * s_linear;

            // Convert linear RGB to sRGB
            const r = srgbTransferFn(r_linear);
            const g = srgbTransferFn(g_linear);
            const b_srgb = srgbTransferFn(b_linear);

            return rgbToHex(r, g, b_srgb);
        }

        function srgbTransferFn(x) {
            // Clamp to valid range first
            x = Math.max(0, Math.min(1, x));
            
            return x <= 0.0031308
                ? 12.92 * x
                : 1.055 * Math.pow(x, 1 / 2.4) - 0.055;
        }

        function rgbToHex(r, g, b) {
            const to255 = (x) => Math.max(0, Math.min(255, Math.round(x * 255)));
            return (
                '#' +
                to255(r).toString(16).padStart(2, '0') +
                to255(g).toString(16).padStart(2, '0') +
                to255(b).toString(16).padStart(2, '0')
            );
        }

        if (isDark) {
            return {
                startOnLoad: false,
                securityLevel: 'loose',
                logLevel: 'error',
                theme: 'base',
                darkMode: true,
                themeVariables: {
                    fontFamily: 'Lexend, sans-serif',
                    
                    // Main colors
                    primaryColor: getCSSVariable('--color-primary-600', '#BB2528'),
                    primaryTextColor: getCSSVariable('--color-primary-50', '#ffffff'),
                    
                    // Secondary colors
                    secondaryColor: getCSSVariable('--color-accent-600', '#006100'),
                    tertiaryColor: getCSSVariable('--color-tertiary-one-600', '#666666'),
                    
                    // Background colors
                    background: getCSSVariable('--color-base-950', '#0a0a0a'),
                    mainBkg: getCSSVariable('--color-base-900', '#1a1a1a'),
                    secondaryBkg: getCSSVariable('--color-base-800', '#2a2a2a'),
                    tertiaryBkg: getCSSVariable('--color-base-700', '#333333'),

                    // Note colors
                    noteBorderColor: getCSSVariable('--color-base-600', '#333333'),
                    noteBkgColor: getCSSVariable('--color-base-800', '#333333'),
                    
                    // Lines and borders
                    lineColor: getCSSVariable('--color-accent-400', '#4ade80'),
                    primaryBorderColor: getCSSVariable('--color-primary-500', '#dc2626'),
                    secondaryBorderColor: getCSSVariable('--color-accent-500', '#22c55e'),
                    tertiaryBorderColor: getCSSVariable('--color-tertiary-one-500', '#6b7280'),
                    
                    // Text colors
                    textColor: getCSSVariable('--color-base-300', '#f3f4f6'),
                    nodeTextColor: getCSSVariable('--color-primary-50', '#ffffff'),
                    edgeLabelColor: getCSSVariable('--color-base-200', '#e5e7eb'),
                    
                    // Edge and label backgrounds
                    edgeLabelBackground: getCSSVariable('--color-base-800', '#1f2937'),
                    
                    // Additional node colors for variety
                    node0: getCSSVariable('--color-primary-600', '#dc2626'),
                    node1: getCSSVariable('--color-accent-600', '#059669'),
                    node2: getCSSVariable('--color-tertiary-one-600', '#4b5563'),
                    node3: getCSSVariable('--color-tertiary-two-600', '#7c3aed')
                }
            };
        } else {
            return {
                startOnLoad: false,
                securityLevel: 'loose',
                logLevel: 'error',
                theme: 'base',
                darkMode: false,
                themeVariables: {
                    // Main colors
                    primaryColor: getCSSVariable('--color-primary-700', '#BB2528'),
                    primaryTextColor: getCSSVariable('--color-base-500', '#ffffff'),
                    
                    // Secondary colors
                    secondaryColor: getCSSVariable('--color-accent-700', '#006100'),
                    tertiaryColor: getCSSVariable('--color-tertiary-one-600', '#4b5563'),
                    
                    // Background colors
                    background: getCSSVariable('--color-base-50', '#f9fafb'),
                    mainBkg: getCSSVariable('--color-base-100', '#f3f4f6'),
                    secondaryBkg: getCSSVariable('--color-base-200', '#e5e7eb'),
                    tertiaryBkg: getCSSVariable('--color-base-150', '#f0f0f0'),

                    // Note colors
                    noteBorderColor: getCSSVariable('--color-base-200', '#333333'),
                    noteBkgColor: getCSSVariable('--monorail-color-base-100', '#333333'),


                    // Lines and borders
                    lineColor: getCSSVariable('--color-accent-600', '#16a34a'),
                    primaryBorderColor: getCSSVariable('--color-primary-600', '#dc2626'),
                    secondaryBorderColor: getCSSVariable('--color-accent-600', '#16a34a'),
                    tertiaryBorderColor: getCSSVariable('--color-tertiary-one-400', '#9ca3af'),
                    
                    // Text colors
                    textColor: getCSSVariable('--color-base-900', '#111827'),
                    nodeTextColor: getCSSVariable('--color-base-900', '#ffffff'),
                    edgeLabelColor: getCSSVariable('--color-base-700', '#374151'),
                    
                    // Edge and label backgrounds
                    edgeLabelBackground: getCSSVariable('--color-base-100', '#f3f4f6'),
                    
                    // Additional node colors for variety
                    node0: getCSSVariable('--color-primary-600', '#dc2626'),
                    node1: getCSSVariable('--color-accent-600', '#16a34a'),
                    node2: getCSSVariable('--color-tertiary-one-600', '#4b5563'),
                    node3: getCSSVariable('--color-tertiary-two-600', '#7c3aed')
                }
            };
        }
    }

    async renderDiagrams() {
        if (!this.mermaidInstance || this.diagrams.length === 0) return;

        for (let i = 0; i < this.diagrams.length; i++) {
            const codeElement = this.diagrams[i];
            const diagramText = codeElement.textContent;
            
            try {
                const {svg} = await this.mermaidInstance.default.render(`mermaid-diagram-${i}`, diagramText);
                
                // Create a div to hold the SVG
                const diagramContainer = document.createElement('div');
                diagramContainer.className = 'mermaid-diagram';
                diagramContainer.innerHTML = svg;
                diagramContainer.dataset.originalText = diagramText; // Store original text for re-rendering
                
                // Replace the code element with the rendered diagram
                codeElement.parentNode.replaceChild(diagramContainer, codeElement);
                
                // Track the rendered diagram
                this.renderedDiagrams.push(diagramContainer);
            } catch (error) {
                console.error(`Failed to render mermaid diagram ${i}:`, error);
            }
        }
    }

    async reinitializeForTheme() {
        if (!this.mermaidLoaded || this.renderedDiagrams.length === 0) return;

        // Re-initialize mermaid with new theme
        this.initializeMermaid();
        
        // Re-render all existing diagrams
        for (let i = 0; i < this.renderedDiagrams.length; i++) {
            const diagramContainer = this.renderedDiagrams[i];
            const diagramText = diagramContainer.dataset.originalText;
            
            if (diagramText) {
                try {
                    const {svg} = await this.mermaidInstance.default.render(`mermaid-diagram-theme-${i}`, diagramText);
                    diagramContainer.innerHTML = svg;
                } catch (error) {
                    console.error(`Failed to re-render mermaid diagram ${i} for theme:`, error);
                }
            }
        }
    }
}

/**
 * Mobile Navigation Manager - Handles mobile menu toggle and interaction
 */
class MobileNavManager {
    constructor() {
        this.menuToggle = null;
        this.navSidebar = null;
        this.mobileOverlay = null;
        this.isInitialized = false;
    }

    init() {
        this.menuToggle = document.getElementById('menu-toggle');
        this.navSidebar = document.getElementById('nav-sidebar');
        this.mobileOverlay = document.getElementById('mobile-overlay');
        
        if (this.menuToggle && this.navSidebar) {
            this.setupEventListeners();
            this.isInitialized = true;
        }
    }

    setupEventListeners() {
        // Toggle menu on button click
        this.menuToggle.addEventListener('click', () => {
            this.toggleMenu();
        });
        
        // Close menu when clicking on a link (mobile only)
        this.navSidebar.addEventListener('click', (e) => {
            if (e.target.tagName === 'A' && window.innerWidth < 1024) {
                this.closeMenu();
            }
        });
        
        // Close menu when clicking on overlay
        if (this.mobileOverlay) {
            this.mobileOverlay.addEventListener('click', () => {
                this.closeMenu();
            });
        }
        
        // Close menu when clicking outside (mobile only)
        document.addEventListener('click', (e) => {
            if (window.innerWidth < 1024 && 
                !this.navSidebar.contains(e.target) && 
                !this.menuToggle.contains(e.target) && 
                this.isMenuOpen()) {
                this.closeMenu();
            }
        });

        // Close menu on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isMenuOpen()) {
                this.closeMenu();
            }
        });
    }

    toggleMenu() {
        if (this.isMenuOpen()) {
            this.closeMenu();
        } else {
            this.openMenu();
        }
    }

    isMenuOpen() {
        return this.navSidebar.getAttribute('aria-expanded') === 'true';
    }

    closeMenu() {
        this.navSidebar.dataset.expanded = 'false';
        
        if (this.mobileOverlay) {
            this.mobileOverlay.setAttribute('aria-hidden', 'true');
        }
        
        // Re-enable body scrolling
        document.body.setAttribute('data-mobile-menu-open', 'false');
    }

    openMenu() {
        this.navSidebar.dataset.expanded = 'true';
        
        if (this.mobileOverlay) {
            this.mobileOverlay.setAttribute('aria-hidden', 'false');
        }
        
        // Prevent body scrolling when menu is open
        document.body.setAttribute('data-mobile-menu-open', 'true');
    }
}

/**
 * Main Site Navigation Manager - Handles hamburger menu for main site links
 */
class MainSiteNavManager {
    constructor() {
        this.menuButton = null;
        this.mobileMenu = null;
    }

    init() {
        this.menuButton = document.getElementById('mobile-menu-button');
        this.mobileMenu = document.getElementById('mobile-menu');
        
        if (this.menuButton && this.mobileMenu) {
            this.setupEventListeners();
        }
    }

    setupEventListeners() {
        // Toggle menu on button click
        this.menuButton.addEventListener('click', () => {
            this.toggleMenu();
        });
        
        // Close menu when clicking on a link
        this.mobileMenu.addEventListener('click', (e) => {
            if (e.target.tagName === 'A') {
                this.closeMenu();
            }
        });
        
        // Close menu when clicking outside
        document.addEventListener('click', (e) => {
            if (!this.mobileMenu.contains(e.target) && 
                !this.menuButton.contains(e.target) && 
                this.isMenuOpen()) {
                this.closeMenu();
            }
        });

        // Close menu on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isMenuOpen()) {
                this.closeMenu();
            }
        });
        
        // Close menu when window is resized to desktop
        window.addEventListener('resize', () => {
            if (window.innerWidth >= 768) { // md breakpoint
                this.closeMenu();
            }
        });
    }

    toggleMenu() {
        if (this.isMenuOpen()) {
            this.closeMenu();
        } else {
            this.openMenu();
        }
    }

    isMenuOpen() {
        return this.mobileMenu.dataset.expanded === 'true';
    }

    openMenu() {
        this.mobileMenu.dataset.expanded = 'true';
    }

    closeMenu() {
        this.mobileMenu.dataset.expanded = 'false';
    }
}

/**
 * Sidebar Toggle Manager - Handles table of contents sidebar toggle for Spectre.Console-style layouts
 */
class SidebarToggleManager {
    constructor() {
        this.sidebarToggle = null;
        this.sidebarOverlay = null;
        this.sidebarClose = null;
        this.sidebarPanel = null;
    }

    init() {
        this.sidebarToggle = document.getElementById('sidebar-toggle');
        this.sidebarOverlay = document.getElementById('sidebar-overlay');
        this.sidebarClose = document.getElementById('sidebar-close');
        this.sidebarPanel = document.getElementById('sidebar-panel');
        
        if (this.sidebarToggle && this.sidebarOverlay) {
            this.setupEventListeners();
        }
    }

    setupEventListeners() {
        // Toggle sidebar on button click
        this.sidebarToggle.addEventListener('click', () => {
            this.toggleSidebar();
        });
        
        // Close sidebar when clicking close button
        if (this.sidebarClose) {
            this.sidebarClose.addEventListener('click', () => {
                this.closeSidebar();
            });
        }
        
        // Close sidebar when clicking on overlay (but not the panel)
        this.sidebarOverlay.addEventListener('click', (e) => {
            if (e.target === this.sidebarOverlay) {
                this.closeSidebar();
            }
        });
        
        // Stop propagation on panel to prevent closing when clicking inside
        if (this.sidebarPanel) {
            this.sidebarPanel.addEventListener('click', (e) => {
                e.stopPropagation();
            });
        }
        
        // Close sidebar on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isSidebarOpen()) {
                this.closeSidebar();
            }
        });
    }

    toggleSidebar() {
        if (this.isSidebarOpen()) {
            this.closeSidebar();
        } else {
            this.openSidebar();
        }
    }

    isSidebarOpen() {
        return !this.sidebarOverlay.classList.contains('hidden');
    }

    openSidebar() {
        this.sidebarOverlay.classList.remove('hidden');
        document.body.setAttribute('data-sidebar-open', 'true');
    }

    closeSidebar() {
        this.sidebarOverlay.classList.add('hidden');
        document.body.setAttribute('data-sidebar-open', 'false');
    }
}

/**
 * Search Manager - Handles custom search with FlexSearch
 */
class SearchManager {
    constructor() {
        this.searchInput = null;
        this.searchModal = null;
        this.searchResults = null;
        this.flexSearchLoaded = false;
        this.searchIndex = null;
        this.searchData = null;
        this.FlexSearch = null;
        this.searchIndexFailed = false; // Track if search index loading failed
    }

    async init() {
        this.searchInput = document.getElementById('search-input');
        if (!this.searchInput) return;

        this.createSearchModal();
        this.setupEventListeners();
        
        // Don't load search index until user actually wants to search
    }

    createSearchModal() {
        // Create modal backdrop
        const modalBackdrop = document.createElement('div');
        modalBackdrop.id = 'search-modal-backdrop';
        modalBackdrop.className = 'search-modal-backdrop hidden';
        
        // Create modal content
        const modalContent = document.createElement('div');
        modalContent.className = 'search-modal-content';
        
        modalContent.innerHTML = `
            <div class="search-modal-header">
                <div class="search-modal-input-container">
                    <input
                        id="search-modal-input"
                        type="text"
                        placeholder="Search documentation..."
                        autocomplete="off"
                        class="search-modal-input"
                    />
                    <svg class="search-modal-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <circle cx="11" cy="11" r="8"></circle>
                        <path d="M21 21l-4.35-4.35"></path>
                    </svg>
                </div>
            </div>
            <div id="search-results" class="search-modal-results">
                <div class="search-modal-placeholder">
                    Start typing to search...
                </div>
            </div>
        `;
        
        modalBackdrop.appendChild(modalContent);
        document.body.appendChild(modalBackdrop);
        
        this.searchModal = modalBackdrop;
        this.searchResults = document.getElementById('search-results');
        this.modalInput = document.getElementById('search-modal-input');
    }

    setupEventListeners() {
        // Open modal when clicking search input
        this.searchInput.addEventListener('click', (e) => {
            e.preventDefault();
            this.openModal();
        });

        // Close modal when clicking backdrop
        this.searchModal.addEventListener('click', (e) => {
            if (e.target === this.searchModal) {
                this.closeModal();
            }
        });

        // Close modal on escape key and open modal on Cmd+K
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && !this.searchModal.classList.contains('hidden')) {
                this.closeModal();
            }
            
            // Open search modal with Cmd+K (Mac) or Ctrl+K (Windows/Linux)
            if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
                e.preventDefault();
                this.openModal();
            }
        });

        // Search as user types
        let searchTimeout;
        this.modalInput.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                this.performSearch(e.target.value);
            }, 300);
        });
    }

    async openModal() {
        this.searchModal.classList.remove('hidden');
        this.modalInput.focus();
        document.body.style.overflow = 'hidden';
        
        // Check if search index loading previously failed
        if (this.searchIndexFailed) {
            this.searchResults.innerHTML = '<div class="search-modal-error">Search is currently unavailable</div>';
            return;
        }
        
        // Load search index on first open
        if (!this.searchData) {
            this.searchResults.innerHTML = '<div class="search-modal-loading">Loading search index...</div>';
            try {
                await this.loadSearchIndex();
                this.searchResults.innerHTML = '<div class="search-modal-placeholder">Start typing to search...</div>';
            } catch (error) {
                console.error('Failed to load search index:', error);
                this.searchIndexFailed = true; // Mark as failed to prevent retries
                this.searchResults.innerHTML = '<div class="search-modal-error">Search is currently unavailable</div>';
            }
        }
    }

    closeModal() {
        this.searchModal.classList.add('hidden');
        this.modalInput.value = '';
        document.body.style.overflow = '';
        this.searchResults.innerHTML = '<div class="search-modal-placeholder">Start typing to search...</div>';
    }

    async loadSearchIndex() {
        if (this.searchData) return;

        try {
            // Load FlexSearch using ES modules
            if (!this.flexSearchLoaded) {
                const flexSearchModule = await import('https://cdnjs.cloudflare.com/ajax/libs/FlexSearch/0.8.2/flexsearch.bundle.module.min.js');
                this.FlexSearch = flexSearchModule.default;
                this.flexSearchLoaded = true;
            }

            // Fetch search index using base URL from body data attribute
            let baseUrl = document.body.getAttribute('data-base-url') || '';
            if (baseUrl.endsWith('/')) {
                baseUrl = baseUrl.slice(0, -1);
            }
            const searchIndexUrl = baseUrl ? `${baseUrl}/search-index.json` : '/search-index.json';
            
            const response = await fetch(searchIndexUrl);
            if (!response.ok) {
                throw new Error(`Failed to fetch search index: ${response.status}`);
            }
            
            const indexData = await response.json();
            this.searchData = indexData.documents;
            
            // Create FlexSearch Document index
            this.searchIndex = new this.FlexSearch.Document({
                tokenize: "forward",
                encoder: this.FlexSearch.Charset.LatinAdvanced,
                cache: 100,
                document: {
                    id: 'id',
                    store: ["title", "description", "content", "headingsText", "url"],
                    index: ["title", "description", "content", "headingsText"]
                }
            });
            
            // Index all documents
            this.searchData.forEach((doc, index) => {
                const url = baseUrl ? `${baseUrl}/{doc.url}` : doc.url;
                
                const docToIndex = {
                    id: index.toString(), // FlexSearch Document API needs string IDs
                    title: doc.title || '',
                    description: doc.description || '',
                    content: doc.content || '',
                    headingsText: this.parseHeadingsText(doc.headings || []),
                    url: url
                };
                
                this.searchIndex.add(docToIndex);
            });
            
        } catch (error) {
            console.error('Failed to load search index:', error);
            this.searchIndexFailed = true; // Mark as failed to prevent retries
            this.searchResults.innerHTML = '<div class="search-modal-error">Search is currently unavailable</div>';
        }
    }

    performSearch(query) {
        if (!query.trim()) {
            this.searchResults.innerHTML = '<div class="search-modal-placeholder">Start typing to search...</div>';
            return;
        }

        if (!this.searchIndex) {
            // Check if search index loading previously failed
            if (this.searchIndexFailed) {
                this.searchResults.innerHTML = '<div class="search-modal-error">Search is currently unavailable</div>';
                return;
            }
            
            // If search index is not loaded yet, try to load it (only once)
            if (!this.searchData) {
                this.searchResults.innerHTML = '<div class="search-modal-loading">Loading search index...</div>';
                this.loadSearchIndex().then(() => {
                    // Retry search after loading only if not failed
                    if (this.modalInput.value === query && !this.searchIndexFailed) {
                        this.performSearch(query);
                    }
                }).catch(() => {
                    // Error handling is done in loadSearchIndex method
                });
            } else {
                this.searchResults.innerHTML = '<div class="search-modal-loading">Search index loading...</div>';
            }
            return;
        }

        try {
            const results = this.searchIndex.search(query, { 
                limit: 10,
                suggest: true,
            });
            this.displayResults(results, query);
        } catch (error) {
            console.error('Search error:', error);
            this.searchResults.innerHTML = '<div class="search-modal-error">Search error occurred</div>';
        }
    }

    parseHeadingsText(headings) {
        // Convert "level:text" format headings to weighted text
        // Higher priority for lower heading levels (H1 > H2 > H3, etc.)
        return headings.map(heading => {
            if (typeof heading === 'string' && heading.includes(':')) {
                const [level, text] = heading.split(':', 2);
                const priority = this.getHeadingPriority(parseInt(level));
                // Repeat text based on priority for better matching
                const repetitions = Math.ceil(priority / 20);
                return Array(repetitions).fill(text).join(' ');
            }
            return heading; // Fallback for legacy format
        }).join(' ');
    }

    getHeadingPriority(level) {
        switch (level) {
            case 1: return 100; // H1 - highest priority
            case 2: return 80;  // H2 - high priority
            case 3: return 60;  // H3 - medium-high priority
            case 4: return 40;  // H4 - medium priority
            case 5: return 20;  // H5 - low priority
            case 6: return 10;  // H6 - lowest priority
            default: return 0;
        }
    }

    combineFieldResults(results) {
        const scoreMap = new Map();
        
        // Field weights
        const fieldWeights = {
            'title': 3,
            'description': 2,
            'headingsText': 1.5,
            'content': 1
        };
        
        results.forEach(fieldResult => {
            const field = fieldResult.field;
            const weight = fieldWeights[field] || 1;
            const docIds = fieldResult.result;
            
            docIds.forEach((docId, index) => {
                // Get the document to access its search priority
                const doc = this.searchData[parseInt(docId)];
                const searchPriority = doc?.searchPriority || 1;
                
                // Give higher score to documents that appear earlier in results
                const positionScore = 1 / (index + 1);
                
                // Apply search priority multiplier
                const totalScore = weight * positionScore * searchPriority;
                
                scoreMap.set(docId, (scoreMap.get(docId) || 0) + totalScore);
            });
        });
        
        // Convert to array and sort by score
        return Array.from(scoreMap.entries())
            .sort((a, b) => b[1] - a[1])
            .map(([docId, score]) => ({ docId, score }));
    }

    displayResults(results, query) {
        if (results.length === 0) {
            this.searchResults.innerHTML = '<div class="search-modal-no-results">No results found</div>';
            return;
        }

        // FlexSearch Document API returns array of field results
        // Combine and score the results from different fields
        const scoredResults = this.combineFieldResults(results);
        
        if (scoredResults.length === 0) {
            this.searchResults.innerHTML = '<div class="search-modal-no-results">No results found</div>';
            return;
        }

        const resultElements = scoredResults.slice(0, 10).map(({ docId, score }) => {
            const doc = this.searchData[parseInt(docId)];
            if (!doc) return '';

            // Use simple highlighting for now
            const highlightedTitle = this.highlightText(doc.title, query);
            const highlightedDescription = this.highlightText(doc.description || '', query);
            
            // Get content snippet with simple highlighting
            const snippet = this.getContentSnippet(doc.content, query);

            let baseUrl = document.body.getAttribute('data-base-url') || '';
            if (baseUrl.endsWith('/')) {
                baseUrl = baseUrl.slice(0, -1);
            }
            
            const url = baseUrl ? `${baseUrl}${doc.url}` : doc.url;
            
            return `
                <div class="search-result-item">
                    <a href="${url}" class="search-result-link">
                        <div class="search-result-header">
                            <h3 class="search-result-title">${highlightedTitle}</h3>
                        </div>
                        ${snippet ? `<p class="search-result-snippet">${snippet}</p>` : ''}
                    </a>
                </div>
            `;
        }).join('');

        this.searchResults.innerHTML = resultElements;
        
        // Add click handlers to close modal when navigating
        this.searchResults.querySelectorAll('a').forEach(link => {
            link.addEventListener('click', () => {
                this.closeModal();
            });
        });
    }

    highlightText(text, query) {
        if (!text || !query) return text;
        
        const words = query.toLowerCase().split(/\s+/);
        let highlightedText = text;
        
        words.forEach(word => {
            if (word.length > 2) {
                const regex = new RegExp(`(${word})`, 'gi');
                highlightedText = highlightedText.replace(regex, '<mark class="search-highlight">$1</mark>');
            }
        });
        
        return highlightedText;
    }


    getContentSnippet(content, query) {
        if (!content || !query) return '';
        
        const words = query.toLowerCase().split(/\s+/);
        const contentLower = content.toLowerCase();
        
        // Find first occurrence of any search term
        let firstIndex = -1;
        for (const word of words) {
            if (word.length > 2) {
                const index = contentLower.indexOf(word);
                if (index !== -1 && (firstIndex === -1 || index < firstIndex)) {
                    firstIndex = index;
                }
            }
        }
        
        if (firstIndex === -1) {
            return content.substring(0, 150) + (content.length > 150 ? '...' : '');
        }
        
        // Get snippet around the found term
        const start = Math.max(0, firstIndex - 75);
        const end = Math.min(content.length, firstIndex + 75);
        let snippet = content.substring(start, end);
        
        if (start > 0) snippet = '...' + snippet;
        if (end < content.length) snippet = snippet + '...';
        
        return this.highlightText(snippet, query);
    }
}

/**
 * Syntax Highlighter - Handles code syntax highlighting with highlight.js
 */
class SyntaxHighlighter {
    constructor() {
        this.prefix = 'language-';
        this.hljs = null;
    }

    async init() {
        const codeNodes = this.getRelevantCodeNodes();
        if (codeNodes.length === 0) return;

        try {
            await this.setupHighlightJs();
            this.highlightCodeNodes(codeNodes);
        } catch (error) {
            console.error('Failed to initialize syntax highlighting:', error);
        }
    }

    getRelevantCodeNodes() {
        const codeNodes = Array.from(document.body.querySelectorAll('code'));
        return codeNodes.filter(node =>
            Array.from(node.classList).some(cls => cls.startsWith(this.prefix) && cls !== this.prefix + 'mermaid' && cls !== this.prefix + 'text' && cls !== this.prefix)
        );
    }

    async setupHighlightJs() {
        // Load highlight.js from CDN
        this.hljs = await import('https://cdn.jsdelivr.net/npm/highlight.js@11/lib/core.min.js');
        
        // Configure highlight.js
        this.hljs.default.configure({
            ignoreUnescapedHTML: true,
            throwUnescapedHTML: false
        });

        // Load common languages
        const languages = [
            'javascript', 'typescript', 'python', 'java', 'csharp', 'cpp', 'c',
            'css', 'html', 'xml', 'json', 'yaml', 'bash', 'shell', 'sql',
            'php', 'ruby', 'go', 'rust', 'kotlin', 'swift', 'markdown'
        ];

        for (const lang of languages) {
            try {
                const langModule = await import(`https://cdn.jsdelivr.net/npm/highlight.js@11/lib/languages/${lang}.min.js`);
                this.hljs.default.registerLanguage(lang, langModule.default);
            } catch (err) {
                // Language not available, skip silently
            }
        }
    }

    highlightCodeNodes(codeNodes) {
        for (const node of codeNodes) {
            try {
                this.highlightSingleNode(node);
            } catch (error) {
                console.error(`Failed to highlight code node:`, error);
            }
        }
    }

    highlightSingleNode(node) {
        const className = Array.from(node.classList)
            .find(cls => cls.startsWith(this.prefix));

        if (!className) return;

        const language = className.slice(this.prefix.length);
        
        // Map some common language aliases
        const languageMap = {
            'js': 'javascript',
            'ts': 'typescript',
            'cs': 'csharp',
            'py': 'python',
            'sh': 'bash',
            'yml': 'yaml'
        };

        const mappedLanguage = languageMap[language] || language;

        try {
            // Check if language is registered
            if (this.hljs.default.getLanguage(mappedLanguage)) {
                const result = this.hljs.default.highlight(node.textContent, { language: mappedLanguage });
                node.innerHTML = result.value;
                node.classList.add('hljs');
            } else {
                // Use auto-detection as fallback
                const result = this.hljs.default.highlightAuto(node.textContent);
                node.innerHTML = result.value;
                node.classList.add('hljs');
            }
        } catch (error) {
            console.warn(`Failed to highlight ${language}:`, error);
        }
    }
}

// Initialize the page manager
const pageManager = new PageManager();

// Make pageManager globally accessible
window.pageManager = pageManager;

