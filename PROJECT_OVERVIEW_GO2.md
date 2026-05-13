# SAMAS Login Demo - Project Overview (GO 2)

## STAR Method Analysis - Action (Detailed Technical Implementation)

---

## **A - ACTION**

### Part 1: Complete Connection Lifecycle

#### **Phase 1: Client Connection to SignalR Hub**

```
Timeline:
┌──────────────────────────────────────────────────────────────┐
│                   CLIENT INITIATION                          │
└──────────────────────────────────────────────────────────────┘
            ↓
┌──────────────────────────────────────────────────────────────┐
│  JavaScript Code Execution:                                  │
│  new signalR.HubConnectionBuilder()                          │
│    .withUrl("http://127.0.0.1:5050/bridgeUtility")           │
│    .build()                                                   │
└──────────────────────────────────────────────────────────────┘
            ↓
┌──────────────────────────────────────────────────────────────┐
│  Establishes WEBSOCKET CONNECTION                            │
│  (Upgrades from HTTP if WebSocket available)                │
└──────────────────────────────────────────────────────────────┘
            ↓
┌──────────────────────────────────────────────────────────────┐
│  SignalR Handshake Protocol:                                 │
│  1. Client sends: {"protocol":"json","version":1}           │
│  2. Server responds: {"error":null}                          │
│  3. Connection object returned to client                     │
└──────────────────────────────────────────────────────────────┘
            ↓
┌──────────────────────────────────────────────────────────────┐
│  ensureConnectionStarted() called:                           │
│  - Sets isConnected = true                                   │
│  - Stores connectionPromise for reuse                        │
│  - Ready for hub method invocations                          │
└──────────────────────────────────────────────────────────────┘
```

**Connection Reuse Pattern** (in `main.js`):
```javascript
// IMPORTANT: Store connection promise to prevent multiple connections
let connectionPromise = null;
let isConnected = false;

async function ensureConnectionStarted() {
    // Only create ONE connection instance
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
                connectionPromise = null;  // Reset promise on failure for retry
                throw err;
            });
    }
    return connectionPromise;  // Reuse existing promise
}
```

**Why Reuse Pattern Matters**:
- Single WebSocket connection handles all hub method calls
- Prevents resource exhaustion from multiple connections
- Maintains single session context across the application
- Automatic reconnection if connection drops

---

#### **Phase 2: Authentication via SignalR Hub Method**

**Client-Side Call**:
```javascript
async function authenticate(username, password, serverIp, serverPort) {
    try {
        await ensureConnectionStarted();

        // Call the Login method on the hub
        const result = await connection.invoke("Login", {
            Username: username,
            Password: password,
            IpAddress: serverIp,
            Port: serverPort
        });

        if (result) {
            console.log("Authentication successful!");
            return true;
        } else {
            console.log("Authentication failed!");
            return false;
        }
    } catch (err) {
        console.error("Login error:", err);
        return false;
    }
}
```

**Server-Side Hub Method** (in `BridgeUtilityHub.cs`):
```csharp
// SignalR Hub method - automatically invokable from client
public async Task<bool> Login(User user)
{
    var cancellationToken = Context.ConnectionAborted;  // Token - connection aborted = cancel
    
    // SEMAPHORE PATTERN: Prevent concurrent login attempts
    await _loginSemaphore.WaitAsync(cancellationToken);
    try
    {
        var now = DateTime.UtcNow;
        var timeSinceLastLogin = now - _lastLoginTime;
        
        // If already logged in within 60 seconds, return cached result (rate limiting)
        if (timeSinceLastLogin < TimeSpan.FromSeconds(60) && _loginService.isLoggedIn)
        {
            _logger.LogInformation("Already logged in, returning cached result.");
            return true;
        }

        // If last login was very recent (< 5s), wait to avoid hammering server
        if (timeSinceLastLogin < TimeSpan.FromSeconds(5))
        {
            var delay = TimeSpan.FromSeconds(5) - timeSinceLastLogin;
            await Task.Delay(delay, cancellationToken);
        }

        try
        {
            // Perform actual login via Recording Server
            await _loginService.LoginAsync(user, cancellationToken);
            
            if (_loginService.isLoggedIn)
                _lastLoginTime = DateTime.UtcNow;
            
            return _loginService.isLoggedIn;
        }
        catch (Exception ex)
        {
            _logger.LogError("{message}", ex.Message);
            return false;
        }
    }
    finally
    {
        _loginSemaphore.Release();  // Always release lock
    }
}
```

**Key Concepts**:

| Concept | Purpose | Benefit |
|---------|---------|---------|
| **SemaphoreSlim** | Limits concurrent login attempts to 1 | Prevents race conditions, ensures thread safety |
| **CancellationToken** | `Context.ConnectionAborted` | Automatically cancels operation if client disconnects |
| **Rate Limiting** | 5-second minimum between login attempts | Prevents brute-force attacks |
| **Caching** | Returns cached result within 60 seconds | Reduces load on recording server |

---

#### **Phase 3: Linux Socket Connection to Recording Server**

**Step-by-Step Handshake**:

```
┌────────────────────────────────────────────────────────────────┐
│                    LOGIN FLOW                                 │
└────────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────────┐
│ 1. Build Command Model:                                        │
│    - Purpose: CMD_MAIN_REQ_LOG (Login)                         │
│    - Credentials: [username, password, serverIP]              │
│    - MediaClientType: 0 (indicates web client)                │
└────────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────────┐
│ 2. Create LinuxSocket:                                         │
│    new LinuxSocket(commandModel, logger)                       │
│    → Creates TCP socket object (not yet connected)             │
└────────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────────┐
│ 3. Connect Async:                                              │
│    await socket.ConnectAsync(cancellationToken)                │
│    → Opens TCP connection to Recording Server                  │
│    → Blocks until connected (or timeout)                       │
│    → Logs: "socket connected"                                  │
└────────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────────┐
│ 4. Build Command String:                                       │
│    CommandString = CommandBuilder.BuildCommandString()         │
│    Format: [SOM]REQ_LOG[FSP]28573[FSP]username[FSP]password   │
│             [FSP]0[FSP]ServerIP[FSP]GUID[EOM]                 │
│                                                                │
│    Legend:                                                     │
│    SOM = 0x01 (Start of Message)                              │
│    FSP = 0x1E (Field Separator)                               │
│    EOM = 0x04 (End of Message)                                │
│    28573 = SMART_CODE (protocol identifier)                   │
└────────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────────┐
│ 5. Send Data:                                                  │
│    await socket.SendDataAsync(commandString)                   │
│    → Converts string to bytes (ASCII encoding)                 │
│    → Sends via TCP socket                                      │
│    → Logs: "data sent"                                         │
└────────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────────┐
│ 6. Receive Response:                                           │
│    Response = await socket.ReceiveResponseAsync()              │
│    → Blocks until data arrives (max 4096 bytes)                │
│    → Decodes response string                                   │
│    → Format: [RPL_LOG][FSP]status[FSP]sessionID[FSP]...      │
└────────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────────┐
│ 7. Parse Response:                                             │
│    - RspValues[0] = "RPL_LOG" (reply type)                    │
│    - RspValues[1] = Error Code (0 = success)                  │
│    - RspValues[2] = Session ID (if success)                   │
│                                                                │
│    Extract: sessionID = RspValues[2]                          │
└────────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────────┐
│ 8. Store Session:                                              │
│    - isLoggedIn = true                                         │
│    - sessionID = RspValues[2]                                  │
│    - Ready for future commands requiring sessionID            │
└────────────────────────────────────────────────────────────────┘
```

**Implementation Code** (in `LoginService.cs`):
```csharp
public async Task LoginAsync(User user, CancellationToken cancellationToken)
{
    // Step 1: Store server details
    string[] reqFields = [user.Username, user.Password, user.IpAddress];
    IpAddress = user.IpAddress;
    port = (uint)user.Port;

    // Step 2: Create command model
    CommandModel loginCommand = new()
    {
        IpAddr = user.IpAddress,
        Port = user.Port,
        CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_LOG,
        ReqFields = reqFields,
        MediaClientType = 0  // Indicates web client
    };

    // Step 3: Create and connect socket
    _commandSocket = new(loginCommand, _linuxSocketLogger);
    await _commandSocket.ConnectAsync(cancellationToken);

    // Step 4: Build and send command
    var loginCommandString = CommandBuilder.BuildCommandString(loginCommand, _commandBuilderLogger);
    /*
    Example output: "\x01REQ_LOG\x1E28573\x1Eadmin\x1Epassword\x1E0\x1E192.168.1.100\x1E550e8400-e29b-41d4-a716-446655440000\x04"
    */
    await _commandSocket.SendDataAsync(loginCommandString, cancellationToken);

    // Step 5: Receive response
    var loginResponse = await _commandSocket.ReceiveResponseAsync(cancellationToken);
    /*
    Example response: "RPL_LOG\x1E0\x1Esession-12345-abcde"
    */
    
    // Step 6: Parse response
    loginCommand.RspValues = CommandBuilder.GetResponseArray(loginResponse);
    /*
    RspValues[0] = "RPL_LOG"
    RspValues[1] = "0" (success code)
    RspValues[2] = "session-12345-abcde" (session ID)
    */

    // Step 7: Extract and store session ID
    int loginStatus = int.Parse(loginCommand.RspValues[1]);
    if (loginStatus == 0)
    {
        isLoggedIn = true;
        sessionID = loginCommand.RspValues[2];
        _logger.LogInformation("Login Successful with session ID {sessionID}", sessionID);
    }
    else
    {
        isLoggedIn = false;
        errorCode = loginStatus;
        _logger.LogError("Login Failed with error code {errorCode}", errorCode);
    }
}
```

---

### Part 2: Recording Server Connection & Stream Setup

#### **Phase 4: Get Recording Server List**

**Purpose**: Discover all available recording servers before connecting to one

```csharp
private async Task GetRSList(RSService rsService, ConfigService configService, CancellationToken cancellationToken)
{
    // Send config request for RS (Recording Server) configuration
    var response = await configService.GetConfig(
        _loginService.IpAddress,           // Server address
        _loginService.port,                 // Server port
        _loginService.sessionID,            // Previously obtained session ID
        cancellationToken,
        Constants.CST_RS_CNFG_CMD_VALUE.ToString()  // "5" = Request RS config
    );

    // Parse response and convert to RecordingServer objects
    rsService._recordingServerList = [.. response.OfType<RecordingServer>()];
    
    /*
    Example _recordingServerList:
    [
        {ID: 1, IPAdd: "192.168.1.10", Port: 8100},
        {ID: 2, IPAdd: "192.168.1.11", Port: 8100},
        {ID: 3, IPAdd: "192.168.1.12", Port: 8100}
    ]
    */
}
```

**RecordingServer Model** (in `Models/RecordingServer.cs`):
```csharp
public class RecordingServer
{
    public int ID { get; set; }          // Server ID (1, 2, 3...)
    public string IPAdd { get; set; }     // IP address (192.168.1.10)
    public int Port { get; set; }         // Port number (8100)
    public string Name { get; set; }      // Server name (e.g., "Main Office")
    // ... other properties
}
```

---

#### **Phase 5: Connect to a Recording Server**

**Three-Socket Architecture**:

```
┌─────────────────────────────────────────────────────────────┐
│              RECORDING SERVER CONNECTION                    │
│           (Multiple Sockets per Connection)                 │
└─────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ Socket 1: _connectCommandSocket                             │
│ - Purpose: Connect (REQ_CON), Polling (REQ_POL)            │
│ - Lifecycle: Created once, reused for connection mgmt      │
│ - Status: Persistent while connected                       │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ Socket 2: _commandSocket                                    │
│ - Purpose: Channel requests (REQ_CHNL)                     │
│ - Lifecycle: Created per session, handles commands         │
│ - Status: Active when streaming                            │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ Socket 3: _dataSocket                                       │
│ - Purpose: Receiving frame data (VIDEO STREAMS)            │
│ - Lifecycle: Created when streaming starts                 │
│ - Status: Active while frames flow                         │
└────────────────────────────────────────────────────────────┘
```

**Connection Code** (in `RSService.cs`):
```csharp
public async Task ConnectRS(uint zeroBasedRSindex, CancellationToken cancellationToken)
{
    // Acquire exclusive lock (only one thread connects at a time)
    if (!await _connectSocketLock.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
        throw new TimeoutException("Failed to acquire _connectSocketLock");

    try
    {
        // Get recording server details from previously fetched list
        RecordingServer recordingServer = _recordingServerList[(int)zeroBasedRSindex];
        
        // Build REQ_CON (request connection) command
        CommandModel connectCommand = new()
        {
            IpAddr = recordingServer.IPAdd,        // e.g., "192.168.1.10"
            Port = recordingServer.Port,            // e.g., 8100
            CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CON,
            ReqFields = [
                recordingServer.ID.ToString(),      // Server ID
                "123456"                            // Password/key
            ],
            MediaClientType = 1                     // 1 = media client
        };

        // Create socket if not already created or if disconnected
        if (_connectCommandSocket is null || !_connectCommandSocket.IsConnected())
        {
            _connectCommandSocket = new(connectCommand, _linuxSocketLogger);
        }

        // Establish TCP connection
        await _connectCommandSocket.ConnectAsync(cancellationToken);

        // Build command string: "\x01REQ_CON\x1E28573\x1E1\x1E1\x1E123456\x04"
        var connectCommandString = CommandBuilder.BuildCommandString(connectCommand, _commandBuilderLogger);
        await _connectCommandSocket.SendDataAsync(connectCommandString, cancellationToken);

        // Receive connection response
        var connectResponse = await _connectCommandSocket.ReceiveResponseAsync(cancellationToken);
        connectCommand.RspValues = CommandBuilder.GetResponseArray(connectResponse);
        
        /*
        Expected response format:
        RspValues[0] = "RPL_CON"           (reply type)
        RspValues[1] = "0"                 (status: 0 = success)
        RspValues[2] = "CRS_ID_12345" (Connection/Recording Server ID)
        RspValues[3] = "Port: 8200"        (assigned port for data socket)
        RspValues[4] = "Status: Ready"     (server status)
        */

        if ((connectCommand.RspValues.Length >= 5) && 
            (int.Parse(connectCommand.RspValues[1]) == 0))
        {
            // Extract and store CRS ID (Connection Recording Server ID)
            _crsId = connectCommand.RspValues[2];

            _logger.LogInformation(
                "Connection established with RS {CRS_ID} {3rd_Value} {4thValue}.",
                connectCommand.RspValues[2],  // CRS_ID
                connectCommand.RspValues[3],  // Port info
                connectCommand.RspValues[4]   // Status
            );
        }
        else
        {
            throw new("Invalid connection response.");
        }
    }
    finally
    {
        _connectSocketLock.Release();  // Always release lock
    }
}
```

---

### Part 3: Live Stream Channel Creation & Data Flow

#### **Phase 6: Create Communication Channels**

**Purpose**: Establish a data channel through which video frames will flow

```csharp
public async Task StartLiveStream(uint zeroBasedRSindex, uint zeroBasedCameraIndex, CancellationToken cancellationToken)
{
    await _commandSocketLock.WaitAsync(cancellationToken);
    try
    {
        RecordingServer recordingServer = _recordingServerList[(int)zeroBasedRSindex];

        // Build REQ_CHNL (request channel) command
        CommandModel createChannelCommand = new()
        {
            IpAddr = recordingServer.IPAdd,
            Port = recordingServer.Port,
            CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CHNL,
            ReqFields = [
                _crsId,        // CRS ID from previous connection
                "0",           // Channel type: 0 = live stream channel
                "-1"           // Playback session ID: -1 = N/A (live not playback)
            ],
            MediaClientType = 1
        };

        // Create or reuse command socket
        if (_commandSocket is null || !_commandSocket.IsConnected())
        {
            _commandSocket = new(createChannelCommand, _linuxSocketLogger);
            
            // First connection: establish socket and start polling
            bool createStatus = await CreateChannel(_commandSocket, createChannelCommand, cancellationToken);
            if (!createStatus) 
                throw new("Could not create Command Channel.");

            // Start background polling task to keep connection alive
            _ = Task.Run(async () =>
            {
                try
                {
                    await LongPollInfinitely(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Long polling error");
                }
            }, cancellationToken);
        }

        // Now create DATA channel for actual frame streaming
        CommandModel createDataChannelCommand = new()
        {
            IpAddr = recordingServer.IPAdd,
            Port = recordingServer.Port,
            CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CHNL,
            ReqFields = [
                _crsId,
                "2",           // Channel type: 2 = data channel (live stream)
                "-1"           // Playback session ID: -1 = N/A
            ],
            MediaClientType = 1
        };

        // Create new data socket
        if (_dataSocket is null || !_dataSocket.IsConnected())
        {
            _dataSocket = new(createDataChannelCommand, _linuxSocketLogger);
            bool dataChannelStatus = await CreateChannel(_dataSocket, createDataChannelCommand, cancellationToken);
            if (!dataChannelStatus)
                throw new("Could not create Data Channel.");

            // Start background task to receive frame data
            _ = Task.Run(async () =>
            {
                try
                {
                    await ReceiveLiveStreamDataInfinitely(zeroBasedCameraIndex, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Data reception error");
                }
            }, cancellationToken);
        }

        // Send START_LIVE_STREAM command (live stream for camera index)
        CommandModel startStreamCommand = new()
        {
            IpAddr = recordingServer.IPAdd,
            Port = recordingServer.Port,
            CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_SET_CMD,
            CmdSubId = (int)Constants.CMD_SUB_ID_e.CMD_SUB_SRT_LV_STRM,  // 55 = Start live stream
            SessionId = _crsId,
            ReqFields = [
                "1",                              // Stream ID
                zeroBasedCameraIndex.ToString()   // Camera index
            ]
        };

        var startStreamCommandString = CommandBuilder.BuildCommandString(startStreamCommand, _commandBuilderLogger);
        await _commandSocket.SendDataAsync(startStreamCommandString, cancellationToken);

        var startStreamResponse = await _commandSocket.ReceiveResponseAsync(cancellationToken);
        startStreamCommand.RspValues = CommandBuilder.GetResponseArray(startStreamResponse);

        _logger.LogInformation("Live stream started for camera {cameraIndex}", zeroBasedCameraIndex);
    }
    finally
    {
        _commandSocketLock.Release();
    }
}
```

**Three Communication Channels**:

| Channel | Type | Command | Purpose |
|---------|------|---------|---------|
| **Command Channel** | REQ_CHNL (type=0) | Control commands | Manage streaming, polling, keep connection alive |
| **Data Channel** | REQ_CHNL (type=2) | Frame data stream | Receive actual video frames |
| **Event Channel** | REQ_CHNL (type=4) | Event notifications | Receive connection events (optional) |

---

#### **Phase 7: Long Polling (Keep Alive)**

**Purpose**: Prevent connection timeout by periodically sending keep-alive messages

```csharp
public async Task LongPollInfinitely(CancellationToken cancellationToken)
{
    // Build polling command
    CommandModel pollCommand = new()
    {
        IpAddr = _commandSocket.ipEndpoint.Address.ToString(),
        Port = _commandSocket.ipEndpoint.Port,
        CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_SET_CMD,
        CmdSubId = (int)Constants.CMD_SUB_ID_e.CMD_SUB_REQ_POL,  // 7 = Polling
        SessionId = _crsId
    };

    var pollCommandString = CommandBuilder.BuildCommandString(pollCommand, _commandBuilderLogger);

    int retryCount = 0;
    const int maxRetryCount = 10;
    TimeSpan pollInterval = TimeSpan.FromSeconds(10);

    while (true)
    {
        // Wait before sending next poll
        await Task.Delay(pollInterval, cancellationToken);

        // Send polling command every 10 seconds
        await _commandSocket.SendDataAsync(pollCommandString, cancellationToken);
        
        string pollResponse = await _commandSocket.ReceiveResponseAsync(cancellationToken);
        pollCommand.RspValues = CommandBuilder.GetResponseArray(pollResponse);

        // Check if poll was successful
        if (int.Parse(pollCommand.RspValues[2]) == (int)Constants.CMD_SUB_ID_e.CMD_SUB_REQ_POL &&
            int.Parse(pollCommand.RspValues[1]) == 0)
        {
            retryCount = 0;  // Reset retry counter on success
            _logger.LogInformation("Long Polling successful for session: {CRS_ID}", _crsId);
            continue;
        }

        // Increment retry counter and throw if too many failures
        if (++retryCount >= maxRetryCount)
            throw new($"Long Polling failed for session: {_crsId}, retries: {retryCount - 1}");
    }
}
```

**Polling Timeline**:
```
Time (seconds)
0          10         20         30         40         50
|          |          |          |          |          |
POLL ----> RESPONSE
           POLL ----> RESPONSE
                      POLL ----> RESPONSE
                                 POLL ----> RESPONSE
                                            POLL ----> RESPONSE

Connection State: ✓ ALIVE throughout
```

---

### Part 4: Frame Data Reception & Serialization

#### **Phase 8: Receive Frame Data Continuously**

```csharp
public ChannelReader<cMediaFrame> ReceiveLiveStreamData(
    uint cameraIndex, 
    CancellationToken cancellationToken, 
    bool fileSave = false)
{
    // Check if channel already exists for this camera
    if (_liveStreamChannels.TryGetValue(cameraIndex, out var existingChannel))
    {
        return existingChannel.Reader;  // Reuse existing channel
    }

    // Create new unbounded channel for frame streaming
    var channel = Channel.CreateUnbounded<cMediaFrame>(
        new UnboundedChannelOptions
        {
            SingleWriter = true,    // Only one writer (data reception task)
            SingleReader = false    // Multiple readers allowed (SignalR clients)
        }
    );

    _liveStreamChannels[cameraIndex] = channel;

    // Start background task only once per socket
    lock (_liveStreamTaskLock)
    {
        _liveStreamTask ??= Task.Run(async () =>
        {
            string filePath = $"./temp-{DateTime.UtcNow.Ticks}.h264";
            FileStream? fileStream = fileSave ? new(filePath, FileMode.Create, FileAccess.Write) : null;

            try
            {
                // Continuously receive frames from data socket
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Read frame header (40 bytes)
                    byte[] frameHeaderBytes = new byte[Constants.VIDEO_FRAME_HEADER_LENGTH];
                    int readBytes = await ClientSocket.ReceiveAsync(frameHeaderBytes, SocketFlags.None, cancellationToken);

                    if (readBytes != Constants.VIDEO_FRAME_HEADER_LENGTH)
                        break;  // Connection closed or incomplete header

                    // Parse frame header
                    FrameHeader header = new();
                    header.DecodeFrameHeader(frameHeaderBytes);

                    // Read frame data (variable length)
                    int dataSize = (int)header.mediaFrmLen - 40;
                    byte[] frameData = new byte[dataSize];
                    await ClientSocket.ReceiveExactAsync(frameData, cancellationToken);

                    // Create cMediaFrame object
                    var mediaFrame = new cMediaFrame(header, frameData);

                    // Write to file if requested (for debugging/recording)
                    if (fileSave)
                        await fileStream.WriteAsync(frameData, cancellationToken);

                    // Send frame to all subscribers via channel
                    await channel.Writer.WriteAsync(mediaFrame, cancellationToken);

                    _logger.LogDebug("Frame received - Timestamp: {ts}, Size: {sz}",
                        header.timeStampSec, dataSize);
                }
            }
            catch (OperationCanceledException)
            {
                // Connection cancelled - cleanup
                _logger.LogInformation("Live stream reception cancelled");
            }
            finally
            {
                fileStream?.Dispose();
                channel.Writer.Complete();  // Signal stream completion
            }
        }, cancellationToken);
    }

    return channel.Reader;  // Return reader for client subscription
}
```

---

#### **Frame Header Structure** (40 Bytes)

**Model** (in `Models/FrameHeader.cs`):
```csharp
public class FrameHeader
{
    public uint magicCode { get; set; }          // 511 - Protocol identifier
    public byte headerVersion { get; set; }       // Version of this header format
    public byte productType { get; set; }         // Device type (DVR, NVR, etc.)
    public uint mediaFrmLen { get; set; }         // Total frame size (40 + data bytes)
    public uint timeStampSec { get; set; }        // Seconds since epoch
    public ushort timeStampMsec { get; set; }     // Milliseconds (0-999)
    public byte streamType { get; set; }          // Primary or secondary stream
    public byte codecType { get; set; }           // H.264, MJPEG, H.265, etc.
    public byte fps { get; set; }                 // Frames per second (30, 60, etc.)
    public byte frmType { get; set; }             // I-frame (0), P-frame (1), B-frame (2)
    public byte vidResolution { get; set; }       // 720p, 1080p, 4K, etc.
    public byte vidFormat { get; set; }           // PAL, NTSC, etc.
    public byte scanType { get; set; }            // Interlaced or progressive
    public byte playBackStatus { get; set; }      // Play, pause, stop status
    public byte vidLoss { get; set; }             // Video loss indicator
    public ushort audSampleFrq { get; set; }      // Audio sample frequency
    public byte playbackSynNum { get; set; }      // Synchronization number
    public byte[] reserveByte { get; set; }       // Reserved for future use
    public uint preReserveMediaLen { get; set; }  // Pre-reserved length
    public ushort cameraSeqNo { get; set; }       // Camera index/sequence number
}
```

**Decoding Example**:
```csharp
public void DecodeFrameHeader(byte[] rawHeaderBytes)
{
    // Example: Parsing binary data
    using var reader = new BinaryReader(new MemoryStream(rawHeaderBytes));
    
    magicCode = reader.ReadUInt32();           // Bytes 0-3
    headerVersion = reader.ReadByte();          // Byte 4
    productType = reader.ReadByte();            // Byte 5
    mediaFrmLen = reader.ReadUInt32();          // Bytes 6-9
    timeStampSec = reader.ReadUInt32();         // Bytes 10-13
    timeStampMsec = reader.ReadUInt16();        // Bytes 14-15
    streamType = reader.ReadByte();             // Byte 16
    codecType = reader.ReadByte();              // Byte 17
    fps = reader.ReadByte();                    // Byte 18
    frmType = reader.ReadByte();                // Byte 19
    // ... continue for other fields ...
}
```

**Frame Data Structure**:
```
┌─────────────────────────────────────────────────────┐
│              cMediaFrame                            │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌────────────────────────────────────┐            │
│  │   FrameHeader (40 bytes)           │            │
│  │  ┌──────────────────────────────┐  │            │
│  │  │ magicCode: 511                │  │            │
│  │  │ headerVersion: 1              │  │            │
│  │  │ mediaFrmLen: 41,840 bytes     │  │            │
│  │  │ timeStampSec: 1711884653      │  │            │
│  │  │ timeStampMsec: 123            │  │            │
│  │  │ codecType: 96 (H.264)         │  │            │
│  │  │ fps: 30                       │  │            │
│  │  │ frmType: 0 (I-frame)          │  │            │
│  │  │ vidResolution: 1080p          │  │            │
│  │  │ cameraSeqNo: 1                │  │            │
│  │  └──────────────────────────────┘  │            │
│  └────────────────────────────────────┘            │
│                                                     │
│  ┌────────────────────────────────────┐            │
│  │   Frame Data (41,800 bytes)        │            │
│  │  ┌──────────────────────────────┐  │            │
│  │  │ H.264 encoded video data      │  │            │
│  │  │ (compressed image)             │  │            │
│  │  │ Binary format                  │  │            │
│  │  └──────────────────────────────┘  │            │
│  └────────────────────────────────────┘            │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

### Part 5: SignalR Streaming to Client

#### **Phase 9: Hub Method Returns Stream**

**Server-Side Hub Method**:
```csharp
public async Task<ChannelReader<cMediaFrame>> StartLiveStream(
    uint zeroBasedRSindex, 
    uint oneBasedCameraIndex)
{
    var cancellationToken = CancellationTokenSource.Token;
    
    // Security check
    if (!_loginService.isLoggedIn)
        throw new HubException("Not Logged In");

    // Ensure connected to recording server
    await ConnectRS(zeroBasedRSindex, _rsService, _configService, cancellationToken);
    
    // Start streaming from camera
    await _rsService.StartLiveStream(zeroBasedRSindex, oneBasedCameraIndex, cancellationToken);
    
    // Get channel reader for frame delivery
    var channelReader = _rsService.StreamCMediaFrames(oneBasedCameraIndex, cancellationToken);

    // Return the channel reader to client
    // SignalR will automatically stream all frames through this channel
    return channelReader;
}
```

**Client-Side Stream Subscription**:
```javascript
async function startLiveStream(zeroBasedRSindex, oneBasedCameraIndex) {
    try {
        await ensureConnectionStarted();

        // Call hub method that returns a stream
        const stream = connection.stream(
            "StartLiveStream",
            zeroBasedRSindex,
            oneBasedCameraIndex
        );

        // Subscribe to frame data
        stream.subscribe({
            // Invoked for each frame received
            next: (frame) => {
                /*
                frame = {
                    header: {
                        magicCode: 511,
                        mediaFrmLen: 41840,
                        timeStampSec: 1711884653,
                        codecType: 96,
                        frmType: 0,
                        cameraSeqNo: 1,
                        ...
                    },
                    data: UInt8Array(...) // Binary video data
                }
                */
                
                console.log(`Frame received - Size: ${frame.data.byteLength} bytes`);
                
                // Process frame (display, save, etc.)
                displayFrame(frame);
                recordFrame(frame);
            },
            
            // Invoked when stream completes
            complete: () => {
                console.log("Stream completed.");
            },
            
            // Invoked if error occurs
            error: (err) => {
                console.error("Stream error:", err);
                // Handle error - reconnect if needed
            }
        });
    } catch (err) {
        console.error("Error starting live stream:", err);
    }
}
```

---

### Part 6: Command Protocol Reference

#### **Command Format & Serialization**

**General Command Structure**:
```
[SOM] | COMMAND_NAME | [FSP] | PARAM1 | [FSP] | PARAM2 | ... | [EOM]

Where:
- SOM (Start of Message) = 0x01
- EOM (End of Message) = 0x04
- FSP (Field Separator) = 0x1E
- All parameters are ASCII strings
```

**Supported Commands**:

| Command | ID | Purpose | Parameters |
|---------|-----|---------|------------|
| **REQ_LOG** | 0 | User authentication | username, password, client_type, server_ip |
| **REQ_CHNL** | 1 | Create data channel | crs_id, channel_type, playback_session_id |
| **SET_CMD** | 2 | Send control command | session_id, sub_cmd_id, parameters... |
| **REQ_CON** | 3 | Connect to RS | server_id, password |

**Example Commands Built**:

```csharp
// 1. LOGIN Command
string loginCmd = BuildCommandString(new CommandModel {
    CmdMainId = CMD_MAIN_REQ_LOG,
    ReqFields = ["admin", "password123", "192.168.1.100"]
});
// Output: "\x01REQ_LOG\x1E28573\x1Eadmin\x1Epassword123\x1E0\x1E192.168.1.100\x1E{guid}\x04"

// 2. CONNECT TO RS Command
string connectCmd = BuildCommandString(new CommandModel {
    CmdMainId = CMD_MAIN_REQ_CON,
    ReqFields = ["1", "123456"]  // Server ID, password
});
// Output: "\x01REQ_CON\x1E28573\x1E1\x1E1\x1E123456\x04"

// 3. CREATE CHANNEL Command
string channelCmd = BuildCommandString(new CommandModel {
    CmdMainId = CMD_MAIN_REQ_CHNL,
    ReqFields = ["CRS_ID_12345", "2", "-1"]  // CRS ID, channel type (2=data), playback ID
});
// Output: "\x01REQ_CHNL\x1ECRS_ID_12345\x1E2\x1E-1\x04"

// 4. START LIVE STREAM Command
string startCmd = BuildCommandString(new CommandModel {
    CmdMainId = CMD_MAIN_SET_CMD,
    CmdSubId = CMD_SUB_SRT_LV_STRM,  // 55 = Start live stream
    SessionId = "session-id-12345",
    ReqFields = ["1", "0"]  // Stream ID, camera index
});
// Output: "\x01SET_CMD\x1Esession-id-12345\x1E55\x1E1\x1E0\x04"
```

---

## **Summary of GO 2**

### ✅ **Complete Connection Lifecycle**
- Phase 1: Client connects to SignalR Hub (WebSocket handshake)
- Phase 2: User authenticates via Hub method (Login command via socket)
- Phase 3: Socket connection established, credentials sent, session ID received
- Phase 4: Recording servers discovered via configuration query
- Phase 5: Connection established to specific recording server (3-socket architecture)
- Phase 6: Data channels created (command + data channels)
- Phase 7: Keep-alive polling implemented (every 10 seconds)
- Phase 8: Frame data reception in continuous loop
- Phase 9: Frames streamed to SignalR clients

### ✅ **Detailed Technical Implementations**
- Semaphore-based concurrency control (prevents race conditions)
- Command string serialization with separators (SOM/EOM/FSP)
- Frame header parsing (40 bytes binary format)
- Channel-based producer-consumer pattern
- Bidirectional socket communication
- Polling for connection health

### ✅ **Protocol References**
- Command IDs and sub-command IDs
- Command serialization examples
- Response parsing patterns
- Error handling codes

---

**Document Version**: GO 2 - Action Analysis  
**Date**: March 30, 2026  
**Status**: Detailed Technical Implementation Complete

**Next**: GO 3 - Results & Performance Metrics, Playback Features, Scaling Considerations
