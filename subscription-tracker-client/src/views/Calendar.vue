<template>
  <div class="calendar-page">
    <h1>Calendar</h1>
    <vue-calendar-3 :events="calendarEvents" />
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import VueCalendar from 'vue-calendar-3'

export default {
  name: "CalendarPage",
  components: {
    'vue-calendar-3': VueCalendar
  },
  setup() {
    const calendarEvents = ref([])

    const fetchSubscriptions = async () => {
      try {
        const response = await axios.get("https://localhost:5001/api/subscription")
        // Map each subscription to a calendar event with title and date.
        calendarEvents.value = response.data.map(sub => ({
          title: sub.name,
          date: new Date(sub.paymentDate).toISOString().split('T')[0]
        }))
      } catch (error) {
        console.error("Error fetching subscriptions for calendar:", error)
      }
    }

    onMounted(() => {
      fetchSubscriptions()
    })

    return { calendarEvents }
  }
}
</script>

<style scoped>
.calendar-page {
  margin-top: 2rem;
}
</style>
