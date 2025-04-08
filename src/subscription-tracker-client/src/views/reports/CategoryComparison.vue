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
            <canvas :key="'chart-' + chartType + '-' + selectedCategories.length" ref="comparisonChart"></canvas>
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
import { getCurrentTheme } from '@/services/themeService';

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
    const isDarkMode = ref(getCurrentTheme() === 'dark');

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

    // Get chart options based on current theme
    const getChartOptions = () => {
      // Common options for both themes
      const commonOptions = {
        responsive: true,
        maintainAspectRatio: false,
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
      };

      // Bar chart specific options
      if (chartType.value === 'bar') {
        return {
          ...commonOptions,
          scales: {
            y: {
              beginAtZero: true,
              grid: {
                color: isDarkMode.value ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)'
              },
              ticks: {
                color: isDarkMode.value ? 'rgba(255, 255, 255, 0.7)' : 'rgba(0, 0, 0, 0.7)'
              }
            },
            x: {
              grid: {
                color: isDarkMode.value ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)'
              },
              ticks: {
                color: isDarkMode.value ? 'rgba(255, 255, 255, 0.7)' : 'rgba(0, 0, 0, 0.7)'
              }
            }
          }
        };
      }

      // Radar chart specific options
      return {
        ...commonOptions,
        scales: {
          r: {
            beginAtZero: true,
            angleLines: {
              color: isDarkMode.value ? 'rgba(255, 255, 255, 0.2)' : 'rgba(0, 0, 0, 0.1)',
              lineWidth: isDarkMode.value ? 2 : 1
            },
            grid: {
              color: isDarkMode.value ? 'rgba(255, 255, 255, 0.2)' : 'rgba(0, 0, 0, 0.1)',
              lineWidth: isDarkMode.value ? 2 : 1
            },
            pointLabels: {
              color: isDarkMode.value ? 'rgba(255, 255, 255, 0.9)' : 'rgba(0, 0, 0, 0.7)',
              font: {
                size: 12,
                weight: isDarkMode.value ? 'bold' : 'normal'
              }
            },
            ticks: {
              color: isDarkMode.value ? 'rgba(255, 255, 255, 0.7)' : 'rgba(0, 0, 0, 0.7)',
              backdropColor: isDarkMode.value ? 'rgba(0, 0, 0, 0.5)' : 'rgba(255, 255, 255, 0.5)',
              showLabelBackdrop: isDarkMode.value
            }
          }
        },
        elements: {
          line: {
            borderWidth: isDarkMode.value ? 3 : 2,
            tension: 0.1 // Slightly smooth the lines for better visibility
          },
          point: {
            radius: isDarkMode.value ? 5 : 3,
            hoverRadius: isDarkMode.value ? 8 : 5,
            borderWidth: isDarkMode.value ? 2 : 1,
            backgroundColor: isDarkMode.value ? 'rgba(255, 255, 255, 0.9)' : undefined,
            // Add a shadow to points in dark mode for better visibility
            shadowOffsetX: isDarkMode.value ? 1 : 0,
            shadowOffsetY: isDarkMode.value ? 1 : 0,
            shadowBlur: isDarkMode.value ? 5 : 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        },
        // Add a subtle shadow to the entire chart in dark mode
        plugins: {
          ...commonOptions.plugins,
          // Add a subtle shadow to the chart in dark mode
          shadowPlugin: {
            beforeDraw: function(chart) {
              if (isDarkMode.value) {
                const ctx = chart.ctx;
                ctx.shadowColor = 'rgba(255, 255, 255, 0.15)';
                ctx.shadowBlur = 10;
              }
            }
          }
        }
      };
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
          const color = getCategoryColor(categoryId);
          return {
            label: getCategoryName(categoryId),
            data: [
              stats[categoryId].totalMonthly,
              stats[categoryId].count,
              stats[categoryId].averagePrice,
              stats[categoryId].maxPrice,
              stats[categoryId].minPrice
            ],
            backgroundColor: isDarkMode.value
              ? `${color}80` // Higher opacity in dark mode (50%)
              : `${color}40`, // Lower opacity in light mode (25%)
            borderColor: color,
            borderWidth: isDarkMode.value ? 3 : 2,
            pointBackgroundColor: isDarkMode.value ? '#fff' : color, // White points in dark mode for contrast
            pointBorderColor: color,
            pointHoverBackgroundColor: isDarkMode.value ? '#fff' : '#fff',
            pointHoverBorderColor: color,
            // Add a glow effect to the line in dark mode
            borderDash: isDarkMode.value && chartType.value === 'radar' ? [] : undefined,
            fill: true
          };
        });

        // Get chart options based on current theme
        const options = getChartOptions();

        // Create chart
        chartInstance.value = new Chart(ctx, {
          type: chartType.value,
          data: {
            labels: labels,
            datasets: datasets
          },
          options: options
        });
      } catch (err) {
        console.error('Error initializing chart:', err);
      }
    };

    // Update chart when selections change via UI
    const updateChart = () => {
      // This is now only used for UI events (checkbox changes, dropdown changes)
      // The actual chart updates are handled by the watchers
      // No need to do anything here as the watchers will handle it
    };

    // Watch for theme changes in the document body
    const watchThemeChanges = () => {
      const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
          if (mutation.attributeName === 'class') {
            const newDarkMode = document.body.classList.contains('dark-mode');
            if (isDarkMode.value !== newDarkMode) {
              isDarkMode.value = newDarkMode;
              // Reinitialize chart when theme changes
              if (!loading.value && selectedCategories.value.length > 0) {
                setTimeout(() => initChart(), 100);
              }
            }
          }
        });
      });

      observer.observe(document.body, { attributes: true });
      return observer;
    };

    // Initialize component
    onMounted(() => {
      fetchData();

      // Set initial dark mode state
      isDarkMode.value = document.body.classList.contains('dark-mode');

      // Watch for theme changes
      const themeObserver = watchThemeChanges();

      // Initialize chart after the component is fully mounted and data is loaded
      setTimeout(() => {
        if (selectedCategories.value.length > 0 && !loading.value) {
          initChart();
        }
      }, 500); // Use a longer timeout to ensure everything is ready

      // Clean up observer when component is unmounted
      return () => {
        themeObserver.disconnect();
      };
    });

    // Watch for changes in data
    watch([subscriptions, categories], () => {
      if (!loading.value) {
        updateChart();
      }
    });

    // Watch for changes in chart type
    watch(chartType, () => {
      // The canvas will be recreated automatically due to the :key binding
      // Just make sure the chart instance is destroyed
      if (chartInstance.value) {
        try {
          chartInstance.value.destroy();
          chartInstance.value = null;
        } catch (err) {
          console.error('Error destroying chart:', err);
        }
      }

      // Wait for the next tick to ensure the new canvas is created
      setTimeout(() => {
        if (!loading.value && selectedCategories.value.length > 0) {
          try {
            initChart();
          } catch (err) {
            console.error('Error initializing chart after type change:', err);
          }
        }
      }, 100);
    });

    // Watch for changes in selected categories
    watch(selectedCategories, () => {
      // Destroy the existing chart
      if (chartInstance.value) {
        try {
          chartInstance.value.destroy();
          chartInstance.value = null;
        } catch (err) {
          console.error('Error destroying chart:', err);
        }
      }

      // Wait for the next tick to ensure the DOM is updated
      setTimeout(() => {
        if (!loading.value && selectedCategories.value.length > 0) {
          try {
            initChart();
          } catch (err) {
            console.error('Error initializing chart after category change:', err);
          }
        }
      }, 100);
    });

    return {
      comparisonChart,
      categories,
      loading,
      error,
      selectedCategories,
      chartType,
      updateChart,
      isDarkMode
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
