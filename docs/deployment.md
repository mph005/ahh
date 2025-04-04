# Deployment Guide for Massage Therapy Booking System

This document provides step-by-step instructions for deploying the Massage Therapy Booking System to Microsoft Azure. The system consists of an ASP.NET Core Web API backend and a React frontend.

## Prerequisites

Before you begin, ensure you have the following:

- An active Azure subscription
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed and configured
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) installed
- [Node.js](https://nodejs.org/) (v16+) and [npm](https://www.npmjs.com/) (v8+) installed
- [Git](https://git-scm.com/) installed
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Entity Framework Core CLI tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)

## Azure Resources Overview

The deployment requires the following Azure resources:

1. **Azure SQL Database** - For storing application data
2. **Azure App Service** - For hosting the ASP.NET Core Web API
3. **Azure Static Web Apps** - For hosting the React frontend
4. **Azure Active Directory B2C** - For authentication and user management
5. **Azure Application Insights** - For monitoring and diagnostics
6. **Azure Key Vault** - For secrets management

## Step 1: Set Up Azure Resources

### Azure SQL Database

1. **Create a SQL Server and Database:**
   ```bash
   # Login to Azure
   az login

   # Create a resource group
   az group create --name massage-booking-rg --location eastus

   # Create SQL Server
   az sql server create --name massage-booking-sql --resource-group massage-booking-rg \
     --location eastus --admin-user dbadmin --admin-password "YourSecurePassword123!"

   # Configure firewall to allow Azure services
   az sql server firewall-rule create --name AllowAzureServices \
     --server massage-booking-sql --resource-group massage-booking-rg \
     --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

   # Create database
   az sql db create --name MassageBookingDb --resource-group massage-booking-rg \
     --server massage-booking-sql --service-objective S0
   ```

2. **Get the connection string:**
   ```bash
   az sql db show-connection-string --name MassageBookingDb --server massage-booking-sql \
     --client ado.net
   ```
   Note: Replace `{your_username}` and `{your_password}` with your actual SQL Server credentials.

### Azure Key Vault

1. **Create a Key Vault:**
   ```bash
   az keyvault create --name massage-booking-kv --resource-group massage-booking-rg \
     --location eastus
   ```

2. **Store the database connection string:**
   ```bash
   az keyvault secret set --vault-name massage-booking-kv --name "SqlConnectionString" \
     --value "Your-Connection-String-Here"
   ```

### Azure Active Directory B2C

1. **Create an Azure AD B2C tenant** through the Azure portal:
   - Go to Azure portal > Create a resource > Azure Active Directory B2C
   - Follow the wizard to create a new tenant or link to an existing one

2. **Register your application:**
   - In the Azure AD B2C tenant, go to "App registrations"
   - Register a new application for the Web API
   - Configure API permissions, authentication settings, and app roles

3. **Configure user flows:**
   - Create sign-up and sign-in policies
   - Configure password reset flows
   - Set up profile editing policies

## Step 2: Configure and Deploy the Backend API

### Update Application Settings

1. **Create a production appsettings file:**
   ```json
   // appsettings.Production.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=tcp:massage-booking-sql.database.windows.net,1433;Database=MassageBookingDb;..."
     },
     "AzureAdB2C": {
       "Instance": "https://yourtenant.b2clogin.com/",
       "Domain": "yourtenant.onmicrosoft.com",
       "ClientId": "your-client-id",
       "SignUpSignInPolicyId": "B2C_1_signupsignin",
       "TenantId": "your-tenant-id"
     },
     "ApplicationInsights": {
       "ConnectionString": "your-app-insights-connection-string"
     },
     "KeyVault": {
       "Endpoint": "https://massage-booking-kv.vault.azure.net/"
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

2. **Configure the application to use Azure Key Vault** in `Program.cs` or `Startup.cs`.

### Prepare the Database

1. **Update database schema:**
   ```bash
   # Generate a SQL script from migrations
   dotnet ef migrations script --output ./migrations-script.sql --idempotent
   ```

2. **Apply migrations to Azure SQL Database:**
   ```bash
   # Option 1: Use Entity Framework Core
   dotnet ef database update --connection "your-connection-string"

   # Option 2: Execute the generated SQL script using Azure CLI
   az sql db execute -g massage-booking-rg -s massage-booking-sql -d MassageBookingDb \
     -f ./migrations-script.sql
   ```

### Deploy the Web API

1. **Create an App Service Plan:**
   ```bash
   az appservice plan create --name massage-booking-plan --resource-group massage-booking-rg \
     --sku S1 --is-linux
   ```

2. **Create an App Service:**
   ```bash
   az webapp create --name massage-booking-api --resource-group massage-booking-rg \
     --plan massage-booking-plan --runtime "DOTNET|7.0"
   ```

3. **Configure app settings:**
   ```bash
   az webapp config appsettings set --name massage-booking-api \
     --resource-group massage-booking-rg --settings \
     ASPNETCORE_ENVIRONMENT="Production" \
     AZURE_KEY_VAULT_ENDPOINT="https://massage-booking-kv.vault.azure.net/" \
     WEBSITE_RUN_FROM_PACKAGE=1
   ```

4. **Set up managed identity for Key Vault access:**
   ```bash
   # Enable managed identity
   az webapp identity assign --name massage-booking-api --resource-group massage-booking-rg

   # Get the principal ID
   principalId=$(az webapp identity show --name massage-booking-api \
     --resource-group massage-booking-rg --query principalId --output tsv)

   # Grant access to Key Vault
   az keyvault set-policy --name massage-booking-kv --object-id $principalId \
     --secret-permissions get list
   ```

5. **Deploy the API:**
   ```bash
   # Option 1: Deploy from local machine
   dotnet publish -c Release
   cd bin/Release/net7.0/publish
   zip -r site.zip .
   az webapp deployment source config-zip --name massage-booking-api \
     --resource-group massage-booking-rg --src site.zip

   # Option 2: Set up continuous deployment with GitHub Actions or Azure DevOps
   ```

6. **Enable application insights:**
   ```bash
   # Create App Insights resource
   az monitor app-insights component create --app massage-booking-insights \
     --resource-group massage-booking-rg --location eastus --kind web \
     --application-type web

   # Get the instrumentation key
   instrumentationKey=$(az monitor app-insights component show --app massage-booking-insights \
     --resource-group massage-booking-rg --query instrumentationKey --output tsv)

   # Configure the web app
   az webapp config appsettings set --name massage-booking-api \
     --resource-group massage-booking-rg --settings \
     APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey
   ```

## Step 3: Configure and Deploy the Frontend

### Build the React Application

1. **Update environment variables:**
   Create a `.env.production` file in your React project:
   ```
   REACT_APP_API_BASE_URL=https://massage-booking-api.azurewebsites.net/api
   REACT_APP_AUTH_CLIENT_ID=your-b2c-client-id
   REACT_APP_AUTH_AUTHORITY=https://yourtenant.b2clogin.com/yourtenant.onmicrosoft.com/B2C_1_signupsignin
   REACT_APP_AUTH_REDIRECT_URI=https://massage-booking-client.azurestaticapps.net
   ```

2. **Build the production version:**
   ```bash
   cd src/massage-booking-client
   npm install
   npm run build
   ```

### Deploy to Azure Static Web Apps

1. **Create Static Web App:**
   ```bash
   az staticwebapp create --name massage-booking-client --resource-group massage-booking-rg \
     --location "eastus2" --source https://github.com/yourusername/massage-booking-system \
     --branch main --app-location "src/massage-booking-client" --output-location "build" \
     --login-with-github
   ```

2. **Configure API proxy (optional):**
   Create a `staticwebapp.config.json` file in your React project's public directory:
   ```json
   {
     "routes": [
       {
         "route": "/api/*",
         "methods": ["GET", "POST", "PUT", "DELETE"],
         "allowedRoles": ["anonymous", "authenticated"],
         "backendUri": "https://massage-booking-api.azurewebsites.net/api/*"
       }
     ],
     "navigationFallback": {
       "rewrite": "/index.html",
       "exclude": ["/images/*.{png,jpg,gif}", "/css/*", "/js/*"]
     },
     "responseOverrides": {
       "404": {
         "rewrite": "/index.html",
         "statusCode": 200
       }
     }
   }
   ```

3. **Set up GitHub Actions for continuous deployment:**
   - Azure Static Web Apps automatically creates a GitHub Actions workflow
   - Customize the workflow in `.github/workflows/` as needed

## Step 4: Configure CORS, Custom Domain, and SSL

### Configure CORS on the API

1. **Enable CORS for your Static Web App:**
   ```bash
   az webapp cors add --name massage-booking-api --resource-group massage-booking-rg \
     --allowed-origins "https://massage-booking-client.azurestaticapps.net"
   ```

### Add Custom Domains

1. **For App Service:**
   ```bash
   az webapp config hostname add --webapp-name massage-booking-api \
     --resource-group massage-booking-rg --hostname api.yourdomain.com
   ```

2. **For Static Web App:**
   - In the Azure Portal, navigate to your Static Web App
   - Go to "Custom domains"
   - Follow the steps to add and verify your domain

## Step 5: Set Up Monitoring and Alerts

1. **Create dashboards:**
   ```bash
   az portal dashboard create --name "MassageBookingMonitoring" \
     --resource-group massage-booking-rg --location eastus \
     --input-path ./dashboard-template.json
   ```

2. **Configure alerts:**
   ```bash
   # Create an action group for notifications
   az monitor action-group create --name massage-booking-alerts \
     --resource-group massage-booking-rg --short-name mbAlerts \
     --email-receiver name=admin email=admin@yourdomain.com

   # Set up an alert for API response time
   az monitor metrics alert create --name HighResponseTime \
     --resource-group massage-booking-rg \
     --scopes $(az webapp show --name massage-booking-api \
       --resource-group massage-booking-rg --query id -o tsv) \
     --condition "avg HttpResponseTime > 5" \
     --window-size 5m --action $(az monitor action-group show \
       --name massage-booking-alerts --resource-group massage-booking-rg \
       --query id -o tsv)
   ```

## Step 6: Post-Deployment Verification

1. **Verify API endpoints:**
   ```bash
   # Test API health
   curl https://massage-booking-api.azurewebsites.net/api/health

   # Test with authentication token (requires Azure AD B2C token)
   curl -H "Authorization: Bearer YOUR_TOKEN" https://massage-booking-api.azurewebsites.net/api/appointments
   ```

2. **Verify frontend functionality:**
   - Open the static web app URL in a browser
   - Test authentication flows
   - Verify that the frontend can communicate with the API

## Step 7: Backup and Disaster Recovery

1. **Configure database backups:**
   ```bash
   # Enable automated backups
   az sql db update --name MassageBookingDb --resource-group massage-booking-rg \
     --server massage-booking-sql --long-term-retention-backup-resource-id \
     "/subscriptions/{subscription-id}/resourceGroups/massage-booking-rg/providers/Microsoft.Sql/servers/massage-booking-sql/databases/MassageBookingDb"
   ```

2. **Set up geo-replication (optional):**
   ```bash
   # Create a failover group
   az sql failover-group create --name massage-booking-failover \
     --resource-group massage-booking-rg --server massage-booking-sql \
     --partner-server massage-booking-sql-secondary --partner-resource-group massage-booking-rg-secondary \
     --databases MassageBookingDb
   ```

## CI/CD Pipeline with Azure DevOps

### Backend CI/CD Pipeline

Create an `azure-pipelines-backend.yml` file in your repository:

```yaml
trigger:
  branches:
    include:
      - main
  paths:
    include:
      - src/MassageBooking.API/**

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  projectPath: 'src/MassageBooking.API/MassageBooking.API.csproj'

stages:
- stage: Build
  jobs:
  - job: BuildJob
    steps:
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: '$(projectPath)'

    - task: DotNetCoreCLI@2
      displayName: 'Build project'
      inputs:
        command: 'build'
        projects: '$(projectPath)'
        arguments: '--configuration $(buildConfiguration)'

    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        projects: 'tests/**/*.csproj'
        arguments: '--configuration $(buildConfiguration)'

    - task: DotNetCoreCLI@2
      displayName: 'Publish'
      inputs:
        command: 'publish'
        publishWebProjects: true
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        zipAfterPublish: true

    - task: PublishBuildArtifacts@1
      displayName: 'Publish artifacts'
      inputs:
        pathtoPublish: '$(Build.ArtifactStagingDirectory)'
        artifactName: 'api'

- stage: Deploy
  dependsOn: Build
  jobs:
  - deployment: DeployJob
    environment: 'Production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureRmWebAppDeployment@4
            inputs:
              ConnectionType: 'AzureRM'
              azureSubscription: 'Your-Azure-Subscription'
              appType: 'webApp'
              WebAppName: 'massage-booking-api'
              packageForLinux: '$(Pipeline.Workspace)/api/*.zip'
              AppSettings: '-ASPNETCORE_ENVIRONMENT Production'
```

## Troubleshooting

### Common Issues and Resolutions

1. **Connection String Issues:**
   - Verify Key Vault access is properly configured
   - Check that managed identity is enabled on the App Service
   - Ensure the connection string format is correct for Azure SQL

2. **CORS Issues:**
   - Verify CORS settings in the API
   - Check that the frontend is using the correct API URL
   - Ensure authentication headers are being sent correctly

3. **Authentication Problems:**
   - Verify Azure AD B2C policies are correctly configured
   - Check client ID and authority URLs in the frontend configuration
   - Test token acquisition with a tool like Postman

4. **Deployment Failures:**
   - Check app service logs: `az webapp log tail --name massage-booking-api --resource-group massage-booking-rg`
   - Verify build artifacts are correctly configured
   - Check for any missing dependencies or incorrect framework versions

## Security Considerations

- Enable [Azure DDoS Protection](https://docs.microsoft.com/en-us/azure/ddos-protection/ddos-protection-overview)
- Configure [Azure Web Application Firewall](https://docs.microsoft.com/en-us/azure/web-application-firewall/overview)
- Enable [TLS 1.2+ only](https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-bindings#enforce-tls-versions)
- Implement proper [role-based access control (RBAC)](https://docs.microsoft.com/en-us/azure/role-based-access-control/overview)
- Use [Azure Private Link](https://docs.microsoft.com/en-us/azure/private-link/private-link-overview) for secure service access

## Maintenance and Updates

- Configure regular database maintenance
- Set up automatic scaling rules
- Establish a process for regular security updates
- Implement blue-green deployment for zero-downtime updates

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure Static Web Apps Documentation](https://docs.microsoft.com/en-us/azure/static-web-apps/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Azure AD B2C Documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/)
- [Azure DevOps Documentation](https://docs.microsoft.com/en-us/azure/devops/) 