import axios from 'axios';
import { config } from '../config';
import { authService } from './authService';

// Create an axios instance with default configuration
const apiClient = axios.create({
  baseURL: config.baseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to include the authentication token
apiClient.interceptors.request.use(
  async (config) => {
    try {
      // Only add token if user is authenticated
      const isAuthenticated = await authService.isAuthenticated();
      if (isAuthenticated) {
        try {
          const token = await authService.getAccessToken();
          config.headers.Authorization = `Bearer ${token}`;
        } catch (error) {
          console.error('Error getting access token:', error);
          // Don't reject here, let the request proceed without the token
          // The server will return 401 if the token is required
        }
      }
      return config;
    } catch (error) {
      console.error('Error in request interceptor:', error);
      return config;
    }
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add a response interceptor to handle authentication errors
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    // If the error is due to an invalid or expired token (401 Unauthorized)
    if (error.response && error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Try to get a new token
        await authService.login();
        return apiClient(originalRequest);
      } catch (refreshError) {
        console.error('Error refreshing authentication:', refreshError);
        // Redirect to login page
        window.location.href = '/login?error=session_expired';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

// API service for making authenticated requests
export const apiService = {
  // User endpoints
  async getCurrentUser() {
    return apiClient.get('/api/user/me');
  },

  // Subscription endpoints
  async getSubscriptions(categoryId = null) {
    const params = categoryId ? { categoryId } : {};
    return apiClient.get('/api/subscription', { params });
  },

  async getSubscription(id) {
    return apiClient.get(`/api/subscription/${id}`);
  },

  async createSubscription(subscription) {
    return apiClient.post('/api/subscription', subscription);
  },

  async updateSubscription(id, subscription) {
    return apiClient.put(`/api/subscription/${id}`, subscription);
  },

  async deleteSubscription(id) {
    return apiClient.delete(`/api/subscription/${id}`);
  },

  // Category endpoints
  async getCategories() {
    return apiClient.get('/api/category');
  },

  async getCategory(id) {
    return apiClient.get(`/api/category/${id}`);
  },

  async createCategory(category) {
    return apiClient.post('/api/category', category);
  },

  async updateCategory(id, category) {
    return apiClient.put(`/api/category/${id}`, category);
  },

  async deleteCategory(id) {
    return apiClient.delete(`/api/category/${id}`);
  }
};
