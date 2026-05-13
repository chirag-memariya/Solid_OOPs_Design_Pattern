using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using WebApi.Models;
using WebApi.Services;
using WebApi.Utils;
using System.Threading.Channels;

class BridgeUtilityHub(LoginService loginService, ConfigService configService, RSService rsService, ILogger<BridgeUtilityHub> logger) : Hub
{
    private readonly LoginService _loginService = loginService;
    private readonly RSService _rsService = rsService;
    private readonly ConfigService _configService = configService;
    private readonly ILogger<BridgeUtilityHub> _logger = logger;

    private static readonly ConcurrentDictionary<Guid, PlaybackSessionDetails> playbackSessions = [];
    private static readonly SemaphoreSlim _loginSemaphore = new(1, 1);
    private static DateTime _lastLoginTime = DateTime.MinValue;
    private static readonly SemaphoreSlim _connectRSSemaphore = new(1, 1);

    private const int queueSize = 60;
    private readonly Queue<double> _frameSendTimes = new(queueSize);
    private DateTime _lastFrameSentTime = DateTime.UtcNow;

    #region LiveStream

    public async Task<bool> Login(User user)
    {
        var cancellationToken = Context.ConnectionAborted;
        await _loginSemaphore.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var timeSinceLastLogin = now - _lastLoginTime;
            if (timeSinceLastLogin < TimeSpan.FromSeconds(60) && _loginService.isLoggedIn)
            {
                // If already logged in and less than 5s since last login, return immediately
                _logger.LogInformation("Already logged in, returning cached result.");
                return true;
            }

            if (timeSinceLastLogin < TimeSpan.FromSeconds(5))
            {
                // Wait until 5s have passed since last login
                var delay = TimeSpan.FromSeconds(5) - timeSinceLastLogin;
                await Task.Delay(delay, cancellationToken);
            }

            try
            {
                await _loginService.LoginAsync(user, cancellationToken);
                if (_loginService.isLoggedIn) _lastLoginTime = DateTime.UtcNow;
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
            _loginSemaphore.Release();
        }
    }

    public async Task<ChannelReader<cMediaFrame>> StartLiveStream(uint zeroBasedRSindex, uint oneBasedCameraIndex) //[EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var CancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = CancellationTokenSource.Token;
        if (!_loginService.isLoggedIn) throw new HubException("Not Logged In");

        await ConnectRS(zeroBasedRSindex, _rsService, _configService, cancellationToken);
        await _rsService.StartLiveStream(zeroBasedRSindex, oneBasedCameraIndex, cancellationToken);
        var channelReader = _rsService.StreamCMediaFrames(oneBasedCameraIndex, cancellationToken);

        return channelReader;
    }

    private async Task ConnectRS(uint zeroBasedIndex, RSService rsService, ConfigService configService, CancellationToken cancellationToken)
    {
        await _connectRSSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (await rsService.IsConnected(cancellationToken)) return;

            if (rsService._recordingServerList.Count == 0) await GetRSList(rsService, configService, cancellationToken);
            if (rsService._recordingServerList.Count == 0) throw new HubException("recordingServer List Empty!");
            else if (rsService._recordingServerList.Count < zeroBasedIndex + 1)
                throw new HubException($"RS doesn't exist for requested index-{zeroBasedIndex}.");

            await rsService.ConnectRS(zeroBasedIndex, cancellationToken);
        }
        finally
        {
            _connectRSSemaphore.Release();
        }
    }

    private async Task GetRSList(RSService rsService, ConfigService configService, CancellationToken cancellationToken)
    {
        var response = await configService.GetConfig(_loginService.IpAddress, _loginService.port, _loginService.sessionID, cancellationToken, Constants.CST_RS_CNFG_CMD_VALUE.ToString());
        rsService._recordingServerList = [.. response.OfType<RecordingServer>()];
    }


    // TODO: Remove if not used after 1-2 next stable points
    // public override async Task OnDisconnectedAsync(Exception? exception)
    // {
    //     // Clean up all channels for this connection
    //     var connectionId = Context.ConnectionId;
    //     var keysToRemove = _liveStreamChannels.Keys.Where(k => k.connectionId == connectionId).ToList();
    //     foreach (var key in keysToRemove)
    //     {
    //         if (_liveStreamChannels.TryRemove(key, out var channel))
    //         {
    //             channel.Writer.TryComplete();
    //         }
    //     }
    //     await base.OnDisconnectedAsync(exception);
    // }

    #endregion

    #region Playback

    // To be invokable from the hub client, methods must be instance (non-static) methods.
#pragma warning disable CA1822 // Mark members as static
    public IEnumerable<string> ListRecordings(string path)
#pragma warning restore CA1822 // Mark members as static
    {
        if (string.IsNullOrWhiteSpace(path))
            path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BridgeUtility", "MrdFiles");

        var files = Directory.EnumerateFiles(path, "*.mrd", SearchOption.AllDirectories);
        return files;
    }

    // To be invokable from the hub client, methods must be instance (non-static) methods.
#pragma warning disable CA1822 // Mark members as static
    public void UpdatePlaybackSpeed(Guid sessionId, decimal playbackSpeed)
#pragma warning restore CA1822 // Mark members as static
    {
        var playbackSessionDetails = playbackSessions[sessionId];
        lock (playbackSessionDetails)
        {
            playbackSessionDetails.PlaybackSpeed = playbackSpeed;
            playbackSessionDetails.FramesSentCountAtPreviousSpeeds = playbackSessionDetails.FramesSentCountAtCurrentSpeed;
            playbackSessionDetails.FramesSentCountAtCurrentSpeed = 0;
            playbackSessionDetails.PreviousSpeed_LastFrame_MaxAheadTime = playbackSessionDetails.CurrentSpeed_LastCalculated_MaxAheadTime;
            _logger.LogInformation("Playback speed updated for session {sessionId}: {playbackSpeed}", sessionId, playbackSpeed);
        }
    }

    public IAsyncEnumerable<cMediaFrame> StartPlayback(Guid clientGeneratedSessionId, string path, uint zeroBasedIndex, decimal initialPlaybackSpeed, CancellationToken cancellationToken)
    {
        var recordings = ListRecordings(path).OrderBy(f => f).ToArray();
        var filePath = zeroBasedIndex < recordings.Length ?
            recordings[zeroBasedIndex] :
            string.Empty;

        if (string.IsNullOrWhiteSpace(filePath))
            throw new HubException("File not found at the specified index.");
        if (!File.Exists(filePath))
            throw new HubException("File does not exist.");

        var stream = File.OpenRead(filePath);
        stream.Seek(2000, SeekOrigin.Begin);

        playbackSessions[clientGeneratedSessionId] = new PlaybackSessionDetails(initialPlaybackSpeed);
        return ReadFileChunksAsync(clientGeneratedSessionId, stream, cancellationToken);
    }


    private async IAsyncEnumerable<cMediaFrame> ReadFileChunksAsync(Guid clientGeneratedSessionId, FileStream stream, [EnumeratorCancellation] CancellationToken ct)
    {
        try
        {
            ulong framesProcessedCount = 0;
            var headerBuffer = new byte[Constants.VIDEO_FRAME_HEADER_LENGTH];
            ulong sendAheadBufferSize = 10;
            var playbackSessionDetails = playbackSessions[clientGeneratedSessionId];
            playbackSessionDetails.PreviousSpeed_LastFrame_MaxAheadTime = DateTime.UtcNow;

            while ((await stream.ReadAsync(headerBuffer, ct)) == Constants.VIDEO_FRAME_HEADER_LENGTH)
            {
                ct.ThrowIfCancellationRequested();

                FrameHeader header = new();
                header.DecodeFrameHeader(headerBuffer);
                if (header.mediaFrmLen < 40) continue;
                int dataSize = (int)header.mediaFrmLen - 40;

                ++framesProcessedCount;
                if (header.frmType != 0 && SkipFrameForPlayback(framesProcessedCount, playbackSessionDetails.PlaybackSpeed))
                {
                    stream.Seek(dataSize, SeekOrigin.Current);
                    continue;
                }

                if (header.fps <= 0)
                {
                    _logger.LogWarning("Invalid FPS value in header: {fps}. Defaulting to 25.", header.fps);
                    header.fps = 25;
                }

                lock (playbackSessionDetails)
                {
                    ulong temp = sendAheadBufferSize + 1;
                    double value = (playbackSessionDetails.FramesSentCountAtCurrentSpeed > temp)
                        ? (playbackSessionDetails.FramesSentCountAtCurrentSpeed - temp) * 1000.0 / header.fps / (double)playbackSessionDetails.PlaybackSpeed
                        : 0;
                    playbackSessionDetails.CurrentSpeed_LastCalculated_MaxAheadTime = playbackSessionDetails.PreviousSpeed_LastFrame_MaxAheadTime.AddMilliseconds(value);
                }
                var now = DateTime.UtcNow;
                if (now < playbackSessionDetails.CurrentSpeed_LastCalculated_MaxAheadTime)
                {
                    var wait = playbackSessionDetails.CurrentSpeed_LastCalculated_MaxAheadTime - now;
                    if (wait > TimeSpan.Zero)
                        await Task.Delay(wait, ct);
                }
                _logger.LogDebug("Wait time for {FramesSentCountAtCurrentSpeed} {waitTime} {previous} {current}", playbackSessionDetails.FramesSentCountAtCurrentSpeed, playbackSessionDetails.CurrentSpeed_LastCalculated_MaxAheadTime - now, playbackSessionDetails.PreviousSpeed_LastFrame_MaxAheadTime, playbackSessionDetails.CurrentSpeed_LastCalculated_MaxAheadTime);

                var dataBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent((int)Math.Ceiling(dataSize / 1000.0) * 1000);
                int bytesRead = await stream.ReadAsync(dataBuffer.AsMemory(0, dataSize), ct);
                if (bytesRead == 0)
                    throw new("No data available in file.");
                if (bytesRead < dataSize)
                    _logger.LogWarning("Read data length is less than expected as per mediaHeader.mediaFrameLength");

                playbackSessionDetails.FramesSentCountAtCurrentSpeed++;

                double movingAvg = CalculateMovingAverage();
                if (playbackSessionDetails.FramesSentCountAtCurrentSpeed % (queueSize / 2) == 0)
                    _logger.LogInformation("For Session {sessionId} Moving average of frame sending time: {movingAvg} ms", clientGeneratedSessionId, movingAvg);

                yield return new cMediaFrame(header, dataBuffer.AsMemory(0, bytesRead));
            }
        }
        finally
        {
            playbackSessions.Remove(clientGeneratedSessionId, out _);
            _logger.LogInformation("Playback session {sessionId} ended.", clientGeneratedSessionId);
            stream.Dispose();
        }
    }

    private static bool SkipFrameForPlayback(ulong frameReadCount, decimal playbackSpeed)
    {
        return false;
    }

    private double CalculateMovingAverage()
    {
        var currentTime = DateTime.UtcNow;
        var sendingDuration = currentTime - _lastFrameSentTime;
        _lastFrameSentTime = currentTime;

        if (_frameSendTimes.Count >= queueSize)
        {
            _frameSendTimes.Dequeue();
        }
        _frameSendTimes.Enqueue(sendingDuration.TotalMilliseconds);
        return _frameSendTimes.Average();
    }

    #endregion
}
