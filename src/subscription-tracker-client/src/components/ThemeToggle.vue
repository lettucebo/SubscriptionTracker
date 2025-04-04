<template>
  <button
    @click="toggleTheme"
    class="theme-toggle-btn"
    :title="themeText"
    aria-label="Toggle dark mode"
  >
    <i :class="themeIcon" aria-hidden="true"></i>
  </button>
</template>

<script>
import { ref, computed, onMounted } from 'vue';
import { getCurrentTheme, toggleTheme } from '@/services/themeService';

export default {
  name: 'ThemeToggle',

  setup() {
    const currentTheme = ref('light');

    // Update current theme state
    const updateThemeState = () => {
      currentTheme.value = getCurrentTheme();
    };

    // Toggle theme and update state
    const handleToggleTheme = () => {
      toggleTheme();
      updateThemeState();

      // Update root component's darkMode property
      if (window.$root) {
        window.$root.darkMode = currentTheme.value === 'dark';
      }
    };

    // Computed properties for icon and text
    const themeIcon = computed(() =>
      currentTheme.value === 'dark' ? 'fas fa-sun' : 'fas fa-moon'
    );

    const themeText = computed(() =>
      currentTheme.value === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'
    );

    // Initialize on mount
    onMounted(() => {
      updateThemeState();

      // Store root component reference for global access
      window.$root = document.getElementById('app').__vue__;
    });

    return {
      currentTheme,
      themeIcon,
      themeText,
      toggleTheme: handleToggleTheme
    };
  }
};
</script>

<style scoped>
.theme-toggle-btn {
  background: transparent;
  border: none;
  color: inherit;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-speed) ease;
}

.theme-toggle-btn:hover {
  background-color: rgba(0, 0, 0, 0.1);
}

.dark-mode .theme-toggle-btn:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.theme-toggle-btn i {
  font-size: 1.2rem;
}
</style>
