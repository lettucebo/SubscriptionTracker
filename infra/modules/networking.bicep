// SubscriptionTracker Azure Deployment - Networking Module
// This module handles the deployment of networking resources including
// Virtual Network and subnets required for private connectivity.

// ========== Parameters ==========

@description('The location for all resources')
param location string

@description('The name of the virtual network')
param vnetName string

// ========== Variables ==========

// Network configuration
var vnetAddressPrefix = '10.0.0.0/16'
var privateEndpointSubnetName = 'subnet-privateendpoints'
var privateEndpointSubnetAddressPrefix = '10.0.0.0/24'
var integrationSubnetName = 'subnet-integration'
var integrationSubnetAddressPrefix = '10.0.1.0/24'

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
          addressPrefix: privateEndpointSubnetAddressPrefix
          privateEndpointNetworkPolicies: 'Disabled'
        }
      },
      {
        name: integrationSubnetName
        properties: {
          addressPrefix: integrationSubnetAddressPrefix
          delegations: [
            {
              name: 'delegation-to-webapp'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
    ]
  }
}

// ========== Outputs ==========

@description('The ID of the virtual network')
output vnetId string = virtualNetwork.id

@description('The ID of the subnet for private endpoints')
output privateEndpointSubnetId string = virtualNetwork.properties.subnets[0].id

@description('The ID of the subnet for VNet integration')
output integrationSubnetId string = virtualNetwork.properties.subnets[1].id
