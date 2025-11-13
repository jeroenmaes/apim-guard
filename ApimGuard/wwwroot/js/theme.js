// Theme handling for Bootstrap dark mode
(function() {
    'use strict';

    // Function to set the theme
    function setTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
    }

    // Function to get the preferred theme from system settings
    function getPreferredTheme() {
        // Check if user prefers dark mode
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        return 'light';
    }

    // Set initial theme
    setTheme(getPreferredTheme());

    // Listen for changes in system color scheme preference
    if (window.matchMedia) {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function(e) {
            setTheme(e.matches ? 'dark' : 'light');
        });
    }
})();
