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