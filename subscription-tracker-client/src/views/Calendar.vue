<template>
  <div class="calendar-view">
    <div class="header-container mb-4">
      <h1>Calendar View</h1>
    </div>

    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="error" class="alert alert-danger" role="alert">
      {{ error }}
    </div>

    <div v-else>
      <FullCalendar 
        :options="calendarOptions"
        class="calendar-container" 
      />
    </div>

    <!-- Edit Modal -->
    <div class="modal fade" id="editModal" tabindex="-1" ref="editModal">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Edit Subscription</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div v-if="error" class="alert alert-danger mb-3">
              {{ error }}
            </div>
            <div class="mb-3">
              <label class="form-label">Start Date</label>
              <input type="date" class="form-control" v-model="editEvent.startDate" :disabled="updating">
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="updating">Cancel</button>
            <button type="button" class="btn btn-primary" @click="saveEventChanges" :disabled="updating">
              <span v-if="updating" class="spinner-border spinner-border-sm me-1" role="status"></span>
              {{ updating ? 'Saving...' : 'Save changes' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { config } from '@/config'
import { Modal } from 'bootstrap'
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
    const editModal = ref(null)
    const editEvent = ref({
      id: null,
      startDate: ''
    })
    const updating = ref(false)

    const fetchEvents = async () => {
      loading.value = true
      error.value = null
      try {
        const response = await axios.get(`${config.baseUrl}/api/subscription`)
        subscriptions.value = response.data
      } catch (err) {
        error.value = 'Failed to load subscriptions'
        console.error('Error fetching events:', err)
      } finally {
        loading.value = false
      }
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

    const calendarEvents = computed(() => 
      subscriptions.value.map(sub => ({
        id: sub.id,
        title: sub.name,
        start: sub.startDate,
        allDay: true,
        extendedProps: {
          amount: sub.amount,
          billingCycle: sub.billingCycle
        },
        backgroundColor: getEventColor(sub.category?.name),
        borderColor: getEventColor(sub.category?.name)
      }))
    )

    const handleEventClick = (info) => {
      editEvent.value = {
        id: info.event.id,
        startDate: info.event.start.toISOString().split('T')[0]
      }
      error.value = null
      editModal.value.show()
    }

    const handleEventDrop = async (info) => {
      updating.value = true
      editEvent.value.id = info.event.id

      try {
        await axios.put(`${config.baseUrl}/api/subscription/${info.event.id}/dates`, {
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
        editModal.value.hide()
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
          <div class="fc-content">
            <div class="fc-title">${arg.event.title}</div>
            <div class="fc-description small">
              ${arg.event.extendedProps.amount ? '$' + arg.event.extendedProps.amount.toFixed(2) : ''}
              ${updating.value && arg.event.id === editEvent.value.id ? ' (Updating...)' : ''}
            </div>
          </div>
        `
      })
    }))

    onMounted(() => {
      editModal.value = new Modal(document.getElementById('editModal'))
      fetchEvents()
    })

    return {
      loading,
      error,
      calendarOptions,
      editEvent,
      editModal,
      saveEventChanges,
      updating
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
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
}

:deep(.fc-event) {
  cursor: pointer;
}

:deep(.fc-event-title) {
  padding: 2px 4px;
  font-weight: 500;
}

:deep(.fc-toolbar-title) {
  font-size: 1.5rem !important;
}

@media (max-width: 768px) {
  :deep(.fc-toolbar) {
    flex-direction: column;
    gap: 1rem;
  }

  :deep(.fc-toolbar-title) {
    font-size: 1.2rem !important;
  }
}
</style>
