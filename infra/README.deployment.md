# Azure Deployment Guide for SubscriptionTracker

This guide explains how to deploy the SubscriptionTracker application to Azure using the provided Bicep templates.

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed
- Azure subscription
- Sufficient permissions to create resources in your Azure subscription

## Project Structure

The deployment is organized using a modular Bicep structure:

```
infra/
├── main.bicep                 # Main deployment template
├── modules/                   # Modular components
│   ├── networking.bicep       # Virtual Network and subnets
│   ├── database.bicep         # SQL Server and Database
│   ├── backend.bicep          # Web App for API
│   └── frontend.bicep         # Static Web App for client
├── deploy.bicep              # Single-file deployment (alternative)
├── deploy.parameters.json     # Deployment parameters
├── deploy.ps1                 # PowerShell deployment script
└── README.deployment.md       # This documentation
```

## Resources Created

The deployment creates the following Azure resources:

1. **Azure Static Web App (Free tier)** - Hosts the Vue.js frontend (subscription-tracker-client)
2. **Azure Web App (Windows, Basic tier)** - Hosts the ASP.NET Core API (SubscriptionTracker.Api)
3. **Azure SQL Database (Basic tier)** - Stores application data
4. **Virtual Network** - Provides network isolation
5. **Private Endpoint** - Secures the connection between the Web App and SQL Database
6. **Private DNS Zone** - Enables private DNS resolution for the SQL Server

## Deployment Steps

### 1. Login to Azure

```bash
az login
```

### 2. Set your subscription

```bash
az account set --subscription "<your-subscription-id>"
```

### 3. Create a Resource Group

```bash
az group create --name SubTracker --location japaneast
```

### 4. Update Parameters

Edit the `deploy.parameters.json` file to set your preferred values, especially:
- `sqlAdminLogin`: SQL Server administrator username
- `sqlAdminPassword`: SQL Server administrator password (or use Key Vault)
- `environmentName`: Environment name (dev, test, prod)
- `resourceNamePrefix`: Prefix for resource names

### 5. Deploy Using PowerShell Script

```powershell
# Navigate to the infra directory
cd infra

# Run the deployment script
.\deploy.ps1 -ResourceGroupName "SubTracker" -SqlAdminLogin "sqladmin" -SqlAdminPassword (ConvertTo-SecureString -String "P@ssw0rd" -AsPlainText -Force)
```

### 6. Deploy Using Azure CLI

Alternatively, you can deploy directly with Azure CLI:

```bash
# Navigate to the infra directory
cd infra

# Run the deployment
az deployment group create \
  --resource-group SubTracker \
  --template-file main.bicep \
  --parameters deploy.parameters.json
```

### 7. Configure Static Web App

After deployment, you need to configure the Static Web App to connect to your repository:

1. Go to the Azure Portal
2. Navigate to the Static Web App resource
3. Click on "GitHub" under "Source Control"
4. Follow the prompts to connect to your GitHub repository
5. Configure the build settings:
   - Build Preset: Vue.js
   - App location: `/src/subscription-tracker-client`
   - Output location: `dist`

### 8. Update API URL in Frontend

Update the API URL in the frontend configuration to point to your deployed Web App:

1. In your repository, update the `src/subscription-tracker-client/src/config.js` file:
   ```javascript
   export const config = {
     baseUrl: 'https://app-subtracker-dev.azurewebsites.net'
   }
   ```

2. Commit and push the changes to trigger a redeployment of the Static Web App.

## Post-Deployment Configuration

### Configure CORS in the Web App

Ensure the Web App allows requests from the Static Web App:

1. Go to the Azure Portal
2. Navigate to your Web App
3. Under "API" section, select "CORS"
4. Add the URL of your Static Web App to the allowed origins

### Database Migrations

The application is configured to apply migrations automatically on startup. The first time the Web App runs, it will create the database schema.

## Modifying the Deployment

### Adding New Resources

To add new resources to the deployment:

1. Create a new module file in the `modules/` directory if appropriate
2. Add the resource definition to the module
3. Update the main.bicep file to reference the new module
4. Add any required parameters to the module and main template

### Updating Existing Resources

To update existing resources:

1. Modify the appropriate module file
2. Test the deployment in a development environment before applying to production

## Troubleshooting

### Private Endpoint Connection

If the Web App cannot connect to the SQL Database:

1. Verify the Private Endpoint is provisioned correctly
2. Check the Private DNS Zone is linked to the Virtual Network
3. Ensure the Web App is configured to use the Virtual Network integration

### Static Web App Deployment

If the Static Web App deployment fails:

1. Check the GitHub Actions workflow logs
2. Verify the build configuration is correct
3. Ensure the repository structure matches the expected paths

## Cleanup

To remove all deployed resources:

```bash
az group delete --name SubTracker
```
