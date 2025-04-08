<template>
  <!-- Main application container -->
  <div id="app">
    <!-- Primary navigation bar with responsive toggle -->
    <nav class="navbar navbar-expand-lg">
      <div class="container">
        <!-- Brand logo and name -->
        <router-link class="navbar-brand" to="/">Subscription Tracker</router-link>

        <!-- Mobile menu toggle button -->
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto">
            <!-- Navigation items -->
            <li class="nav-item">
              <router-link to="/" class="nav-link">Home</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/subscriptions" class="nav-link">Subscriptions</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/categories" class="nav-link">Categories</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/calendar" class="nav-link">Calendar</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/report" class="nav-link">Report</router-link>
            </li>
          </ul>

          <!-- Theme toggle and login buttons -->
          <div class="d-flex align-items-center">
            <ThemeToggle class="me-3" />
            <LoginButton />
          </div>
        </div>
      </div>
    </nav>
    <div class="container mt-3">
      <router-view />
    </div>
  </div>
</template>

<script>
import ThemeToggle from '@/components/ThemeToggle.vue';
import LoginButton from '@/components/LoginButton.vue';
import { initTheme } from '@/services/themeService';

/**
 * Root application component containing global layout and navigation
 * @component
 * @vue-data {boolean} isMobileMenuOpen - Tracks mobile menu visibility state
 * @vue-data {Object} currentRoute - Current active route object
 * @vue-computed {Array} navRoutes - Filtered list of navigation routes
 * @vue-event {void} toggleMobileMenu - Toggles mobile menu visibility
 */
export default {
  name: 'App',

  components: {
    ThemeToggle,
    LoginButton
  },

  /**
   * Component reactive data properties
   * @returns {Object} Initial state values
   */
  data() {
    return {
      /** @type {boolean} Tracks mobile navigation menu visibility state */
      isMobileMenuOpen: false,

      /** @type {Object} Stores current active route information */
      currentRoute: this.$route,

      /** @type {boolean} Tracks dark mode state */
      darkMode: document.body.classList.contains('dark-mode')
    }
  },

  /**
   * Computed properties
   */
  computed: {
    /**
     * Filters navigation routes to exclude edit subscription route
     * @returns {Array<Object>} Filtered list of route objects
     */
    navRoutes() {
      return this.$router.options.routes.filter(r => r.name !== 'EditSubscription')
    }
  },

  /**
   * Lifecycle hooks
   */
  mounted() {
    /** Initialize theme based on user preference */
    initTheme();

    /** Watch for route changes to update current route and close mobile menu */
    this.$watch(
      () => this.$route,
      (to) => {
        this.currentRoute = to
        this.isMobileMenuOpen = false
      }
    );

    /** Watch for dark mode changes */
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.attributeName === 'class') {
          this.darkMode = document.body.classList.contains('dark-mode');
        }
      });
    });

    observer.observe(document.body, { attributes: true });
  },

  /**
   * Component methods
   */
  methods: {
    /**
     * Toggles mobile navigation menu visibility
     */
    toggleMobileMenu() {
      this.isMobileMenuOpen = !this.isMobileMenuOpen
    }
  }
}
</script>

<style>
/* Bootstrap framework CSS */
@import url("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css");

/* Font Awesome icons */
@import url("https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css");

/* Custom dark mode overrides */
@import "./assets/dark-mode.css";

/* Base body styles */
body {
  margin: 0;
  padding: 0;
  font-family: "Helvetica Neue", Helvetica, Arial, sans-serif;
  transition: background-color 0.3s ease, color 0.3s ease;
}

/* Navbar styling with theme support */
.navbar {
  transition: all 0.3s ease;
  background-color: #f8f9fa;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.dark-mode .navbar {
  background-color: #1e1e1e;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.navbar-brand, .nav-link {
  color: #212529;
  transition: color 0.3s ease;
}

.dark-mode .navbar-brand, .dark-mode .nav-link {
  color: #e0e0e0;
}

.nav-link:hover {
  color: rgb(var(--bs-primary-rgb));
}

.dark-mode .nav-link:hover {
  color: rgb(var(--bs-primary-rgb));
}

/* Active link styles for dark mode */
.dark-mode .router-link-active,
.dark-mode .router-link-exact-active {
  color: rgb(var(--bs-primary-rgb)) !important;
}

/* Dark mode styles for the entire app */
.dark-mode {
  background-color: var(--bs-dark-bg) !important;
  color: var(--bs-dark-text) !important;
}

/* Force override Bootstrap's table-light class */
.dark-mode .table-light,
.dark-mode .table-light > th,
.dark-mode .table-light > td {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
}

.dark-mode .card,
.dark-mode .stat-card,
.dark-mode .subscription-details {
  background-color: var(--bs-dark-surface) !important;
  border-color: #2d2d2d !important;
  color: var(--bs-dark-text) !important;
}

.dark-mode .card-header {
  background-color: rgba(0, 0, 0, 0.2) !important;
  border-color: #2d2d2d !important;
}

.dark-mode .hero-section {
  background-color: var(--bs-dark-surface) !important;
  color: var(--bs-dark-text) !important;
}

.dark-mode .form-control {
  background-color: #2d2d2d !important;
  border-color: #444 !important;
  color: #e0e0e0 !important;
}

.dark-mode .form-control:focus {
  background-color: #333 !important;
}

.dark-mode .table {
  color: #e0e0e0;
}

.dark-mode .table-striped > tbody > tr:nth-of-type(odd) > * {
  background-color: rgba(255, 255, 255, 0.05);
  color: #e0e0e0;
}

.dark-mode table {
  color: #e0e0e0 !important;
}

.dark-mode table th {
  color: #e0e0e0 !important;
  border-color: #444 !important;
  background-color: #2d2d2d !important;
}

.dark-mode table td {
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-mode table tr {
  background-color: var(--bs-dark-surface) !important;
}

.dark-mode table tr:hover {
  background-color: rgba(255, 255, 255, 0.05) !important;
}

.dark-mode .badge {
  border: none !important;
}

.dark-mode .badge.bg-warning,
.dark-mode .badge.bg-info,
.dark-mode .badge.bg-success {
  color: #000;
}

.dark-mode .modal-content {
  background-color: var(--bs-dark-surface);
  color: var(--bs-dark-text);
}

.dark-mode .text-muted {
  color: #adb5bd !important;
}

.dark-mode .border {
  border-color: #2d2d2d !important;
}

.dark-mode .table-light {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
}

.dark-mode .dropdown-menu {
  background-color: #2d2d2d;
  border-color: #444;
}

.dark-mode .dropdown-item {
  color: #e0e0e0;
}

.dark-mode .dropdown-item:hover {
  background-color: #444;
}

.dark-mode .list-group-item {
  background-color: var(--bs-dark-surface);
  border-color: #444;
  color: var(--bs-dark-text);
}

/* Navbar toggler icon for dark mode */
.dark-mode .navbar-toggler-icon {
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='rgba%28255, 255, 255, 0.75%29' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e");
}

.dark-mode .navbar-toggler {
  border-color: rgba(255, 255, 255, 0.1);
}
</style>
