# Deployment Guide for Massage Therapy Booking System

This document provides step-by-step instructions for deploying the Massage Therapy Booking System to Microsoft Azure using Terraform as the Infrastructure as Code (IaC) solution. The system consists of an ASP.NET Core Web API backend and a React frontend.

## Prerequisites

Before you begin, ensure you have the following:

- An active Azure subscription
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed and configured
- [Terraform](https://www.terraform.io/downloads.html) (v1.1.0+) installed
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) installed
- [Node.js](https://nodejs.org/) (v16+) and [npm](https://www.npmjs.com/) (v8+) installed
- [Git](https://git-scm.com/) installed
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Entity Framework Core CLI tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)

## Infrastructure Overview

The Terraform configuration creates the following Azure resources:

1. **Networking** - Virtual Network, Subnets, NSGs, and Private DNS zones
2. **Database** - Azure SQL Database with private endpoints
3. **Compute** - App Service for API, Static Web App for frontend
4. **Security** - Key Vault, Azure AD B2C, Storage for security features
5. **Monitoring** - Application Insights, Log Analytics Workspace, Diagnostics

## Terraform Structure

The Terraform code is organized into modules for better maintainability:

```
terraform/
├── modules/                    # Reusable infrastructure modules
│   ├── networking/             # Network resources (VNet, Subnet, NSG)
│   ├── database/               # Database resources (Azure SQL)
│   ├── compute/                # Compute resources (App Service, Static Web App)
│   ├── security/               # Security resources (Key Vault, AAD B2C)
│   └── monitoring/             # Monitoring resources (App Insights, Log Analytics)
├── environments/               # Environment-specific configurations
│   ├── dev/                    # Development environment
│   ├── staging/                # Staging environment (future)
│   └── prod/                   # Production environment (future)
└── provider.tf                 # Provider configuration
```

## Step 1: Set Up Azure Authentication

1. **Login to Azure**:
   ```bash
   az login
   ```

2. **Set Active Subscription** (if you have multiple):
   ```bash
   az account set --subscription "Your Subscription Name or ID"
   ```

## Step 2: Prepare Terraform State Storage

For production use, it's recommended to use remote state storage in Azure:

1. **Create a Resource Group for Terraform State**:
   ```bash
   az group create --name tfstate --location eastus
   ```

2. **Create a Storage Account**:
   ```bash
   az storage account create --name massagebookingtfstate --resource-group tfstate --sku Standard_LRS --encryption-services blob
   ```

3. **Create a Container**:
   ```bash
   az storage container create --name tfstate --account-name massagebookingtfstate
   ```

4. **Configure Remote State** (update the `provider.tf` file):
   ```hcl
   terraform {
     backend "azurerm" {
       resource_group_name  = "tfstate"
       storage_account_name = "massagebookingtfstate"
       container_name       = "tfstate"
       key                  = "terraform.tfstate"
     }
   }
   ```

## Step 3: Configure Environment Variables

1. **Navigate to the Environment Directory**:
   ```bash
   cd terraform/environments/dev
   ```

2. **Create Terraform Variables File**:
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

3. **Edit Variables**: Customize the `terraform.tfvars` file with your specific configuration:
   ```hcl
   # General
   prefix      = "massage"
   environment = "dev"
   location    = "eastus"
   tags = {
     Environment = "Development"
     Project     = "Massage Booking System"
     Owner       = "Your Team"
   }

   # Networking
   vnet_address_space    = ["10.0.0.0/16"]
   api_subnet_cidr       = "10.0.1.0/24"
   db_subnet_cidr        = "10.0.2.0/24"
   allowed_ip_addresses  = ["your-office-ip/32"]

   # Database
   sql_admin_username    = "sqladmin"
   sql_admin_password    = "YourSecureP@ssw0rd" # Better to use Key Vault

   # Compute
   app_service_sku       = "P1v2"
   static_web_app_sku    = "Standard"

   # Security
   tenant_id = "your-tenant-id"
   devops_service_principal_object_id = "your-sp-object-id"

   # Monitoring
   log_retention_days    = 30
   alert_email           = "your-email@example.com"
   ```

## Step 4: Deploy Infrastructure with Terraform

1. **Initialize Terraform**:
   ```bash
   terraform init
   ```

2. **Validate Configuration**:
   ```bash
   terraform validate
   ```

3. **Plan Deployment**:
   ```bash
   terraform plan -out=tfplan
   ```

4. **Apply Changes**:
   ```bash
   terraform apply tfplan
   ```

5. **Retrieve Outputs**:
   ```bash
   terraform output
   ```

   Save important outputs like connection strings, endpoints, and keys.

## Step 5: Database Migration

1. **Configure the Connection String**:
   Use the SQL connection string from the Terraform outputs.

2. **Apply Migrations**:
   ```bash
   # Use Entity Framework Core
   dotnet ef database update --connection "your-connection-string-from-outputs"
   ```

## Step 6: Configure and Deploy the Backend API

1. **Update Application Settings**:
   Create a production `appsettings.json` file with values from Terraform outputs:
   ```json
   {
     "AzureAdB2C": {
       "Instance": "https://yourtenant.b2clogin.com/",
       "Domain": "yourtenant.onmicrosoft.com",
       "ClientId": "your-client-id-from-terraform",
       "SignUpSignInPolicyId": "B2C_1_signupsignin",
       "TenantId": "your-tenant-id"
     },
     "KeyVault": {
       "Endpoint": "https://your-keyvault-name-from-terraform.vault.azure.net/"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     }
   }
   ```

2. **Deploy the API**:
   ```bash
   # Build and package the application
   dotnet publish -c Release
   cd bin/Release/net7.0/publish
   zip -r site.zip .
   
   # Deploy to App Service
   az webapp deployment source config-zip --name your-api-name-from-terraform \
     --resource-group your-resource-group-from-terraform --src site.zip
   ```

## Step 7: Configure and Deploy the Frontend

1. **Update Environment Variables**:
   Create a `.env.production` file with values from Terraform outputs:
   ```
   REACT_APP_API_BASE_URL=https://your-api-url-from-terraform/api
   REACT_APP_AUTH_CLIENT_ID=your-b2c-client-id-from-terraform
   REACT_APP_AUTH_AUTHORITY=https://yourtenant.b2clogin.com/yourtenant.onmicrosoft.com/B2C_1_signupsignin
   REACT_APP_AUTH_REDIRECT_URI=https://your-frontend-url-from-terraform
   ```

2. **Build and Deploy**:
   ```bash
   # Build the frontend
   cd src/massage-booking-client
   npm install
   npm run build
   
   # Deploy to Static Web App
   # The Static Web App deployment is usually handled through GitHub Actions
   # that were configured by Terraform
   ```

## Step 8: Infrastructure Maintenance

### Updating Infrastructure

1. **Make Changes to Terraform Files**:
   Update module configurations or variables as needed.

2. **Plan and Apply Changes**:
   ```bash
   terraform plan -out=tfplan
   terraform apply tfplan
   ```

### Scaling Resources

Adjust the following variables in your `terraform.tfvars` file:

```hcl
# Scaling App Service
app_service_sku = "P2v2"  # Increase size

# Scaling Database
sql_sku_name = "S1"       # Increase database tier
```

### Adding New Environments

1. **Create a New Environment Directory**:
   ```bash
   mkdir -p terraform/environments/staging
   cp terraform/environments/dev/main.tf terraform/environments/staging/
   cp terraform/environments/dev/variables.tf terraform/environments/staging/
   cp terraform/environments/dev/outputs.tf terraform/environments/staging/
   cp terraform/environments/dev/terraform.tfvars.example terraform/environments/staging/
   ```

2. **Customize Variables** for the new environment.

3. **Deploy the New Environment**:
   ```bash
   cd terraform/environments/staging
   terraform init
   terraform plan -out=tfplan
   terraform apply tfplan
   ```

## Step 9: Troubleshooting

### Common Terraform Issues

1. **State Lock Issues**:
   ```bash
   # Force unlock state (use with caution)
   terraform force-unlock LOCK_ID
   ```

2. **Resource Creation Failures**:
   - Check Azure Activity Log in the portal
   - Review Terraform logs with increased verbosity:
     ```bash
     TF_LOG=DEBUG terraform apply
     ```

3. **Authentication Issues**:
   - Ensure Azure CLI is logged in: `az login`
   - Check if token is expired: `az account get-access-token`

### Application Issues

1. **Connection String Problems**:
   - Verify Key Vault access policies
   - Check managed identity configuration
   - Ensure correct connection string format

2. **CORS Issues**:
   - Verify frontend URL is added to CORS allowlist in App Service

3. **Authentication Problems**:
   - Check AAD B2C policies and application registrations
   - Verify client IDs and redirect URIs

## Step 10: Security Best Practices

1. **Secrets Management**:
   - Store sensitive values in Key Vault
   - Use Terraform variables marked as sensitive
   - Consider using Azure Key Vault provider for Terraform

2. **Network Security**:
   - Use private endpoints for services where possible
   - Restrict public access with NSGs
   - Enable Azure DDoS Protection

3. **Identity and Access**:
   - Use managed identities for Azure resources
   - Implement proper role-based access control (RBAC)
   - Enable MFA for all administrative accounts

## Additional Resources

- [Terraform Azure Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure Static Web Apps Documentation](https://docs.microsoft.com/en-us/azure/static-web-apps/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Azure AD B2C Documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/)

## Module-Specific Configuration

### Networking Module

The networking module creates the foundation for the infrastructure including virtual networks, subnets, and network security groups.

```hcl
module "networking" {
  source = "../modules/networking"
  
  prefix                = var.prefix
  environment           = var.environment
  location              = var.location
  resource_group_name   = azurerm_resource_group.main.name
  vnet_address_space    = var.vnet_address_space
  api_subnet_cidr       = var.api_subnet_cidr
  db_subnet_cidr        = var.db_subnet_cidr
  allowed_ip_addresses  = var.allowed_ip_addresses
  
  tags                  = var.tags
}
```

### Database Module

The database module creates an Azure SQL Server and Database with appropriate security settings.

```hcl
module "database" {
  source = "../modules/database"
  
  prefix              = var.prefix
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  subnet_id           = module.networking.db_subnet_id
  admin_username      = var.sql_admin_username
  admin_password      = var.sql_admin_password
  
  tags                = var.tags
}
```

### Compute Module

The compute module sets up App Service and Static Web App resources.

```hcl
module "compute" {
  source = "../modules/compute"
  
  prefix                      = var.prefix
  environment                 = var.environment
  location                    = var.location
  resource_group_name         = azurerm_resource_group.main.name
  app_subnet_id               = module.networking.app_subnet_id
  app_service_sku             = var.app_service_sku
  static_web_app_sku          = var.static_web_app_sku
  key_vault_id                = module.security.key_vault_id
  app_insights_instrumentation_key = module.monitoring.app_insights_instrumentation_key
  
  tags                        = var.tags
}
```

### Security Module

The security module provisions Key Vault, AAD B2C configurations, and security-related storage.

```hcl
module "security" {
  source = "../modules/security"
  
  prefix                          = var.prefix
  environment                     = var.environment
  location                        = var.location
  resource_group_name             = azurerm_resource_group.main.name
  tenant_id                       = var.tenant_id
  app_subnet_id                   = module.networking.app_subnet_id
  allowed_ips                     = var.allowed_ip_addresses
  devops_service_principal_object_id = var.devops_service_principal_object_id
  sql_connection_string           = module.database.sql_connection_string
  app_insights_connection_string  = module.monitoring.app_insights_connection_string
  
  tags                            = var.tags
}
```

### Monitoring Module

The monitoring module creates Application Insights, Log Analytics, and diagnostic settings.

```hcl
module "monitoring" {
  source = "../modules/monitoring"
  
  prefix              = var.prefix
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  api_app_id          = module.compute.api_app_id
  sql_database_id     = module.database.sql_database_id
  alert_email         = var.alert_email
  
  tags                = var.tags
}
```

## Terraform Workspaces

Instead of using separate directories for each environment, you can use Terraform workspaces to manage multiple environments.

### Creating and Using Workspaces

1. **Initialize Terraform**:
   ```bash
   terraform init
   ```

2. **Create Workspaces**:
   ```bash
   terraform workspace new dev
   terraform workspace new staging
   terraform workspace new prod
   ```

3. **Select Workspace**:
   ```bash
   terraform workspace select dev
   ```

4. **Use Conditional Logic Based on Workspace**:
   ```hcl
   locals {
     environment = terraform.workspace
     
     # Conditional settings based on workspace
     settings = {
       dev = {
         instance_count = 1
         instance_size  = "Standard_B2s"
       }
       staging = {
         instance_count = 2
         instance_size  = "Standard_D2s_v3"
       }
       prod = {
         instance_count = 3
         instance_size  = "Standard_D4s_v3"
       }
     }
     
     # Use the settings for the current workspace
     current_settings = local.settings[local.environment]
   }
   ```

## CI/CD Integration

### Azure DevOps Pipeline

Create an `azure-pipelines.yml` file for Terraform deployment:

```yaml
trigger:
  branches:
    include:
      - main
  paths:
    include:
      - terraform/**

pool:
  vmImage: 'ubuntu-latest'

variables:
  - group: terraform-secrets
  - name: environment
    value: 'dev'

stages:
- stage: Validate
  jobs:
  - job: ValidateTerraform
    steps:
    - task: TerraformInstaller@0
      displayName: 'Install Terraform'
      inputs:
        terraformVersion: '1.3.0'
    
    - task: TerraformCLI@0
      displayName: 'Terraform Init'
      inputs:
        command: 'init'
        workingDirectory: '$(System.DefaultWorkingDirectory)/terraform/environments/$(environment)'
        backendType: 'azurerm'
        backendServiceArm: 'Azure-Service-Connection'
        backendAzureRmResourceGroupName: 'tfstate'
        backendAzureRmStorageAccountName: 'massagebookingtfstate'
        backendAzureRmContainerName: 'tfstate'
        backendAzureRmKey: '$(environment).terraform.tfstate'
    
    - task: TerraformCLI@0
      displayName: 'Terraform Validate'
      inputs:
        command: 'validate'
        workingDirectory: '$(System.DefaultWorkingDirectory)/terraform/environments/$(environment)'
    
    - task: TerraformCLI@0
      displayName: 'Terraform Plan'
      inputs:
        command: 'plan'
        workingDirectory: '$(System.DefaultWorkingDirectory)/terraform/environments/$(environment)'
        environmentServiceName: 'Azure-Service-Connection'
        publishPlanResults: 'tfplan'
        commandOptions: '-var-file="terraform.tfvars" -out=tfplan'

- stage: Apply
  dependsOn: Validate
  condition: succeeded()
  jobs:
  - job: ApplyTerraform
    steps:
    - task: TerraformInstaller@0
      displayName: 'Install Terraform'
      inputs:
        terraformVersion: '1.3.0'
    
    - task: TerraformCLI@0
      displayName: 'Terraform Init'
      inputs:
        command: 'init'
        workingDirectory: '$(System.DefaultWorkingDirectory)/terraform/environments/$(environment)'
        backendType: 'azurerm'
        backendServiceArm: 'Azure-Service-Connection'
        backendAzureRmResourceGroupName: 'tfstate'
        backendAzureRmStorageAccountName: 'massagebookingtfstate'
        backendAzureRmContainerName: 'tfstate'
        backendAzureRmKey: '$(environment).terraform.tfstate'
    
    - task: TerraformCLI@0
      displayName: 'Terraform Apply'
      inputs:
        command: 'apply'
        workingDirectory: '$(System.DefaultWorkingDirectory)/terraform/environments/$(environment)'
        environmentServiceName: 'Azure-Service-Connection'
        commandOptions: 'tfplan'
```

### GitHub Actions Workflow

Create a `.github/workflows/terraform.yml` file:

```yaml
name: 'Terraform CI/CD'

on:
  push:
    branches: [ main ]
    paths:
      - 'terraform/**'
  pull_request:
    branches: [ main ]
    paths:
      - 'terraform/**'

jobs:
  terraform:
    name: 'Terraform'
    runs-on: ubuntu-latest
    environment: dev

    steps:
    - name: Checkout
      uses: actions/checkout@v2

    - name: Setup Terraform
      uses: hashicorp/setup-terraform@v1
      with:
        terraform_version: 1.3.0

    - name: Terraform Format
      id: fmt
      run: terraform fmt -check -recursive
      working-directory: ./terraform

    - name: Configure Azure credentials
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}

    - name: Terraform Init
      id: init
      run: terraform init -backend-config="storage_account_name=massagebookingtfstate" -backend-config="container_name=tfstate" -backend-config="key=dev.terraform.tfstate" -backend-config="resource_group_name=tfstate"
      working-directory: ./terraform/environments/dev

    - name: Terraform Validate
      id: validate
      run: terraform validate
      working-directory: ./terraform/environments/dev

    - name: Terraform Plan
      id: plan
      if: github.event_name == 'pull_request'
      run: terraform plan -var-file="terraform.tfvars" -no-color
      working-directory: ./terraform/environments/dev
      continue-on-error: true

    - name: Update Pull Request
      uses: actions/github-script@v5
      if: github.event_name == 'pull_request'
      with:
        github-token: ${{ secrets.GITHUB_TOKEN }}
        script: |
          const output = `#### Terraform Format and Style 🖌\`${{ steps.fmt.outcome }}\`
          #### Terraform Initialization ⚙️\`${{ steps.init.outcome }}\`
          #### Terraform Validation 🤖\`${{ steps.validate.outcome }}\`
          #### Terraform Plan 📖\`${{ steps.plan.outcome }}\`

          <details><summary>Show Plan</summary>
          
          \`\`\`
          ${process.env.PLAN}
          \`\`\`
          
          </details>`;
          
          github.rest.issues.createComment({
            issue_number: context.issue.number,
            owner: context.repo.owner,
            repo: context.repo.repo,
            body: output
          })

    - name: Terraform Apply
      if: github.ref == 'refs/heads/main' && github.event_name == 'push'
      run: terraform apply -var-file="terraform.tfvars" -auto-approve
      working-directory: ./terraform/environments/dev
```

## State Management Best Practices

### Remote State with Locking

To prevent concurrent modifications, use Azure Storage with state locking:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "tfstate"
    storage_account_name = "massagebookingtfstate"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
    use_azuread_auth     = true
    subscription_id      = "your-subscription-id"
    tenant_id            = "your-tenant-id"
  }
}
```

### Handling Sensitive Data

1. **Use Variables Marked as Sensitive**:
   ```hcl
   variable "sql_admin_password" {
     description = "The password for the SQL administrator"
     type        = string
     sensitive   = true
   }
   ```

2. **Store Secrets in Key Vault**:
   ```hcl
   data "azurerm_key_vault_secret" "sql_password" {
     name         = "SqlAdminPassword"
     key_vault_id = data.azurerm_key_vault.main.id
   }
   ```

3. **Use Environment Variables**:
   ```bash
   export TF_VAR_sql_admin_password="YourSecurePassword"
   ```

### State Security

1. **Enable Storage Account Encryption**:
   ```bash
   az storage account update --name massagebookingtfstate --resource-group tfstate --require-infrastructure-encryption true
   ```

2. **Restrict Access to Storage Account**:
   ```bash
   az storage account network-rule add --account-name massagebookingtfstate --resource-group tfstate --subnet your-subnet-id
   ```

## Drift Detection and Remediation

### Detect Drift

1. **Use Terraform Plan to Check for Drift**:
   ```bash
   terraform plan -detailed-exitcode
   ```
   - Exit code 0: No changes
   - Exit code 1: Error
   - Exit code 2: Plan contains changes

2. **Scheduled Drift Detection**:
   Add a scheduled task in your CI/CD system to run terraform plan and alert on drift.

### Remediate Drift

1. **Import Resources Back into State**:
   ```bash
   terraform import module.security.azurerm_key_vault.key_vault /subscriptions/your-subscription-id/resourceGroups/your-resource-group/providers/Microsoft.KeyVault/vaults/your-key-vault
   ```

2. **Update State to Match Reality**:
   ```bash
   terraform apply -refresh-only
   ```

3. **Force Replacement When Necessary**:
   ```bash
   terraform apply -replace="module.compute.azurerm_app_service.api"
   ```

## Terraform Import

### Importing Existing Resources

If you already have resources deployed manually or through another process, import them into Terraform:

1. **Create Resource Configuration First**:
   ```hcl
   resource "azurerm_resource_group" "main" {
     name     = "massage-booking-rg"
     location = "eastus"
     tags = {
       Environment = "Production"
     }
   }
   ```

2. **Import Resource**:
   ```bash
   terraform import azurerm_resource_group.main /subscriptions/your-subscription-id/resourceGroups/massage-booking-rg
   ```

3. **Import Resources with Modules**:
   ```bash
   terraform import module.compute.azurerm_app_service.api /subscriptions/your-subscription-id/resourceGroups/massage-booking-rg/providers/Microsoft.Web/sites/massage-booking-api
   ```

### Bulk Import

For multiple resources, create a script:

```bash
#!/bin/bash

# Resource group
terraform import azurerm_resource_group.main /subscriptions/subscription-id/resourceGroups/massage-booking-rg

# Storage account
terraform import module.security.azurerm_storage_account.security_storage /subscriptions/subscription-id/resourceGroups/massage-booking-rg/providers/Microsoft.Storage/storageAccounts/massagebookingstorage

# Key Vault
terraform import module.security.azurerm_key_vault.key_vault /subscriptions/subscription-id/resourceGroups/massage-booking-rg/providers/Microsoft.KeyVault/vaults/massage-booking-kv

# And so on...
```

## Output Documentation

### Key Terraform Outputs

After applying your Terraform configuration, you'll have access to these important outputs:

```hcl
output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "api_url" {
  description = "The URL of the API App Service"
  value       = module.compute.api_url
}

output "frontend_url" {
  description = "The URL of the Static Web App"
  value       = module.compute.frontend_url
}

output "sql_connection_string" {
  description = "The connection string for the SQL database"
  value       = module.database.sql_connection_string
  sensitive   = true
}

output "key_vault_uri" {
  description = "The URI of the Key Vault"
  value       = module.security.key_vault_uri
}

output "app_insights_instrumentation_key" {
  description = "The instrumentation key for Application Insights"
  value       = module.monitoring.app_insights_instrumentation_key
  sensitive   = true
}
```

### Using Outputs in Scripts

You can use these outputs in other scripts:

```bash
#!/bin/bash

# Get outputs from Terraform
API_URL=$(terraform output -raw api_url)
RESOURCE_GROUP=$(terraform output -raw resource_group_name)

# Use the outputs
echo "API is available at: $API_URL"
echo "Resource Group: $RESOURCE_GROUP"

# Deploy application to the infrastructure
az webapp deployment source config-zip \
  --name $(echo $API_URL | cut -d'/' -f3 | cut -d'.' -f1) \
  --resource-group $RESOURCE_GROUP \
  --src ./api.zip
```

## Cost Optimization

### Resource Sizing Best Practices

1. **Right-Size Resources**:
   ```hcl
   # Development environment - smaller resources
   app_service_sku = "B1"
   sql_sku_name    = "Basic"
   
   # Production environment - appropriately sized
   app_service_sku = "P1v2"
   sql_sku_name    = "S1"
   ```

2. **Auto-Scaling Configuration**:
   ```hcl
   resource "azurerm_monitor_autoscale_setting" "app_service_autoscale" {
     name                = "${var.prefix}-${var.environment}-autoscale"
     resource_group_name = var.resource_group_name
     location            = var.location
     target_resource_id  = azurerm_app_service_plan.app_plan.id
     
     profile {
       name = "Default"
       
       capacity {
         default = 1
         minimum = 1
         maximum = 3
       }
       
       rule {
         metric_trigger {
           metric_name        = "CpuPercentage"
           metric_resource_id = azurerm_app_service_plan.app_plan.id
           time_grain         = "PT1M"
           statistic          = "Average"
           time_window        = "PT5M"
           time_aggregation   = "Average"
           operator           = "GreaterThan"
           threshold          = 75
         }
         
         scale_action {
           direction = "Increase"
           type      = "ChangeCount"
           value     = "1"
           cooldown  = "PT5M"
         }
       }
       
       rule {
         metric_trigger {
           metric_name        = "CpuPercentage"
           metric_resource_id = azurerm_app_service_plan.app_plan.id
           time_grain         = "PT1M"
           statistic          = "Average"
           time_window        = "PT5M"
           time_aggregation   = "Average"
           operator           = "LessThan"
           threshold          = 25
         }
         
         scale_action {
           direction = "Decrease"
           type      = "ChangeCount"
           value     = "1"
           cooldown  = "PT5M"
         }
       }
     }
   }
   ```

### Cost Reduction Strategies

1. **Dev/Test Subscriptions**:
   Use Azure Dev/Test subscriptions for non-production environments.

2. **Reserved Instances**:
   For stable workloads, use Azure Reserved Instances to reduce costs.

3. **Resource Scheduling**:
   ```hcl
   # Use Azure Automation for scheduling
   resource "azurerm_automation_account" "automation" {
     name                = "${var.prefix}-${var.environment}-automation"
     location            = var.location
     resource_group_name = var.resource_group_name
     sku_name            = "Basic"
   }
   
   resource "azurerm_automation_schedule" "stop_schedule" {
     name                    = "StopVMsEvening"
     resource_group_name     = var.resource_group_name
     automation_account_name = azurerm_automation_account.automation.name
     frequency               = "Day"
     interval                = 1
     timezone                = "America/New_York"
     start_time              = "2023-01-01T19:00:00-05:00"
     description             = "Stop VMs every evening at 7 PM"
   }
   ```

4. **Resource Tagging for Cost Allocation**:
   ```hcl
   tags = {
     Environment   = var.environment
     Project       = "MassageBooking"
     Owner         = "IT Department"
     CostCenter    = "CC123"
     BusinessUnit  = "Healthcare"
   }
   ```

## Disaster Recovery

### Database Backup and Restore

1. **Configure Long-Term Backup Retention**:
   ```hcl
   resource "azurerm_mssql_database_extended_auditing_policy" "example" {
     database_id                             = module.database.sql_database_id
     storage_endpoint                        = module.security.primary_blob_endpoint
     storage_account_access_key              = module.security.primary_access_key
     storage_account_access_key_is_secondary = false
     retention_in_days                       = 90
   }
   ```

2. **Geo-Replication for Production**:
   ```hcl
   resource "azurerm_sql_failover_group" "failover" {
     name                = "${var.prefix}-${var.environment}-failover-group"
     resource_group_name = var.resource_group_name
     server_name         = module.database.sql_server_name
     databases           = [module.database.sql_database_id]
     
     partner_servers {
       id = azurerm_sql_server.secondary.id
     }
     
     read_write_endpoint_failover_policy {
       mode          = "Automatic"
       grace_minutes = 60
     }
   }
   ```

### Infrastructure Recovery Plan

1. **Store Terraform State with Redundancy**:
   Configure the storage account for state to use geo-redundant storage (GRS).

2. **Document Recovery Procedures**:
   Create a runbook for infrastructure recovery:
   
   a. Recover state from backup if necessary
   b. Run `terraform init` and `terraform apply`
   c. Restore database from backup if needed
   d. Verify application functionality

3. **Regular Recovery Testing**:
   Schedule quarterly disaster recovery tests.

## Compliance and Governance

### Implementing Azure Policy

```hcl
resource "azurerm_policy_assignment" "require_tags" {
  name                 = "require-tags"
  scope                = azurerm_resource_group.main.id
  policy_definition_id = "/providers/Microsoft.Authorization/policyDefinitions/871b6d14-10aa-478d-b590-94f262ecfa99"
  description          = "Requires specified tags on all resources"
  display_name         = "Require specific tags on resources"
  
  parameters = <<PARAMETERS
  {
    "tagName": {
      "value": "Environment"
    }
  }
  PARAMETERS
}
```

### Security and Compliance Monitoring

1. **Enable Azure Security Center**:
   ```hcl
   resource "azurerm_security_center_subscription_pricing" "standard" {
     tier          = "Standard"
     resource_type = "VirtualMachines"
   }
   
   resource "azurerm_security_center_contact" "security_contact" {
     email = "security@yourcompany.com"
     phone = "+1-555-123-4567"
     
     alert_notifications = true
     alerts_to_admins    = true
   }
   ```

2. **Compliance Reporting**:
   Configure regular compliance reports using Azure Policy and Azure Security Center.

### Governance Framework

1. **Resource Naming Standards**:
   ```hcl
   locals {
     resource_naming = {
       key_vault       = "${var.prefix}-${var.environment}-kv"
       sql_server      = "${var.prefix}${var.environment}sql"
       app_service     = "${var.prefix}-${var.environment}-app"
       storage_account = "${var.prefix}${var.environment}st"
     }
   }
   ```

2. **Role-Based Access Control (RBAC)**:
   ```hcl
   resource "azurerm_role_assignment" "webapp_keyvault_reader" {
     scope                = module.security.key_vault_id
     role_definition_name = "Key Vault Reader"
     principal_id         = module.compute.webapp_principal_id
   }
   ```

3. **Change Management Processes**:
   Document procedures for:
   - Approving infrastructure changes
   - Testing changes before production
   - Rolling back failed changes 

## Azure AD B2C Manual Configuration

While Terraform creates the Azure AD B2C application registrations, several aspects of B2C configuration require manual setup in the Azure portal. Follow these steps after the Terraform deployment to complete your B2C setup.

### Step 1: Create or Configure Azure AD B2C Tenant

If you don't already have an Azure AD B2C tenant:

1. **Navigate to Azure AD B2C**:
   - Go to the Azure Portal
   - Search for "Azure AD B2C" and select it

2. **Create a New B2C Tenant**:
   - Click "Create a new tenant"
   - Select "Create a new Azure AD B2C tenant"
   - Fill in the organization name, initial domain name, country/region
   - Click "Review + create" and then "Create"

3. **Switch to Your B2C Tenant**:
   - Click the directory switcher in the top right corner of the Azure portal
   - Select your newly created B2C tenant

### Step 2: Configure User Flows

User flows define the user experience for sign-up, sign-in, and profile editing.

1. **Create a Sign-up and Sign-in User Flow**:
   - In your B2C tenant, navigate to "User flows"
   - Click "New user flow"
   - Select "Sign up and sign in"
   - Choose "Recommended" version
   - Enter a name (e.g., "B2C_1_signupsignin")
   
   - **Configure Identity Providers**:
     - Select "Email signup" at minimum
     - Add any social providers you've configured
   
   - **Configure Multi-factor Authentication**:
     - Choose when MFA is required (recommended for production)
   
   - **Collect User Attributes**:
     - Select the attributes to collect during sign-up (e.g., Display Name, Email Address, Given Name, Surname)
     - Select which attributes you want returned in the token (e.g., Display Name, Email Address, User's Object ID)
   
   - Click "Create"

2. **Create a Password Reset User Flow**:
   - Click "New user flow"
   - Select "Password reset"
   - Enter a name (e.g., "B2C_1_passwordreset")
   - Select email as the verification method
   - Select which attributes to collect and return in the token
   - Click "Create"

3. **Create a Profile Editing User Flow**:
   - Click "New user flow"
   - Select "Profile editing"
   - Enter a name (e.g., "B2C_1_profileedit")
   - Select the attributes users can edit
   - Select attributes to return in the token
   - Click "Create"

### Step 3: Configure Custom Attributes (if needed)

If your application requires custom user attributes beyond the standard ones:

1. **Create Custom Attributes**:
   - In your B2C tenant, navigate to "User attributes"
   - Click "Add"
   - Enter the name of your custom attribute (e.g., "PreferredTherapist")
   - Select the data type (String, Boolean, etc.)
   - Click "Create"

2. **Add Custom Attributes to User Flows**:
   - Go back to your user flows
   - Select the user flow you want to modify
   - Click "User attributes"
   - Check the custom attributes you want to collect and/or return in the token
   - Click "Save"

### Step 4: Configure Identity Providers (Optional)

To allow users to sign in with social accounts:

1. **Add a Social Identity Provider**:
   - In your B2C tenant, navigate to "Identity providers"
   - Select the provider you want to add (e.g., Google, Facebook, Microsoft)
   - Follow the provider-specific instructions to obtain client ID and secret
   - Enter the required information
   - Click "Save"

2. **Update User Flows to Include the Provider**:
   - Navigate to your sign-up/sign-in user flow
   - Select "Identity providers"
   - Check the social provider you just added
   - Click "Save"

### Step 5: Customize UI and Branding

Customize the user experience with your branding:

1. **Configure Company Branding**:
   - In your B2C tenant, navigate to "Company branding"
   - Upload your logo, background image, and customize colors
   - Click "Save"

2. **Customize Page Templates** (Advanced):
   - Navigate to "User flows"
   - Select your user flow
   - Click "Page layouts"
   - Select the page you want to customize
   - Download the default template
   - Modify the HTML and CSS as needed
   - Upload the customized template
   - Click "Save"

### Step 6: Update Terraform Configuration with B2C Details

After manual configuration, update your Terraform configuration with the relevant B2C information:

1. **Update your `terraform.tfvars` file**:
   ```hcl
   b2c_tenant_name = "yourtenant.onmicrosoft.com"
   b2c_signup_signin_policy_name = "B2C_1_signupsignin"
   b2c_password_reset_policy_name = "B2C_1_passwordreset"
   b2c_profile_edit_policy_name = "B2C_1_profileedit"
   ```

2. **Apply the updated configuration**:
   ```bash
   terraform plan -out=tfplan
   terraform apply tfplan
   ```

### Step 7: Configure Your Applications to Use B2C

1. **Update API Configuration**:
   - In your API's `appsettings.json`, update the B2C settings:
   ```json
   "AzureAdB2C": {
     "Instance": "https://yourtenant.b2clogin.com/",
     "Domain": "yourtenant.onmicrosoft.com",
     "ClientId": "your-api-client-id-from-terraform",
     "SignUpSignInPolicyId": "B2C_1_signupsignin",
     "TenantId": "your-b2c-tenant-id"
   }
   ```

2. **Update Frontend Configuration**:
   - In your React application's `.env.production` file:
   ```
   REACT_APP_AUTH_CLIENT_ID=your-frontend-client-id-from-terraform
   REACT_APP_AUTH_AUTHORITY=https://yourtenant.b2clogin.com/yourtenant.onmicrosoft.com/B2C_1_signupsignin
   REACT_APP_AUTH_REDIRECT_URI=https://your-frontend-url-from-terraform
   REACT_APP_AUTH_SIGN_UP_POLICY=B2C_1_signupsignin
   REACT_APP_AUTH_RESET_PASSWORD_POLICY=B2C_1_passwordreset
   REACT_APP_AUTH_EDIT_PROFILE_POLICY=B2C_1_profileedit
   ```

### Step 8: Test Your B2C Implementation

1. **Test Sign-up and Sign-in**:
   - Navigate to your user flow in the Azure portal
   - Click "Run user flow"
   - Test the sign-up process with a test account
   - Test the sign-in process with the created account

2. **Test Password Reset**:
   - Run the password reset user flow
   - Follow the process to reset a password

3. **Test with Your Application**:
   - Deploy your application with the updated configuration
   - Try to sign up, sign in, and use other B2C features from your actual application

### Troubleshooting B2C Issues

1. **Check Application Registration**:
   - Verify the redirect URIs in your application registration
   - Ensure your application has the necessary API permissions

2. **Examine Sign-in Logs**:
   - In your B2C tenant, navigate to "Sign-ins"
   - Look for failed sign-in attempts and error messages

3. **Verify Token Configuration**:
   - Check that your application is requesting and receiving the correct claims
   - Verify that the token audience matches your application's client ID

4. **Common Issues**:
   - CORS errors: Ensure your application's domain is properly configured in B2C
   - Redirect URI mismatch: The URI in your application code must match exactly what's registered in B2C
   - Missing claims: Ensure you've selected the necessary attributes to return in the token

## Manual Steps Required Outside of Terraform

While Terraform automates most of the infrastructure deployment, several aspects require manual configuration in the Azure portal either before or after applying the Terraform configuration.

### Pre-Terraform Deployment Steps

#### 1. Create Service Principal for Terraform

Terraform needs a Service Principal to interact with Azure:

1. **Create Service Principal**:
   ```bash
   az ad sp create-for-rbac --name "MassageBookingTerraform" --role="Contributor" --scopes="/subscriptions/your-subscription-id"
   ```

2. **Save the credentials**:
   - Store the `appId`, `password`, `tenant` values securely
   - Use these values for authentication in CI/CD pipelines or store as environment variables

#### 2. Prepare Subscription for Azure AD B2C Integration

1. **Register Resource Provider**:
   ```bash
   az provider register --namespace "Microsoft.AzureActiveDirectory"
   ```

### Post-Terraform Deployment Steps

#### 1. Configure Custom Domains

Custom domains need to be manually configured:

1. **For App Service**:
   - In the Azure Portal, navigate to your App Service
   - Go to "Custom domains"
   - Select "Add custom domain"
   - Enter your domain name (e.g., `api.yourdomain.com`)
   - Verify domain ownership through either DNS verification or domain verification
   - Add TXT or CNAME records as instructed
   - Once verified, click "Add custom domain"
   - Navigate to "TLS/SSL settings" to add a certificate

2. **For Static Web App**:
   - In the Azure Portal, navigate to your Static Web App
   - Go to "Custom domains"
   - Click "Add"
   - Enter your domain name (e.g., `booking.yourdomain.com`)
   - Add the required DNS records as instructed
   - Validate and add the domain

#### 2. Set Up Email Notifications for Backups

1. **Configure Backup Alerts**:
   - In the Azure Portal, navigate to your SQL Server
   - Select "SQL databases" and then your database
   - Go to "Backups"
   - Click on "Configure backup alerts"
   - Add email recipients for backup failure notifications
   - Set up the alert conditions

#### 3. Configure CDN (Optional)

For better performance, you might want to set up a CDN:

1. **Create CDN Profile**:
   - In the Azure Portal, search for "CDN profiles"
   - Click "Create"
   - Select your subscription and resource group
   - Fill in the name and pricing tier
   - Click "Create"

2. **Add CDN Endpoint**:
   - Once the CDN profile is created, navigate to it
   - Click "Add endpoint"
   - Configure the endpoint with your Static Web App URL
   - Set up caching rules as needed

#### 4. Set Up Database Maintenance Tasks

1. **Index Maintenance**:
   - In the Azure Portal, navigate to your SQL database
   - Go to "Query editor"
   - Create scheduled maintenance jobs for index rebuilding/reorganizing

#### 5. Configure VNet Service Endpoints

If you're using a hybrid network architecture:

1. **Set Up VNet Integration**:
   - In the Azure Portal, navigate to your App Service
   - Go to "Networking" > "VNet integration"
   - Click "Add VNet" and configure the integration with your existing network

#### 6. Configure CORS for API App Service

For the frontend to communicate with your API:

1. **Set Up CORS Rules**:
   - In the Azure Portal, navigate to your App Service
   - Go to "API" > "CORS"
   - Add your frontend domain to the allowed origins
   - Check "Enable Access-Control-Allow-Credentials"
   - Click "Save"

#### 7. Register Application Insights with API

If using Application Insights with manual instrumentation:

1. **Update Application Settings**:
   - In the Azure Portal, navigate to your App Service
   - Go to "Configuration" > "Application settings"
   - Add the Application Insights connection string from your Terraform outputs
   - Click "Save"

### Ongoing Manual Operations

These operations need to be performed periodically:

1. **Certificate Renewal**:
   - Monitor SSL certificate expiration dates
   - Renew certificates before expiration
   - Update certificates in Azure Key Vault

2. **Review and Adjust Auto-Scaling Rules**:
   - Monitor application performance
   - Adjust scaling thresholds based on actual usage patterns

3. **Database Performance Tuning**:
   - Review query performance with Azure SQL Database Advisor
   - Implement recommended performance improvements

// ... existing code ... 