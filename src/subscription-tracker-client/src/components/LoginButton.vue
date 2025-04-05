<template>
  <div>
    <button v-if="!isAuthenticated" @click="login" class="btn btn-primary">
      <i class="fas fa-sign-in-alt me-2"></i> Login
    </button>
    <div v-else class="dropdown">
      <button class="btn btn-outline-secondary dropdown-toggle" type="button" id="userDropdown" data-bs-toggle="dropdown" aria-expanded="false">
        <i class="fas fa-user-circle me-2"></i> {{ userName }}
      </button>
      <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="userDropdown">
        <li><span class="dropdown-item-text">{{ userEmail }}</span></li>
        <li><hr class="dropdown-divider"></li>
        <li><a class="dropdown-item" href="#" @click.prevent="logout">Logout</a></li>
      </ul>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue';
import { authService } from '@/services/authService';

export default {
  name: 'LoginButton',
  setup() {
    const isAuthenticated = ref(false);
    const userAccount = ref(null);

    const userName = computed(() => {
      return userAccount.value?.name || userAccount.value?.username || 'User';
    });

    const userEmail = computed(() => {
      return userAccount.value?.username || '';
    });

    const checkAuthStatus = async () => {
      try {
        await authService.initialize();
        const account = authService.getAccount();
        isAuthenticated.value = account !== null;
        userAccount.value = account;
      } catch (error) {
        console.error('Error checking auth status:', error);
        isAuthenticated.value = false;
        userAccount.value = null;
      }
    };

    const login = async () => {
      try {
        await authService.login();
      } catch (error) {
        console.error('Login error:', error);
      }
    };

    const logout = async () => {
      try {
        await authService.logout();
      } catch (error) {
        console.error('Logout error:', error);
      }
    };

    onMounted(async () => {
      // Initialize authentication and check status
      await checkAuthStatus();
    });

    return {
      isAuthenticated,
      userName,
      userEmail,
      login,
      logout
    };
  }
};
</script>

<style scoped>
.dropdown-item-text {
  color: #6c757d;
  font-size: 0.875rem;
}
</style>
