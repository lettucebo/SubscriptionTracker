<template>
  <div class="subscriptions">
    <div class="header-container">
      <div class="d-flex flex-column">
        <h1>Subscriptions</h1>
        <p class="text-muted" v-if="subscriptions.length > 0">
          Total Monthly: ${{ totalMonthlyAmount.toFixed(2) }}
        </p>
      </div>
      <router-link to="/subscription-form" class="btn btn-primary">
        <i class="fas fa-circle-plus me-2"></i>New Subscription
      </router-link>
    </div>

    <div class="mb-3">
      <input
        type="text"
        v-model="searchQuery"
        class="form-control"
        placeholder="Search subscriptions..."
        @input="filterSubscriptions"
      >
    </div>

    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="error" class="alert alert-danger" role="alert">
      {{ error }}
    </div>

    <div v-else-if="filteredSubscriptions.length === 0" class="text-center my-5">
      <p class="text-muted">No subscriptions found</p>
    </div>

    <div v-else class="table-responsive">
      <table class="table table-hover border subscription-table">
      <thead class="table-light subscription-table-header" :class="{ 'dark-header': $root.darkMode }">
        <tr>
          <th @click="sort('name')" class="sortable" :class="{ 'dark-th': $root.darkMode }">
            Name <i :class="getSortIconClass('name')"></i>
          </th>
          <th @click="sort('billingCycle')" class="sortable" :class="{ 'dark-th': $root.darkMode }">
            Billing Cycle <i :class="getSortIconClass('billingCycle')"></i>
          </th>
          <th @click="sort('amount')" class="sortable" :class="{ 'dark-th': $root.darkMode }">
            Amount <i :class="getSortIconClass('amount')"></i>
          </th>
          <th @click="sort('effectiveMonthlyPrice')" class="sortable" :class="{ 'dark-th': $root.darkMode }">
            Monthly Cost <i :class="getSortIconClass('effectiveMonthlyPrice')"></i>
          </th>
          <th @click="sort('startDate')" class="sortable" :class="{ 'dark-th': $root.darkMode }">
            Date Range <i :class="getSortIconClass('startDate')"></i>
          </th>
          <th @click="sort('category.name')" class="sortable" :class="{ 'dark-th': $root.darkMode }">
            Category <i :class="getSortIconClass('category.name')"></i>
          </th>
          <th @click="sort('isShared')" class="sortable" :class="{ 'dark-th': $root.darkMode }">
            Sharing <i :class="getSortIconClass('isShared')"></i>
          </th>
          <th :class="{ 'dark-th': $root.darkMode }">Status</th>
          <th :class="{ 'dark-th': $root.darkMode }">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="sub in filteredSubscriptions" :key="sub.id" :class="{ 'dark-row': $root.darkMode }">
          <td :class="{ 'dark-cell': $root.darkMode }">
            <div class="d-flex align-items-center">
              <span class="subscription-name">{{ sub.name }}</span>
            </div>
          </td>
          <td class="text-capitalize" :class="{ 'dark-cell': $root.darkMode }">
            <span class="badge" :style="getStandardBadgeStyle('secondary')">{{ sub.billingCycle }}</span>
          </td>
          <td :class="{ 'dark-cell': $root.darkMode }">
            <span class="badge" :style="getStandardBadgeStyle('secondary')">{{ sub.billingCycle }}</span>
            ${{ formatCurrency(sub.amount) }}
          </td>
          <td :class="{ 'dark-cell': $root.darkMode }">${{ formatCurrency(sub.effectiveMonthlyPrice) }}</td>
          <td :class="{ 'dark-cell': $root.darkMode }">
            <div class="small">
              {{ formatDate(sub.startDate) }}
              <br>
              <span :class="{ 'text-danger': isExpiringSoon(sub) }">
                {{ sub.endDate ? formatDate(sub.endDate) : 'Active' }}
              </span>
            </div>
          </td>
          <td :class="{ 'dark-cell': $root.darkMode }">
            <span class="badge" :style="getCategoryBadgeStyle(sub.category?.colorCode)">
              {{ sub.category?.name }}
            </span>
          </td>
          <td :class="{ 'dark-cell': $root.darkMode }">
            <span v-if="sub.isShared" class="badge" :style="getStandardBadgeStyle('success')" :title="sub.contactInfo">
              <i class="fas fa-users me-1"></i> Shared
            </span>
            <span v-else class="badge" :style="getStandardBadgeStyle('secondary')">
              <i class="fas fa-user me-1"></i> Personal
            </span>
          </td>
          <td :class="{ 'dark-cell': $root.darkMode }">
            <span
              class="badge"
              :style="getStandardBadgeStyle(getStatusClass(sub).replace('bg-', ''))"
              :title="getStatusTitle(sub)"
            >
              {{ getStatusText(sub) }}
            </span>
          </td>
          <td :class="{ 'dark-cell': $root.darkMode }">
            <div class="btn-group">
              <router-link
                :to="`/subscription-form/${sub.id}`"
                class="btn btn-sm btn-outline-primary"
                title="Edit"
              >
                <i class="fas fa-pen-to-square"></i>
              </router-link>
              <button
                @click="deleteSubscription(sub.id)"
                class="btn btn-sm btn-outline-danger"
                title="Delete"
              >
                <i class="fas fa-trash-can"></i>
              </button>
            </div>
          </td>
        </tr>
      </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import { apiService } from '@/services/apiService'

export default {
  name: "SubscriptionsPage",
  /**
   * Component data properties
   * @returns {Object} Initial state
   * @vue-data {Array} subscriptions - List of all subscriptions
   * @vue-data {Array} filteredSubscriptions - Filtered list of subscriptions
   * @vue-data {boolean} loading - Data loading state
   * @vue-data {string|null} error - Error message
   * @vue-data {string} searchQuery - Search filter query
   * @vue-data {string} sortKey - Current sort key
   * @vue-data {string} sortOrder - Current sort order (asc/desc)
   */
  data() {
    return {
      subscriptions: [],
      filteredSubscriptions: [],
      loading: true,
      error: null,
      searchQuery: '',
      sortKey: 'name',
      sortOrder: 'asc'
    }
  },
  computed: {
    /**
     * Calculate total monthly cost of all subscriptions
     * @returns {number} Total monthly amount
     */
    totalMonthlyAmount() {
      return this.subscriptions.reduce((total, sub) => {
        const monthlyAmount = sub.effectiveMonthlyPrice || 0;
        return total + Math.round(monthlyAmount * 100) / 100;
      }, 0);
    }
  },
  /**
   * Lifecycle hook - runs when component is created
   */
  created() {
    this.fetchSubscriptions()
  },
  methods: {
    /**
     * Fetch subscriptions from API
     * @async
     * @returns {Promise<void>}
     */
    async fetchSubscriptions() {
      this.loading = true
      this.error = null
      try {
        const response = await apiService.getSubscriptions()
        this.subscriptions = response.data
        this.filterSubscriptions()
      } catch (error) {
        this.error = "Failed to load subscriptions. Please try again later."
        console.error("Error fetching subscriptions:", error)
      } finally {
        this.loading = false
      }
    },
    /**
     * Delete a subscription by ID
     * @async
     * @param {number} id - Subscription ID to delete
     * @returns {Promise<void>}
     */
    async deleteSubscription(id) {
      if (confirm('Are you sure you want to delete this subscription?')) {
        try {
          await apiService.deleteSubscription(id)
          this.fetchSubscriptions()
        } catch (error) {
          this.error = "Failed to delete subscription. Please try again."
          console.error("Error deleting subscription:", error)
        }
      }
    },
    /**
     * Filter subscriptions based on search query
     * and apply current sort
     */
    filterSubscriptions() {
      const query = this.searchQuery.toLowerCase()
      this.filteredSubscriptions = this.subscriptions.filter(sub =>
        sub.name.toLowerCase().includes(query) ||
        (sub.category?.name || '').toLowerCase().includes(query) ||
        sub.billingCycle.toLowerCase().includes(query)
      )
      this.sort(this.sortKey)
    },
    /**
     * Sort subscriptions by specified key
     * @param {string} key - Column key to sort by
     */
    sort(key) {
      this.sortOrder = this.sortKey === key ?
        this.sortOrder === 'asc' ? 'desc' : 'asc' : 'asc'
      this.sortKey = key

      this.filteredSubscriptions.sort((a, b) => {
        let comparison = 0
        const aVal = this.getSortValue(a, key)
        const bVal = this.getSortValue(b, key)

        if (aVal > bVal) comparison = 1
        if (aVal < bVal) comparison = -1
        return this.sortOrder === 'desc' ? comparison * -1 : comparison
      })
    },
    /**
     * Get value for sorting from subscription object
     * @param {Object} sub - Subscription object
     * @param {string} key - Sort key
     * @returns {any} Value to sort by
     */
    getSortValue(sub, key) {
      if (key === 'startDate' || key === 'endDate') {
        return new Date(sub[key] || 0).getTime()
      }
      if (key === 'category.name') {
        return sub.category?.name || ''
      }
      return sub[key] || ''
    },
    /**
     * Get icon class for sort indicator
     * @param {string} key - Column key
     * @returns {string} Bootstrap icon class
     */
    getSortIconClass(key) {
      if (this.sortKey !== key) return 'bi bi-arrow-down-up'
      return this.sortOrder === 'asc' ? 'bi bi-sort-up' : 'bi bi-sort-down'
    },
    /**
     * Format date to localized string
     * @param {string} date - ISO date string
     * @returns {string} Formatted date
     */
    formatDate(date) {
      return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      })
    },
    /**
     * Format currency value
     * @param {number} amount - Currency amount
     * @returns {string} Formatted currency string
     */
    formatCurrency(amount) {
      if (amount === null || amount === undefined) return '0.00';
      return (Math.round(amount * 100) / 100).toFixed(2);
    },
    /**
     * Check if subscription is expiring within 7 days
     * @param {Object} sub - Subscription object
     * @returns {boolean} True if expiring soon
     */
    isExpiringSoon(sub) {
      if (!sub.endDate) return false
      const daysUntilExpiry = Math.ceil(
        (new Date(sub.endDate) - new Date()) / (1000 * 60 * 60 * 24)
      )
      return daysUntilExpiry <= 7 && daysUntilExpiry > 0
    },
    /**
     * Get CSS class for status badge
     * @param {Object} sub - Subscription object
     * @returns {string} Bootstrap badge class
     */
    getStatusClass(sub) {
      if (!sub.endDate) return 'bg-success'
      return this.isExpiringSoon(sub) ? 'bg-warning' : 'bg-info'
    },

    /**
     * Get badge style based on category color
     * @param {string} colorCode - Hex color code
     * @returns {Object} Style object with background and text color
     */
    getCategoryBadgeStyle(colorCode) {
      const defaultColor = '#17a2b8';
      const color = colorCode || defaultColor;

      // Calculate if the color is light or dark
      const isLight = this.isLightColor(color);

      return {
        backgroundColor: color,
        color: isLight ? '#000000' : '#ffffff',
        textShadow: isLight ? '0 0 1px rgba(0,0,0,0.4)' : '0 0 3px rgba(0,0,0,0.8), 0 0 1px rgba(0,0,0,1)',
        border: this.$root.darkMode && isLight ? '1px solid rgba(0,0,0,0.2)' : 'none'
      };
    },

    /**
     * Determine if a color is light or dark
     * @param {string} colorHex - Hex color code
     * @returns {boolean} True if color is light
     */
    isLightColor(colorHex) {
      // Default to a dark color if none provided
      if (!colorHex) return false;

      // Remove the # if it exists
      const hex = colorHex.replace('#', '');

      // Convert hex to RGB
      const r = parseInt(hex.substring(0, 2), 16);
      const g = parseInt(hex.substring(2, 2), 16);
      const b = parseInt(hex.substring(4, 2), 16);

      // Calculate perceived brightness using the formula
      // (299*R + 587*G + 114*B) / 1000
      const brightness = (r * 299 + g * 587 + b * 114) / 1000;

      // Return true if the color is light (brightness > 155)
      return brightness > 155;
    },

    /**
     * Get badge style for standard Bootstrap colors
     * @param {string} colorName - Bootstrap color name (primary, secondary, success, etc.)
     * @returns {Object} Style object with background and text color
     */
    getStandardBadgeStyle(colorName) {
      // Map of Bootstrap color names to hex values
      const colorMap = {
        primary: '#0d6efd',
        secondary: '#6c757d',
        success: '#198754',
        danger: '#dc3545',
        warning: '#ffc107',
        info: '#0dcaf0',
        light: '#f8f9fa',
        dark: '#212529'
      };

      const colorHex = colorMap[colorName] || colorMap.secondary;
      const isLight = this.isLightColor(colorHex);

      return {
        backgroundColor: colorHex,
        color: isLight ? '#000000' : '#ffffff',
        textShadow: isLight ? '0 0 1px rgba(0,0,0,0.4)' : '0 0 3px rgba(0,0,0,0.8), 0 0 1px rgba(0,0,0,1)',
        border: this.$root.darkMode && isLight ? '1px solid rgba(0,0,0,0.2)' : 'none'
      };
    },
    /**
     * Get status display text
     * @param {Object} sub - Subscription object
     * @returns {string} Status text
     */
    getStatusText(sub) {
      if (!sub.endDate) return 'Active'
      const daysLeft = Math.ceil(
        (new Date(sub.endDate) - new Date()) / (1000 * 60 * 60 * 24)
      )
      return daysLeft > 0 ? `${daysLeft} days left` : 'Expired'
    },
    /**
     * Get status tooltip text
     * @param {Object} sub - Subscription object
     * @returns {string} Tooltip text
     */
    getStatusTitle(sub) {
      if (!sub.endDate) return 'Ongoing subscription'
      return this.isExpiringSoon(sub) ? 'Subscription ending soon' : 'Fixed term subscription'
    }
  }
}
</script>

<style>
.subscriptions {
  margin-top: 2rem;
  padding: 0 1rem;
}
.header-container {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.5rem;
}
.sortable {
  cursor: pointer;
  user-select: none;
  transition: background-color 0.3s ease;
}
.sortable:hover {
  background-color: rgba(0, 0, 0, 0.05);
}
.dark-mode .sortable:hover {
  background-color: rgba(255, 255, 255, 0.05);
}
.subscription-name {
  font-weight: 500;
}
.table-responsive {
  border-radius: 0.25rem;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
  transition: box-shadow 0.3s ease;
}
.dark-mode .table-responsive {
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.3);
}
.table {
  margin-bottom: 0;
}
th {
  white-space: nowrap;
}
.btn-group {
  display: flex;
  gap: 0.25rem;
}

/* Dark mode specific styles */
.dark-mode .table-light,
.dark-mode .subscription-table-header {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
}

/* Direct style overrides for dark mode */
.dark-mode .subscription-table {
  color: #e0e0e0 !important;
  background-color: var(--bs-dark-surface) !important;
}

.dark-header {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-row {
  background-color: var(--bs-dark-surface) !important;
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-row:hover {
  background-color: rgba(255, 255, 255, 0.05) !important;
}

.dark-cell {
  color: #e0e0e0 !important;
  border-color: #444 !important;
  background-color: transparent !important;
}

.dark-th {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-mode .subscription-table th {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-mode .subscription-table td {
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-mode .subscription-table tr {
  background-color: var(--bs-dark-surface) !important;
}

.dark-mode .subscription-table tr:hover {
  background-color: rgba(255, 255, 255, 0.05) !important;
}

:deep(.dark-mode .table-hover tbody tr:hover) {
  background-color: rgba(255, 255, 255, 0.05) !important;
  color: #e0e0e0 !important;
}

:deep(.dark-mode .badge.bg-secondary) {
  background-color: #444 !important;
  color: #e0e0e0 !important;
}

:deep(.dark-mode .text-muted) {
  color: #adb5bd !important;
}

:deep(.dark-mode .subscription-table) {
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

:deep(.dark-mode .subscription-table th) {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

:deep(.dark-mode .subscription-table td) {
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

:deep(.dark-mode .subscription-table tr) {
  background-color: var(--bs-dark-surface) !important;
  border-color: #444 !important;
}

:deep(.dark-mode .subscription-table tr:hover) {
  background-color: rgba(255, 255, 255, 0.05) !important;
}
/* Category badge styling */
.badge {
  font-weight: 600;
  padding: 5px 8px;
  letter-spacing: 0.3px;
}

/* Fix form control styling in dark mode */
.dark-mode .form-control {
  background-color: #2d2d2d !important;
  border-color: #444 !important;
  color: #e0e0e0 !important;
}

.dark-mode .form-control:focus {
  background-color: #333 !important;
  border-color: rgb(var(--bs-primary-rgb)) !important;
}

/* Fix placeholder text color in dark mode */
.dark-mode .form-control::placeholder {
  color: #aaa;
  opacity: 0.7;
}

.dark-mode .form-control::-webkit-input-placeholder {
  color: #aaa;
  opacity: 0.7;
}

.dark-mode .form-control::-moz-placeholder {
  color: #aaa;
  opacity: 0.7;
}

@media (max-width: 768px) {
  .table-responsive {
    font-size: 0.875rem;
  }
  .header-container {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  .header-container .btn {
    align-self: flex-start;
  }
}
</style>
