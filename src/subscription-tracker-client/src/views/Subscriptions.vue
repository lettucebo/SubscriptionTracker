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
      <table class="table table-hover border">
      <thead class="table-light">
        <tr>
          <th @click="sort('name')" class="sortable">
            Name <i :class="getSortIconClass('name')"></i>
          </th>
          <th @click="sort('billingCycle')" class="sortable">
            Billing Cycle <i :class="getSortIconClass('billingCycle')"></i>
          </th>
          <th @click="sort('amount')" class="sortable">
            Amount <i :class="getSortIconClass('amount')"></i>
          </th>
          <th @click="sort('effectiveMonthlyPrice')" class="sortable">
            Monthly Cost <i :class="getSortIconClass('effectiveMonthlyPrice')"></i>
          </th>
          <th @click="sort('startDate')" class="sortable">
            Date Range <i :class="getSortIconClass('startDate')"></i>
          </th>
          <th @click="sort('category.name')" class="sortable">
            Category <i :class="getSortIconClass('category.name')"></i>
          </th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="sub in filteredSubscriptions" :key="sub.id">
          <td>
            <div class="d-flex align-items-center">
              <span class="subscription-name">{{ sub.name }}</span>
            </div>
          </td>
          <td class="text-capitalize">
            <span class="badge bg-secondary">{{ sub.billingCycle }}</span>
          </td>
          <td>
            <span class="badge bg-secondary">{{ sub.billingCycle }}</span>
            ${{ formatCurrency(sub.amount) }}
          </td>
          <td>${{ formatCurrency(sub.effectiveMonthlyPrice) }}</td>
          <td>
            <div class="small">
              {{ formatDate(sub.startDate) }}
              <br>
              <span :class="{ 'text-danger': isExpiringSoon(sub) }">
                {{ sub.endDate ? formatDate(sub.endDate) : 'Active' }}
              </span>
            </div>
          </td>
          <td>
            <span class="badge bg-info">
              {{ sub.category?.name }}
            </span>
          </td>
          <td>
            <span 
              class="badge" 
              :class="getStatusClass(sub)"
              :title="getStatusTitle(sub)"
            >
              {{ getStatusText(sub) }}
            </span>
          </td>
          <td>
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
import axios from 'axios'
import { config } from '@/config'

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
        const response = await axios.get(`${config.baseUrl}/api/subscription`)
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
          await axios.delete(`${config.baseUrl}/api/subscription/${id}`)
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

<style scoped>
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
}
.sortable:hover {
  background-color: rgba(0, 0, 0, 0.05);
}
.subscription-name {
  font-weight: 500;
}
.table-responsive {
  border-radius: 0.25rem;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
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
