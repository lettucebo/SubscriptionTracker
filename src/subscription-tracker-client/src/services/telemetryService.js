/**
 * Telemetry service for Application Insights
 * @module telemetryService
 */

import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { config } from '../config';

// Connection string for Application Insights
const connectionString = config.applicationInsights?.connectionString;

// Parse the connection string to get the instrumentation key
let instrumentationKey = '';
if (connectionString) {
  const match = connectionString.match(/InstrumentationKey=([^;]+)/i);
  if (match && match[1]) {
    instrumentationKey = match[1];
  }
}

// Application Insights instance
let appInsights = null;

// Flag to track initialization status
let isInitialized = false;

/**
 * Telemetry service for tracking application usage and errors
 */
export const telemetryService = {
  /**
   * Initialize the telemetry service
   */
  initialize() {
    // Only initialize if instrumentation key is provided and not already initialized
    if (!instrumentationKey || isInitialized) {
      return;
    }

    try {
      // Initialize Application Insights
      appInsights = new ApplicationInsights({
        config: {
          instrumentationKey: instrumentationKey,
          enableAutoRouteTracking: true, // Automatically track route changes
          enableCorsCorrelation: true,   // Enable correlation headers for CORS requests
          enableRequestHeaderTracking: true, // Track request headers
          enableResponseHeaderTracking: true, // Track response headers
          disableFetchTracking: false,   // Enable fetch tracking
          disableAjaxTracking: false,    // Enable AJAX tracking
          disableExceptionTracking: false, // Enable exception tracking
          autoTrackPageVisitTime: true,  // Track time spent on page
        }
      });

      // Start tracking
      appInsights.loadAppInsights();
      appInsights.trackPageView(); // Track initial page view

      console.log('Telemetry service initialized successfully');
      isInitialized = true;
    } catch (error) {
      console.error('Failed to initialize telemetry service:', error);
    }
  },

  /**
   * Track a custom event
   * @param {string} name - Event name
   * @param {Object} [properties] - Custom properties to include with the event
   */
  trackEvent(name, properties = {}) {
    if (!isInitialized || !appInsights) {
      return;
    }

    try {
      appInsights.trackEvent({ name }, properties);
    } catch (error) {
      console.error(`Failed to track event ${name}:`, error);
    }
  },

  /**
   * Track an exception
   * @param {Error} error - The error object
   * @param {Object} [properties] - Custom properties to include with the exception
   */
  trackException(error, properties = {}) {
    if (!isInitialized || !appInsights) {
      return;
    }

    try {
      appInsights.trackException({
        exception: error,
        properties: properties
      });
    } catch (e) {
      console.error('Failed to track exception:', e);
    }
  },

  /**
   * Track a page view
   * @param {string} name - Page name
   * @param {string} url - Page URL
   */
  trackPageView(name, url) {
    if (!isInitialized || !appInsights) {
      return;
    }

    try {
      appInsights.trackPageView({
        name: name,
        uri: url
      });
    } catch (error) {
      console.error(`Failed to track page view ${name}:`, error);
    }
  }
};
