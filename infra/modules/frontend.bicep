// SubscriptionTracker Azure Deployment - Frontend Module
// This module handles the deployment of frontend resources including
// Web App for hosting the Vue.js client application.

// ========== Parameters ==========

@description('The location for all resources')
param location string

@description('The name of the Frontend Web App')
param frontendWebAppName string

@description('The name of the App Service Plan')
param appServicePlanName string

// ========== Resources ==========

// Reference to existing App Service Plan (shared with backend)
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' existing = {
  name: appServicePlanName
}

// Web App for frontend (using shared App Service Plan)
resource frontendWebApp 'Microsoft.Web/sites@2022-09-01' = {
  name: frontendWebAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'NODE|22-lts' // Node.js 22 LTS on Linux
      appSettings: [
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'true'
        }
      ]
    }
  }
}

// ========== Outputs ==========

@description('The URL of the deployed Frontend Web App')
output frontendWebAppUrl string = frontendWebApp.properties.defaultHostName

