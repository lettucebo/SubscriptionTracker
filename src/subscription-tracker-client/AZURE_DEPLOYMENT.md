# Azure Web App Deployment Guide

This document explains how to deploy the Subscription Tracker frontend to Azure Web App on Linux and configure environment variables.

## Prerequisites

- Azure subscription
- Azure CLI installed (optional, for command-line deployment)
- Node.js 22.x or later

## Deployment Steps

### 1. Build the Application

```bash
# Install dependencies
npm install

# Build the application
npm run build

# Copy server files to the build directory
cp -r public/server.js public/startup.sh public/package.json public/web.config dist/
```

### 2. Create Azure Web App

You can create an Azure Web App using the Azure Portal or Azure CLI:

```bash
# Using Azure CLI
az webapp create --resource-group SubTracker --plan subtracker-plan --name subtracker-app-frontend --runtime "NODE|22-lts"
```

### 3. Configure Environment Variables

In the Azure Portal:

1. Go to your Web App (subtracker-app-frontend)
2. Navigate to Settings > Configuration
3. Add the following Application settings:

| Name | Value | Description |
|------|-------|-------------|
| API_URL | https://subtracker-app-backend.azurewebsites.net | URL of your backend API |
| AUTH_CLIENT_ID | your-client-id | Entra ID client ID |
| AUTH_AUTHORITY | https://login.microsoftonline.com/your-tenant-id | Entra ID authority URL |
| APPLICATIONINSIGHTS_CONNECTION_STRING | your-connection-string | Application Insights connection string |
| WEBSITE_RUN_FROM_PACKAGE | 0 | Disable run from package |
| WEBSITE_NODE_DEFAULT_VERSION | ~22 | Use Node.js 22 |
| STARTUP_COMMAND | npm run start:azure | Use our custom startup script |

4. Click "Save" to apply the settings

### 4. Deploy the Application

You can deploy using Azure CLI, GitHub Actions, or the Azure Portal:

```bash
# Using Azure CLI
az webapp deployment source config-zip --resource-group SubTracker --name subtracker-app-frontend --src ./dist.zip
```

### 5. Verify Deployment

1. Navigate to your Web App URL (https://subtracker-app-frontend.azurewebsites.net)
2. Check the application logs to verify that environment variables are loaded correctly

## Troubleshooting

### Check Logs

In the Azure Portal:

1. Go to your Web App
2. Navigate to Monitoring > Log stream
3. Check for any errors in the application startup

### Common Issues

- **Environment variables not loading**: Verify that you've set the environment variables correctly in the Azure Portal
- **Application not starting**: Check that the STARTUP_COMMAND is set correctly
- **API connection issues**: Verify that the API_URL is correct and that the backend is accessible from the frontend

## Additional Resources

- [Azure Web Apps Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Node.js on Azure Web Apps](https://docs.microsoft.com/en-us/azure/app-service/configure-language-nodejs)
