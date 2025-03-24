<template>
  <div class="report-page">
    <h1>Report</h1>
    <div class="mb-3">
      <label for="categoryFilter" class="form-label">Filter by Category:</label>
      <select id="categoryFilter" v-model="selectedCategory" class="form-select">
        <option value="">All</option>
        <option v-for="category in uniqueCategories" :key="category" :value="category">
          {{ category }}
        </option>
      </select>
    </div>
    <table class="table table-striped">
      <thead>
        <tr>
          <th>ID</th>
          <th>Name</th>
          <th>Fee</th>
          <th>Payment Date</th>
          <th>Category</th>
          <th>Remaining Days</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="sub in filteredSubscriptions" :key="sub.id">
          <td>{{ sub.id }}</td>
          <td>{{ sub.name }}</td>
          <td>{{ sub.fee }}</td>
          <td>{{ new Date(sub.paymentDate).toLocaleDateString() }}</td>
          <td>{{ sub.category }}</td>
          <td>{{ sub.remainingDays }}</td>
        </tr>
      </tbody>
    </table>
    <div class="mt-3">
      <h4>Total Fee: {{ totalFee.toFixed(2) }}</h4>
    </div>
  </div>
</template>

<script>
import axios from 'axios'
import { ref, onMounted, computed } from 'vue'

export default {
  name: "ReportPage",
  setup() {
    const subscriptions = ref([])
    const selectedCategory = ref("")

    const fetchSubscriptions = async () => {
      try {
        const response = await axios.get("https://localhost:5001/api/subscription")
        subscriptions.value = response.data
      } catch (error) {
        console.error("Error fetching subscriptions:", error)
      }
    }

    onMounted(() => {
      fetchSubscriptions()
    })

    // Computed: Filter subscriptions by category and sort by remaining days.
    const filteredSubscriptions = computed(() => {
      let subs = subscriptions.value
      if (selectedCategory.value) {
        subs = subs.filter(sub => sub.category === selectedCategory.value)
      }
      return subs.sort((a, b) => a.remainingDays - b.remainingDays)
    })

    // Computed: Unique categories.
    const uniqueCategories = computed(() => {
      const categories = subscriptions.value.map(sub => sub.category)
      return [...new Set(categories)]
    })

    // Computed: Total fee for filtered subscriptions.
    const totalFee = computed(() => {
      return filteredSubscriptions.value.reduce((sum, sub) => sum + sub.fee, 0)
    })

    return {
      selectedCategory,
      filteredSubscriptions,
      uniqueCategories,
      totalFee
    }
  }
}
</script>

<style scoped>
.report-page {
  margin-top: 2rem;
}
</style>
