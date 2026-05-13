using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using WebApi.Models;
using WebApi.Utils;

namespace WebApi.Sockets;

public class LinuxSocket
{
    public IPEndPoint ipEndpoint;
    public CommandModel command;
    public Socket ClientSocket;
    private readonly ILogger<LinuxSocket> _logger;

    // Key: (rsindex, cameraIndex), Value: Channel<cMediaFrame>
    private static readonly ConcurrentDictionary<uint, Channel<cMediaFrame>> _liveStreamChannels = new();
    private static Task? _liveStreamTask = null;
    private static readonly object _liveStreamTaskLock = new();

    // public ICollection<uint> RequestedCameraSeqNumbers()
    //     => _liveStreamChannels.Keys;

    public void NullifyLiveStreamTask()
    {
        _liveStreamTask = null;
    }

    public LinuxSocket(CommandModel cmd, ILogger<LinuxSocket> logger)
    {
        _logger = logger;

        if (string.IsNullOrEmpty(cmd.IpAddr) || cmd.Port <= 0)
        {
            _logger.LogError("Invalid IP address or port number.");
            throw new ArgumentException("Invalid IP address or port number.");
        }

        command = cmd;
        ipEndpoint = new(IPAddress.Parse(command.IpAddr), command.Port);
        ClientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await ClientSocket.ConnectAsync(ipEndpoint, cancellationToken);
        _logger.LogDebug("socket connected");
    }

    public bool IsConnected()
    {
        try
        {
            if (ClientSocket is null) return false;

            // Check if socket is connected and not disposed
            bool part1 = ClientSocket.Connected;

            // Poll (true if readable, but no data available = disconnected)
            bool part2 = !(ClientSocket.Poll(1, SelectMode.SelectRead) && ClientSocket.Available == 0);

            return part1 && part2;
        }
        catch
        {
            return false;
        }
    }

    public async Task SendDataAsync(string data, CancellationToken cancellationToken)
    {
        _logger.LogDebug("data to send: {data}", data);
        byte[] dataBuffer = Encoding.ASCII.GetBytes(data);

        await ClientSocket.SendAsync(dataBuffer, SocketFlags.None, cancellationToken);
        _logger.LogDebug("data sent");
    }

    /// <summary>
    /// Starts or returns a ChannelReader for live stream data for the given RS and camera index.
    /// </summary>
    public ChannelReader<cMediaFrame> ReceiveLiveStreamData(uint cameraIndex, CancellationToken cancellationToken, bool fileSave = false)
    {
        if (_liveStreamChannels.TryGetValue(cameraIndex, out var existingChannel))
        {
            return existingChannel.Reader;
        }

        var channel = Channel.CreateUnbounded<cMediaFrame>(new UnboundedChannelOptions { SingleWriter = true, SingleReader = false });
        _liveStreamChannels[cameraIndex] = channel;

        // Start the background task only once
        lock (_liveStreamTaskLock)
        {
            _liveStreamTask ??= Task.Run(async () =>
                {
                    string filePath = $"./temp-{DateTime.UtcNow.Ticks}.h264";
                    FileStream? fileStream = fileSave ? new(filePath, FileMode.Create, FileAccess.Write) : null;
                    try
                    {
                        byte[] frameHeaderBuffer = new byte[Constants.VIDEO_FRAME_HEADER_LENGTH];
                        int totalTransferedData = 0;
                        // Moving average variables
                        const int windowSize = 2000;
                        Queue<double> intervals = new();
                        DateTime? lastFrameTime = null;
                        double sumIntervals = 0;
                        int frameCount = 0;
                        DateTime dataReceivedTime = DateTime.UtcNow;

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                            cts.CancelAfter(TimeSpan.FromSeconds(15));
                            int bytesRead = 0;
                            try
                            {
                                bytesRead = await ClientSocket.ReceiveAsync(frameHeaderBuffer, SocketFlags.None, cts.Token);
                            }
                            catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                            {
                                _logger.LogInformation("ReceiveAsync timed out after 15 seconds.");
                            }
                            if (bytesRead < 40)
                            {
                                _logger.LogInformation("No live stream data received");
                                await Task.Delay(500, cancellationToken);

                                if (DateTime.UtcNow - dataReceivedTime > TimeSpan.FromSeconds(30))
                                {
                                    _logger.LogInformation("No data received for 30 seconds, closing media channel.");
                                    ClientSocket.Close();
                                    _liveStreamTask = null;
                                    break;
                                }

                                continue;
                            }

                            dataReceivedTime = DateTime.UtcNow;
                            totalTransferedData += bytesRead;

                            FrameHeader header = new();
                            header.DecodeFrameHeader(frameHeaderBuffer);
                            if (header.mediaFrmLen < 40) continue;

                            int frameSize = (int)header.mediaFrmLen - 40;
                            byte[] videoFrameBuffer = new byte[frameSize];
                            int totalBytes = 0;
                            while (totalBytes < frameSize)
                            {
                                bytesRead = await ClientSocket.ReceiveAsync(new ArraySegment<byte>(videoFrameBuffer, totalBytes, frameSize - totalBytes), SocketFlags.None, cancellationToken);
                                if (bytesRead == 0)
                                {
                                    _logger.LogInformation("Error: Connection closed by the client.");
                                    break;
                                }
                                totalBytes += bytesRead;
                            }

                            // Calculate moving average of time interval
                            var now = DateTime.UtcNow;
                            if (lastFrameTime.HasValue)
                            {
                                double intervalMs = (now - lastFrameTime.Value).TotalMilliseconds;
                                intervals.Enqueue(intervalMs);
                                sumIntervals += intervalMs;
                                if (intervals.Count > windowSize)
                                {
                                    sumIntervals -= intervals.Dequeue();
                                }
                                frameCount++;
                                if (frameCount % windowSize / 2 == 0)
                                {
                                    double movingAvg = sumIntervals / intervals.Count;
                                    _logger.LogInformation("Moving average interval (ms) over last {Count} frames: {movingAvg}", intervals.Count, movingAvg);
                                }
                            }
                            lastFrameTime = now;

                            var frame = new cMediaFrame(header, videoFrameBuffer);
                            // _logger.LogInformation("Received frame cameraSeqNo: {FrameHeader}, CameraIndex: {cameraIndex}", frame.Header.cameraSeqNo, cameraIndex);

                            // Write to the correct channel based on cameraSeqNo
                            if (_liveStreamChannels.TryGetValue(frame.Header.cameraSeqNo, out var camChannel))
                            {
                                await camChannel.Writer.WriteAsync(frame, cancellationToken);

                                totalTransferedData += frameSize;
                                _logger.LogDebug("bytes: {bytes}", totalTransferedData);

                                if (fileSave && totalTransferedData <= Constants.VIDEO_FILE_LIMIT)
                                {
                                    fileStream?.Write(videoFrameBuffer, 0, frameSize);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in live stream background task");
                    }
                    finally
                    {
                        if (fileSave)
                        {
                            fileStream?.Close();
                            fileStream?.Dispose();
                            _logger.LogInformation("File saved at: {filePath}", filePath);
                        }
                        CompleteAndClearChannels();
                    }
                }, cancellationToken);
        }

        return channel.Reader;
    }

    public void CompleteAndClearChannels()
    {
        foreach (var kvp in _liveStreamChannels)
        {
            kvp.Value.Writer.TryComplete();
        }
        _liveStreamChannels.Clear();
    }


    public async Task<string> ReceiveResponseAsync(CancellationToken cancellationToken)
    {
        var receiveBuffer = new byte[Constants.SOCKET_RECV_BUFFER_SIZE];
        var tempBuffer = new byte[Constants.SOCKET_READ_COUNT];

        Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
        int totalBytes = 0;

        while (true)
        {
            int bytesRead = await ClientSocket.ReceiveAsync(tempBuffer, SocketFlags.None, cancellationToken);
            if (bytesRead != 0)
            {
                Buffer.BlockCopy(tempBuffer, 0, receiveBuffer, totalBytes, tempBuffer.Length);
                totalBytes += bytesRead;

                byte lastbyte = tempBuffer[bytesRead - 1];

                if (lastbyte == Constants.EOM) break;

                Array.Clear(tempBuffer, 0, tempBuffer.Length);
            }

        }
        // Only use bytes up to the EOM marker
        int eomIndex = Array.IndexOf(receiveBuffer, Constants.EOM, 0, totalBytes);
        int length = eomIndex >= 0 ? eomIndex + 1 : totalBytes;
        string response = Encoding.ASCII.GetString(receiveBuffer, 0, length).Trim();
        _logger.LogInformation("Response receiveBuffer: {receiveBuffer}", response);
        return response;
    }

    public void Close()
    {
        if (ClientSocket.Connected)
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
        }
        ClientSocket.Close();
        _logger.LogDebug("Socket closed");
    }
}
