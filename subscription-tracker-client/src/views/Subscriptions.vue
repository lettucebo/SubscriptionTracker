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
          <th>Name</th>
          <th>Billing Cycle</th>
          <th>Amount (Cycle)</th>
          <th>Effective Monthly</th>
          <th>Date Range</th>
          <th>Category</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="sub in subscriptions" :key="sub.id">
          <td>{{ sub.name }}</td>
          <td class="text-capitalize">{{ sub.billingCycle }}</td>
          <td>${{ sub.amount.toFixed(2) }} ({{ sub.billingCycle }})</td>
          <td>${{ sub.effectiveMonthlyPrice?.toFixed(2) }}</td>
          <td>
            {{ new Date(sub.startDate).toLocaleDateString() }} - 
            {{ sub.endDate ? new Date(sub.endDate).toLocaleDateString() : 'Active' }}
          </td>
          <td>{{ sub.category }}</td>
          <td :class="{'text-danger': sub.remainingDays < 7}">
            {{ sub.remainingDays }} days
          </td>
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
