// SubscriptionTracker Azure Deployment - Frontend Module
// This module handles the deployment of frontend resources including
// Static Web App for hosting the Vue.js client application.

// ========== Parameters ==========

@description('The location for all resources')
param location string

@description('The name of the Static Web App')
param staticWebAppName string

// ========== Resources ==========

// Static Web App for client (Free tier, Non Regional)
resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
  name: staticWebAppName
  location: 'westus2' // Fixed location for Non Regional deployment
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    // The repository configuration would typically be set up through the Azure Portal
    // or using GitHub Actions after deployment
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    provider: 'None' // Manual deployment
    buildProperties: {
      skipGithubActionWorkflowGeneration: true
    }
    deploymentProperties: {
      deploymentTarget: 'global' // Enables Non Regional deployment
    }
  }
}

// ========== Outputs ==========

@description('The URL of the deployed Static Web App')
output staticWebAppUrl string = staticWebApp.properties.defaultHostname

