<template>
  <div class="home">
    <div class="hero-section">
      <h1>Welcome to Subscription Tracker</h1>
      <p class="lead">Manage your subscriptions efficiently and stay on top of your expenses</p>
      <router-link to="/subscription-form" class="btn btn-primary btn-lg mt-3">
        <i class="bi bi-plus-circle"></i> Add New Subscription
      </router-link>
    </div>

    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="error" class="alert alert-danger" role="alert">
      {{ error }}
    </div>

    <div v-else class="dashboard-stats">
      <div class="row g-4">
        <div class="col-md-6 col-lg-3">
          <div class="stat-card">
            <div class="stat-card-body">
              <h3 class="stat-card-title">Total Subscriptions</h3>
              <p class="stat-card-number">{{ subscriptions.length }}</p>
            </div>
          </div>
        </div>
        
        <div class="col-md-6 col-lg-3">
          <div class="stat-card">
            <div class="stat-card-body">
              <h3 class="stat-card-title">Monthly Cost</h3>
              <p class="stat-card-number">${{ totalMonthlyAmount.toFixed(2) }}</p>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-3">
          <div class="stat-card">
            <div class="stat-card-body">
              <h3 class="stat-card-title">Active Subscriptions</h3>
              <p class="stat-card-number">{{ activeSubscriptions.length }}</p>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-3">
          <div class="stat-card">
            <div class="stat-card-body">
              <h3 class="stat-card-title">Expiring Soon</h3>
              <p class="stat-card-number">{{ expiringSoonCount }}</p>
            </div>
          </div>
        </div>
      </div>

      <div class="row mt-5">
        <div class="col-md-6">
          <div class="card">
            <div class="card-body">
              <h3 class="card-title">Categories</h3>
              <div class="category-list">
                <div v-for="(stats, categoryId) in categoryStats" :key="categoryId" class="category-item">
                  <span class="badge bg-info">{{ getCategoryName(categoryId) }}</span>
                  <span class="category-count">{{ stats.count }} (${{ stats.monthlyTotal.toFixed(2) }})</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-md-6">
          <div class="card">
            <div class="card-body">
              <h3 class="card-title">Quick Actions</h3>
              <div class="quick-actions">
                <router-link to="/subscriptions" class="btn btn-outline-primary mb-2">
                  <i class="bi bi-list"></i> View All Subscriptions
                </router-link>
                <router-link to="/report" class="btn btn-outline-info mb-2">
                  <i class="bi bi-graph-up"></i> View Reports
                </router-link>
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
import { ref, computed, onMounted } from 'vue'
import { config } from '@/config'

export default {
  name: "HomePage",
  /**
   * Component setup function
   * @returns {Object} Component properties and methods
   */
  setup() {
    const subscriptions = ref([])
    const categories = ref([])
    const loading = ref(true)
    const error = ref(null)

    /**
     * Fetch initial subscription and category data
     * @async
     * @returns {Promise<void>}
     */
    const fetchData = async () => {
      try {
        const [subsResponse, catsResponse] = await Promise.all([
          axios.get(`${config.baseUrl}/api/subscription`),
          axios.get(`${config.baseUrl}/api/category`)
        ])
        subscriptions.value = subsResponse.data
        categories.value = catsResponse.data
      } catch (err) {
        error.value = "Failed to load data"
        console.error("Error fetching data:", err)
      } finally {
        loading.value = false
      }
    }

    onMounted(fetchData)

    /**
     * Calculate total monthly cost across all subscriptions
     * @computed
     * @returns {number} Total monthly amount
     */
    const totalMonthlyAmount = computed(() => {
      return subscriptions.value.reduce((total, sub) => {
        if (sub.billingCycle === 'yearly') {
          return total + ((sub.amount * (1 - (sub.discountRate || 0))) / 12)
        }
        return total + sub.amount
      }, 0)
    })

    /**
     * Filter active subscriptions
     * @computed
     * @returns {Array} Active subscriptions
     */
    const activeSubscriptions = computed(() =>
      subscriptions.value.filter(sub => !sub.endDate || new Date(sub.endDate) > new Date())
    )

    /**
     * Count subscriptions expiring within 7 days
     * @computed
     * @returns {number} Count of expiring subscriptions
     */
    const expiringSoonCount = computed(() =>
      subscriptions.value.filter(sub => {
        if (!sub.endDate) return false
        const daysUntilExpiry = Math.ceil(
          (new Date(sub.endDate) - new Date()) / (1000 * 60 * 60 * 24)
        )
        return daysUntilExpiry <= 7 && daysUntilExpiry > 0
      }).length
    )

    /**
     * Calculate statistics per category
     * @computed
     * @returns {Object} Category statistics
     */
    const categoryStats = computed(() => {
      return categories.value.reduce((acc, category) => {
        const categorySubscriptions = subscriptions.value.filter(sub => {
          return sub.category?.id === category.id
        })
        
        const monthlyTotal = categorySubscriptions.reduce((total, sub) => {
          if (sub.billingCycle === 'yearly') {
            return total + ((sub.amount * (1 - (sub.discountRate || 0))) / 12)
          }
          return total + sub.amount
        }, 0)

        acc[category.id] = {
          count: categorySubscriptions.length,
          monthlyTotal: monthlyTotal
        }
        return acc
      }, {})
    })

    /**
     * Get category name by ID
     * @param {number} categoryId - Category ID
     * @returns {string} Category name
     */
    const getCategoryName = (categoryId) => {
      const category = categories.value.find(c => c.id === parseInt(categoryId))
      return category?.name || 'Unknown'
    }

    return {
      subscriptions,
      loading,
      error,
      totalMonthlyAmount,
      activeSubscriptions,
      expiringSoonCount,
      categoryStats,
      getCategoryName
    }
  }
}
</script>

<style scoped>
.home {
  padding: 2rem 1rem;
}

.hero-section {
  text-align: center;
  padding: 3rem 0;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border-radius: 1rem;
  margin-bottom: 3rem;
}

.hero-section h1 {
  font-size: 2.5rem;
  margin-bottom: 1rem;
  color: #2c3e50;
}

.hero-section .lead {
  font-size: 1.25rem;
  color: #6c757d;
  max-width: 600px;
  margin: 0 auto;
}

.stat-card {
  background: white;
  border-radius: 1rem;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.08);
  transition: transform 0.2s;
  height: 100%;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.stat-card-body {
  padding: 1.5rem;
}

.stat-card-title {
  font-size: 1rem;
  color: #6c757d;
  margin-bottom: 0.5rem;
}

.stat-card-number {
  font-size: 1.75rem;
  font-weight: bold;
  color: #2c3e50;
  margin: 0;
}

.card {
  border-radius: 1rem;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.08);
  height: 100%;
}

.card-title {
  font-size: 1.25rem;
  color: #2c3e50;
  margin-bottom: 1.5rem;
}

.category-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.category-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background: #f8f9fa;
  border-radius: 0.5rem;
}

.category-count {
  font-weight: 500;
  color: #6c757d;
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.quick-actions .btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  justify-content: center;
  padding: 0.75rem;
  font-weight: 500;
}

@media (max-width: 768px) {
  .hero-section {
    padding: 2rem 1rem;
  }

  .hero-section h1 {
    font-size: 2rem;
  }

  .stat-card-body {
    padding: 1rem;
  }

  .stat-card-number {
    font-size: 1.5rem;
  }
}
</style>
