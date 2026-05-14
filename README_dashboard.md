# P2P Dashboard

P2P Dashboard is a full-stack monitoring application with:
- **Dashboard API**: ASP.NET Core (`net8.0`)
- **Dashboard Client**: Angular (`15.2.10`)
- **Automation**: PowerShell-based build and deployment pipeline for Windows environments

## Repository Structure

- `src/presentation/dashboardApi` — backend API and SignalR hub
- `src/presentation/dashboard` — Angular dashboard client
- `automation/build/scripts` — end-to-end build, packaging, and deploy scripts
- `docs/deployment` — deployment and NSSM service setup guides
- `P2PDashboard.sln` — .NET solution entry point

## Prerequisites

- Windows machine (recommended for current automation scripts)
- .NET SDK 8.x
- Node.js (project build docs target `v14.20.0`)
- npm
- PowerShell 5.1+
- NSSM (for Windows service hosting)

## Local Development Workflow

## 1) Run API locally

From repo root:

```powershell
dotnet restore .\P2PDashboard.sln
dotnet build .\P2PDashboard.sln -c Debug
dotnet run --project .\src\presentation\dashboardApi\DashboardApi.csproj
```

Notes:
- API configuration is in `src/presentation/dashboardApi/appsettings.json`.
- Update DB connection, SMTP settings, and allowed origins for your environment.

## 2) Run Angular client locally

```powershell
Set-Location .\src\presentation\dashboard
npm install
npm run start
```

Notes:
- Dev server defaults to port `4200`.
- Set API endpoint in Angular environment files (for example `src/environments/environment.ts` or `environment.prod.ts`).

## Build & Packaging Workflow (Automation)

All build automation scripts are in `automation/build/scripts`.

### Typical sequence

1. Prepare environment variables:

```powershell
Set-Location .\automation\build\scripts
.\prep-environment.ps1
```

2. Run the main build:

```powershell
.\1-build.ps1 -version "1.0.0" -build_type "beta" -configuration "Release" -clean_required $true
```

`1-build.ps1` orchestrates:
- code build (`2-build-code.ps1`)
- API build (`2_1-build-dashboard-api-code.ps1`)
- client build (`2_2-build-dashboard-client-code.ps1`)
- post-build operations (`3-post-build.ps1`)

### Build types

Supported values:
- `feature`
- `alpha`
- `beta`
- `rc`
- `production`
- `pr`

## Deployment Workflow

For package deployment to remote Windows server:

```powershell
Set-Location .\automation\build\scripts
.\4-deploy.ps1 -Build_name "<generated-build-name>"
```

Deployment script expects environment variables used for remote access (see automation docs):
- `DEPLOYMENT_SERVER_IP`
- `DEPLOYMENT_SERVER_USER`
- `DEPLOYMENT_SERVER_PASSWORD`
- `ROOT_DIR`

## Run as Windows Services (NSSM)

Use NSSM to run both API and client as services after deployment:
- API service docs: `docs/deployment/dashboard-api.md`
- Client service docs: `docs/deployment/dashboard-client.md`
- NSSM reference: `docs/deployment/nssm.md`

## Useful Script References

- `automation/build/scripts/1-build.ps1` — main build orchestration
- `automation/build/scripts/2-build-code.ps1` — compile API + client
- `automation/build/scripts/3-post-build.ps1` — packaging/signing flow
- `automation/build/scripts/4-deploy.ps1` — remote deployment
- `automation/build/scripts/configure-nssm-services.ps1` — service setup helper

## Documentation

- Build automation details: `automation/build/README.md`
- API deployment: `docs/deployment/dashboard-api.md`
- Client deployment: `docs/deployment/dashboard-client.md`
- NSSM guide: `docs/deployment/nssm.md`

## Security Note

Before using this repository in a new environment:
- rotate any credentials and API keys present in config files,
- move secrets to secure environment configuration,
- avoid committing environment-specific secrets.
