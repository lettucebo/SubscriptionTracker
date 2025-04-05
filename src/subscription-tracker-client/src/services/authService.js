import { PublicClientApplication } from '@azure/msal-browser';

// MSAL configuration
const msalConfig = {
  auth: {
    clientId: 'bff69ff1-dbac-43ef-88a1-f2a0c9aba3dc', // Replace with your Entra ID app registration client ID
    authority: 'https://login.microsoftonline.com/87befcf7-8c47-4964-9582-2942bfc01159',
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  }
};

// Create the MSAL application object
const msalInstance = new PublicClientApplication(msalConfig);

// Flag to track initialization status
let isInitialized = false;

// Login request object
const loginRequest = {
  scopes: ['openid', 'profile', 'email', 'api://bff69ff1-dbac-43ef-88a1-f2a0c9aba3dc/access_as_user']
};

// Token request object for API calls
const tokenRequest = {
  scopes: ['api://bff69ff1-dbac-43ef-88a1-f2a0c9aba3dc/access_as_user']
};

/**
 * Authentication service for handling user login, logout, and token acquisition
 */
export const authService = {
  /**
   * Initialize the authentication service
   */
  async initialize() {
    // Only initialize once
    if (isInitialized) {
      return Promise.resolve();
    }

    try {
      // Initialize the MSAL instance
      await msalInstance.initialize();

      // Handle redirect promise after login
      const response = await msalInstance.handleRedirectPromise();

      // Mark as initialized
      isInitialized = true;

      // If response is not null, user is authenticated
      if (response !== null) {
        return this.getAccount();
      }
      return null;
    } catch (error) {
      console.error('Error during MSAL initialization:', error);
      throw error;
    }
  },

  /**
   * Get the current authenticated account
   * @returns {Object|null} The current account or null if not authenticated
   */
  getAccount() {
    const currentAccounts = msalInstance.getAllAccounts();
    if (currentAccounts.length === 0) {
      return null;
    }
    return currentAccounts[0];
  },

  /**
   * Login the user
   */
  async login() {
    // Ensure MSAL is initialized
    if (!isInitialized) {
      await this.initialize();
    }
    return msalInstance.loginRedirect(loginRequest);
  },

  /**
   * Logout the user
   */
  async logout() {
    // Ensure MSAL is initialized
    if (!isInitialized) {
      await this.initialize();
    }
    const logoutRequest = {
      account: this.getAccount(),
    };
    return msalInstance.logoutRedirect(logoutRequest);
  },

  /**
   * Get an access token for API calls
   * @returns {Promise<string>} A promise that resolves to an access token
   */
  async getAccessToken() {
    // Ensure MSAL is initialized
    if (!isInitialized) {
      await this.initialize();
    }

    const account = this.getAccount();
    if (!account) {
      throw new Error('User is not authenticated');
    }

    const silentRequest = {
      ...tokenRequest,
      account: account
    };

    try {
      const response = await msalInstance.acquireTokenSilent(silentRequest);
      return response.accessToken;
    } catch (error) {
      // If silent token acquisition fails, try interactive method
      if (error.name === 'InteractionRequiredAuthError') {
        try {
          const response = await msalInstance.acquireTokenRedirect(silentRequest);
          return response.accessToken;
        } catch (interactiveError) {
          console.error('Error acquiring token interactively:', interactiveError);
          throw interactiveError;
        }
      } else {
        console.error('Error acquiring token silently:', error);
        throw error;
      }
    }
  },

  /**
   * Check if the user is authenticated
   * @returns {boolean} True if authenticated, false otherwise
   */
  async isAuthenticated() {
    // Ensure MSAL is initialized
    if (!isInitialized) {
      await this.initialize();
    }
    return this.getAccount() !== null;
  }
};
