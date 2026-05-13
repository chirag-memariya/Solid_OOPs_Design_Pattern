using System.Threading.Channels;
using WebApi.Models;
using WebApi.Sockets;
using WebApi.Utils;

namespace WebApi.Services;

public class RSService(ILogger<RSService> logger, ILogger<LinuxSocket> linuxSocketLogger, ILogger<CommandBuilder> commandBuilderLogger) : BaseService
{
    private LinuxSocket? _dataSocket;
    private new LinuxSocket? _commandSocket;
    private LinuxSocket? _connectCommandSocket; // Separate command socket for ConnectRS
    public List<RecordingServer> _recordingServerList = [];
    private string _crsId = string.Empty;
    private readonly SemaphoreSlim _createDataChannelLock = new(1, 1);
    private readonly SemaphoreSlim _commandSocketLock = new(1, 1);
    private readonly SemaphoreSlim _connectSocketLock = new(1, 1);

    private readonly ILogger<RSService> _logger = logger;
    private readonly ILogger<LinuxSocket> _linuxSocketLogger = linuxSocketLogger;
    private readonly ILogger<CommandBuilder> _commandBuilderLogger = commandBuilderLogger;

    public async Task<bool> IsConnected(CancellationToken cancellationToken = default)
    {
        var returnvalue = (_connectCommandSocket is not null) && _connectCommandSocket.IsConnected() && !string.IsNullOrEmpty(_crsId);
        if (!returnvalue)
        {
            if (!await _connectSocketLock.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
                throw new TimeoutException("Failed to acquire _connectSocketLock");

            try
            {
                _commandSocket?.Close();
                _dataSocket?.Close();
                _dataSocket?.CompleteAndClearChannels();
                _dataSocket?.NullifyLiveStreamTask();
            }
            finally
            {
                _connectSocketLock.Release();
            }
        }
        return returnvalue;
    }

    public async Task ConnectRS(uint zeroBasedRSindex, CancellationToken cancellationToken)
    {
        if (!await _connectSocketLock.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
            throw new TimeoutException("Failed to acquire _connectSocketLock");

        try
        {
            RecordingServer recordingServer = _recordingServerList[(int)zeroBasedRSindex];

            string[] reqFeilds = [recordingServer.ID.ToString(), "123456"];
            CommandModel req_con_CommandModel = new()
            {
                IpAddr = recordingServer.IPAdd,
                Port = recordingServer.Port,
                CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CON,
                ReqFields = reqFeilds,
                MediaClientType = 1
            };
            if (_connectCommandSocket is null || !_connectCommandSocket.IsConnected())
                _connectCommandSocket = new(req_con_CommandModel, _linuxSocketLogger);

            await _connectCommandSocket.ConnectAsync(cancellationToken);

            var req_con_command = CommandBuilder.BuildCommandString(req_con_CommandModel, _commandBuilderLogger);
            await _connectCommandSocket.SendDataAsync(req_con_command, cancellationToken);
            var req_con_reponse = await _connectCommandSocket.ReceiveResponseAsync(cancellationToken);
            req_con_CommandModel.RspValues = CommandBuilder.GetResponseArray(req_con_reponse);
            _logger.LogDebug("Req_Con reponse {0th_Value}", req_con_CommandModel.RspValues[0]);

            if ((req_con_CommandModel.RspValues.Length >= 5) && (int.Parse(req_con_CommandModel.RspValues[1]) == 0))
            {
                _logger.LogInformation("Connection established with RS {CRS_ID} {3rd_Value} {4thValue}.",
                    req_con_CommandModel.RspValues[2],
                    req_con_CommandModel.RspValues[3],
                    req_con_CommandModel.RspValues[4]);

                _crsId = req_con_CommandModel.RspValues[2];

                //     _ = Task.Run(async () =>
                //    {
                //        try
                //        {
                //            await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                //            await CheckDataSocketAndRestartLiveStreams(cancellationToken);
                //        }
                //        // catch (OperationCanceledException)
                //        // {
                //        //     // Ignore cancellation
                //        // }
                //        catch (Exception ex)
                //        {
                //            _logger.LogError(ex, "Error occurred while restarting live streams.");
                //        }
                //    }, cancellationToken);
            }
            else throw new("Invalid response.");
        }
        finally
        {
            _connectSocketLock.Release();
        }
    }


    // public async Task CheckDataSocketAndRestartLiveStreams(CancellationToken cancellationToken)
    // {
    //     while (true)
    //     {
    //         if (_dataSocket.IsConnected())
    //         {
    //             await Task.Delay(500, cancellationToken);
    //             continue;
    //         }
    //         else
    //         {
    //             var cameraSeqNumbersToRestart = _dataSocket.RequestedCameraSeqNumbers();
    //             int secondsToWaitForRSCameraReconnection = 30 * 1000;
    //             await Task.Delay(secondsToWaitForRSCameraReconnection, cancellationToken);
    //             foreach (var cameraSeqNo in Enumerable.Range(4, 7)) // 4 to 10 inclusive
    //             {
    //                 try
    //                 {                    // TODO: replace hardcoded RSIndex with stored and passes dynamically
    //                     await StartLiveStream(0, (uint)cameraSeqNo, cancellationToken);
    //                     _ = await StreamCMediaFrames((uint)cameraSeqNo, cancellationToken);
    //                 }
    //                 catch
    //                 {
    //                     _logger.LogError("Failed to restart live stream for camera sequence number: {CameraSeqNo}", cameraSeqNo);
    //                 }
    //             }
    //             await Task.Delay(secondsToWaitForRSCameraReconnection, cancellationToken);
    //         }
    //     }
    // }

    public async Task LongPollInfinitely(CancellationToken cancellationToken)
    {
        // Request Live Stream
        CommandModel reqPol_CommandModel = new()
        {
            IpAddr = _commandSocket!.ipEndpoint.Address.ToString(),
            Port = _commandSocket.ipEndpoint.Port,
            CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_SET_CMD,
            CmdSubId = (int)Constants.CMD_SUB_ID_e.CMD_SUB_REQ_POL,
            SessionId = _crsId
        };
        var rqlPol_command = CommandBuilder.BuildCommandString(reqPol_CommandModel, _commandBuilderLogger);

        int retryCount = 0, maxRetryCount = 10;
        TimeSpan pollInterval = TimeSpan.FromSeconds(10);

        while (true)
        {
            await Task.Delay(pollInterval, cancellationToken);
            await _commandSocket.SendDataAsync(rqlPol_command, cancellationToken);
            string reqPolResponse = await _commandSocket.ReceiveResponseAsync(cancellationToken);
            reqPol_CommandModel.RspValues = CommandBuilder.GetResponseArray(reqPolResponse);

            if (int.Parse(reqPol_CommandModel.RspValues[2]) == (int)Constants.CMD_SUB_ID_e.CMD_SUB_REQ_POL && int.Parse(reqPol_CommandModel.RspValues[1]) == 0)
            {
                retryCount = 0;
                // TODO: change level to Debug
                _logger.LogInformation("Long Polling reply received for session: {CRS_ID}, Response: {Response} Successful", _crsId, reqPol_CommandModel.RspValues[0]);
                continue;
            }

            if (++retryCount >= maxRetryCount) throw new($"Long Polling reply indicates failure for session: {_crsId}, retrycount, {retryCount - 1}");
        }
    }

    public async Task StartLiveStream(uint zeroBasedRSindex, uint zeroBasedCameraIndex, CancellationToken cancellationToken)
    {
        await _commandSocketLock.WaitAsync(cancellationToken);
        try
        {
            RecordingServer recordingServer = _recordingServerList[(int)zeroBasedRSindex];
            // Create Command Channel
            CommandModel createCommandChannel_CommandModel = new()
            {
                IpAddr = recordingServer.IPAdd,
                Port = recordingServer.Port,
                CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CHNL,
                // crsId, channelID (2 : live stream), Playback Session ID 
                ReqFields = [_crsId, "0", "-1"],
                MediaClientType = 1
            };
            if (_commandSocket is null || !_commandSocket.IsConnected())
            {
                _commandSocket = new(createCommandChannel_CommandModel, _linuxSocketLogger);
                bool createCommandChannelStatus = await CreateChannel(_commandSocket, createCommandChannel_CommandModel, cancellationToken);
                if (!createCommandChannelStatus) throw new("Could not create Command Channel.");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                        await LongPollInfinitely(cancellationToken);
                    }
                    // catch (OperationCanceledException)
                    // {
                    //     // Ignore cancellation
                    // }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred during LongPollInfinitely.");
                    }
                }, cancellationToken);
            }
            await _createDataChannelLock.WaitAsync(cancellationToken);
            try
            {
                if ((_dataSocket is null) || !_dataSocket.IsConnected())
                {
                    //data channel
                    CommandModel createDataChannel_CommandModel = new()
                    {
                        IpAddr = recordingServer.IPAdd,
                        Port = recordingServer.Port,
                        CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CHNL,
                        ReqFields = [_crsId, "2", "-1"],
                        MediaClientType = 1
                    };
                    _dataSocket = new(createDataChannel_CommandModel, _linuxSocketLogger);
                    bool createDataChannelStatus = await CreateChannel(_dataSocket, createDataChannel_CommandModel, cancellationToken);
                    if (!createDataChannelStatus) throw new("Could not create Data Channel.");
                }
            }
            finally
            {
                _createDataChannelLock.Release();
            }

            // Request Live Stream
            CommandModel startLiveView_CommandModel = new()
            {
                IpAddr = recordingServer.IPAdd,
                CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_SET_CMD,
                CmdSubId = (int)Constants.CMD_SUB_ID_e.CMD_SUB_SRT_LV_STRM,
                // Camera SeqNo (30), stream type (1: recording), RSID, stream mode(0: unicast), profile ID
                ReqFields = [zeroBasedCameraIndex.ToString(), "0", recordingServer.ID.ToString(), "0"],
                SessionId = _crsId
            };
            var startLiveView_command = CommandBuilder.BuildCommandString(startLiveView_CommandModel, _commandBuilderLogger);
            await _commandSocket.SendDataAsync(startLiveView_command, cancellationToken);

            string liveViewResponse = await _commandSocket.ReceiveResponseAsync(cancellationToken);
            startLiveView_CommandModel.RspValues = CommandBuilder.GetResponseArray(liveViewResponse);
            if (int.Parse(startLiveView_CommandModel.RspValues[1]) == 0)
                _logger.LogInformation("Live View Successful, {1st_Value}", startLiveView_CommandModel.RspValues[1]);
            else if (int.Parse(startLiveView_CommandModel.RspValues[1]) == 54)
                _logger.LogInformation("Live View Already Running, {1st_Value}", startLiveView_CommandModel.RspValues[1]);
            else throw new($"Response for start LiveView: {startLiveView_CommandModel.RspValues[1]}, Session ID: {_crsId}");
        }
        finally
        {
            _commandSocketLock.Release();
        }
    }

    public ChannelReader<cMediaFrame> StreamCMediaFrames(uint oneBasedCameraIndex, CancellationToken cancellationToken)
    {
        if (_dataSocket is null) throw new("Data socket is not initialized. Please start the live stream first.");
        return _dataSocket.ReceiveLiveStreamData(oneBasedCameraIndex, cancellationToken, false);
    }

    private async Task<bool> CreateChannel(LinuxSocket channelSocket, CommandModel commandModel, CancellationToken cancellationToken)
    {
        if (!channelSocket.IsConnected())
            await channelSocket.ConnectAsync(cancellationToken);

        var request = CommandBuilder.BuildCommandString(commandModel, _commandBuilderLogger);
        await channelSocket.SendDataAsync(request, cancellationToken);

        var response = await channelSocket.ReceiveResponseAsync(cancellationToken);
        commandModel.RspValues = CommandBuilder.GetResponseArray(response);

        if (int.Parse(commandModel.RspValues[1]) == 0)
        {
            _logger.LogDebug("Channel acquired successfully.");
            return true;
        }
        else
        {
            _logger.LogError("Error RS status code: {ReplyStatus}", commandModel.RspValues[0]);
            return false;
        }
    }

}
