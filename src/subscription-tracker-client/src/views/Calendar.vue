<template>
  <div class="calendar-view">
    <div class="header-container mb-4">
      <div class="d-flex align-items-center">
        <i class="fas fa-calendar-alt text-primary me-3 fs-2"></i>
        <h1 class="mb-0">Calendar View</h1>
      </div>
      <div class="btn-group">
        <button @click="refreshCalendar" class="btn btn-outline-primary" :disabled="loading">
          <i class="fas fa-sync-alt" :class="{'fa-spin': loading}"></i>
          <span class="ms-2 d-none d-md-inline">Refresh</span>
        </button>
      </div>
    </div>

    <div v-if="loading" class="text-center my-5 py-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-3 text-muted">Loading your subscriptions...</p>
    </div>

    <div v-else-if="error" class="alert alert-danger shadow-sm" role="alert">
      <i class="fas fa-exclamation-circle me-2"></i>
      {{ error }}
    </div>

    <div v-else>
      <div class="card shadow-sm border-0">
        <div class="card-body p-0 p-md-3">
          <FullCalendar 
            :options="calendarOptions"
            class="calendar-container" 
          />
        </div>
      </div>
    </div>

    <!-- Edit Modal -->
    <div class="modal fade" :class="{ show: isModalOpen, 'd-block': isModalOpen }" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content shadow">
          <div class="modal-header bg-light">
            <h5 class="modal-title">
              <i class="fas fa-edit me-2 text-primary"></i>
              Edit Subscription
            </h5>
            <button type="button" class="btn-close" @click="closeModal"></button>
          </div>
          <div class="modal-body">
            <div v-if="error" class="alert alert-danger mb-3">
              <i class="fas fa-exclamation-triangle me-2"></i>
              {{ error }}
            </div>
            <div class="mb-3">
              <label class="form-label">
                <i class="far fa-calendar me-2 text-muted"></i>
                Start Date
              </label>
              <div class="input-group">
                <span class="input-group-text"><i class="fas fa-calendar-day"></i></span>
                <input type="date" class="form-control" v-model="editEvent.startDate" :disabled="updating">
              </div>
              <small class="form-text text-muted mt-1">
                <i class="fas fa-info-circle me-1"></i>
                Changing this date will affect future billing cycles
              </small>
            </div>
          </div>
          <div class="modal-footer bg-light">
            <button type="button" class="btn btn-outline-secondary" @click="closeModal" :disabled="updating">
              <i class="fas fa-times me-1"></i>
              Cancel
            </button>
            <button type="button" class="btn btn-primary" @click="saveEventChanges" :disabled="updating">
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

    const refreshCalendar = () => {
      fetchEvents()
    }

    const getEventColor = (category) => {
      const colors = {
        'Entertainment': '#ff4d4d',
        'Utilities': '#4d94ff',
        'Software': '#47d147',
        'Other': '#ff944d'
      }
      return colors[category] || '#808080'
    }

    const generateRecurringEvents = (subscription) => {
      const events = [];
      const startDate = new Date(subscription.startDate);
      const endDate = new Date();
      endDate.setMonth(endDate.getMonth() + 12); // Show next 12 months

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
            isRecurring: true
          },
          backgroundColor: getEventColor(subscription.category?.name),
          borderColor: getEventColor(subscription.category?.name)
        });

        // Add months based on billing cycle
        switch (subscription.billingCycle) {
          case 'Monthly'.toLowerCase():
            currentDate.setMonth(currentDate.getMonth() + 1);
            break;
          case 'Quarterly'.toLowerCase():
            currentDate.setMonth(currentDate.getMonth() + 3);
            break;
          case 'Yearly'.toLowerCase():
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

    const calendarOptions = computed(() => ({
      plugins: [dayGridPlugin, interactionPlugin],
      initialView: 'dayGridMonth',
      editable: true,
      events: calendarEvents.value,
      eventClick: handleEventClick,
      eventDrop: handleEventDrop,
      headerToolbar: {
        left: 'prev,next today',
        center: 'title',
        right: 'dayGridMonth,dayGridWeek'
      },
      eventContent: (arg) => ({
        html: `
          <div class="fc-content p-1">
            <div class="fc-title fw-bold text-truncate">
              <i class="fas fa-receipt me-1"></i>
              ${arg.event.title}
            </div>
            <div class="fc-description small mt-1 d-flex justify-content-between align-items-center">
              <span>
                ${arg.event.extendedProps.amount ? '$' + arg.event.extendedProps.amount.toFixed(2) : ''}
              </span>
              <span class="badge rounded-pill bg-light text-dark border">
                ${arg.event.extendedProps.billingCycle ? arg.event.extendedProps.billingCycle : ''}
                ${arg.event.extendedProps.billingCycle === 'Monthly' ? '<i class="fas fa-sync-alt ms-1"></i>' : ''}
                ${arg.event.extendedProps.billingCycle === 'Quarterly' ? '<i class="fas fa-calendar-alt ms-1"></i>' : ''}
                ${arg.event.extendedProps.billingCycle === 'Annually' ? '<i class="fas fa-calendar-check ms-1"></i>' : ''}
              </span>
              ${updating.value && arg.event.id === editEvent.value.id ? '<div class="spinner-border spinner-border-sm text-white mt-1" role="status"></div>' : ''}
            </div>
          </div>
        `
      })
    }))

    onMounted(() => {
      fetchEvents()
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
      refreshCalendar
    }
  }
}
</script>

<style scoped>
.calendar-view {
  margin-top: 2rem;
  padding: 0 1rem;
}

.header-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.calendar-container {
  background-color: white;
  padding: 1rem;
  border-radius: 0.5rem;
}

/* FullCalendar custom styles */
:deep(.fc) {
  --fc-border-color: #dee2e6;
  --fc-today-bg-color: rgba(var(--bs-primary-rgb), 0.1);
}

:deep(.fc-event) {
  cursor: pointer;
  transition: transform 0.15s ease;
  border-left-width: 4px !important;
}

:deep(.fc-event:hover) {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  z-index: 5 !important;
}

:deep(.fc-event-title) {
  padding: 2px 4px;
  font-weight: 500;
}

:deep(.fc-toolbar-title) {
  font-size: 1.5rem !important;
  font-weight: 600;
}

:deep(.fc-button-primary) {
  background-color: var(--bs-primary) !important;
  border-color: var(--bs-primary) !important;
}

:deep(.fc-button-primary:not(:disabled):hover) {
  background-color: var(--bs-primary-darker, #0257d5) !important;
  border-color: var(--bs-primary-darker, #0257d5) !important;
}

:deep(.fc-button-primary:disabled) {
  background-color: rgba(var(--bs-primary-rgb), 0.65) !important;
  border-color: rgba(var(--bs-primary-rgb), 0.65) !important;
}

:deep(.fc-day-today) {
  background-color: rgba(var(--bs-primary-rgb), 0.1) !important;
}

:deep(.fc-day-today .fc-daygrid-day-number) {
  background-color: var(--bs-primary);
  color: white;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 5px;
  margin-top: 5px;
}

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
}
</style>
