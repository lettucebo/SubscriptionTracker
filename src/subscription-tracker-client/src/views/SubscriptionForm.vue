<template>
  <div class="subscription-form">
    <div class="form-container">
      <div class="header-section">
        <h1>{{ isEdit ? "Edit Subscription" : "New Subscription" }}</h1>
        <p class="text-muted">{{ isEdit ? "Update your subscription details" : "Add a new subscription to track" }}</p>
      </div>

      <form @submit.prevent="submitForm" class="needs-validation" novalidate>
        <div class="card form-card">
          <div class="card-body">
            <div class="row g-4">
              <div class="col-md-6">
                <div class="form-group">
                  <label for="name" class="form-label">Name <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text">
                    <i class="fas fa-tag"></i>
                    </span>
                    <input
                      type="text"
                      id="name"
                      class="form-control"
                      v-model="subscription.name"
                      required
                      :class="{ 'is-invalid': showError && !subscription.name }"
                    >
                  </div>
                  <div class="invalid-feedback" v-if="showError && !subscription.name">
                    Please enter a subscription name
                  </div>
                </div>
              </div>

              <div class="col-md-6">
                <div class="form-group">
                  <label for="category" class="form-label">Category <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text">
                      <i class="fas fa-folder-open"></i>
                    </span>
                    <select
                      id="category"
                      class="form-select"
                      v-model="subscription.categoryId"
                      required
                      :class="{ 'is-invalid': showError && !subscription.categoryId }"
                    >
                      <option :value="null">Select a category</option>
                      <option v-for="category in categories" :key="category.id" :value="category.id">
                        {{ category.name }}
                      </option>
                    </select>
                  </div>
                  <div class="invalid-feedback" v-if="showError && !subscription.categoryId">
                    Please select a category
                  </div>
                </div>
              </div>

              <div class="col-md-6">
                <div class="form-group">
                  <label for="amount" class="form-label">Amount <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text">$</span>
                    <input
                      type="number"
                      id="amount"
                      class="form-control"
                      v-model.number="subscription.amount"
                      step="0.01"
                      required
                      :class="{ 'is-invalid': showError && !subscription.amount }"
                    >
                  </div>
                  <div class="invalid-feedback" v-if="showError && !subscription.amount">
                    Please enter an amount
                  </div>
                </div>
              </div>

              <div class="col-md-6">
                <div class="form-group">
                  <label for="billingCycle" class="form-label">Billing Cycle <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text">
                      <i class="bi bi-calendar"></i>
                    </span>
                    <select
                      id="billingCycle"
                      class="form-select"
                      v-model="subscription.billingCycle"
                      required
                      :class="{ 'is-invalid': showError && !subscription.billingCycle }"
                    >
                      <option value="">Select billing cycle</option>
                      <option value="monthly">Monthly</option>
                      <option value="yearly">Yearly</option>
                    </select>
                  </div>
                  <div class="invalid-feedback" v-if="showError && !subscription.billingCycle">
                    Please select a billing cycle
                  </div>
                </div>
              </div>

              <div class="col-md-6">
                <div class="form-group">
                  <label for="startDate" class="form-label">Start Date <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text">
                      <i class="bi bi-calendar-date"></i>
                    </span>
                    <input
                      type="date"
                      id="startDate"
                      class="form-control"
                      v-model="subscription.startDate"
                      required
                      :class="{ 'is-invalid': showError && !subscription.startDate }"
                    >
                  </div>
                  <div class="invalid-feedback" v-if="showError && !subscription.startDate">
                    Please select a start date
                  </div>
                </div>
              </div>

              <div class="col-md-6">
                <div class="form-group">
                  <label for="endDate" class="form-label">End Date</label>
                  <div class="input-group">
                    <span class="input-group-text">
                      <i class="bi bi-calendar-x"></i>
                    </span>
                    <input
                      type="date"
                      id="endDate"
                      class="form-control"
                      v-model="subscription.endDate"
                      :min="subscription.startDate"
                    >
                  </div>
                  <div class="form-text">Leave empty for ongoing subscriptions</div>
                </div>
              </div>

              <div class="col-md-6" v-if="subscription.billingCycle === 'yearly'">
                <div class="form-group">
                  <label for="discountRate" class="form-label">Yearly Discount</label>
                  <div class="input-group">
                    <input
                      type="number"
                      id="discountRate"
                      class="form-control"
                      v-model.number="discountRatePercentage"
                      step="1"
                      min="0"
                      max="100"
                    >
                    <span class="input-group-text">%</span>
                  </div>
                  <div class="form-text">
                    Effective monthly price: ${{ effectiveMonthlyPrice }}
                  </div>
                </div>
              </div>
            </div>

            <div class="form-actions mt-4">
              <router-link to="/subscriptions" class="btn btn-outline-secondary me-2">
                Cancel
              </router-link>
              <button type="submit" class="btn btn-primary">
                <i class="fas" :class="isEdit ? 'fa-check-circle' : 'fa-plus-circle'"></i>
                {{ isEdit ? "Update Subscription" : "Create Subscription" }}
              </button>
            </div>
          </div>
        </div>
      </form>
    </div>
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
      billingCycle: '',
      amount: null,
      discountRate: 0,
      startDate: '',
      endDate: null,
      categoryId: null
    })
    const isEdit = ref(false)
    const showError = ref(false)
    const route = useRoute()
    const router = useRouter()
    const categories = ref([])

    const loadCategories = async (subscriptionCategoryId = null) => {
      try {
        const response = await axios.get(`${config.baseUrl}/api/category`)
        categories.value = response.data

        // If editing and the subscription's category was soft-deleted
        if (subscriptionCategoryId && !categories.value.find(c => c.id === subscriptionCategoryId)) {
          try {
            const categoryResponse = await axios.get(`${config.baseUrl}/api/category/${subscriptionCategoryId}`)
            if (categoryResponse.data) {
              categories.value = [...categories.value, categoryResponse.data]
            }
          } catch (error) {
            console.error("Error fetching soft-deleted category:", error)
          }
        }
      } catch (error) {
        console.error("Error loading categories:", error)
      }
    }

    onMounted(async () => {
      if (route.params.id) {
        isEdit.value = true
        try {
          const response = await axios.get(`${config.baseUrl}/api/subscription/${route.params.id}`)
          subscription.value = {
            ...response.data,
            startDate: response.data.startDate?.substr(0, 10),
            endDate: response.data.endDate?.substr(0, 10)
          }
          await loadCategories(subscription.value.categoryId)
        } catch (error) {
          console.error("Error fetching subscription:", error)
          showError.value = true
        }
      } else {
        await loadCategories()
      }
    })

    const effectiveMonthlyPrice = computed(() => {
      if (!subscription.value.amount) return '0.00';

      switch (subscription.value.billingCycle?.toLowerCase()) {
        case 'yearly':
          return (Math.round((subscription.value.amount * (1 - (subscription.value.discountRate || 0))) / 12 * 100) / 100).toFixed(2);
        case 'monthly':
          return subscription.value.amount.toFixed(2);
        default:
          return '0.00';
      }
    })

    const discountRatePercentage = computed({
      get: () => (subscription.value.discountRate * 100) || 0,
      set: (val) => subscription.value.discountRate = val / 100
    })

    /**
     * Handle form submission
     * @async
     * @returns {Promise<void>}
     */
    const submitForm = async () => {
      showError.value = true

      // Basic form validation
      if (!subscription.value.name ||
          !subscription.value.categoryId ||
          !subscription.value.amount ||
          !subscription.value.billingCycle ||
          !subscription.value.startDate) {
        return
      }

      try {
        if (isEdit.value) {
          await axios.put(`${config.baseUrl}/api/subscription/${subscription.value.id}`, subscription.value)
        } else {
          await axios.post(`${config.baseUrl}/api/subscription`, subscription.value)
        }
        router.push('/subscriptions')
      } catch (error) {
        console.error("Error saving subscription:", error)
        alert('Failed to save subscription. Please try again.')
      }
    }

    return {
      subscription,
      categories,
      isEdit,
      showError,
      submitForm,
      effectiveMonthlyPrice,
      discountRatePercentage,
      loadCategories
    }
  }
}
</script>

<style scoped>
.subscription-form {
  padding: 2rem 1rem;
}

.form-container {
  max-width: 1200px;
  margin: 0 auto;
}

.header-section {
  text-align: center;
  margin-bottom: 2rem;
}

.header-section h1 {
  color: #2c3e50;
  margin-bottom: 0.5rem;
  transition: color 0.3s ease;
}

.dark-mode .header-section h1 {
  color: var(--bs-dark-text);
}

.form-card {
  background: white;
  border-radius: 1rem;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.08);
  transition: background-color 0.3s ease, box-shadow 0.3s ease;
}

.dark-mode .form-card {
  background-color: var(--bs-dark-surface);
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.2);
}

.form-group {
  margin-bottom: 0.5rem;
}

.form-label {
  font-weight: 500;
  color: #2c3e50;
  margin-bottom: 0.5rem;
  transition: color 0.3s ease;
}

.dark-mode .form-label {
  color: #e0e0e0;
}

.input-group {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);
  border-radius: 0.375rem;
}

.input-group-text {
  background-color: #f8f9fa;
  border-color: #dee2e6;
  transition: background-color 0.3s ease, border-color 0.3s ease, color 0.3s ease;
}

.dark-mode .input-group-text {
  background-color: #2d2d2d;
  border-color: #444;
  color: #e0e0e0;
}

.form-control, .form-select {
  border-color: #dee2e6;
}

.form-control:focus, .form-select:focus {
  border-color: #86b7fe;
  box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
}

.form-text {
  font-size: 0.875rem;
  color: #6c757d;
  margin-top: 0.25rem;
}

@media (max-width: 768px) {
  .form-actions {
    flex-direction: column;
    gap: 0.5rem;
  }

  .btn {
    width: 100%;
    justify-content: center;
  }
}
</style>
