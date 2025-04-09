# Azure Application Insights Integration

This document describes how Azure Application Insights is integrated into the Subscription Tracker application using the OpenTelemetry SDK.

## Overview

Azure Application Insights is used to monitor the application's performance, track usage patterns, and detect errors in both the frontend and backend components. The implementation uses the OpenTelemetry SDK to provide a standardized approach to telemetry collection.

## Backend Implementation

### Configuration

The backend uses the Azure Monitor OpenTelemetry SDK to send telemetry data to Application Insights. The configuration is done in `Program.cs`:

```csharp
// Configure Azure Application Insights with OpenTelemetry
string? appInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    // Add Azure Monitor OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .UseAzureMonitor(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        })
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddSqlClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.SetDbStatementForText = true;
                });
        });
}
```

### Connection String

The connection string is stored in `appsettings.json` under the `ConnectionStrings` section:

```json
"ConnectionStrings": {
  "DefaultConnection": "",
  "ApplicationInsights": "InstrumentationKey=...;IngestionEndpoint=...;LiveEndpoint=...;ApplicationId=..."
}
```

For security reasons, it's recommended to:
- Use environment variables in production
- Use user secrets during development
- Never commit the actual connection string to source control

## Frontend Implementation

### Configuration

The frontend uses the Azure Monitor OpenTelemetry SDK for JavaScript to send telemetry data to Application Insights. The implementation is in `src/services/telemetryService.js`.

### Connection String

The connection string is stored in `src/config.js`:

```javascript
export const config = {
  // Other configuration...
  applicationInsights: {
    connectionString: 'InstrumentationKey=...;IngestionEndpoint=...;LiveEndpoint=...;ApplicationId=...'
  }
}
```

For security reasons:
- In production, the connection string should be injected during the build process
- Use environment variables where possible
- Consider using a config.local.js file for local development (excluded from git)

### Usage

The telemetry service provides the following methods:

- `trackEvent(name, properties)`: Track a custom event
- `trackException(error, properties)`: Track an exception
- `trackPageView(name, url)`: Track a page view

Example usage:

```javascript
import { telemetryService } from '@/services/telemetryService';

// Track a custom event
telemetryService.trackEvent('ButtonClicked', { buttonId: 'submit-button' });

// Track an exception
try {
  // Some code that might throw
} catch (error) {
  telemetryService.trackException(error, { component: 'LoginForm' });
}
```

## Automatic Tracking

The implementation includes automatic tracking for:

- Page views (frontend)
- HTTP requests (frontend and backend)
- SQL queries (backend)
- Exceptions (frontend and backend)
- Document load performance (frontend)

## Security Considerations

- Connection strings should never be committed to source control
- Use environment variables or secure configuration providers in production
- Review what data is being collected to ensure no sensitive information is included

## Viewing Telemetry Data

To view the collected telemetry data:

1. Go to the [Azure Portal](https://portal.azure.com)
2. Navigate to your Application Insights resource
3. Use the various sections (Overview, Performance, Failures, etc.) to analyze the data

## Troubleshooting

If telemetry data is not appearing in Application Insights:

1. Check that the connection string is correct
2. Verify that the SDK is properly initialized
3. Look for any errors in the browser console or server logs
4. Ensure the application has internet connectivity to send telemetry data
