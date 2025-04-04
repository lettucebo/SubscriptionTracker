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
param environmentName string = 'dev'

@description('SQL Server administrator login')
param sqlAdminLogin string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('The name prefix for all resources')
param resourceNamePrefix string = 'subtracker'

// ========== Variables ==========

// Resource naming
var resourceSuffix = '${resourceNamePrefix}-${environmentName}'
var staticWebAppName = 'stapp-${resourceSuffix}'
var webAppName = 'app-${resourceSuffix}'
var appServicePlanName = 'plan-${resourceSuffix}'
var sqlServerName = 'sql-${resourceSuffix}'
var sqlDatabaseName = 'sqldb-${resourceSuffix}'
var vnetName = 'vnet-${resourceSuffix}'
var privateEndpointName = 'pe-sql-${resourceSuffix}'

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
  }
}

// Deploy frontend resources
module frontendModule 'modules/frontend.bicep' = {
  name: 'frontendDeployment'
  params: {
    location: location
    staticWebAppName: staticWebAppName
  }
}

// ========== Outputs ==========

@description('The URL of the deployed Static Web App')
output staticWebAppUrl string = frontendModule.outputs.staticWebAppUrl

@description('The URL of the deployed Web App')
output webAppUrl string = backendModule.outputs.webAppUrl

@description('The name of the SQL Server')
output sqlServerName string = databaseModule.outputs.sqlServerName

@description('The name of the SQL Database')
output sqlDatabaseName string = databaseModule.outputs.sqlDatabaseName
