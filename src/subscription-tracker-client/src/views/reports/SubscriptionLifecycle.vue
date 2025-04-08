<template>
  <div class="subscription-lifecycle">
    <div class="card">
      <div class="card-body">
        <h2 class="card-title">Subscription Lifecycle</h2>
        <p class="card-text">View the timeline of your subscriptions.</p>

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
            <label for="sortBy" class="form-label">Sort By</label>
            <select id="sortBy" class="form-select" v-model="sortBy" @change="sortSubscriptions">
              <option value="startDate">Start Date</option>
              <option value="endDate">End Date</option>
              <option value="name">Name</option>
              <option value="category">Category</option>
              <option value="amount">Amount</option>
            </select>
          </div>

          <div class="mb-3">
            <label for="filterCategory" class="form-label">Filter by Category</label>
            <select id="filterCategory" class="form-select" v-model="filterCategory" @change="filterSubscriptions">
              <option value="all">All Categories</option>
              <option v-for="category in categories" :key="category.id" :value="category.id">
                {{ category.name }}
              </option>
            </select>
          </div>

          <div class="timeline-container">
            <div class="timeline-header">
              <div class="timeline-scale">
                <div v-for="(month, index) in timelineMonths" :key="index" class="timeline-month">
                  {{ month }}
                </div>
              </div>
            </div>

            <div class="timeline-body">
              <div v-for="subscription in filteredSubscriptions" :key="subscription.id" class="timeline-item">
                <div class="timeline-item-label" :title="subscription.name">
                  {{ subscription.name }}
                </div>
                <div class="timeline-item-bar-container">
                  <div
                    class="timeline-item-bar"
                    :style="{
                      left: `${getStartPosition(subscription)}%`,
                      width: `${getDuration(subscription)}%`,
                      backgroundColor: getCategoryColor(subscription.category.id)
                    }"
                    :title="`${subscription.name} (${formatDate(subscription.startDate)} - ${subscription.endDate ? formatDate(subscription.endDate) : 'Ongoing'})`"
                  >
                    <span class="timeline-item-price">${{ calculateMonthlyPrice(subscription).toFixed(2) }}/mo</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="timeline-legend mt-4">
            <h5>Categories</h5>
            <div class="d-flex flex-wrap gap-3">
              <div v-for="category in categories" :key="category.id" class="d-flex align-items-center">
                <div class="color-box me-2" :style="{ backgroundColor: category.colorCode }"></div>
                <span>{{ category.name }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import { apiService } from '@/services/apiService';

export default {
  name: 'SubscriptionLifecycle',
  setup() {
    const subscriptions = ref([]);
    const filteredSubscriptions = ref([]);
    const categories = ref([]);
    const loading = ref(true);
    const error = ref(null);
    const sortBy = ref('startDate');
    const filterCategory = ref('all');

    // Timeline configuration
    const timelineStartDate = ref(new Date());
    const timelineEndDate = ref(new Date());
    const timelineMonths = ref([]);

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

        // Initialize timeline dates
        initializeTimelineDates();

        // Apply initial sorting and filtering
        sortSubscriptions();

        loading.value = false;
      } catch (err) {
        console.error('Error fetching data:', err);
        error.value = 'Failed to load data. Please try again later.';
        loading.value = false;
      }
    };

    // Initialize timeline dates
    const initializeTimelineDates = () => {
      // Find earliest start date and latest end date
      let earliestDate = new Date();
      let latestDate = new Date();

      subscriptions.value.forEach(subscription => {
        const startDate = new Date(subscription.startDate);
        if (startDate < earliestDate) {
          earliestDate = startDate;
        }

        if (subscription.endDate) {
          const endDate = new Date(subscription.endDate);
          if (endDate > latestDate) {
            latestDate = endDate;
          }
        } else {
          // For ongoing subscriptions, add 1 year from today
          const oneYearFromNow = new Date();
          oneYearFromNow.setFullYear(oneYearFromNow.getFullYear() + 1);
          if (oneYearFromNow > latestDate) {
            latestDate = oneYearFromNow;
          }
        }
      });

      // Set timeline start date to the beginning of the month of the earliest date
      timelineStartDate.value = new Date(earliestDate.getFullYear(), earliestDate.getMonth(), 1);

      // Set timeline end date to the end of the month of the latest date
      timelineEndDate.value = new Date(latestDate.getFullYear(), latestDate.getMonth() + 1, 0);

      // Generate timeline months
      generateTimelineMonths();
    };

    // Generate timeline months
    const generateTimelineMonths = () => {
      const months = [];
      const currentDate = new Date(timelineStartDate.value);

      while (currentDate <= timelineEndDate.value) {
        months.push(currentDate.toLocaleString('default', { month: 'short', year: 'numeric' }));
        currentDate.setMonth(currentDate.getMonth() + 1);
      }

      timelineMonths.value = months;
    };

    // Calculate monthly price
    const calculateMonthlyPrice = (subscription) => {
      if (subscription.billingCycle.toLowerCase() === 'yearly') {
        return (subscription.amount * (1 - subscription.discountRate)) / 12;
      }
      return subscription.amount;
    };

    // Format date for display
    const formatDate = (dateString) => {
      const date = new Date(dateString);
      return date.toLocaleDateString('default', { year: 'numeric', month: 'short', day: 'numeric' });
    };

    // Get category color
    const getCategoryColor = (categoryId) => {
      const category = categories.value.find(c => c.id === categoryId);
      return category ? category.colorCode : '#cccccc';
    };

    // Calculate start position percentage for timeline
    const getStartPosition = (subscription) => {
      const startDate = new Date(subscription.startDate);
      const timelineStart = timelineStartDate.value;
      const timelineDuration = timelineEndDate.value - timelineStart;

      const startPosition = ((startDate - timelineStart) / timelineDuration) * 100;
      return Math.max(0, Math.min(100, startPosition));
    };

    // Calculate duration percentage for timeline
    const getDuration = (subscription) => {
      const startDate = new Date(subscription.startDate);
      const endDate = subscription.endDate ? new Date(subscription.endDate) : timelineEndDate.value;
      const timelineStart = timelineStartDate.value;
      const timelineDuration = timelineEndDate.value - timelineStart;

      const duration = ((endDate - startDate) / timelineDuration) * 100;
      return Math.max(0, Math.min(100 - getStartPosition(subscription), duration));
    };

    // Sort subscriptions
    const sortSubscriptions = () => {
      const sorted = [...subscriptions.value];

      switch (sortBy.value) {
        case 'startDate':
          sorted.sort((a, b) => new Date(a.startDate) - new Date(b.startDate));
          break;
        case 'endDate':
          sorted.sort((a, b) => {
            if (!a.endDate && !b.endDate) return 0;
            if (!a.endDate) return 1;
            if (!b.endDate) return -1;
            return new Date(a.endDate) - new Date(b.endDate);
          });
          break;
        case 'name':
          sorted.sort((a, b) => a.name.localeCompare(b.name));
          break;
        case 'category':
          sorted.sort((a, b) => a.category.name.localeCompare(b.category.name));
          break;
        case 'amount':
          sorted.sort((a, b) => calculateMonthlyPrice(b) - calculateMonthlyPrice(a));
          break;
      }

      // Apply filtering
      filterSubscriptions(sorted);
    };

    // Filter subscriptions
    const filterSubscriptions = (sortedSubscriptions = null) => {
      const toFilter = sortedSubscriptions || [...subscriptions.value];

      if (filterCategory.value === 'all') {
        filteredSubscriptions.value = toFilter;
      } else {
        filteredSubscriptions.value = toFilter.filter(s => s.category.id === parseInt(filterCategory.value));
      }
    };

    // Initialize component
    onMounted(() => {
      fetchData();
    });

    return {
      subscriptions,
      filteredSubscriptions,
      categories,
      loading,
      error,
      sortBy,
      filterCategory,
      timelineMonths,
      calculateMonthlyPrice,
      formatDate,
      getCategoryColor,
      getStartPosition,
      getDuration,
      sortSubscriptions,
      filterSubscriptions
    };
  }
}
</script>

<style scoped>
.subscription-lifecycle {
  padding: 1rem 0;
}

.card {
  transition: transform 0.2s;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  margin-bottom: 1.5rem;
}

.card:hover {
  transform: translateY(-5px);
}

.timeline-container {
  margin-top: 2rem;
  overflow-x: auto;
}

.timeline-header {
  border-bottom: 1px solid #dee2e6;
  padding-bottom: 0.5rem;
}

.timeline-scale {
  display: flex;
  position: relative;
}

.timeline-month {
  flex: 1;
  text-align: center;
  font-size: 0.8rem;
  color: #6c757d;
}

.timeline-body {
  margin-top: 1rem;
}

.timeline-item {
  display: flex;
  margin-bottom: 1rem;
  align-items: center;
}

.timeline-item-label {
  width: 150px;
  padding-right: 1rem;
  text-align: right;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.timeline-item-bar-container {
  flex: 1;
  height: 30px;
  position: relative;
  background-color: #f8f9fa;
  border-radius: 4px;
}

.timeline-item-bar {
  position: absolute;
  height: 100%;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 0.8rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.timeline-item-price {
  padding: 0 0.5rem;
}

.timeline-legend {
  border-top: 1px solid #dee2e6;
  padding-top: 1rem;
}

.color-box {
  width: 20px;
  height: 20px;
  border-radius: 4px;
}
</style>
