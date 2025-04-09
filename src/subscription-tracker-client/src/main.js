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
import { telemetryService } from './services/telemetryService'

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

// Global error handler
app.config.errorHandler = (err, vm, info) => {
  console.error('Vue Error:', err);
  // Track the error with telemetry
  telemetryService.trackException(err, {
    component: vm?.$options?.name || 'unknown',
    info: info,
    location: window.location.href
  });
}

// Initialize theme based on user preference
initTheme()

// Initialize telemetry service
telemetryService.initialize()

// Initialize authentication service
authService.initialize().then(() => {
  console.log('Authentication service initialized');
  // Mount application to DOM element after auth initialization
  app.mount('#app') // CSS selector matching index.html mount point

  // Track page view after app is mounted
  router.afterEach((to) => {
    telemetryService.trackPageView(to.name, to.fullPath);
  });
}).catch(error => {
  console.error('Failed to initialize authentication service:', error);
  // Mount the application anyway to allow the user to see the login page
  app.mount('#app')
})
