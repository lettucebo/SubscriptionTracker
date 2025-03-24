<template>
  <div class="report-page">
    <div class="header-section">
      <h1>Subscription Report</h1>
      <p class="text-muted">Track and analyze your subscription expenses</p>
    </div>

    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="error" class="alert alert-danger" role="alert">
      {{ error }}
    </div>

    <div v-else class="report-content">
      <div class="row g-4 mb-4">
        <div class="col-md-4">
          <div class="filter-card">
            <h3 class="filter-title">Filters</h3>
            <div class="mb-3">
              <label for="categoryFilter" class="form-label">Category</label>
              <select id="categoryFilter" v-model="selectedCategory" class="form-select">
                <option value="">All Categories</option>
                <option v-for="category in uniqueCategories" :key="category" :value="category">
                  {{ category }}
                </option>
              </select>
            </div>
            <div class="mb-3">
              <label for="sortBy" class="form-label">Sort By</label>
              <select id="sortBy" v-model="sortOption" class="form-select">
                <option value="name">Name</option>
                <option value="amount">Amount</option>
                <option value="dueDate">Due Date</option>
                <option value="category">Category</option>
              </select>
            </div>
          </div>

          <div class="summary-card mt-4">
            <h3 class="summary-title">Summary</h3>
            <div class="summary-item">
              <span>Total Subscriptions:</span>
              <strong>{{ filteredSubscriptions.length }}</strong>
            </div>
            <div class="summary-item">
              <span>Total Monthly:</span>
              <strong>${{ totalMonthly.toFixed(2) }}</strong>
            </div>
            <div class="summary-item">
              <span>Average Cost:</span>
              <strong>${{ averageCost.toFixed(2) }}</strong>
            </div>
          </div>
        </div>

        <div class="col-md-8">
          <div class="table-card">
            <div class="table-responsive">
              <table class="table table-hover">
                <thead class="table-light">
                  <tr>
                    <th>Name</th>
                    <th>Amount</th>
                    <th>Next Payment</th>
                    <th>Category</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="sub in sortedSubscriptions" :key="sub.id">
                    <td>
                      <div class="subscription-name">{{ sub.name }}</div>
                    </td>
                    <td>${{ sub.amount.toFixed(2) }}</td>
                    <td>
                      <div class="small">
                        {{ formatDate(getNextPaymentDate(sub)) }}
                      </div>
                    </td>
                    <td>
                      <span class="badge" :class="getCategoryClass(sub.category)">
                        {{ sub.category }}
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
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div class="chart-section mt-4">
            <div class="card">
              <div class="card-body">
                <h3 class="card-title">Expenses by Category</h3>
                <div class="category-chart">
                  <div v-for="(amount, category) in categoryExpenses" 
                       :key="category" 
                       class="category-bar-item">
                    <div class="category-bar-label">
                      <span class="badge" :class="getCategoryClass(category)">{{ category }}</span>
                    </div>
                    <div class="category-bar-container">
                      <div class="category-bar" 
                           :style="{ width: getCategoryPercentage(amount) + '%' }">
                      </div>
                      <span class="category-amount">${{ amount.toFixed(2) }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'
import { ref, onMounted, computed } from 'vue'
import { config } from '@/config'

export default {
  name: "ReportPage",
  setup() {
    const subscriptions = ref([])
    const selectedCategory = ref("")
    const sortOption = ref("name")
    const loading = ref(true)
    const error = ref(null)

    const fetchSubscriptions = async () => {
      loading.value = true
      error.value = null
      try {
        const response = await axios.get(`${config.baseUrl}/api/subscription`)
        subscriptions.value = response.data
      } catch (err) {
        error.value = "Failed to load subscription data"
        console.error("Error fetching subscriptions:", err)
      } finally {
        loading.value = false
      }
    }

    onMounted(fetchSubscriptions)

    const filteredSubscriptions = computed(() => {
      let subs = subscriptions.value
      if (selectedCategory.value) {
        subs = subs.filter(sub => sub.category === selectedCategory.value)
      }
      return subs
    })

    const sortedSubscriptions = computed(() => {
      return [...filteredSubscriptions.value].sort((a, b) => {
        switch (sortOption.value) {
          case 'amount':
            return b.amount - a.amount
          case 'dueDate':
            return new Date(getNextPaymentDate(a)) - new Date(getNextPaymentDate(b))
          case 'category':
            return a.category.localeCompare(b.category)
          default: // name
            return a.name.localeCompare(b.name)
        }
      })
    })

    const uniqueCategories = computed(() => {
      const categories = subscriptions.value.map(sub => sub.category)
      return [...new Set(categories)].sort()
    })

    const totalMonthly = computed(() => 
      filteredSubscriptions.value.reduce((sum, sub) => 
        sum + (sub.effectiveMonthlyPrice || 0), 0)
    )

    const averageCost = computed(() => 
      filteredSubscriptions.value.length ? 
        totalMonthly.value / filteredSubscriptions.value.length : 0
    )

    const categoryExpenses = computed(() => {
      const expenses = {}
      filteredSubscriptions.value.forEach(sub => {
        expenses[sub.category] = (expenses[sub.category] || 0) + 
          (sub.effectiveMonthlyPrice || 0)
      })
      return expenses
    })

    const getCategoryPercentage = (amount) => {
      const max = Math.max(...Object.values(categoryExpenses.value))
      return (amount / max) * 100
    }

    const formatDate = (date) => {
      return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      })
    }

    const getNextPaymentDate = (sub) => {
      // Implementation depends on your business logic
      return sub.endDate || new Date()
    }

    const getCategoryClass = (category) => {
      const classes = {
        'Entertainment': 'bg-purple',
        'Software': 'bg-info',
        'Utilities': 'bg-success',
        'Shopping': 'bg-warning',
        'Other': 'bg-secondary'
      }
      return classes[category] || 'bg-secondary'
    }

    const getStatusClass = (sub) => {
      if (!sub.endDate) return 'bg-success'
      return isExpiringSoon(sub) ? 'bg-warning' : 'bg-info'
    }

    const getStatusText = (sub) => {
      if (!sub.endDate) return 'Active'
      const daysLeft = Math.ceil(
        (new Date(sub.endDate) - new Date()) / (1000 * 60 * 60 * 24)
      )
      return daysLeft > 0 ? `${daysLeft} days left` : 'Expired'
    }

    const getStatusTitle = (sub) => {
      if (!sub.endDate) return 'Ongoing subscription'
      return isExpiringSoon(sub) ? 'Subscription ending soon' : 'Fixed term subscription'
    }

    const isExpiringSoon = (sub) => {
      if (!sub.endDate) return false
      const daysUntilExpiry = Math.ceil(
        (new Date(sub.endDate) - new Date()) / (1000 * 60 * 60 * 24)
      )
      return daysUntilExpiry <= 7 && daysUntilExpiry > 0
    }

    return {
      subscriptions,
      selectedCategory,
      sortOption,
      loading,
      error,
      filteredSubscriptions,
      sortedSubscriptions,
      uniqueCategories,
      totalMonthly,
      averageCost,
      categoryExpenses,
      formatDate,
      getNextPaymentDate,
      getCategoryClass,
      getStatusClass,
      getStatusText,
      getStatusTitle,
      getCategoryPercentage
    }
  }
}
</script>

<style scoped>
.report-page {
  padding: 2rem 1rem;
}

.header-section {
  text-align: center;
  margin-bottom: 2rem;
}

.header-section h1 {
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.filter-card, .summary-card, .table-card, .card {
  background: white;
  border-radius: 1rem;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.08);
  padding: 1.5rem;
  height: 100%;
}

.filter-title, .summary-title {
  font-size: 1.25rem;
  color: #2c3e50;
  margin-bottom: 1.5rem;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  border-bottom: 1px solid #e9ecef;
}

.summary-item:last-child {
  border-bottom: none;
}

.table {
  margin-bottom: 0;
}

.subscription-name {
  font-weight: 500;
  color: #2c3e50;
}

.category-chart {
  margin-top: 1.5rem;
}

.category-bar-item {
  display: flex;
  align-items: center;
  margin-bottom: 1rem;
}

.category-bar-label {
  width: 120px;
  margin-right: 1rem;
}

.category-bar-container {
  flex-grow: 1;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.category-bar {
  height: 12px;
  background: #e9ecef;
  border-radius: 6px;
  transition: width 0.3s ease;
}

.category-amount {
  min-width: 80px;
  text-align: right;
  font-weight: 500;
  color: #6c757d;
}

.bg-purple {
  background-color: #6f42c1;
  color: white;
}

@media (max-width: 768px) {
  .filter-card, .summary-card, .table-card {
    margin-bottom: 1.5rem;
  }

  .category-bar-label {
    width: 100px;
  }

  .category-amount {
    min-width: 60px;
  }
}
</style>
