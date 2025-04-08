<template>
  <div class="category-comparison">
    <div class="card">
      <div class="card-body">
        <h2 class="card-title">Category Comparison</h2>
        <p class="card-text">Compare spending across different categories.</p>

        <div v-if="loading" class="text-center my-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <div v-else-if="error" class="alert alert-danger" role="alert">
          {{ error }}
        </div>

        <div v-else>
          <div class="mb-4">
            <label class="form-label">Select Categories to Compare</label>
            <div class="category-selection">
              <div v-for="category in categories" :key="category.id" class="form-check form-check-inline">
                <input
                  class="form-check-input"
                  type="checkbox"
                  :id="'category-' + category.id"
                  :value="category.id"
                  v-model="selectedCategories"
                  @change="updateChart"
                >
                <label class="form-check-label" :for="'category-' + category.id" :style="{ color: category.colorCode }">
                  {{ category.name }}
                </label>
              </div>
            </div>
          </div>

          <div class="mb-3">
            <label for="chartType" class="form-label">Chart Type</label>
            <select id="chartType" class="form-select" v-model="chartType" @change="updateChart">
              <option value="bar">Bar Chart</option>
              <option value="radar">Radar Chart</option>
            </select>
          </div>

          <div v-if="selectedCategories.length === 0" class="alert alert-info">
            Please select at least one category to display the chart.
          </div>

          <div v-else-if="selectedCategories.length > 0" class="chart-container" style="position: relative; height: 400px;">
            <canvas ref="comparisonChart"></canvas>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, watch } from 'vue';
import { apiService } from '@/services/apiService';
import Chart from 'chart.js/auto';

export default {
  name: 'CategoryComparison',
  setup() {
    const comparisonChart = ref(null);
    const chartInstance = ref(null);
    const subscriptions = ref([]);
    const categories = ref([]);
    const loading = ref(true);
    const error = ref(null);
    const selectedCategories = ref([]);
    const chartType = ref('bar'); // Default to bar chart

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

        // Pre-select first 3 categories (or all if less than 3)
        if (categories.value.length > 0) {
          selectedCategories.value = categories.value
            .slice(0, Math.min(3, categories.value.length))
            .map(c => c.id);
        }

        loading.value = false;

        // We'll initialize the chart when the component is fully mounted
        // Don't try to initialize here as the DOM might not be ready
      } catch (err) {
        console.error('Error fetching data:', err);
        error.value = 'Failed to load data. Please try again later.';
        loading.value = false;
      }
    };

    // Calculate monthly price based on billing cycle
    const calculateMonthlyPrice = (subscription) => {
      if (subscription.billingCycle.toLowerCase() === 'yearly') {
        return (subscription.amount * (1 - subscription.discountRate)) / 12;
      }
      return subscription.amount;
    };

    // Get category statistics
    const getCategoryStats = () => {
      const stats = {};

      // Initialize stats for selected categories
      selectedCategories.value.forEach(categoryId => {
        stats[categoryId] = {
          count: 0,
          totalMonthly: 0,
          averagePrice: 0,
          maxPrice: 0,
          minPrice: Infinity
        };
      });

      // Calculate stats for each subscription
      subscriptions.value.forEach(subscription => {
        const categoryId = subscription.category.id;

        // Skip if category not selected
        if (!selectedCategories.value.includes(categoryId)) return;

        const monthlyPrice = calculateMonthlyPrice(subscription);

        stats[categoryId].count++;
        stats[categoryId].totalMonthly += monthlyPrice;

        if (monthlyPrice > stats[categoryId].maxPrice) {
          stats[categoryId].maxPrice = monthlyPrice;
        }

        if (monthlyPrice < stats[categoryId].minPrice) {
          stats[categoryId].minPrice = monthlyPrice;
        }
      });

      // Calculate averages
      selectedCategories.value.forEach(categoryId => {
        if (stats[categoryId].count > 0) {
          stats[categoryId].averagePrice = stats[categoryId].totalMonthly / stats[categoryId].count;
        }

        // Fix min price if no subscriptions
        if (stats[categoryId].minPrice === Infinity) {
          stats[categoryId].minPrice = 0;
        }
      });

      return stats;
    };

    // Get category name by ID
    const getCategoryName = (categoryId) => {
      const category = categories.value.find(c => c.id === categoryId);
      return category ? category.name : 'Unknown';
    };

    // Get category color by ID
    const getCategoryColor = (categoryId) => {
      const category = categories.value.find(c => c.id === categoryId);
      return category ? category.colorCode : '#cccccc';
    };

    // Initialize chart
    const initChart = () => {
      try {
        if (chartInstance.value) {
          chartInstance.value.destroy();
        }

        // Check if the canvas element exists
        if (!comparisonChart.value) {
          console.error('Canvas element not found');
          return;
        }

        const ctx = comparisonChart.value.getContext('2d');
        if (!ctx) {
          console.error('Could not get 2d context from canvas');
          return;
        }

        // Ensure we have selected categories
        if (selectedCategories.value.length === 0) {
          console.warn('No categories selected for chart');
          return;
        }

        const stats = getCategoryStats();

        // Prepare data for chart
        const labels = ['Total Monthly Cost', 'Number of Subscriptions', 'Average Cost', 'Maximum Cost', 'Minimum Cost'];
        const datasets = selectedCategories.value.map(categoryId => {
          return {
            label: getCategoryName(categoryId),
            data: [
              stats[categoryId].totalMonthly,
              stats[categoryId].count,
              stats[categoryId].averagePrice,
              stats[categoryId].maxPrice,
              stats[categoryId].minPrice
            ],
            backgroundColor: `${getCategoryColor(categoryId)}40`, // Add transparency
            borderColor: getCategoryColor(categoryId),
            borderWidth: 2
          };
        });

        // Create chart
        chartInstance.value = new Chart(ctx, {
        type: chartType.value,
        data: {
          labels: labels,
          datasets: datasets
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: chartType.value === 'bar' ? {
            y: {
              beginAtZero: true
            }
          } : {},
          plugins: {
            tooltip: {
              callbacks: {
                label: function(context) {
                  const label = context.dataset.label || '';
                  const value = context.raw;
                  const dataIndex = context.dataIndex;

                  // Format based on data type
                  if (dataIndex === 0 || dataIndex === 2 || dataIndex === 3 || dataIndex === 4) {
                    return `${label}: $${value.toFixed(2)}`;
                  } else {
                    return `${label}: ${value}`;
                  }
                }
              }
            }
          }
        }
      });
      } catch (err) {
        console.error('Error initializing chart:', err);
      }
    };

    // Update chart when selections change
    const updateChart = () => {
      // Use a longer timeout to ensure the DOM is fully updated
      setTimeout(() => {
        if (selectedCategories.value.length > 0 && !loading.value) {
          initChart();
        }
      }, 500); // Increased timeout for better reliability
    };

    // Initialize component
    onMounted(() => {
      fetchData();

      // Initialize chart after the component is fully mounted and data is loaded
      setTimeout(() => {
        if (selectedCategories.value.length > 0 && !loading.value) {
          initChart();
        }
      }, 500); // Use a longer timeout to ensure everything is ready
    });

    // Watch for changes in data
    watch([subscriptions, categories], () => {
      if (!loading.value) {
        updateChart();
      }
    });

    // Watch for changes in chart type
    watch(chartType, () => {
      if (!loading.value && selectedCategories.value.length > 0) {
        updateChart();
      }
    });

    // Watch for changes in selected categories
    watch(selectedCategories, () => {
      if (!loading.value && selectedCategories.value.length > 0) {
        updateChart();
      }
    });

    return {
      comparisonChart,
      categories,
      loading,
      error,
      selectedCategories,
      chartType,
      updateChart
    };
  }
}
</script>

<style scoped>
.category-comparison {
  padding: 1rem 0;
}

.chart-container {
  margin: 0 auto;
}

.card {
  transition: transform 0.2s;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  margin-bottom: 1.5rem;
}

.card:hover {
  transform: translateY(-5px);
}

.category-selection {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.5rem;
}
</style>
