# SubscriptionTracker Azure Infrastructure

This directory contains the Azure infrastructure deployment files for the SubscriptionTracker application.

## Overview

The Bicep templates in this directory deploy the following Azure resources:

- Azure Static Web App (Free tier) for the Vue.js frontend
- Azure Web App (Windows, Basic tier) for the ASP.NET Core API
- Azure SQL Database (Basic tier) for data storage
- Virtual Network and Private Endpoint for secure connectivity

## Directory Structure

```
infra/
├── main.bicep                 # Main deployment template (modular approach)
├── modules/                   # Modular components
│   ├── networking.bicep       # Virtual Network and subnets
│   ├── database.bicep         # SQL Server and Database
│   ├── backend.bicep          # Web App for API
│   └── frontend.bicep         # Static Web App for client
├── deploy.bicep              # Single-file deployment (alternative)
├── deploy.parameters.json     # Deployment parameters
├── deploy.ps1                 # PowerShell deployment script
└── README.deployment.md       # Detailed deployment instructions
```

## Quick Start

1. Update the parameters in `deploy.parameters.json`
2. Run the deployment script:

```powershell
cd infra
.\deploy.ps1 -ResourceGroupName "SubTracker" -SqlAdminLogin "sqladmin" -SqlAdminPassword (ConvertTo-SecureString -String "YourStrongPassword" -AsPlainText -Force)
```

## Detailed Documentation

For detailed deployment instructions, see [README.deployment.md](./README.deployment.md).
