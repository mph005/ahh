# Massage Therapy Booking System - Infrastructure as Code

This directory contains the Terraform code for provisioning and managing the infrastructure for the Massage Therapy Booking System in Microsoft Azure.

## Architecture

The infrastructure follows a modular approach, with the following components:

- **Networking**: Virtual Network, Subnets, NSGs, Private DNS
- **Database**: Azure SQL Database with private endpoints
- **Compute**: App Service for API, Static Web App for frontend
- **Security**: Key Vault, Azure AD B2C, Storage for security features
- **Monitoring**: Application Insights, Log Analytics, Diagnostics

## Directory Structure

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
│   ├── staging/                # Staging environment
│   └── prod/                   # Production environment
└── provider.tf                 # Provider configuration
```

## Prerequisites

- [Terraform](https://www.terraform.io/downloads.html) (v1.1.0+)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (v2.20.0+)
- Azure subscription with Contributor access
- Azure AD tenant with permissions to create service principals

## Getting Started

### 1. Authentication

Log in to Azure using the Azure CLI:

```bash
az login
```

### 2. Configure Environment Variables

Copy the example variables file and customize it for your environment:

```bash
cd environments/dev
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` to set your specific values for the environment.

### 3. Initialize Terraform

Initialize the Terraform working directory:

```bash
terraform init
```

### 4. Plan the Deployment

Generate and review an execution plan:

```bash
terraform plan -out=tfplan
```

### 5. Apply the Changes

Apply the changes to create the infrastructure:

```bash
terraform apply tfplan
```

## Terraform State Management

For production use, it's recommended to use remote state storage. After creating a storage account for state, uncomment and edit the backend configuration in `provider.tf`:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "tfstate"
    storage_account_name = "massagetherapytfstate"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}
```

## Multi-Environment Deployment

This project supports multiple environments (dev, staging, prod). To deploy to a different environment:

```bash
cd environments/staging  # or prod
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars for the environment
terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

## Best Practices

1. **Secret Management**: Never store sensitive values in version control. Use Key Vault for secrets.
2. **State Management**: Use remote state with locking to prevent concurrent modifications.
3. **Environment Isolation**: Keep each environment completely separate.
4. **Tagging**: Apply consistent tags for resource management and cost allocation.
5. **Least Privilege**: Follow the principle of least privilege for all resources.

## Common Issues

- **Dependency Cycles**: If you encounter dependency issues, double-check the `depends_on` blocks.
- **Resource Locking**: Azure may lock certain resources during operations. Check the portal for locks.
- **Permission Issues**: Ensure your account has the necessary permissions for all resources.

## Contributing

1. Create a feature branch for your changes
2. Make your changes following the established patterns
3. Test your changes with `terraform validate` and `terraform plan`
4. Submit a pull request for review 