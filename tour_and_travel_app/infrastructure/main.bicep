@description('Location for all resources.')
param location string = resourceGroup().location

@description('A unique suffix to append to resource names to ensure global uniqueness.')
param uniqueSuffix string = uniqueString(resourceGroup().id)

@description('The administrator username for PostgreSQL.')
param pgAdminUsername string = 'tourmateadmin'

@secure()
@description('The administrator password for PostgreSQL.')
param pgAdminPassword string

@description('The name of the PostgreSQL database.')
param pgDatabaseName string = 'tourmatedb'

// ==========================================
// 1. Azure Container Registry (ACR)
// ==========================================
resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: 'tourmateacr${uniqueSuffix}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// ==========================================
// 3. Azure Storage Account (Blob)
// ==========================================
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'tmtstg${uniqueSuffix}' // Name must be < 24 characters
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: true
  }
}

// Storage account blob service and containers
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  parent: storageAccount
  name: 'default'
}

resource rawImagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  parent: blobService
  name: 'raw-images'
  properties: {
    publicAccess: 'None' // Private for uploading
  }
}

resource webImagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  parent: blobService
  name: 'web-images'
  properties: {
    publicAccess: 'Blob' // Public read access for web
  }
}

resource userDocumentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  parent: blobService
  name: 'user-documents'
  properties: {
    publicAccess: 'None' // STRICTLY private for security
  }
}

// ==========================================
// 4. Azure Database for PostgreSQL (Flexible Server)
// ==========================================
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: 'tourmate-pg-${uniqueSuffix}'
  location: location
  sku: {
    name: 'Standard_B1ms' // Burstable for cost-saving, change for prod
    tier: 'Burstable'
  }
  properties: {
    version: '15'
    administratorLogin: pgAdminUsername
    administratorLoginPassword: pgAdminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
    }
  }
}

// Configure PostgreSQL Firewall to allow Azure Services
resource allowAzureIPs 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  parent: postgresServer
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: pgDatabaseName
}

// ==========================================
// 5. Azure Kubernetes Service (AKS) for Backend
// ==========================================
resource aksCluster 'Microsoft.ContainerService/managedClusters@2023-08-01' = {
  name: 'tourmate-aks'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: 'tourmate-aks-${uniqueSuffix}'
    agentPoolProfiles: [
      {
        name: 'agentpool'
        count: 1
        vmSize: 'Standard_D2s_v3'
        osType: 'Linux'
        mode: 'System'
      }
    ]
  }
}


// ==========================================
// 6. Azure Static Web Apps for Frontend
// ==========================================
resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
  name: 'tourmate-swa'
  location: 'eastasia' // Static web apps have specific regional availability, typically distinct from main resources. Use a generic valid one or location if supported.
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    repositoryUrl: 'https://github.com/iniyan007/Presidio-intern'
    branch: 'main' // Or azure-deployment
  }
}

// ==========================================
// 7. Azure Function App (Serverless Consumption Plan)
// ==========================================
// Consumption plan for Azure Functions
resource functionAppPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: 'tourmate-func-plan-${uniqueSuffix}'
  location: location
  sku: {
    name: 'Y1' // Consumption
    tier: 'Dynamic'
  }
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: 'tourmate-func-${uniqueSuffix}'
  location: location
  kind: 'functionapp' // Windows
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionAppPlan.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED'
          value: '1'
        }
      ]
    }
  }
}

// ==========================================
// 8. Azure Managed Redis (Enterprise)
// ==========================================
resource redisCache 'Microsoft.Cache/redisEnterprise@2025-04-01' = {
  name: 'tmtredis-${uniqueSuffix}'
  location: location
  sku: {
    name: 'Balanced_B5'
  }
}

resource redisDatabase 'Microsoft.Cache/redisEnterprise/databases@2025-04-01' = {
  name: 'default'
  parent: redisCache
  properties: {
    clientProtocol: 'Encrypted'
    clusteringPolicy: 'OSSCluster'
    evictionPolicy: 'VolatileLRU'
    port: 10000
  }
}


// Outputs
output frontendUrl string = 'https://${staticWebApp.properties.defaultHostname}'
output acrLoginServer string = acr.properties.loginServer
output postgresServerName string = postgresServer.name
output redisHostName string = redisCache.properties.hostName
output aksClusterName string = aksCluster.name
