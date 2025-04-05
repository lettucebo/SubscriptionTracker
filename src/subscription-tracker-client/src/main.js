/**
 * Vue application entry point and core configuration
 * @file Bootstraps the Vue application with global plugins and styles
 * @module main
 * @see {@link App} for the root component
 * @see {@link router} for navigation configuration
 */

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

// UI Framework imports - Required for core styling and components
import '@fortawesome/fontawesome-free/css/all.min.css' // Icon library (v5.15.4)
import 'bootstrap/dist/css/bootstrap.min.css'          // Bootstrap CSS framework (v5.1.3)
import 'bootstrap/dist/js/bootstrap.bundle.min.js'     // Bootstrap JS with Popper.js

// Application styling
import './assets/custom.css' // Custom CSS variables and overrides

// Services
import { initTheme } from './services/themeService'
import { authService } from './services/authService'

/**
 * Initialize Vue application instance with core configurations
 * @type {import('vue').App}
 * @property {Object} App - Root Vue component
 * @remarks
 * - Configures global plugins
 * - Sets up application-wide providers
 * - Applies global mixins if needed
 */
const app = createApp(App)

// Register global plugins and utilities
app.use(router) // Vue Router for SPA navigation (v4.0.12)

// Initialize theme based on user preference
initTheme()

// Initialize authentication service
authService.initialize().then(() => {
  // Mount application to DOM element after auth initialization
  app.mount('#app') // CSS selector matching index.html mount point
})
