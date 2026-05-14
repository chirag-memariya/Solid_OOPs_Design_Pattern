# Camera Writer POC - Strategy Walkthrough

This repository contains multiple RabbitMQ-based camera event routing demos.
The goal is to show how events can be routed from an edge gateway to NVR and storage services using different exchange strategies.

## What is in this repo

- `CameraWriterPubSubBasic/` - Basic direct-exchange publisher/consumer sample.
- `Demo/` - Full direct-exchange multi-service demo.
- `Demo - Header - Exchange/` - Full headers-exchange multi-service demo (with Docker Compose).
- `Solution-Poc/` - Extended solution scaffold for future evolution.
- `camera-writer-react-ui/` - Separate React UI workspace.

## Strategy Guide

| Strategy | Status in repo | Main folder | Routing style | Best when |
|---|---|---|---|---|
| Direct Exchange | Implemented | `Demo/` and `CameraWriterPubSubBasic/` | Exact routing key match | Consumer is selected by one exact key (for example camera or service id) |
| Topic Exchange | Concept only | Notes in `Demo - Header - Exchange/Direct vs Topic vs Header.md` | Pattern matching (`*`, `#`) | Routing needs wildcard patterns and hierarchical keys |
| Headers Exchange | Implemented | `Demo - Header - Exchange/` | Header key/value matching | Routing depends on multiple attributes (cameraId, location, type, etc.) |

## 1) Direct Exchange Walkthrough

Use this when each message should go to consumers bound with an exact routing key.

### Flow

1. `EdgeGateWay` publishes camera event with routing key (for example `CAMERA-001` or `NVR-1`).
2. RabbitMQ direct exchange matches only queues bound with the same key.
3. Target services (`NVR1Service`, `NVR2Service`, `Storage1Service`, `Storage2Service`) receive only matching events.

### Run (manual)

Open 6 terminals from repository root.

```bash
docker run --hostname rabbit --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

```bash
dotnet run --project Demo/EdgeGateWay
```

```bash
dotnet run --project Demo/NVR1Service
dotnet run --project Demo/NVR2Service
dotnet run --project Demo/Storage1Service
dotnet run --project Demo/Storage2Service
```

```bash
cd Demo/camera-event-visualizer
npm install
npm run dev
```

RabbitMQ UI: http://localhost:15672 (guest / guest)

## 2) Headers Exchange Walkthrough

Use this when routing should depend on message metadata (headers) instead of routing keys.

### Flow

1. `EdgeGateWay` publishes event to headers exchange with headers dictionary.
2. Consumer queues bind with header arguments and `x-match` logic.
3. RabbitMQ routes to queues where header filters match (`all` or `any`).

### Run (Docker Compose)

```bash
cd "Demo - Header - Exchange"
docker compose up --build
```

Services (default):

- Edge Gateway: http://localhost:8080
- NVR1: http://localhost:5001
- NVR2: http://localhost:5002
- Storage1: http://localhost:5003
- Storage2: http://localhost:5004
- Web UI: http://localhost:5172
- RabbitMQ UI: http://localhost:15672

## 3) Topic Exchange Walkthrough (Concept + Next Step)

Topic exchange is documented in `Demo - Header - Exchange/Direct vs Topic vs Header.md`, but a runnable Topic demo is not yet present in this repository.

If you implement Topic next, use this approach:

1. Declare exchange type as `topic`.
2. Publish keys like `camera.nvr.CAMERA-001`.
3. Bind consumers with patterns like `camera.nvr.*` or `camera.#`.
4. Keep direct/header demos unchanged and add topic as a separate folder for clean comparison.

## Recommended learning order

1. Start with `Demo/` (Direct).
2. Move to `Demo - Header - Exchange/` (Headers).
3. Add Topic strategy as a third runnable variant.

This order makes it easy to understand increasing routing flexibility from exact match to metadata-based filtering.