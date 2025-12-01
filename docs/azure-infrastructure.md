# Azure Infrastructure Setup

This document contains all Azure CLI commands used to provision Gridiron infrastructure.

## Prerequisites

- Azure CLI installed (`az --version`)
- Logged into Azure (`az login`)
- Visual Studio Professional subscription with monthly credits

## Resource Naming Convention

| Resource | Pattern | Example |
|----------|---------|---------|
| Resource Group | `rg-<app>-<env>` | `rg-gridiron-dev` |
| App Service Plan | `asp-<app>-<env>` | `asp-gridiron-dev` |
| App Service (API) | `app-<app>-<env>` | `app-gridiron-api-dev` |
| Static Web App | `stapp-<app>-<env>` | `stapp-gridiron-web-dev` |
| SQL Server | `sql-<app>-<env>` | `sql-gridiron-dev` |
| SQL Database | `sqldb-<app>-<env>` | `sqldb-gridiron-dev` |

## Existing Resources (GTG Resource Group)

Using existing resources in `GTG` resource group:
- SQL Server: `gtg-gridiron` (centralus)

## DEV Environment Setup

### 1. App Service Plan

Creates a Linux App Service Plan on Basic B1 tier (~$13/month).

```bash
az appservice plan create \
  --name asp-gridiron-dev \
  --resource-group GTG \
  --location centralus \
  --sku B1 \
  --is-linux
```

### 2. App Service (API)

Creates the App Service for the C# Web API with .NET 9.0 runtime.

```bash
az webapp create \
  --name app-gridiron-api-dev \
  --resource-group GTG \
  --plan asp-gridiron-dev \
  --runtime "DOTNETCORE:9.0"
```

### 3. Configure App Service Settings

#### Connection String (SQL Authentication)

```bash
az webapp config connection-string set \
  --name app-gridiron-api-dev \
  --resource-group GTG \
  --connection-string-type SQLAzure \
  --settings GridironDb="Server=tcp:gtg-gridiron.database.windows.net,1433;Initial Catalog=gtg-gridiron;Persist Security Info=False;User ID=<username>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### Azure Entra Authentication Settings

```bash
az webapp config appsettings set \
  --name app-gridiron-api-dev \
  --resource-group GTG \
  --settings \
    AzureAd__Instance="https://login.microsoftonline.com/" \
    AzureAd__TenantId="<tenant-id>" \
    AzureAd__ClientId="<client-id>" \
    AzureAd__Audience="api://gridiron-api"
```

#### CORS Configuration

```bash
az webapp cors add \
  --name app-gridiron-api-dev \
  --resource-group GTG \
  --allowed-origins "http://localhost:5173" "http://localhost:3000" "https://<static-web-app-url>"
```

### 4. Static Web App (Frontend)

Creates an Azure Static Web App linked to the gridiron-web GitHub repository. This automatically sets up GitHub Actions for deployment.

```bash
az staticwebapp create \
  --name stapp-gridiron-web-dev \
  --resource-group GTG \
  --location centralus \
  --source https://github.com/merciless-creations/gridiron-web \
  --branch master \
  --app-location "/" \
  --output-location "dist" \
  --login-with-github
```

After creation, add the Static Web App URL to API CORS:

```bash
az webapp cors add \
  --name app-gridiron-api-dev \
  --resource-group GTG \
  --allowed-origins "https://<static-web-app-hostname>.azurestaticapps.net"
```

## URLs

- API: https://app-gridiron-api-dev.azurewebsites.net
- Frontend: https://nice-bay-03bfc8b10.3.azurestaticapps.net

## GitHub Actions Deployment

### API Deployment (`gridiron` repo)

Workflow file: `.github/workflows/deploy-api.yml`

**Required GitHub Secret:**

Add `AZURE_WEBAPP_PUBLISH_PROFILE` to the repository secrets. Get the publish profile with:

```bash
az webapp deployment list-publishing-profiles \
  --name app-gridiron-api-dev \
  --resource-group GTG \
  --xml
```

Copy the entire XML output and add it as the secret value.

**Trigger:** Push to master branch

### Frontend Deployment (`gridiron-web` repo)

Automatically configured by Azure Static Web Apps during resource creation. GitHub Actions workflow is created in the gridiron-web repo.

**Trigger:** Push to master branch

## Configuration Values

| Setting | Value | Notes |
|---------|-------|-------|
| SQL Server | gtg-gridiron.database.windows.net | Port 1433 |
| SQL Database | gtg-gridiron | |
| Entra Tenant ID | 8a101213-4cc7-4424-91f7-87fc81ef3a01 | External ID tenant |
| Entra Client ID | 29348959-a014-4550-b3c3-044585c83f0a | Goal to Go Football app |
