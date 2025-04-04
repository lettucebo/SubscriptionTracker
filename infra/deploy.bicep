// SubscriptionTracker Azure Deployment
// This Bicep template deploys the SubscriptionTracker application to Azure with:
// - Static Web App for the Vue.js frontend (Free tier)
// - Web App for the ASP.NET Core API (Windows, Basic tier)
// - SQL Database for data storage (Basic tier)
// - Private Endpoint for secure database connectivity

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
var privateDnsZoneName = 'privatelink${environment().suffixes.sqlServerHostname}'
var privateDnsZoneGroupName = 'privatednszonegroup-sql'

// Network configuration
var vnetAddressPrefix = '10.0.0.0/16'
var subnetAddressPrefix = '10.0.0.0/24'
var privateEndpointSubnetName = 'subnet-privateendpoints'

// ========== Resources ==========

// Virtual Network for Private Endpoint
resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressPrefix
      ]
    }
    subnets: [
      {
        name: privateEndpointSubnetName
        properties: {
          addressPrefix: subnetAddressPrefix
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

// App Service Plan (Windows, Basic tier)
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1' // Basic tier
    tier: 'Basic'
  }
  properties: {
    reserved: false // Windows
  }
}

// Web App for API
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName == 'prod' ? 'Production' : 'Development'
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
          type: 'SQLAzure'
        }
      ]
    }
  }
}

// Static Web App for client (Free tier)
resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
  name: staticWebAppName
  location: location
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
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    publicNetworkAccess: 'Disabled' // Disable public access for security
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

// SQL Database (Basic tier)
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 1073741824 // 1GB for Basic tier
  }
}

// Private DNS Zone for SQL Server
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: privateDnsZoneName
  location: 'global'
}

// Link the Private DNS Zone to the Virtual Network
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZone
  name: '${privateDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetwork.id
    }
  }
}

// Private Endpoint for SQL Server
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpointName
  location: location
  properties: {
    subnet: {
      id: virtualNetwork.properties.subnets[0].id
    }
    privateLinkServiceConnections: [
      {
        name: privateEndpointName
        properties: {
          privateLinkServiceId: sqlServer.id
          groupIds: [
            'sqlServer'
          ]
        }
      }
    ]
  }
}

// Private DNS Zone Group for the Private Endpoint
resource privateDnsZoneGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  parent: privateEndpoint
  name: privateDnsZoneGroupName
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config1'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}

// ========== Outputs ==========

@description('The URL of the deployed Static Web App')
output staticWebAppUrl string = staticWebApp.properties.defaultHostname

@description('The URL of the deployed Web App')
output webAppUrl string = webApp.properties.defaultHostName

@description('The name of the SQL Server')
output sqlServerName string = sqlServer.name

@description('The name of the SQL Database')
output sqlDatabaseName string = sqlDatabase.name
