<template>
  <div class="time-trend-analysis">
    <div class="card">
      <div class="card-body">
        <h2 class="card-title">Time Trend Analysis</h2>
        <p class="card-text">View your subscription spending trends over time.</p>

        <div v-if="loading" class="text-center my-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <div v-else-if="error" class="alert alert-danger" role="alert">
          {{ error }}
        </div>

        <div v-else>
          <div class="mb-3">
            <label for="timeRange" class="form-label">Time Range</label>
            <select id="timeRange" class="form-select" v-model="timeRange" @change="updateChart">
              <option value="6">Last 6 Months</option>
              <option value="12">Last 12 Months</option>
            </select>
          </div>

          <div class="chart-container" style="position: relative; height: 400px;">
            <canvas ref="trendChart"></canvas>
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
  name: 'TimeTrendAnalysis',
  setup() {
    const trendChart = ref(null);
    const chartInstance = ref(null);
    const subscriptions = ref([]);
    const loading = ref(true);
    const error = ref(null);
    const timeRange = ref('6'); // Default to 6 months

    // Fetch subscription data
    const fetchData = async () => {
      loading.value = true;
      error.value = null;

      try {
        const response = await apiService.getSubscriptions();
        subscriptions.value = response.data;
        loading.value = false;

        // Wait for the DOM to update before initializing the chart
        setTimeout(() => {
          initChart();
        }, 100);
      } catch (err) {
        console.error('Error fetching subscription data:', err);
        error.value = 'Failed to load subscription data. Please try again later.';
        loading.value = false;
      }
    };

    // Calculate monthly spending for the past N months
    const calculateMonthlySpending = (months) => {
      const today = new Date();
      const data = [];
      const labels = [];

      // Generate labels and initialize data array
      for (let i = months - 1; i >= 0; i--) {
        const date = new Date(today.getFullYear(), today.getMonth() - i, 1);
        const monthName = date.toLocaleString('default', { month: 'short' });
        const year = date.getFullYear();
        labels.push(`${monthName} ${year}`);
        data.push(0);
      }

      // Calculate spending for each month
      subscriptions.value.forEach(subscription => {
        const startDate = new Date(subscription.startDate);
        const endDate = subscription.endDate ? new Date(subscription.endDate) : null;

        // Skip if subscription hasn't started yet
        if (startDate > today) return;

        // Skip if subscription has ended before the time range
        if (endDate && endDate < new Date(today.getFullYear(), today.getMonth() - (months - 1), 1)) return;

        // Use the effectiveMonthlyPrice from the API response
        const monthlyCost = subscription.effectiveMonthlyPrice;

        // Add cost to each applicable month
        for (let i = 0; i < months; i++) {
          const monthDate = new Date(today.getFullYear(), today.getMonth() - (months - 1) + i, 1);

          // Skip if subscription hasn't started yet for this month
          if (startDate > monthDate) continue;

          // Skip if subscription has ended before this month
          if (endDate && endDate < monthDate) continue;

          data[i] += monthlyCost;
        }
      });

      return { labels, data };
    };

    // Initialize chart
    const initChart = () => {
      if (chartInstance.value) {
        chartInstance.value.destroy();
      }

      // Check if the canvas element exists
      if (!trendChart.value) {
        console.error('Canvas element not found');
        return;
      }

      const ctx = trendChart.value.getContext('2d');
      if (!ctx) {
        console.error('Could not get 2d context from canvas');
        return;
      }

      const { labels, data } = calculateMonthlySpending(parseInt(timeRange.value));

      chartInstance.value = new Chart(ctx, {
        type: 'line',
        data: {
          labels: labels,
          datasets: [{
            label: 'Monthly Spending',
            data: data,
            borderColor: '#3a86ff',
            backgroundColor: 'rgba(58, 134, 255, 0.1)',
            borderWidth: 2,
            fill: true,
            tension: 0.4
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            y: {
              beginAtZero: true,
              ticks: {
                callback: function(value) {
                  return '$' + value.toFixed(2);
                }
              }
            }
          },
          plugins: {
            tooltip: {
              callbacks: {
                label: function(context) {
                  return '$' + context.raw.toFixed(2);
                }
              }
            },
            legend: {
              display: true,
              position: 'top'
            }
          }
        }
      });
    };

    // Update chart when time range changes
    const updateChart = () => {
      if (subscriptions.value.length > 0) {
        // Wait for the DOM to update before initializing the chart
        setTimeout(() => {
          initChart();
        }, 100);
      }
    };

    // Initialize component
    onMounted(() => {
      fetchData();
    });

    // Watch for changes in subscriptions
    watch(subscriptions, () => {
      if (subscriptions.value.length > 0) {
        initChart();
      }
    });

    return {
      trendChart,
      loading,
      error,
      timeRange,
      updateChart
    };
  }
}
</script>

<style scoped>
.time-trend-analysis {
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
</style>
