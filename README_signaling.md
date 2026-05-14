# P2P Signaling Server

Windows-based .NET 8 solution for managing P2P signaling between NVR devices and mobile clients.  
The repository includes:

- A Windows Service host for the signaling runtime (`P2PService`)
- A Windows Forms tray manager (`P2PServerManager`) to control the service and configure runtime settings
- Shared domain/infrastructure/logger libraries
- WiX installer project and build/deployment automation scripts

## 1) Solution Overview

Main solution: `P2PSignaling.sln`

Projects included:

- `src/common/domain` (`Domain.csproj`) — entities, enums, message contracts, configuration models, shared utilities
- `src/common/logger` (`Logger.csproj`) — file logging + optional SignalR log forwarding
- `src/core/infrastructure` (`Infrastructure.csproj`) — DB/cache/repository access, appsettings-based connection strings
- `src/core/application` (`Application.csproj`) — socket server runtime, command processing, orchestration
- `src/presentation/service` (`Service.csproj`) — Windows Service executable host
- `src/presentation/serverManager` (`ServerManager.csproj`) — tray app for start/stop/restart/config
- `src/ProjectSetup/P2PServerSetup` — WiX MSI installer
- `tests/integration/integrationUtility/*` — integration helper utilities (host/core/application/dashboard)

## 2) Runtime Architecture

### Core flow

1. Service/tray startup calls `cAppGlobal.StartServer()`.
2. Runtime reads configuration periodically (default every 60s).
3. It parses NVR and Mobile TCP port lists.
4. It starts one or more `P2PServer` instances (NVR, Mobile, or Common per overlapping ports).
5. Each server hosts a TCP listener (`SocketHandler`) with a fixed binary header:
   - 2 bytes: `SOM` (start marker, value `1`)
   - 2 bytes: payload length
6. Payload is UTF-8 JSON command data handled by `CommandProcess`.
7. In-memory peer state is synced to SQL Server on timer intervals; offline/expired peers are cleaned periodically.

### Main runtime responsibilities

- Manage active peer sockets for:
  - NVR devices
  - Mobile clients
- Authenticate/register peers and keep sessions alive via heartbeat
- Facilitate P2P negotiation and fallback-to-relay signaling
- Maintain relay host mappings by timezone
- Persist peer snapshots and command trends to SQL Server
- Emit troubleshooting logs (file mode and optional SignalR)

## 3) Message / Command Model

Signaling commands are defined in `Domain.Enums.CommandID`.

Common command groups:

- Device → Server:
  - `REGISTER` (1001)
  - `HEARTBEAT` (1002)
  - `UNREGISTER` (1003)
  - `STUN_QUERY` (1004)
  - `P2P_CONNECTION_DETAILS` (1005)
  - fallback/relay related commands (`1006`–`1008`)
- Mobile → Server:
  - `P2P_CONNECTION_REQUEST_M2S` (2001)
  - `FALLBACK_TO_RELAY_REQUEST_M2S` (2002)
- Server → Device/Mobile responses:
  - `REGISTER_SUCCESS` (1502), `REGISTER_FAIL` (1503), `HEARTBEAT_ACK` (1504), etc.
  - Mobile response range (`2501`–`2508`)

Command counters are aggregated and written to `CommandCounts` table on hourly reset.

## 4) Configuration

## A. XML runtime configuration (service/manager)

Location at runtime (relative to app base directory):

- `Settings/P2PServerConfig.xml`

Model: `P2PServerSettings`

- `NVRSettings`:
  - `TcpPort` (comma-separated port list)
  - `ReadTimeout`, `SendTimeout` (seconds)
  - `DbSyncInterval` (seconds)
  - `MemoryCleanInterval` (seconds)
  - `RejectIdealPeer` (bool)
- `MobileSettings`: same fields
- `P2PServerManagerLogSettings` / `P2PServiceLogSettings`:
  - `EnabledFileModeLog`, `RetentionDays`, `InfoLogs`, `WarningLogs`, `ErrorLogs`

If config file is missing, `ServerManager` creates default values (example defaults include `TcpPorts = "5555, 55555"`).

## B. JSON DB connection configuration

Files:

- `src/core/infrastructure/appsettings.json`
- `src/core/infrastructure/appsettings.Development.json`

Keys:

- `AppSettings:MasterConnectionString`
- `AppSettings:ConnectionString`

`DOTNET_ENVIRONMENT` controls loading `appsettings.{Environment}.json` for runtime connection selection.

## C. Operational stop switch

You can disable auto-restart behavior for the tray manager logic by creating:

- `Settings/stop.txt`

with value:

- `true`

Reference: `docs/help/stop-auto-restart-service-process-guide.md`

## 5) Database

SQL Server database name used by startup bootstrap:

- `P2PDb`

During tray startup, `DbRepository.Create("P2PDb")` attempts creation and runs SQL script(s):

- `Scripts/1-CreateAllTables.sql`

Primary tables created:

- `Device`, `DeviceTemp`
- `Clients`, `ClientTemp`
- `RelayHost`
- `CommandCounts`

## 6) Prerequisites

- Windows (service + WinForms + installer flow are Windows-specific)
- .NET 8 SDK
- SQL Server instance reachable by configured connection strings
- Visual Studio 2022 (or Build Tools) for full build/installer workflow
- WiX Toolset v3.11 for MSI build
- (Optional) Posh-SSH for remote signing/deployment scripts

## 7) Local Development

From repository root:

```powershell
dotnet restore .\P2PSignaling.sln
dotnet build .\P2PSignaling.sln -c Debug
```

Run tray manager (recommended for interactive testing):

```powershell
dotnet run --project .\src\presentation\serverManager\ServerManager.csproj
```

Run service host as console process (development run):

```powershell
dotnet run --project .\src\presentation\service\Service.csproj
```

## 8) Build Automation

Automation docs:

- `automation/build/README.md`

Scripts live under:

- `automation/build/scripts`

Typical flow:

1. `prep-environment.ps1` — load `.env` and build environment
2. `1-build.ps1` — orchestrate full build
3. `2-build-code.ps1` — restore/build/publish `ServerManager`
4. `3-post-build.ps1` and sub-scripts — naming, obfuscation/signing, MSI, artifact packaging
5. `4-deploy.ps1` — remote deployment

Example:

```powershell
.\automation\build\scripts\prep-environment.ps1
.\automation\build\scripts\1-build.ps1 -version "1.2.3" -build_type "production" -configuration "Release"
```

## 9) Installer (WiX)

Installer solution:

- `src/ProjectSetup/ProjectSetup.sln`

WiX project:

- `src/ProjectSetup/P2PServerSetup/P2PServerSetup.wixproj`

Product metadata (name, upgrade code, service name, branding) is defined in:

- `src/ProjectSetup/P2PServerSetup/ProductInfo.wxi`

Configured Windows service identity in installer metadata:

- `MatrixCE.P2PService`

## 10) Logging & Monitoring

Troubleshooting logger supports:

- File mode log writing with retention
- Optional SignalR push to dashboard API endpoint

Log behavior is controlled via XML settings (`P2PServerManagerLogSettings`, `P2PServiceLogSettings`) and constants in `Domain.Common.Constants`.

## 11) Versioning & Signing

- Central version fields are defined in `Directory.Build.props` (`VersionPrefix`, assembly/file/informational versions)
- Assemblies are strong-name signed using key file:
  - `docs/StrongKey/P2P.snk`

## 12) Repository Structure (High Level)

```text
src/
  common/
    domain/
    logger/
  core/
    application/
    infrastructure/
  presentation/
    service/
    serverManager/
  ProjectSetup/
automation/
  build/
docs/
tests/
```

## 13) Notes / Caveats

- Service naming appears in multiple places (host setup and installer metadata). Keep service identity aligned across Service code, manager logic, and WiX if modified.
- Tray manager and service manifests request administrator privileges (`requireAdministrator`).
- Default SQL scripts are copied from `src/presentation/serverManager/Scripts` into build output; ensure script files are present when running from custom output locations.

## 14) Useful Paths

- Main solution: `P2PSignaling.sln`
- Service entry point: `src/presentation/service/Program.cs`
- Tray manager entry point: `src/presentation/serverManager/Program.cs`
- Server runtime orchestration: `src/core/application/cAppGlobal.cs`
- Socket listener: `src/core/application/Communication/SocketHandler.cs`
- Command handling: `src/core/application/Services/CommandProcess.cs`
- SQL schema script: `src/presentation/serverManager/Scripts/1-CreateAllTables.sql`
