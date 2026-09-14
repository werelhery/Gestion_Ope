@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Unique suffix for resource names')
param suffix string = uniqueString(resourceGroup().id)

@description('Environment name (e.g. dev, staging, prod)')
param environment string = 'prod'

// 1. Log Analytics Workspace (Monitoring)
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'log-gestion-ope-${suffix}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// 2. Azure Container Registry (ACR)
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: 'crgestionope${suffix}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// 3. Azure Container Apps Managed Environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-gestion-ope-${suffix}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// 4. Backend Container App (.NET 8 Web API)
resource backendApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ca-gestion-ope-api'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 5000
        transport: 'auto'
      }
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.name
          passwordSecretRef: 'registry-password'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: '${containerRegistry.properties.loginServer}/gestion-ope-api:latest'
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 5
      }
    }
  }
}

// 5. Frontend Container App (Angular 18 SPA)
resource frontendApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ca-gestion-ope-frontend'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 80
        transport: 'auto'
      }
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.name
          passwordSecretRef: 'registry-password'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'frontend'
          image: '${containerRegistry.properties.loginServer}/gestion-ope-frontend:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}

output acrLoginServer string = containerRegistry.properties.loginServer
output backendFqdn string = backendApp.properties.configuration.ingress.fqdn
output frontendFqdn string = frontendApp.properties.configuration.ingress.fqdn
