<template>
  <div class="subscriptions">
    <div class="header-container">
      <h1>Subscriptions</h1>
      <router-link to="/subscription-form" class="btn btn-primary mb-3">
        New Subscription
      </router-link>
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
          <th>Actions</th>
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
          <td>
            <router-link :to="`/subscription-form/${sub.id}`" class="btn btn-sm btn-outline-primary me-2">
              Edit
            </router-link>
            <button @click="deleteSubscription(sub.id)" class="btn btn-sm btn-outline-danger">
              Delete
            </button>
          </td>
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
      subscriptions: [],
      router: this.$router
    }
  },
  created() {
    this.fetchSubscriptions()
  },
  methods: {
    async fetchSubscriptions() {
      try {
        const response = await axios.get(`${config.baseUrl}/api/subscription`);
        this.subscriptions = response.data;
      } catch (error) {
        console.error("Error fetching subscriptions:", error);
      }
    },
    async deleteSubscription(id) {
      if (confirm('Are you sure you want to delete this subscription?')) {
        try {
          await axios.delete(`${config.baseUrl}/api/subscription/${id}`);
          this.fetchSubscriptions();
        } catch (error) {
          console.error("Error deleting subscription:", error);
        }
      }
    }
  }
}
</script>

<style scoped>
.subscriptions {
  margin-top: 2rem;
}
.header-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
</style>
