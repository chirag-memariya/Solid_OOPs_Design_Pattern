# SAMAS Login Demo - Project Overview (GO 1)

## STAR Method Analysis - First Go

---

## **S - SITUATION**

### Project Context & Environment

**Project Name**: `samas-login-demo`  
**Type**: Real-Time Video Streaming & Recording Server Bridge Application  
**Architecture**: Client-Server with WebSocket/SignalR Communication  
**Tech Stack**: ASP.NET Core 8.0 (Backend) + Node.js/JavaScript (Client)

### What Is This Product?

The **SAMAS Login Demo** is a **remote client management and video streaming application** that acts as a bridge between:

1. **Frontend Client** (JavaScript/Node.js) - Web-based user interface
2. **ASP.NET Core WebAPI** (C#) - Central hub handling authentication and orchestration
3. **Recording Servers** (Remote Linux-based systems) - Source of live video streams

**Key Purpose**: Enable users to authenticate with a central system and receive **real-time live video streams** from multiple recording servers through a unified interface.

```
┌─────────────────────────────────────────────────────────────┐
│                    USER'S WEB CLIENT                         │
│              (JavaScript/Node.js Application)                │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ SignalR WebSocket Connection
                     │ (Bidirectional, Real-time)
                     │
┌────────────────────▼────────────────────────────────────────┐
│              ASP.NET CORE 8.0 WEBAPI HUB                     │
│           (BridgeUtilityHub - SignalR Hub)                   │
│                                                               │
│  - User Authentication (Login)                              │
│  - Recording Server Management                              │
│  - Live Stream Orchestration                                │
│  - Channel-based Data Streaming                             │
└────────────┬──────────────────────┬────────────────────────┘
             │                      │
    Linux Socket (CMD)     Linux Socket (Data)
             │                      │
└────────────▼──────────────────────▼────────────────────────┘
│         RECORDING SERVERS (Remote Systems)                  │
│                                                              │
│  - Server 1, Server 2, Server 3, ... (Multiple RS)         │
│  - Each with multiple cameras                              │
│  - Live stream capability                                  │
└──────────────────────────────────────────────────────────────┘
```

---

## **T - TECHNOLOGIES**

### 1. **SignalR** - Real-Time Communication Hub

#### What is SignalR?

SignalR is a **real-time communication framework** that automatically chooses the best transport mechanism (WebSocket, Server-Sent Events, or Long Polling) to enable bidirectional communication between client and server.

#### How It's Used in SAMAS:

**Server-Side Setup** (in `Program.cs`):
```csharp
// Register SignalR services
builder.Services.AddSignalR();

// Map the SignalR Hub to a URL endpoint
app.MapHub<BridgeUtilityHub>("/bridgeUtility");
```

**Client-Side Connection** (in `signalr-test/main.js`):
```javascript
import * as signalR from "@microsoft/signalr";

// Create and configure the connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://127.0.0.1:5050/bridgeUtility")  // Connect to hub endpoint
    .build();

// Ensure connection is started before making calls
async function ensureConnectionStarted() {
    if (!connectionPromise) {
        connectionPromise = connection
            .start()
            .then(() => {
                isConnected = true;
                console.log("Connected to SignalR hub.");
            })
            .catch((err) => {
                isConnected = false;
                console.error("Connection error:", err);
                connectionPromise = null;
                throw err;
            });
    }
    return connectionPromise;
}
```

#### Connection Management:

**Key Concepts**:
- **Connection Reuse**: The code maintains a single `connectionPromise` to avoid creating multiple connections
- **Lazy Initialization**: Connection only starts when the first call is made (in `ensureConnectionStarted()`)
- **Error Handling**: Failed connections reset `connectionPromise` to allow retry

**Why This Matters**: 
- Opens a single WebSocket tunnel that stays open throughout the session
- Reduces overhead compared to HTTP request-response cycles
- Enables server to push data to client in real-time without polling

---

### 2. **Linux Sockets** - Backend Server Communication

#### What are Linux Sockets?

Linux Sockets are **inter-process communication (IPC) mechanisms** used for local or network communication. In this project, they're used for:
- **Command Socket**: Send control commands to Recording Servers
- **Data Socket**: Receive video frame data from Recording Servers

#### How Sockets Are Created and Used:

**Socket Class** (in `Services/BaseService.cs` and `Sockets/LinuxSocket.cs`):
```csharp
public class LinuxSocket
{
    private Socket? _socket;
    private readonly CommandModel _commandModel;
    private readonly ILogger<LinuxSocket> _logger;

    public LinuxSocket(CommandModel commandModel, ILogger<LinuxSocket> logger)
    {
        _commandModel = commandModel;
        _logger = logger;
    }

    // Establish connection to recording server
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var ipAddress = IPAddress.Parse(_commandModel.IpAddr);
        var endPoint = new IPEndPoint(ipAddress, _commandModel.Port);
        
        await _socket.ConnectAsync(endPoint, cancellationToken);
    }

    // Send command data
    public async Task SendDataAsync(string data, CancellationToken cancellationToken)
    {
        var buffer = Encoding.UTF8.GetBytes(data);
        await _socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
    }

    // Receive response
    public async Task<string> ReceiveResponseAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        int received = await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        return Encoding.UTF8.GetString(buffer, 0, received);
    }
}
```

**Connection Flow**:
1. Create `LinuxSocket` instance with connection parameters
2. Call `ConnectAsync()` → Opens TCP socket to recording server
3. Call `SendDataAsync()` → Sends serialized command
4. Call `ReceiveResponseAsync()` → Waits for server response

**Example Usage** (in `LoginService.cs`):
```csharp
public async Task LoginAsync(User user, CancellationToken cancellationToken)
{
    // Prepare login command
    CommandModel loginCommand = new()
    {
        IpAddr = user.IpAddress,
        Port = user.Port,
        CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_LOG,  // Login command ID
        ReqFields = new[] { user.Username, user.Password, user.IpAddress }
    };

    // Create socket and connect to recording server
    _commandSocket = new(loginCommand, _linuxSocketLogger);
    await _commandSocket.ConnectAsync(cancellationToken);

    // Build and send login command
    var commandString = CommandBuilder.BuildCommandString(loginCommand, _commandBuilderLogger);
    await _commandSocket.SendDataAsync(commandString, cancellationToken);

    // Receive and parse response
    var response = await _commandSocket.ReceiveResponseAsync(cancellationToken);
    loginCommand.RspValues = CommandBuilder.GetResponseArray(response);

    // Check login status and extract session ID
    if (int.Parse(loginCommand.RspValues[1]) == 0)
    {
        isLoggedIn = true;
        sessionID = loginCommand.RspValues[2];  // Session ID from server
        _logger.LogInformation("Login Successful with session ID {sessionID}", sessionID);
    }
}
```

---

### 3. **Channels** - Streaming Data Protocol

#### What are Channels?

In this context, **Channels** are a .NET feature for asynchronous, producer-consumer communication patterns. They allow:
- **Producer** (Recording Server) to send data items
- **Consumer** (Web Client) to receive data items asynchronously
- **Backpressure handling** when consumer is slower than producer

#### How SignalR Streams Use Channels:

**Server-Side Streaming Method** (in `BridgeUtilityHub.cs`):
```csharp
public async Task<ChannelReader<cMediaFrame>> StartLiveStream(
    uint zeroBasedRSindex, 
    uint oneBasedCameraIndex)
{
    if (!_loginService.isLoggedIn) 
        throw new HubException("Not Logged In");

    // Connect to recording server if not already connected
    await ConnectRS(zeroBasedRSindex, _rsService, _configService, cancellationToken);
    
    // Start streaming from the specified camera
    await _rsService.StartLiveStream(zeroBasedRSindex, oneBasedCameraIndex, cancellationToken);
    
    // Return a channel reader that client can consume frames from
    var channelReader = _rsService.StreamCMediaFrames(oneBasedCameraIndex, cancellationToken);

    return channelReader;
}
```

**Client-Side Stream Consumption** (in `signalr-test/main.js`):
```javascript
async function startLiveStream(zeroBasedRSindex, oneBasedCameraIndex, id) {
    try {
        await ensureConnectionStarted();

        // Call the hub method that returns a stream
        const stream = connection.stream(
            "StartLiveStream",
            zeroBasedRSindex,
            oneBasedCameraIndex
        );

        // Subscribe to receive frames as they arrive
        stream.subscribe({
            next: (frame) => {
                // frame is a cMediaFrame object containing video data
                console.log("Received frame:", frame);
                // Display/process frame (e.g., show in canvas, save, etc.)
            },
            complete: () => {
                console.log("Stream completed.");
            },
            error: (err) => {
                console.error("Stream error:", err);
            },
        });
    } catch (err) {
        console.error("Connection error:", err);
    }
}
```

**Key Concept - cMediaFrame Model** (in `Models/cMediaFrame.cs`):
```csharp
public class cMediaFrame
{
    public FrameHeader Header { get; set; }      // Frame metadata (timestamp, size, etc.)
    public byte[] CompressedData { get; set; }   // Actual video frame data (H.264, MJPEG, etc.)
    public byte[] TransformData { get; set; }    // Additional transformation data
}
```

**How Channels Work**:

```
Recording Server
       │
       │ Produces frames (one per camera capture)
       ▼
┌─────────────────┐
│   Channel<T>    │  ←── Producer writes frames
│  (FIFO Queue)   │
│                 │  ←── Consumer reads frames
└────────┬────────┘
         │
         │ Transmitted via SignalR
         │
         ▼
    Web Client
    Receives frames in order as they're produced
    Can process/display each frame
```

---

## **Key Product Features**

### 1. **Authentication & Session Management**

```
User
  │
  ├─→ Provides: Username, Password, Server IP/Port
  │
  ▼
Login Service (via SignalR Hub)
  │
  ├─→ Creates Linux Socket connection to Recording Server
  ├─→ Sends CMD_MAIN_REQ_LOG command with credentials
  ├─→ Receives response with Session ID
  │
  ▼
Session Established
  │
  └─→ Session ID stored for future API calls
      (Prevents replay attacks, manages user session lifetime)
```

**Authentication Flow**:
- Client calls `Login(User)` via SignalR
- Server creates socket connection to recording server
- Sends login command with credentials
- Records session ID for future commands
- Client can now use recorded commands requiring authentication

### 2. **Multi-Server, Multi-Camera Architecture**

The system supports:
- **Multiple Recording Servers** (indexed 0, 1, 2, ...)
- **Multiple Cameras per Server** (indexed 1-based: 1, 2, 3, ...)
- **Independent Stream Management** (each camera can be streamed separately)

```csharp
// Client can stream from different servers/cameras independently
startLiveStream(0, 1)  // Server 0, Camera 1
startLiveStream(0, 2)  // Server 0, Camera 2
startLiveStream(1, 1)  // Server 1, Camera 1 (different server)
```

### 3. **Connection Pooling & Reuse**

The system uses **Semaphore** locks to prevent concurrent connection attempts:

```csharp
private static readonly SemaphoreSlim _connectRSSemaphore = new(1, 1);

private async Task ConnectRS(uint zeroBasedIndex, ...){
    await _connectRSSemaphore.WaitAsync(cancellationToken);  // Wait for lock
    try
    {
        if (await rsService.IsConnected(cancellationToken)) 
            return;  // Reuse existing connection
        
        // Create new connection only if needed
        await rsService.ConnectRS(zeroBasedIndex, cancellationToken);
    }
    finally
    {
        _connectRSSemaphore.Release();  // Release lock for next request
    }
}
```

**Why Semaphores?**:
- Prevents race conditions where multiple clients might try connecting simultaneously
- Ensures only one connect operation at a time
- Other requests wait in queue instead of failing

---

## **Core Architecture Components**

### Services Layer

| Service | Purpose | Responsibility |
|---------|---------|-----------------|
| **LoginService** | Authentication | Handles user login via Linux Socket, manages session IDs |
| **ConfigService** | Configuration | Fetches recording server list and configuration |
| **RSService** | Recording Server Bridge | Manages connections to recording servers, streaming frames |
| **BaseService** | Parent Service | Provides common functionality (logging, socket management) |

### Communication Layers

| Layer | Protocol | Direction | Purpose |
|-------|----------|-----------|---------|
| **Client ↔ Hub** | SignalR (WebSocket) | Bidirectional | Real-time method calls, stream data |
| **Hub ↔ Recording Server** | Linux Sockets (TCP) | Bidirectional | Control commands, video frames |

---

## **CORS & Security Configuration**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://192.168.27.79:4201")  // Only allow specific frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();  // Important: allow SignalR cookies/auth
        }
    );
});
```

**Security Considerations**:
- **Whitelist Only**: Only the specific frontend URL is allowed
- **Credentials**: Enabled for SignalR authentication
- **Headers & Methods**: Flexible for API needs but domain is restricted

---

## **Logging & Monitoring**

```csharp
// Development: Debug output or IDE console
if (Debugger.IsAttached)
{
    builder.Logging.ClearProviders();
    builder.Logging.AddDebug();
}
// Production: File-based rotating logs
else
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.File("Logs/webapi-log.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.Console()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();
    builder.Host.UseSerilog();
}
```

**Key Information Logged**:
- Login attempts and session IDs
- Connection status to recording servers
- Frame streaming metrics
- Error codes and exceptions

---

## **Summary of GO 1**

✅ **What**: Real-time video streaming bridge connecting web clients to remote recording servers  
✅ **How**: SignalR for client communication, Linux Sockets for server communication, Channels for frame streaming  
✅ **Why**: Enables remote monitoring of multiple cameras across multiple servers through a single authenticated session  

### **Next Steps (GO 2)** 
- Detailed Action Analysis: Connection lifecycle, handshake protocols, frame serialization
- SignalR detailed method signatures and hub operations

### **Next Steps (GO 3)**
- Results & Performance: Throughput metrics, connection stability, scaling considerations
- Code examples for playback, stream control, and advanced features

---

**Document Version**: GO 1 - Situation & Technologies  
**Date**: March 30, 2026  
**Status**: Foundation Analysis Complete
