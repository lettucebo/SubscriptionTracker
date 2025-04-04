/**
 * Theme service for managing application theme
 * @module themeService
 */

// Theme constants
const THEME_STORAGE_KEY = 'subscription-tracker-theme';
const DARK_MODE_CLASS = 'dark-mode';

/**
 * Get the current theme from localStorage or system preference
 * @returns {string} 'dark' or 'light'
 */
export const getCurrentTheme = () => {
  // Check if theme is stored in localStorage
  const storedTheme = localStorage.getItem(THEME_STORAGE_KEY);
  
  if (storedTheme) {
    return storedTheme;
  }
  
  // If no stored preference, check system preference
  if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
    return 'dark';
  }
  
  // Default to light theme
  return 'light';
};

/**
 * Set the theme and save to localStorage
 * @param {string} theme - 'dark' or 'light'
 */
export const setTheme = (theme) => {
  // Save to localStorage
  localStorage.setItem(THEME_STORAGE_KEY, theme);
  
  // Apply theme to document
  if (theme === 'dark') {
    document.body.classList.add(DARK_MODE_CLASS);
  } else {
    document.body.classList.remove(DARK_MODE_CLASS);
  }
};

/**
 * Toggle between light and dark themes
 */
export const toggleTheme = () => {
  const currentTheme = getCurrentTheme();
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
  setTheme(newTheme);
  return newTheme;
};

/**
 * Initialize theme based on stored preference or system preference
 */
export const initTheme = () => {
  const theme = getCurrentTheme();
  setTheme(theme);
  
  // Listen for system theme changes
  if (window.matchMedia) {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      // Only change theme if user hasn't set a preference
      if (!localStorage.getItem(THEME_STORAGE_KEY)) {
        setTheme(e.matches ? 'dark' : 'light');
      }
    });
  }
};
