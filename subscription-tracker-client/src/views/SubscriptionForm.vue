<template>
  <div class="subscription-form">
    <h1>{{ isEdit ? "Edit Subscription" : "Add Subscription" }}</h1>
    <form @submit.prevent="submitForm">
      <div class="mb-3">
        <label for="name" class="form-label">Name</label>
        <input type="text" id="name" class="form-control" v-model="subscription.name" required>
      </div>
      <div class="mb-3">
        <label for="fee" class="form-label">Fee</label>
        <input type="number" id="fee" class="form-control" v-model.number="subscription.fee" required>
      </div>
      <div class="mb-3">
        <label for="paymentDate" class="form-label">Payment Date</label>
        <input type="date" id="paymentDate" class="form-control" v-model="subscription.paymentDate" required>
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
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

export default {
  name: "SubscriptionFormPage",
  setup() {
    const subscription = ref({
      name: '',
      fee: 0,
      paymentDate: '',
      category: ''
    })
    const isEdit = ref(false)
    const route = useRoute()
    const router = useRouter()

    onMounted(async () => {
      if (route.params.id) {
        isEdit.value = true
        try {
          const response = await axios.get(`https://localhost:5001/api/subscription/${route.params.id}`)
          subscription.value = response.data
          if (subscription.value.paymentDate) {
            // Format date string to YYYY-MM-DD for input field
            subscription.value.paymentDate = subscription.value.paymentDate.substr(0, 10)
          }
        } catch (error) {
          console.error("Error fetching subscription:", error)
        }
      }
    })

    const submitForm = async () => {
      try {
        if (isEdit.value) {
          await axios.put(`https://localhost:5001/api/subscription/${subscription.value.id}`, subscription.value)
        } else {
          await axios.post("https://localhost:5001/api/subscription", subscription.value)
        }
        router.push('/subscriptions')
      } catch (error) {
        console.error("Error saving subscription:", error)
      }
    }

    return { subscription, isEdit, submitForm }
  }
}
</script>

<style scoped>
.subscription-form {
  margin-top: 2rem;
}
</style>
