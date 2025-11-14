// Theme handling for Bootstrap dark mode
(function() {
    'use strict';

    // Function to get cookie value by name
    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    // Function to set cookie
    function setCookie(name, value, days) {
        const expires = new Date();
        expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);
        document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/;SameSite=Lax`;
    }

    // Function to set the theme
    function setTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        setCookie('theme', theme, 365); // Store for 1 year
    }

    // Function to get the preferred theme
    function getPreferredTheme() {
        // First, check if user has a saved preference in cookie
        const savedTheme = getCookie('theme');
        if (savedTheme) {
            return savedTheme;
        }

        // Otherwise, check system preference
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        return 'light';
    }

    // Set initial theme
    setTheme(getPreferredTheme());

    // Listen for changes in system color scheme preference (only if no saved preference)
    if (window.matchMedia && !getCookie('theme')) {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function(e) {
            setTheme(e.matches ? 'dark' : 'light');
        });
    }

    // Expose setTheme function globally for use by theme toggle
    window.setTheme = setTheme;
    window.getPreferredTheme = getPreferredTheme;
})();
