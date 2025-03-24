<template>
  <div class="subscription-form">
    <h1>{{ isEdit ? "Edit Subscription" : "Add Subscription" }}</h1>
    <form @submit.prevent="submitForm">
      <div class="mb-3">
        <label for="name" class="form-label">Name</label>
        <input type="text" id="name" class="form-control" v-model="subscription.name" required>
      </div>
      
      <div class="mb-3">
        <label for="billingCycle" class="form-label">Billing Cycle</label>
        <select id="billingCycle" class="form-select" v-model="subscription.billingCycle" required>
          <option value="monthly">Monthly</option>
          <option value="yearly">Yearly</option>
        </select>
      </div>

      <div class="mb-3">
        <label for="amount" class="form-label">Amount</label>
        <input type="number" id="amount" class="form-control" 
               v-model.number="subscription.amount" 
               step="0.01" 
               required>
      </div>

      <div class="mb-3" v-if="subscription.billingCycle === 'yearly'">
        <label for="discountRate" class="form-label">Discount Rate (%)</label>
        <input type="number" id="discountRate" class="form-control"
               v-model.number="discountRatePercentage"
               step="1"
               min="0"
               max="100">
          <div class="form-text">Effective monthly price: {{ effectiveMonthlyPrice }}</div>
      </div>

      <div class="mb-3">
        <label for="startDate" class="form-label">Start Date</label>
        <input type="date" id="startDate" class="form-control" v-model="subscription.startDate" required>
      </div>

      <div class="mb-3">
        <label for="category" class="form-label">Category</label>
        <input type="text" id="category" class="form-control" v-model="subscription.category">
      </div>

      <button type="submit" class="btn btn-primary">{{ isEdit ? "Update" : "Create" }}</button>
    </form>
  </div>
</template>

<script>
import axios from 'axios'
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { config } from '@/config'

export default {
  name: "SubscriptionFormPage",
  setup() {
    const subscription = ref({
      name: '',
      fee: 0,
      billingCycle: 'monthly',
      amount: 0,
      discountRate: 0,
      startDate: '',
      endDate: null,
      category: ''
    })
    const isEdit = ref(false)
    const route = useRoute()
    const router = useRouter()

    onMounted(async () => {
      if (route.params.id) {
        isEdit.value = true
        try {
          const response = await axios.get(`${config.baseUrl}/api/subscription/${route.params.id}`)
          subscription.value = response.data
          if (subscription.value.startDate) {
            // Format date string to YYYY-MM-DD for input field
            subscription.value.startDate = subscription.value.startDate.substr(0, 10)
          }
        } catch (error) {
          console.error("Error fetching subscription:", error)
        }
      }
    })

    const effectiveMonthlyPrice = computed(() => {
      if (subscription.value.billingCycle === 'yearly') {
        return ((subscription.value.amount * (1 - (subscription.value.discountRate || 0))) / 12).toFixed(2)
      }
      return subscription.value.amount.toFixed(2)
    })

    const discountRatePercentage = computed({
      get: () => (subscription.value.discountRate * 100) || 0,
      set: (val) => subscription.value.discountRate = val / 100
    })

    const submitForm = async () => {
      try {
        if (isEdit.value) {
          await axios.put(`${config.baseUrl}/api/subscription/${subscription.value.id}`, subscription.value)
        } else {
          await axios.post(`${config.baseUrl}/api/subscription`, subscription.value)
        }
        router.push('/subscriptions')
      } catch (error) {
        console.error("Error saving subscription:", error)
      }
    }

    return { 
      subscription, 
      isEdit, 
      submitForm, 
      effectiveMonthlyPrice,
      discountRatePercentage 
    }
  }
}
</script>

<style scoped>
.subscription-form {
  margin-top: 2rem;
}
</style>
