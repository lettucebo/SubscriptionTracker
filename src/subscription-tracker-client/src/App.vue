<template>
  <!-- Main application container -->
  <div id="app">
    <!-- Primary navigation bar with responsive toggle -->
    <nav class="navbar navbar-expand-lg navbar-light bg-light">
      <div class="container">
        <!-- Brand logo and name -->
        <a class="navbar-brand" href="#">Subscription Tracker</a>
        
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
          </ul>
        </div>
      </div>
    </nav>
    <div class="container mt-3">
      <router-view />
    </div>
  </div>
</template>

<script>
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
  
  /**
   * Component reactive data properties
   * @returns {Object} Initial state values
   */
  data() {
    return {
      /** @type {boolean} Tracks mobile navigation menu visibility state */
      isMobileMenuOpen: false,
      
      /** @type {Object} Stores current active route information */
      currentRoute: this.$route
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
    /** Watch for route changes to update current route and close mobile menu */
    this.$watch(
      () => this.$route,
      (to) => {
        this.currentRoute = to
        this.isMobileMenuOpen = false
      }
    )
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

/* Base body styles */
body {
  margin: 0;
  padding: 0;
  font-family: "Helvetica Neue", Helvetica, Arial, sans-serif;
}
</style>
