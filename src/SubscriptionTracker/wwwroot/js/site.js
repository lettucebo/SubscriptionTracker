// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Dark mode functionality
document.addEventListener('DOMContentLoaded', () => {
    const themeToggleBtn = document.getElementById('theme-toggle');
    const themeIcon = document.getElementById('theme-icon');
    const themeText = document.getElementById('theme-text');
    
    // Check if user has a saved preference
    const savedTheme = localStorage.getItem('theme');
    
    // Apply the saved theme or default to light
    if (savedTheme === 'dark') {
        document.body.classList.add('dark-mode');
        updateToggleButton(true);
    }
    
    // Handle theme toggle button click
    themeToggleBtn.addEventListener('click', () => {
        // Toggle dark mode class on body
        const isDarkMode = document.body.classList.toggle('dark-mode');
        
        // Save user preference
        localStorage.setItem('theme', isDarkMode ? 'dark' : 'light');
        
        // Update button appearance
        updateToggleButton(isDarkMode);
    });
    
    // Function to update the appearance of the toggle button
    function updateToggleButton(isDarkMode) {
        if (isDarkMode) {
            themeIcon.classList.remove('bi-moon-fill');
            themeIcon.classList.add('bi-sun-fill');
            themeText.textContent = '淺色模式';
        } else {
            themeIcon.classList.remove('bi-sun-fill');
            themeIcon.classList.add('bi-moon-fill');
            themeText.textContent = '深色模式';
        }
    }
});
