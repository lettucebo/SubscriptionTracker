// SubscriptionTracker Azure Deployment - Database Module
// This module handles the deployment of database resources including
// SQL Server, SQL Database, and Private Endpoint connectivity.

// ========== Parameters ==========

@description('The location for all resources')
param location string

@description('The name of the SQL Server')
param sqlServerName string

@description('The name of the SQL Database')
param sqlDatabaseName string

@description('SQL Server administrator login')
param sqlAdminLogin string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('The ID of the virtual network')
param vnetId string

@description('The ID of the subnet for private endpoints')
param subnetId string

@description('The name of the private endpoint')
param privateEndpointName string

// ========== Variables ==========

var privateDnsZoneName = 'privatelink${environment().suffixes.sqlServerHostname}'
var privateDnsZoneGroupName = 'privatednszonegroup-sql'

// ========== Resources ==========

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
      id: vnetId
    }
  }
}

// Private Endpoint for SQL Server
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpointName
  location: location
  properties: {
    subnet: {
      id: subnetId
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

@description('The name of the SQL Server')
output sqlServerName string = sqlServer.name

@description('The name of the SQL Database')
output sqlDatabaseName string = sqlDatabase.name

@description('The fully qualified domain name of the SQL Server')
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
