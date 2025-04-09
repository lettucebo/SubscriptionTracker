// Get configuration from window.__env if available (injected by server)
const env = window.__env || {};

export const config = {
  // API base URL - use environment variable or fallback to default
  baseUrl: env.API_URL || 'https://localhost:7052',

  // Authentication configuration
  auth: {
    // Entra ID client ID
    clientId: env.AUTH_CLIENT_ID,
    // Authority URL
    authority: env.AUTH_AUTHORITY,
    // Redirect URI after login
    redirectUri: window.location.origin,
    // Redirect URI after logout
    postLogoutRedirectUri: window.location.origin,
  },

  // Application Insights configuration
  applicationInsights: {
    connectionString: env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
}
