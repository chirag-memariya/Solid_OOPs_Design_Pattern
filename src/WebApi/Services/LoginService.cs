using WebApi.Models;
using WebApi.Sockets;
using WebApi.Utils;

namespace WebApi.Services;

public class LoginService(ILogger<LoginService> logger, ILogger<LinuxSocket> linuxSocketLogger, ILogger<CommandBuilder> commandBuilderLogger) : BaseService
{
    public bool isLoggedIn = false;
    public int errorCode = -1;
    public string sessionID = string.Empty;
    public string IpAddress = string.Empty;
    public uint port;

    private readonly ILogger<LoginService> _logger = logger;
    private readonly ILogger<LinuxSocket> _linuxSocketLogger = linuxSocketLogger;
    private readonly ILogger<CommandBuilder> _commandBuilderLogger = commandBuilderLogger;

    public async Task LoginAsync(User user, CancellationToken cancellationToken)
    {
        string[] reqFeilds = [user.Username, user.Password, user.IpAddress];
        IpAddress = user.IpAddress;
        port = (uint)user.Port;

        CommandModel ReqLog_CommandModel = new()
        {
            IpAddr = user.IpAddress,
            Port = user.Port,
            CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_LOG,
            ReqFields = reqFeilds,
            MediaClientType = 0
        };
        _commandSocket = new(ReqLog_CommandModel, _linuxSocketLogger);
        await _commandSocket.ConnectAsync(cancellationToken);

        var ReqLog_Command = CommandBuilder.BuildCommandString(ReqLog_CommandModel, _commandBuilderLogger);
        await _commandSocket.SendDataAsync(ReqLog_Command, cancellationToken);

        var ReqLog_Response = await _commandSocket.ReceiveResponseAsync(cancellationToken);
        ReqLog_CommandModel.RspValues = CommandBuilder.GetResponseArray(ReqLog_Response);

        int loginStatus = int.Parse(ReqLog_CommandModel.RspValues[1]);
        if (loginStatus == 0)
        {
            isLoggedIn = true;
            sessionID = ReqLog_CommandModel.RspValues[2];
            _logger.LogInformation("Login Successful with session ID {sessionID}", sessionID);
        }
        else
        {
            isLoggedIn = false;
            errorCode = loginStatus;
            _logger.LogError("Login Failed with error code {errorCode}", errorCode);
        }
    }
}