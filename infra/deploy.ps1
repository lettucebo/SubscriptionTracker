# PowerShell script to deploy SubscriptionTracker to Azure

# Parameters
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory=$false)]
    [string]$Location = "japaneast",

    [Parameter(Mandatory=$false)]
    [string]$EnvironmentName = "dev",

    [Parameter(Mandatory=$true)]
    [string]$SqlAdminLogin,

    [Parameter(Mandatory=$true)]
    [SecureString]$SqlAdminPassword,

    [Parameter(Mandatory=$false)]
    [string]$ResourceNamePrefix = "subtracker"
)

# Login to Azure (if not already logged in)
$context = Get-AzContext
if (!$context) {
    Write-Host "Please login to Azure..."
    Connect-AzAccount
}

# Create Resource Group if it doesn't exist
$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
if (!$resourceGroup) {
    Write-Host "Creating resource group $ResourceGroupName in $Location..."
    New-AzResourceGroup -Name $ResourceGroupName -Location $Location
}

# Convert SecureString to plain text for deployment
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SqlAdminPassword)
$plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Deploy Bicep template
Write-Host "Deploying SubscriptionTracker resources to Azure..."
New-AzResourceGroupDeployment `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile "$PSScriptRoot\main.bicep" `
    -location $Location `
    -environmentName $EnvironmentName `
    -sqlAdminLogin $SqlAdminLogin `
    -sqlAdminPassword $plainPassword `
    -resourceNamePrefix $ResourceNamePrefix

Write-Host "Deployment completed!"
Write-Host "Please follow the post-deployment steps in README.deployment.md to complete the setup."
