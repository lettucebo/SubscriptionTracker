import { authService } from '@/services/authService';

/**
 * Navigation guard to check if the user is authenticated
 * @param {Object} to - Route the user is navigating to
 * @param {Object} from - Route the user is navigating from
 * @param {Function} next - Function to resolve the navigation
 */
export const authGuard = async (to, from, next) => {
  // Check if the route requires authentication
  if (to.matched.some(record => record.meta.requiresAuth)) {
    try {
      // Initialize authentication service
      await authService.initialize();

      // Check if user is authenticated
      const isAuthenticated = await authService.isAuthenticated();
      if (!isAuthenticated) {
        // Redirect to login page if not authenticated
        next({
          name: 'Login',
          query: { redirect: to.fullPath }
        });
      } else {
        // Continue to the route if authenticated
        next();
      }
    } catch (error) {
      console.error('Authentication check error:', error);
      // In case of error, redirect to login page
      next({
        name: 'Login',
        query: {
          redirect: to.fullPath,
          error: 'auth_error'
        }
      });
    }
  } else {
    // Continue to the route if authentication is not required
    next();
  }
};
