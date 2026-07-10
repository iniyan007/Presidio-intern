@description('Location for all resources.')
param location string = resourceGroup().location

@description('A unique suffix to append to resource names to ensure global uniqueness.')
param uniqueSuffix string = uniqueString(resourceGroup().id)

@description('The Object ID of the user deploying this template (required for Key Vault Access Policy).')
param deployerObjectId string

@description('The administrator username for PostgreSQL.')
param pgAdminUsername string = 'tourmateadmin'

@secure()
@description('The administrator password for PostgreSQL.')
param pgAdminPassword string

@description('The name of the PostgreSQL database.')
param pgDatabaseName string = 'tourmatedb'

@secure()
@description('The JWT Secret Key.')
param jwtKey string

@description('The JWT Issuer.')
param jwtIssuer string = 'TourMate'

@description('The JWT Audience.')
param jwtAudience string = 'TourMateUsers'

@secure()
@description('The Gemini API Key.')
param geminiApiKey string

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
// 2. Azure Key Vault
// ==========================================
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: 'tourmatekv-${uniqueSuffix}'
  location: location
  properties: {
    enableRbacAuthorization: false
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enabledForTemplateDeployment: true
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: deployerObjectId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
          ]
        }
      }
    ]
  }
}

// Store the DB Connection string in Key Vault automatically
resource dbConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'DbConnectionString'
  properties: {
    value: 'Host=${postgresServer.name}.postgres.database.azure.com;Database=${pgDatabaseName};Username=${pgAdminUsername};Password=${pgAdminPassword};Ssl Mode=Require;'
  }
}

// Store the JWT Key
resource jwtKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'Jwt--Key'
  properties: {
    value: jwtKey
  }
}

// Store the JWT Issuer
resource jwtIssuerSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'Jwt--Issuer'
  properties: {
    value: jwtIssuer
  }
}

// Store the JWT Audience
resource jwtAudienceSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'Jwt--Audience'
  properties: {
    value: jwtAudience
  }
}

// Store the Gemini API Key
resource geminiApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'Gemini--ApiKey'
  properties: {
    value: geminiApiKey
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
// 5. Azure App Service (Compute for Containers)
// ==========================================
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: 'tourmate-asp-${uniqueSuffix}'
  location: location
  kind: 'linux'
  properties: {
    reserved: true // Required for Linux plans
  }
  sku: {
    name: 'B1' // Basic tier for dev/test
    tier: 'Basic'
  }
}

// Backend Web App (Container)
resource backendApp 'Microsoft.Web/sites@2022-09-01' = {
  name: 'tourmate-api-${uniqueSuffix}'
  location: location
  kind: 'app,linux,container'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acr.name}.azurecr.io/tourmate-backend:latest'
      appSettings: [
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acr.name}.azurecr.io'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: acr.listCredentials().username
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: acr.listCredentials().passwords[0].value
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=DbConnectionString)'
        }
        {
          name: 'JwtOptions__SecretKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=Jwt--Key)'
        }
        {
          name: 'JwtOptions__Issuer'
          value: jwtIssuer
        }
        {
          name: 'JwtOptions__Audience'
          value: jwtAudience
        }
        {
          name: 'GeminiApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=Gemini--ApiKey)'
        }
        {
          name: 'Redis__ConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=Redis--ConnectionString)'
        }
      ]
    }
  }
}

// Frontend Web App (Container)
resource frontendApp 'Microsoft.Web/sites@2022-09-01' = {
  name: 'tourmate-web-${uniqueSuffix}'
  location: location
  kind: 'app,linux,container'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acr.name}.azurecr.io/tourmate-frontend:latest'
      appSettings: [
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acr.name}.azurecr.io'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: acr.listCredentials().username
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: acr.listCredentials().passwords[0].value
        }
        {
          name: 'API_BASE_URL'
          value: 'https://${backendApp.properties.defaultHostName}'
        }
      ]
    }
  }
}

// ==========================================
// 6. Azure Function App (Image Processing)
// ==========================================
resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: 'tourmate-func-${uniqueSuffix}'
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned' // MSI to access blob storage and key vault
  }
  properties: {
    serverFarmId: appServicePlan.id // Can share the same plan to save costs
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0' // For .NET 8 Isolated functions
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
      ]
    }
  }
}

// ==========================================
// 7. Azure Managed Redis (Enterprise)
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

// Store the Redis Connection string in Key Vault automatically
resource redisConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'Redis--ConnectionString'
  properties: {
    value: '${redisCache.properties.hostName}:10000,abortConnect=false,ssl=true,password=${redisDatabase.listKeys().primaryKey}'
  }
}

// ==========================================
// 8. Key Vault Access Policies for Apps
// ==========================================
resource appAccessPolicies 'Microsoft.KeyVault/vaults/accessPolicies@2023-02-01' = {
  name: 'add'
  parent: keyVault
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: backendApp.identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: functionApp.identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}

// Outputs
output backendUrl string = 'https://${backendApp.properties.defaultHostName}'
output frontendUrl string = 'https://${frontendApp.properties.defaultHostName}'
output acrLoginServer string = acr.properties.loginServer
output postgresServerName string = postgresServer.name
output keyVaultName string = keyVault.name
output redisHostName string = redisCache.properties.hostName
