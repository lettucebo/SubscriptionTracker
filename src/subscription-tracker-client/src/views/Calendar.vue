<template>
  <div class="calendar-view">
    <!-- Page Header with gradient background -->
    <div class="header-container mb-4 p-3 bg-gradient rounded shadow-sm">
      <div class="d-flex align-items-center">
        <div class="calendar-icon-wrapper me-3">
          <i class="fas fa-calendar"></i>
        </div>
        <div>
          <h1 class="mb-0">Calendar View</h1>
          <p class="text-muted mb-0 d-none d-md-block">
            <i class="fas fa-info-circle me-1"></i>
            Track your subscription payments at a glance
          </p>
        </div>
      </div>
      <div>
        <button @click="refreshCalendar" class="btn btn-outline-primary btn-lg rounded-pill" :disabled="loading">
          <i class="fas fa-sync-alt me-1" :class="{ 'fa-spin': loading }"></i>
          <span class="ms-1 d-none d-md-inline">Refresh</span>
        </button>
      </div>
    </div>

    <div v-if="loading" class="text-center my-5 py-5">
      <div class="spinner-grow text-primary" role="status" style="width: 3rem; height: 3rem;">
        <span class="visually-hidden">Loading...</span>
      </div>
      <div class="spinner-grow text-secondary mx-2" role="status" style="width: 3rem; height: 3rem;">
        <span class="visually-hidden">Loading...</span>
      </div>
      <div class="spinner-grow text-primary" role="status" style="width: 3rem; height: 3rem;">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-3 lead text-muted">
        <i class="fas fa-hourglass-half me-2 pulse-animation"></i>
        Loading your subscriptions...
      </p>
    </div>

    <div v-else-if="error" class="alert alert-danger shadow-sm rounded-3 border-start border-danger border-5"
      role="alert">
      <div class="d-flex align-items-center">
        <i class="fas fa-exclamation-triangle fa-2x me-3 text-danger"></i>
        <div>
          <h5 class="alert-heading mb-1">Error Loading Calendar</h5>
          <p class="mb-0">{{ error }}</p>
        </div>
      </div>
    </div>

    <div v-else>
      <div class="card shadow border-0 rounded-3 mb-4" :class="{ 'dark-card': $root.darkMode }">
        <div class="card-header bg-transparent border-0 pt-3" :class="{ 'dark-card-header': $root.darkMode }">
          <div class="d-flex justify-content-between align-items-center mx-2">
            <div class="calendar-legend d-none d-md-flex">
              <span v-for="category in uniqueCategories" :key="category.name" class="legend-item me-3">
                <span class="color-dot" :style="{ backgroundColor: category.color }"></span>
                {{ category.name }}
              </span>
            </div>
          </div>
        </div>
        <div class="card-body p-0 p-md-3" :class="{ 'dark-card-body': $root.darkMode }">
          <FullCalendar ref="fullCalendar" :options="calendarOptions" class="calendar-container" />
        </div>
      </div>
    </div>

    <!-- Edit Subscription Modal with enhanced styling -->
    <div class="modal fade" :class="{ show: isModalOpen, 'd-block': isModalOpen }" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content shadow border-0 rounded-3 overflow-hidden" :class="{ 'dark-modal-content': $root.darkMode }">
          <div class="modal-header bg-gradient text-white" :class="{ 'dark-modal-header': $root.darkMode }">
            <h5 class="modal-title d-flex align-items-center">
              <i class="fas fa-edit me-2"></i>
              Edit Subscription
            </h5>
            <button type="button" class="btn-close btn-close-white" @click="closeModal"></button>
          </div>
          <div class="modal-body p-4" :class="{ 'dark-modal-body': $root.darkMode }">
            <div v-if="error" class="alert alert-danger mb-3 rounded-3 border-start border-danger border-5">
              <div class="d-flex">
                <i class="fas fa-exclamation-triangle me-2 mt-1"></i>
                <span>{{ error }}</span>
              </div>
            </div>

            <div class="subscription-details mb-4 p-3 rounded-3" :class="{ 'bg-light': !$root.darkMode, 'dark-subscription-details': $root.darkMode }">
              <h6 class="text-primary mb-2">
                <i class="fas fa-tag me-2"></i>
                {{ editEvent.title || 'Subscription Details' }}
              </h6>
              <div class="d-flex justify-content-between">
                <div class="text-muted">
                  <i class="fas fa-dollar-sign me-1"></i>
                  {{ editEvent.amount ? editEvent.amount.toFixed(2) : '0.00' }}
                </div>
                <div>
                  <span class="badge rounded-pill" :class="getBillingCycleBadgeClass(editEvent.billingCycle)">
                    <i :class="getBillingCycleIcon(editEvent.billingCycle)"></i>
                    {{ editEvent.billingCycle || 'Not set' }}
                  </span>
                </div>
              </div>
            </div>

            <div class="mb-4">
              <label class="form-label fw-bold">
                <i class="far fa-calendar-alt me-2 text-primary"></i>
                Start Date
              </label>
              <div class="input-group input-group-lg shadow-sm">
                <span class="input-group-text bg-light border-end-0">
                  <i class="fas fa-calendar-day text-primary"></i>
                </span>
                <input type="date" class="form-control border-start-0" v-model="editEvent.startDate"
                  :disabled="updating">
              </div>
              <div class="form-text mt-2">
                <i class="fas fa-info-circle me-1 text-primary"></i>
                Changing this date will affect future billing cycles
              </div>
            </div>
          </div>
          <div class="modal-footer" :class="{ 'bg-light': !$root.darkMode, 'dark-modal-footer': $root.darkMode }">
            <button type="button" class="btn btn-outline-secondary rounded-pill" @click="closeModal"
              :disabled="updating">
              <i class="fas fa-times me-1"></i>
              Cancel
            </button>
            <button type="button" class="btn btn-primary rounded-pill" @click="saveEventChanges" :disabled="updating">
              <span v-if="updating" class="spinner-border spinner-border-sm me-1" role="status"></span>
              <i v-else class="fas fa-save me-1"></i>
              {{ updating ? 'Saving...' : 'Save changes' }}
            </button>
          </div>
        </div>
      </div>
    </div>
    <div v-if="isModalOpen" class="modal-backdrop show"></div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { config } from '@/config'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'

/**
 * Calendar view component for managing subscription events
 * @component
 * @vue-data {boolean} loading - Data loading state
 * @vue-data {string|null} error - Error message
 * @vue-data {Array} subscriptions - List of subscriptions
 * @vue-data {Object} editEvent - Currently edited event data
 * @vue-data {boolean} updating - Update in progress flag
 * @vue-data {boolean} isModalOpen - Modal visibility state
 * @vue-computed {Array} calendarEvents - Generated calendar events from subscriptions
 */
export default {
  name: 'CalendarView',
  components: {
    FullCalendar
  },
  setup() {
    const loading = ref(false)
    const error = ref(null)
    const subscriptions = ref([])
    const editEvent = ref({
      id: null,
      startDate: '',
      title: '',
      amount: 0,
      billingCycle: ''
    })
    const updating = ref(false)
    const isModalOpen = ref(false)
    const fullCalendar = ref(null)
    const currentView = ref('dayGridMonth')

    /**
     * Fetch subscriptions from API
     * @async
     */
    const fetchEvents = async () => {
      loading.value = true
      error.value = null
      try {
        const response = await axios.get(`${config.baseUrl}/api/subscription`)
        subscriptions.value = response.data
      } catch (err) {
        error.value = 'Failed to load subscriptions. Please try again later.'
        console.error('Error fetching events:', err)
      } finally {
        loading.value = false
      }
    }

    const refreshCalendar = async () => {
      loading.value = true
      error.value = null
      try {
        await fetchCategories()
        await fetchEvents()
      } catch (err) {
        error.value = 'Failed to refresh calendar. Please try again.'
        console.error('Error refreshing calendar:', err)
      } finally {
        loading.value = false
      }
    }

    const categories = ref([]);

    /**
     * Fetch categories from API
     * @async
     */
    const fetchCategories = async () => {
      try {
        const response = await axios.get(`${config.baseUrl}/api/category`)
        categories.value = response.data
      } catch (err) {
        console.error('Error fetching categories:', err)
      }
    }

    const getEventColor = (category) => {
      if (!category) return '#808080'; // Default gray color

      // If we have a direct colorCode, use it
      if (typeof category === 'object') {
        if (category.colorCode) {
          return category.colorCode;
        }

        // Find matching category from fetched categories
        if (category.id) {
          const match = categories.value.find(c => c.id === category.id);
          if (match?.colorCode) {
            return match.colorCode;
          }
        }
      }

      return '#808080'; // Fallback to default gray
    }

    /**
     * Generate recurring calendar events from subscription data
     * @param {Object} subscription - Subscription data
     * @returns {Array} Array of calendar events
     */
    const generateRecurringEvents = (subscription) => {
      const events = [];
      const startDate = new Date(subscription.startDate);
      const endDate = new Date();
      endDate.setMonth(endDate.getMonth() + 12); // Show next 12 months

      const categoryColor = getEventColor(subscription.category);

      let currentDate = new Date(startDate);
      while (currentDate <= endDate) {
        events.push({
          id: `${subscription.id}-${currentDate.getTime()}`,
          title: subscription.name,
          start: currentDate.toISOString().split('T')[0],
          allDay: true,
          extendedProps: {
            subscriptionId: subscription.id,
            amount: subscription.amount,
            billingCycle: subscription.billingCycle,
            isRecurring: true,
          },
          backgroundColor: categoryColor,
          borderColor: categoryColor
        });

        // Add months based on billing cycle
        const cycle = subscription.billingCycle?.toLowerCase();
        switch (cycle) {
          case 'monthly':
            currentDate.setMonth(currentDate.getMonth() + 1);
            break;
          case 'quarterly':
            currentDate.setMonth(currentDate.getMonth() + 3);
            break;
          case 'yearly':
            currentDate.setFullYear(currentDate.getFullYear() + 1);
            break;
          default:
            currentDate.setMonth(currentDate.getMonth() + 1);
            console.warn(`Unknown billing cycle: ${subscription.billingCycle}`);
            break;
        }
      }
      return events;
    };

    const calendarEvents = computed(() =>
      subscriptions.value.flatMap(sub => generateRecurringEvents(sub))
    )

    // Extract unique categories from subscriptions for legend
    const uniqueCategories = computed(() => {
      const categoryMap = new Map();

      subscriptions.value.forEach(sub => {
        if (sub.category?.name) {
          console.log(sub.category.name, getEventColor(sub.category));
          categoryMap.set(sub.category.name, getEventColor(sub.category));
        }
      });

      return Array.from(categoryMap).map(([name, color]) => ({ name, color }));
    });

    /**
     * Handle calendar event click
     * @param {Object} info - FullCalendar event info
     */
    const handleEventClick = (info) => {
      const subscriptionId = info.event.extendedProps.subscriptionId || info.event.id;
      editEvent.value = {
        id: subscriptionId,
        startDate: info.event.start.toISOString().split('T')[0],
        title: info.event.title,
        amount: info.event.extendedProps.amount || 0,
        billingCycle: info.event.extendedProps.billingCycle || ''
      }
      error.value = null
      document.body.classList.add('modal-open')
      isModalOpen.value = true
    }

    const closeModal = () => {
      isModalOpen.value = false
      document.body.classList.remove('modal-open')
    }

    /**
     * Handle event drag-and-drop update
     * @async
     * @param {Object} info - FullCalendar event drop info
     */
    const handleEventDrop = async (info) => {
      updating.value = true
      const subscriptionId = info.event.extendedProps.subscriptionId || info.event.id;
      editEvent.value.id = subscriptionId;

      try {
        await axios.put(`${config.baseUrl}/api/subscription/${subscriptionId}/dates`, {
          startDate: info.event.start.toISOString()
        })
        await fetchEvents()
      } catch (err) {
        error.value = 'Failed to update subscription dates'
        console.error('Error updating event:', err)
        info.revert()
      } finally {
        updating.value = false
      }
    }

    /**
     * Save event changes to API
     * @async
     */
    const saveEventChanges = async () => {
      updating.value = true
      error.value = null

      try {
        await axios.put(`${config.baseUrl}/api/subscription/${editEvent.value.id}/dates`, {
          startDate: editEvent.value.startDate
        })
        closeModal()
        await fetchEvents()
      } catch (err) {
        error.value = 'Failed to update subscription dates'
        console.error('Error saving event changes:', err)
      } finally {
        updating.value = false
      }
    }

    // Get appropriate icon for billing cycle
    const getBillingCycleIcon = (billingCycle) => {
      if (!billingCycle) return 'fas fa-question-circle';

      const cycle = billingCycle.toLowerCase();
      if (cycle.includes('month')) return 'fas fa-sync-alt';
      if (cycle.includes('quarter')) return 'fas fa-calendar-alt';
      if (cycle.includes('year') || cycle.includes('annual')) return 'fas fa-calendar-check';
      return 'fas fa-clock';
    }

    // Get appropriate badge class for billing cycle
    const getBillingCycleBadgeClass = (billingCycle) => {
      if (!billingCycle) return 'bg-secondary';

      const cycle = billingCycle.toLowerCase();
      if (cycle.includes('month')) return 'bg-primary';
      if (cycle.includes('quarter')) return 'bg-info';
      if (cycle.includes('year') || cycle.includes('annual')) return 'bg-success';
      return 'bg-secondary';
    }

    // Set calendar view
    const setCalendarView = (view) => {
      if (fullCalendar.value) {
        const calendarApi = fullCalendar.value.getApi();
        calendarApi.changeView(view);
        currentView.value = view;
      }
    }

    const calendarOptions = computed(() => ({
      plugins: [dayGridPlugin, interactionPlugin],
      initialView: 'dayGridMonth',
      editable: true,
      events: calendarEvents.value,
      eventClick: handleEventClick,
      eventDrop: handleEventDrop,
      height: 'auto',
      contentHeight: 'auto',
      fixedWeekCount: false,
      headerToolbar: {
        left: 'prev,next today',
        center: 'title',
        right: 'dayGridMonth,dayGridWeek'
      },
      buttonText: {
        today: 'Today',
        month: 'Month',
        week: 'Week'
      },
      dayMaxEvents: true,
      nowIndicator: true,
      themeSystem: 'bootstrap5',
      bootstrapFontAwesome: {
        prev: 'fa-chevron-left',
        next: 'fa-chevron-right',
        prevYear: 'fa-angle-double-left',
        nextYear: 'fa-angle-double-right',
        today: 'fa-calendar-day',
      },
      eventContent: (arg) => ({
        html: `
          <div class="fc-content p-2 rounded">
            <div class="fc-title fw-bold text-truncate">
              <i class="fas fa-receipt me-2"></i>
              ${arg.event.title}
            </div>
            <div class="fc-description small mt-1 d-flex justify-content-between align-items-center">
              <span class="fw-bold">
                <i class="fas fa-dollar-sign me-1"></i>
                ${arg.event.extendedProps.amount ? arg.event.extendedProps.amount.toFixed(2) : ''}
              </span>
              <span class="badge rounded-pill ${getBillingCycleBadgeClass(arg.event.extendedProps.billingCycle)}">
                <i class="${getBillingCycleIcon(arg.event.extendedProps.billingCycle)} me-1"></i>
                ${arg.event.extendedProps.billingCycle ? arg.event.extendedProps.billingCycle : ''}
              </span>
              ${updating.value && arg.event.id === editEvent.value.id ?
            '<div class="spinner-border spinner-border-sm text-white mt-1" role="status"></div>' : ''}
            </div>
          </div>
        `
      })
    }))

    onMounted(async () => {
      await fetchCategories()
      await fetchEvents()
    })

    return {
      loading,
      error,
      calendarOptions,
      editEvent,
      saveEventChanges,
      updating,
      closeModal,
      isModalOpen,
      refreshCalendar,
      fullCalendar,
      setCalendarView,
      currentView,
      getBillingCycleIcon,
      getBillingCycleBadgeClass,
      uniqueCategories,
      fetchCategories
    }
  }
}
</script>

<style>
.calendar-view {
  margin-top: 2rem;
  padding: 0 1rem;
}

.header-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-radius: 8px;
}

/* Make "Calendar View" heading black */
.header-container h1.mb-0.fw-bold {
  color: #000;
}

/* Header with gradient */
.bg-gradient {
  background: linear-gradient(135deg, var(--bs-primary) 0%, #7366ff 100%);
  color: white;
}

.dark-mode .bg-gradient {
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
}

/* Calendar icon in header */
.calendar-icon-wrapper {
  width: 48px;
  height: 48px;
  background-color: rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 0 15px rgba(var(--bs-primary-rgb), 0.5);
}

.calendar-icon-wrapper i {
  font-size: 24px;
}

/* Calendar container styling */
.calendar-container {
  background-color: white;
  padding: 1rem;
  border-radius: 0.5rem;
  transition: background-color 0.3s ease;
}

.dark-mode .calendar-container {
  background-color: var(--bs-dark-surface);
  color: #e0e0e0;
}

/* Color legend styling */
.calendar-legend {
  display: flex;
  align-items: center;
}

.legend-item {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
  color: #6c757d;
  transition: color 0.3s ease;
}

.dark-mode .legend-item {
  color: #adb5bd;
}

.color-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  display: inline-block;
  margin-right: 6px;
}

/* Loading animation */
.pulse-animation {
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0% {
    opacity: 0.6;
  }

  50% {
    opacity: 1;
  }

  100% {
    opacity: 0.6;
  }
}

/* FullCalendar custom styles */
:deep(.fc) {
  --fc-border-color: #dee2e6;
  --fc-today-bg-color: rgba(var(--bs-primary-rgb), 0.1);
  --fc-event-border-color: transparent;
  --fc-list-event-hover-bg-color: rgba(var(--bs-primary-rgb), 0.1);
  --fc-page-bg-color: transparent;
}

.dark-mode :deep(.fc) {
  --fc-border-color: #444;
  --fc-today-bg-color: rgba(var(--bs-primary-rgb), 0.2);
  --fc-event-border-color: transparent;
  --fc-list-event-hover-bg-color: rgba(var(--bs-primary-rgb), 0.2);
  --fc-page-bg-color: transparent;
}

.dark-mode :deep(.fc-day) {
  background-color: var(--bs-dark-surface);
}

.dark-mode :deep(.fc-col-header-cell) {
  background-color: #2d2d2d;
  color: #e0e0e0;
  border-color: #444;
}

.dark-mode :deep(.fc-daygrid-day-number) {
  color: #e0e0e0;
}

.dark-mode :deep(.fc-toolbar-title) {
  color: #e0e0e0;
}

:deep(.fc-event) {
  cursor: pointer;
  transition: all 0.2s ease;
  border-left-width: 4px !important;
  border-radius: 6px !important;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

:deep(.fc-event:hover) {
  transform: translateY(-2px);
  box-shadow: 0 8px 12px rgba(0, 0, 0, 0.15);
  z-index: 5 !important;
}

:deep(.fc-event-title) {
  padding: 2px 4px;
  font-weight: 500;
}

:deep(.fc-toolbar-title) {
  font-size: 1.5rem !important;
  font-weight: 600;
  color: var(--bs-primary);
}

:deep(.fc-today-button) {
  border-radius: 20px !important;
  padding-left: 15px !important;
  padding-right: 15px !important;
  text-transform: uppercase;
  font-size: 0.85rem !important;
  font-weight: 500 !important;
  letter-spacing: 0.5px;
}

:deep(.fc-button-primary) {
  background-color: var(--bs-primary) !important;
  border-color: var(--bs-primary) !important;
  box-shadow: 0 2px 4px rgba(var(--bs-primary-rgb), 0.4) !important;
  border-radius: 6px !important;
  padding: 7px 12px !important;
  font-weight: 500 !important;
}

:deep(.fc-button-primary:not(:disabled):hover) {
  background-color: var(--bs-primary-darker, #0257d5) !important;
  border-color: var(--bs-primary-darker, #0257d5) !important;
  box-shadow: 0 4px 8px rgba(var(--bs-primary-rgb), 0.5) !important;
  transform: translateY(-1px);
}

:deep(.fc-button-primary:disabled) {
  background-color: rgba(var(--bs-primary-rgb), 0.65) !important;
  border-color: rgba(var(--bs-primary-rgb), 0.65) !important;
  opacity: 0.8;
}

:deep(.fc-prev-button),
:deep(.fc-next-button) {
  background-color: white !important;
  border-color: #e9ecef !important;
  color: var(--bs-primary) !important;
  width: 35px !important;
  height: 35px !important;
  border-radius: 50% !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 0 !important;
  font-size: 0.8rem !important;
  transition: background-color 0.3s ease, border-color 0.3s ease !important;
}

.dark-mode :deep(.fc-prev-button),
.dark-mode :deep(.fc-next-button) {
  background-color: var(--bs-dark-surface) !important;
  border-color: var(--bs-dark-border) !important;
  color: var(--bs-dark-text) !important;
}

:deep(.fc-day-other) {
  background-color: #f8f9fa;
  transition: background-color 0.3s ease;
}

.dark-mode :deep(.fc-day-other) {
  background-color: rgba(0, 0, 0, 0.3);
}

:deep(.fc-day-today) {
  background-color: rgba(var(--bs-primary-rgb), 0.1) !important;
}

:deep(.fc-day-today .fc-daygrid-day-number) {
  background-color: var(--bs-primary);
  color: white;
  width: 26px;
  height: 26px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 5px;
  margin-top: 5px;
  font-weight: 600;
}

:deep(.fc-daygrid-day-number) {
  font-weight: 500;
  color: #495057;
  font-size: 0.9rem;
  transition: color 0.3s ease;
}

.dark-mode :deep(.fc-daygrid-day-number) {
  color: var(--bs-dark-text);
}

:deep(.fc-daygrid-day-top) {
  justify-content: flex-end;
  padding: 5px 8px;
}

:deep(.fc-col-header-cell) {
  background-color: rgba(var(--bs-primary-rgb), 0.05);
  padding: 10px 0;
  transition: background-color 0.3s ease;
}

.dark-mode :deep(.fc-col-header-cell) {
  background-color: rgba(0, 0, 0, 0.2);
}

:deep(.fc-col-header-cell-cushion) {
  color: var(--bs-primary);
  font-weight: 600;
  text-decoration: none !important;
  text-transform: uppercase;
  font-size: 0.8rem;
  letter-spacing: 0.5px;
}

:deep(.fc-theme-bootstrap5 .fc-scrollgrid) {
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid #e9ecef;
}

/* Modal styles */
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1040;
}

.modal {
  z-index: 1050;
}

.modal.fade.show {
  background-color: rgba(0, 0, 0, 0.4);
}

.modal-header.bg-gradient {
  background: linear-gradient(135deg, var(--bs-primary) 0%, #7366ff 100%);
}

.subscription-details {
  border-left: 4px solid var(--bs-primary);
  background-color: #f8f9fa;
  transition: background-color 0.3s ease, border-color 0.3s ease;
}

.dark-mode .subscription-details {
  background-color: rgba(255, 255, 255, 0.05);
  border-left: 4px solid rgb(var(--bs-primary-rgb));
}

@media (max-width: 768px) {
  :deep(.fc-toolbar) {
    flex-direction: column;
    gap: 1rem;
  }

  :deep(.fc-toolbar-title) {
    font-size: 1.2rem !important;
  }

  .header-container {
    flex-direction: column;
    align-items: flex-start;
  }

  .header-container .btn-group {
    margin-top: 1rem;
    align-self: flex-end;
  }

  :deep(.fc-daygrid-day) {
    min-height: 80px;
  }

  :deep(.fc-daygrid-day-events) {
    margin-bottom: 5px !important;
  }

  :deep(.fc-event) {
    margin-left: 4px !important;
    margin-right: 4px !important;
  }
}

/* Hover effects for interactive elements */
.btn-outline-primary:hover {
  box-shadow: 0 4px 8px rgba(var(--bs-primary-rgb), 0.25);
  transform: translateY(-1px);
}

.rounded-pill {
  transition: all 0.2s ease;
}

/* Dark mode card styles */
.dark-card {
  background-color: var(--bs-dark-surface) !important;
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-card-header {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-card-body {
  background-color: var(--bs-dark-surface) !important;
  color: #e0e0e0 !important;
}

/* Dark mode modal styles */
.dark-modal-content {
  background-color: var(--bs-dark-surface) !important;
  color: #e0e0e0 !important;
  border-color: #444 !important;
}

.dark-modal-header {
  border-color: #444 !important;
}

.dark-modal-body {
  background-color: var(--bs-dark-surface) !important;
  color: #e0e0e0 !important;
}

.dark-modal-footer {
  background-color: #2d2d2d !important;
  border-color: #444 !important;
  color: #e0e0e0 !important;
}

.dark-subscription-details {
  background-color: #2d2d2d !important;
  color: #e0e0e0 !important;
}

.dark-mode .legend-item {
  color: #e0e0e0 !important;
}

.rounded-pill:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

/* Make the input fields look more modern */
.form-control:focus {
  box-shadow: 0 0 0 0.25rem rgba(var(--bs-primary-rgb), 0.25);
  border-color: rgba(var(--bs-primary-rgb), 0.5);
}

.input-group-text {
  color: var(--bs-primary);
}

/* Style placeholders in form controls */
::placeholder {
  opacity: 0.5;
}
</style>
