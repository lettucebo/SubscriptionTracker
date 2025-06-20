// SubscriptionTracker Azure Deployment - Backend Module
// This module handles the deployment of backend resources including
// App Service Plan and Web App for hosting the ASP.NET Core API.

// ========== Parameters ==========

@description('The location for all resources')
param location string

@description('The name of the Web App')
param webAppName string

@description('The name of the App Service Plan')
param appServicePlanName string

@description('Environment name (dev, test, prod)')
param environmentName string

@description('The name of the SQL Server')
param sqlServerName string

@description('The name of the SQL Database')
param sqlDatabaseName string

@description('SQL Server administrator login')
param sqlAdminLogin string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('The ID of the subnet for VNet integration')
param integrationSubnetId string

// ========== Resources ==========

// App Service Plan (Linux, Basic tier)
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1' // Basic tier
    tier: 'Basic'
  }
  properties: {
    reserved: true // Linux
  }
}

// Web App for API
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    virtualNetworkSubnetId: integrationSubnetId
    vnetRouteAllEnabled: true // Route all outbound traffic through VNet
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0' // .NET 8.0 on Linux
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName == 'prod' ? 'Production' : 'Development'
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          // Using private endpoint DNS name to ensure connection through private endpoint
          connectionString: 'Server=tcp:${sqlServerName}.privatelink${environment().suffixes.sqlServerHostname},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
          type: 'SQLAzure'
        }
      ]
    }
  }
}

// ========== Outputs ==========

@description('The URL of the deployed Web App')
output webAppUrl string = webApp.properties.defaultHostName
