// SubscriptionTracker Azure Deployment - Main Template
// This is the main Bicep template that orchestrates the deployment of all components
// using modular architecture for better maintainability.

// ========== Parameters ==========

@description('The location for all resources. Default is Japan East.')
param location string = 'japaneast'

@description('Environment name (dev, test, prod)')
@allowed([
  'dev'
  'test'
  'prod'
])
param environmentName string = 'prod'

@description('SQL Server administrator login')
param sqlAdminLogin string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('The name prefix for all resources')
param resourceNamePrefix string = 'subtracker'

// ========== Variables ==========

// Resource naming
var resourceSuffix = toLower('${resourceNamePrefix}')
var frontendWebAppName = '${resourceSuffix}-app-frontend'
var webAppName = '${resourceSuffix}-app-backend'
var appServicePlanName = '${resourceSuffix}-plan'
var sqlServerName = '${resourceSuffix}-sql'
var sqlDatabaseName = '${resourceSuffix}-sqldb'
var vnetName = '${resourceSuffix}-vnet'
var privateEndpointName = '${resourceSuffix}-pe-sql'

// ========== Module Deployments ==========

// Deploy networking resources
module networkingModule 'modules/networking.bicep' = {
  name: 'networkingDeployment'
  params: {
    location: location
    vnetName: vnetName
  }
}

// Deploy database resources
module databaseModule 'modules/database.bicep' = {
  name: 'databaseDeployment'
  params: {
    location: location
    sqlServerName: sqlServerName
    sqlDatabaseName: sqlDatabaseName
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    vnetId: networkingModule.outputs.vnetId
    subnetId: networkingModule.outputs.privateEndpointSubnetId
    privateEndpointName: privateEndpointName
  }
}

// Deploy backend API resources
module backendModule 'modules/backend.bicep' = {
  name: 'backendDeployment'
  params: {
    location: location
    webAppName: webAppName
    appServicePlanName: appServicePlanName
    environmentName: environmentName
    sqlServerFqdn: databaseModule.outputs.sqlServerFqdn
    sqlDatabaseName: sqlDatabaseName
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    integrationSubnetId: networkingModule.outputs.integrationSubnetId
  }
}

// Deploy frontend resources
module frontendModule 'modules/frontend.bicep' = {
  name: 'frontendDeployment'
  params: {
    location: location
    frontendWebAppName: frontendWebAppName
    appServicePlanName: appServicePlanName
    integrationSubnetId: networkingModule.outputs.integrationSubnetId
  }
  dependsOn: [
    backendModule // Ensure App Service Plan is created first
  ]
}

// ========== Outputs ==========

@description('The URL of the deployed Frontend Web App')
output frontendWebAppUrl string = frontendModule.outputs.frontendWebAppUrl

@description('The URL of the deployed Web App')
output webAppUrl string = backendModule.outputs.webAppUrl

@description('The name of the SQL Server')
output sqlServerName string = databaseModule.outputs.sqlServerName

@description('The name of the SQL Database')
output sqlDatabaseName string = databaseModule.outputs.sqlDatabaseName

