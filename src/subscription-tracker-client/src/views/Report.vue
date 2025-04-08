<template>
  <div class="report-page">
    <h1 class="mb-4">Subscription Reports</h1>

    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="error" class="alert alert-danger" role="alert">
      {{ error }}
    </div>

    <div v-else>
      <!-- Tab navigation -->
      <ul class="nav nav-tabs mb-4">
        <li class="nav-item">
          <a class="nav-link" :class="{ active: activeTab === 'overview' }" href="#" @click.prevent="activeTab = 'overview'">Overview</a>
        </li>
        <li class="nav-item">
          <a class="nav-link" :class="{ active: activeTab === 'categories' }" href="#" @click.prevent="activeTab = 'categories'">Category Breakdown</a>
        </li>
      </ul>

      <!-- Overview Tab -->
      <div v-if="activeTab === 'overview'">
        <!-- Summary section -->
        <div class="card mb-4">
          <div class="card-body">
            <h2 class="card-title">Summary</h2>
            <div class="row">
              <div class="col-md-6">
                <p class="fs-4">Total Monthly Cost: <strong>${{ totalMonthlyCost.toFixed(2) }}</strong></p>
                <p>Active Subscriptions: <strong>{{ subscriptions.length }}</strong></p>
              </div>
              <div class="col-md-6">
                <p>Categories: <strong>{{ categories.length }}</strong></p>
              </div>
            </div>
          </div>
        </div>

        <!-- Chart section -->
        <div class="row mb-4">
          <div class="col-md-6">
            <div class="card h-100">
              <div class="card-body">
                <h3 class="card-title">Cost by Category</h3>
                <div class="chart-container" style="position: relative; height: 300px;">
                  <canvas ref="pieChart"></canvas>
                </div>
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="card h-100">
              <div class="card-body">
                <h3 class="card-title">Monthly Distribution</h3>
                <ul class="list-group list-group-flush">
                  <li v-for="(stats, categoryId) in categoryStats" :key="categoryId" class="list-group-item d-flex justify-content-between align-items-center">
                    {{ getCategoryName(categoryId) }}
                    <span class="badge bg-primary rounded-pill">${{ stats.monthlyTotal.toFixed(2) }}</span>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Category Breakdown Tab -->
      <div v-if="activeTab === 'categories'">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h2>Category Breakdown</h2>
          <div class="d-flex gap-2">
            <button class="btn btn-outline-secondary btn-sm" @click="sortCategories('name')">
              <i class="bi bi-sort-alpha-down"></i> Sort by Name
            </button>
            <button class="btn btn-outline-secondary btn-sm" @click="sortCategories('cost')">
              <i class="bi bi-sort-numeric-down"></i> Sort by Cost
            </button>
          </div>
        </div>

        <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
          <div v-for="categoryId in sortedCategoryIds" :key="categoryId" class="col">
            <div class="card h-100" :style="{ 'border-top': '5px solid ' + getCategoryColor(categoryId) }">
              <div class="card-body">
                <h3 class="card-title">{{ getCategoryName(categoryId) }}</h3>
                <p class="card-text">Total Cost: ${{ categoryStats[categoryId].monthlyTotal.toFixed(2) }}</p>
                <p class="card-text">Subscriptions: {{ categoryStats[categoryId].count }}</p>

                <!-- Subscription list for this category -->
                <div class="mt-3">
                  <h6 class="mb-2">Subscriptions:</h6>
                  <ul class="list-group list-group-flush small">
                    <li v-for="sub in getCategorySubscriptions(categoryId)" :key="sub.id"
                        class="list-group-item px-0 py-1 d-flex justify-content-between align-items-center">
                      {{ sub.name }}
                      <span>${{ calculateMonthlyPrice(sub).toFixed(2) }}/mo</span>
                    </li>
                  </ul>
                </div>

                <div class="mt-3">
                  <button class="btn btn-sm btn-outline-primary" @click="navigateToCategory(categoryId)">
                    <i class="bi bi-pencil"></i> Edit Category
                  </button>
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
/* eslint-disable */
import { ref, onMounted, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { apiService } from '@/services/apiService';

export default {
  name: 'ReportView',
  setup() {
    const router = useRouter();
    const subscriptions = ref([]);
    const categories = ref([]);
    const loading = ref(true);
    const error = ref(null);
    const pieChart = ref(null);
    const chartInstance = ref(null);
    const activeTab = ref('overview'); // Default active tab
    const sortBy = ref('cost'); // Default sort order

    // Fetch data
    const fetchData = async () => {
      loading.value = true;
      error.value = null;

      try {
        // Fetch subscriptions and categories in parallel
        const [subscriptionsResponse, categoriesResponse] = await Promise.all([
          apiService.getSubscriptions(),
          apiService.getCategories()
        ]);

        subscriptions.value = subscriptionsResponse.data;
        categories.value = categoriesResponse.data;

        // Initialize chart after data is loaded and DOM is updated
        loading.value = false;
        // Wait for the DOM to update before initializing the chart
        setTimeout(() => {
          initChart();
        }, 100);
      } catch (err) {
        console.error('Error fetching data:', err);
        error.value = 'Failed to load report data. Please try again later.';
        loading.value = false;
      }
    };

    // Calculate total monthly cost
    const totalMonthlyCost = computed(() => {
      return subscriptions.value.reduce((total, subscription) => {
        return total + calculateMonthlyPrice(subscription);
      }, 0);
    });

    // Calculate monthly price based on billing cycle
    const calculateMonthlyPrice = (subscription) => {
      if (subscription.billingCycle.toLowerCase() === 'yearly') {
        return (subscription.amount * (1 - subscription.discountRate)) / 12;
      }
      return subscription.amount;
    };

    // Group subscriptions by category
    const categoryStats = computed(() => {
      const stats = {};

      subscriptions.value.forEach(subscription => {
        const categoryId = subscription.category.id;
        const monthlyPrice = calculateMonthlyPrice(subscription);

        if (!stats[categoryId]) {
          stats[categoryId] = {
            count: 0,
            monthlyTotal: 0
          };
        }

        stats[categoryId].count++;
        stats[categoryId].monthlyTotal += monthlyPrice;
      });

      return stats;
    });

    // Get category name by ID
    const getCategoryName = (categoryId) => {
      const category = categories.value.find(c => c.id === parseInt(categoryId));
      return category ? category.name : 'Unknown';
    };

    // Get category color by ID
    const getCategoryColor = (categoryId) => {
      const category = categories.value.find(c => c.id === parseInt(categoryId));
      return category ? category.colorCode : '#cccccc';
    };

    // Initialize pie chart
    const initChart = () => {
      // Only initialize chart if we're on the overview tab
      if (activeTab.value !== 'overview') {
        return;
      }

      // Wait for the next tick to ensure the DOM is updated
      setTimeout(() => {
        try {
          if (chartInstance.value) {
            chartInstance.value.destroy();
          }

          // Check if the canvas element is available
          if (!pieChart.value) {
            console.error('Canvas element not found');
            return;
          }

          const ctx = pieChart.value.getContext('2d');
          if (!ctx) {
            console.error('Could not get 2d context from canvas');
            return;
          }

          // Prepare data for chart
          const categoryIds = Object.keys(categoryStats.value);

          // Only create chart if we have data
          if (categoryIds.length === 0) {
            console.log('No data available for chart');
            return;
          }

          const data = categoryIds.map(id => categoryStats.value[id].monthlyTotal);
          const labels = categoryIds.map(id => getCategoryName(id));
          const backgroundColor = categoryIds.map(id => getCategoryColor(id));

          // Create chart
          chartInstance.value = new window.Chart(ctx, {
            type: 'pie',
            data: {
              labels: labels,
              datasets: [{
                data: data,
                backgroundColor: backgroundColor,
                borderWidth: 1
              }]
            },
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: {
                  position: 'right',
                  labels: {
                    font: {
                      size: 12
                    }
                  }
                },
                tooltip: {
                  callbacks: {
                    label: function(context) {
                      const value = context.raw;
                      const total = context.dataset.data.reduce((a, b) => a + b, 0);
                      const percentage = Math.round((value / total) * 100);
                      return `$${value.toFixed(2)} (${percentage}%)`;
                    }
                  }
                }
              }
            }
          });
        } catch (err) {
          console.error('Error initializing chart:', err);
        }
      }, 100);
    };

    // Get subscriptions for a specific category
    const getCategorySubscriptions = (categoryId) => {
      return subscriptions.value.filter(sub => sub.category.id === parseInt(categoryId));
    };

    // Sort categories by name or cost
    const sortCategories = (sortType) => {
      sortBy.value = sortType;
    };

    // Computed property for sorted category IDs
    const sortedCategoryIds = computed(() => {
      const categoryIds = Object.keys(categoryStats.value);

      if (sortBy.value === 'name') {
        // Sort by category name
        return categoryIds.sort((a, b) => {
          const nameA = getCategoryName(a).toLowerCase();
          const nameB = getCategoryName(b).toLowerCase();
          return nameA.localeCompare(nameB);
        });
      } else {
        // Sort by cost (default)
        return categoryIds.sort((a, b) => {
          return categoryStats.value[b].monthlyTotal - categoryStats.value[a].monthlyTotal;
        });
      }
    });

    // Navigate to category page
    const navigateToCategory = (categoryId) => {
      // We could use categoryId in the future to navigate to a specific category
      // For now, just navigate to the categories page
      router.push('/categories');
    };

    // Watch for tab changes
    watch(activeTab, (newTab) => {
      if (newTab === 'overview') {
        // Reinitialize chart when switching to overview tab
        setTimeout(() => {
          initChart();
        }, 100);
      }
    });

    // Initialize component
    onMounted(() => {
      fetchData();
    });

    return {
      subscriptions,
      categories,
      loading,
      error,
      pieChart,
      totalMonthlyCost,
      categoryStats,
      getCategoryName,
      getCategoryColor,
      navigateToCategory,
      activeTab,
      sortCategories,
      sortedCategoryIds,
      getCategorySubscriptions,
      calculateMonthlyPrice
    };
  }
}
</script>

<style scoped>
.report-page {
  padding: 1rem 0;
}

.card {
  transition: transform 0.2s;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.card:hover {
  transform: translateY(-5px);
}

.chart-container {
  margin: 0 auto;
}

.nav-tabs {
  border-bottom: 2px solid #dee2e6;
  margin-bottom: 1.5rem;
}

.nav-tabs .nav-link {
  border: none;
  color: #6c757d;
  font-weight: 500;
  padding: 0.75rem 1.25rem;
  border-bottom: 3px solid transparent;
  transition: all 0.2s ease;
}

.nav-tabs .nav-link:hover {
  color: #495057;
  border-bottom-color: #adb5bd;
}

.nav-tabs .nav-link.active {
  color: #0d6efd;
  background-color: transparent;
  border-bottom-color: #0d6efd;
}
</style>
