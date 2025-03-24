<template>
  <div class="subscriptions">
    <h1>Subscriptions</h1>
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
        <tr v-for="sub in subscriptions" :key="sub.id">
          <td>{{ sub.id }}</td>
          <td>{{ sub.name }}</td>
          <td>{{ sub.fee }}</td>
          <td>{{ new Date(sub.paymentDate).toLocaleDateString() }}</td>
          <td>{{ sub.category }}</td>
          <td>{{ sub.remainingDays }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import axios from 'axios'
import { config } from '@/config'

export default {
  name: "SubscriptionsPage",
  data() {
    return {
      subscriptions: []
    }
  },
  created() {
    this.fetchSubscriptions()
  },
  methods: {
    async fetchSubscriptions() {
      try {
        // Adjust the API URL as needed.
        const response = await axios.get(`${config.baseUrl}/api/subscription`);
        this.subscriptions = response.data;
      } catch (error) {
        console.error("Error fetching subscriptions:", error);
      }
    }
  }
}
</script>

<style scoped>
.subscriptions {
  margin-top: 2rem;
}
</style>
