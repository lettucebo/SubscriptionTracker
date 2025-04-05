<template>
  <div class="login-container">
    <div class="card login-card">
      <div class="card-body">
        <h2 class="card-title text-center mb-4">Sign In</h2>
        <p class="text-center mb-4">Sign in to manage your subscriptions</p>
        <div v-if="errorMessage" class="alert alert-danger mb-3">
          {{ errorMessage }}
          <div v-if="showDetails" class="mt-2 small">
            <pre class="error-details">{{ errorDetails }}</pre>
          </div>
          <button v-if="errorDetails" @click="toggleDetails" class="btn btn-sm btn-outline-danger mt-2">
            {{ showDetails ? 'Hide Details' : 'Show Details' }}
          </button>
        </div>
        <div class="d-grid gap-2">
          <button @click="login" class="btn btn-primary btn-lg" :disabled="isLoading">
            <span v-if="isLoading" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
            <i v-else class="fab fa-microsoft me-2"></i> Sign in with Microsoft
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { authService } from '@/services/authService';

export default {
  name: 'LoginPage',
  setup() {
    const router = useRouter();
    const isLoading = ref(false);
    const errorMessage = ref('');
    const errorDetails = ref('');
    const showDetails = ref(false);

    const toggleDetails = () => {
      showDetails.value = !showDetails.value;
    };

    const login = async () => {
      errorMessage.value = '';
      errorDetails.value = '';
      isLoading.value = true;

      try {
        // Make sure MSAL is initialized before login
        await authService.initialize();
        await authService.login();
      } catch (error) {
        console.error('Login error:', error);
        errorMessage.value = 'An error occurred during sign in. Please try again.';
        if (error) {
          errorDetails.value = error.toString();
          if (error.errorCode === 'invalid_resource') {
            errorMessage.value = 'The API resource is not properly configured. Please contact the administrator.';
          }
        }
      } finally {
        isLoading.value = false;
      }
    };

    onMounted(async () => {
      // Check for error query parameter
      const error = router.currentRoute.value.query.error;
      if (error === 'auth_error') {
        errorMessage.value = 'Authentication error. Please try signing in again.';
      } else if (error === 'session_expired') {
        errorMessage.value = 'Your session has expired. Please sign in again.';
      }

      try {
        // Check if user is already authenticated
        await authService.initialize();
        const isAuthenticated = await authService.isAuthenticated();
        if (isAuthenticated) {
          // Get redirect URL from query params or default to home
          const redirectPath = router.currentRoute.value.query.redirect || '/';
          // Redirect to the requested page if already authenticated
          router.push(redirectPath);
        }
      } catch (error) {
        console.error('Authentication check error:', error);
        errorMessage.value = 'An error occurred while checking authentication status.';
        if (error) {
          errorDetails.value = error.toString();
        }
      }
    });

    return {
      login,
      isLoading,
      errorMessage,
      errorDetails,
      showDetails,
      toggleDetails
    };
  }
};
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 70vh;
}

.login-card {
  max-width: 400px;
  width: 100%;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.error-details {
  font-size: 0.8rem;
  background-color: rgba(0, 0, 0, 0.05);
  padding: 0.5rem;
  border-radius: 0.25rem;
  white-space: pre-wrap;
  word-break: break-word;
  max-height: 150px;
  overflow-y: auto;
}
</style>
