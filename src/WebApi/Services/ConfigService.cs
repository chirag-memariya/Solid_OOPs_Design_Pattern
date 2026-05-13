using WebApi.Models;
using WebApi.Sockets;
using WebApi.Utils;

namespace WebApi.Services
{
    public class ConfigService(ILogger<ConfigService> logger, ILogger<LinuxSocket> linuxSocketLogger, ILogger<CommandBuilder> commandBuilderLogger, ILogger<XMLParser> xmlParserLogger) : BaseService
    {
        private readonly ILogger<ConfigService> _logger = logger;
        private readonly ILogger<LinuxSocket> _linuxSocketLogger = linuxSocketLogger;
        private readonly ILogger<CommandBuilder> _commandBuilderLogger = commandBuilderLogger;
        private readonly ILogger<XMLParser> _xmlParserLogger = xmlParserLogger;


        public Task<List<IEntity>> GetConfig(string ipAddress, uint port, string sessionID, CancellationToken cancellationToken, params string[] reqFieldsValue)
        {
            User user = new()
            {
                IpAddress = ipAddress,
                Port = (int)port
            };

            return GetConfig(user, sessionID, cancellationToken, reqFieldsValue);
        }

        public async Task<List<IEntity>> GetConfig(User currentUser, string sessionID, CancellationToken cancellationToken, params string[] reqFieldsValue)
        {
            _logger.LogInformation("reqvalues: {reqFieldsValue}", reqFieldsValue);

            CommandModel getConfig_CommandModel = new()
            {
                IpAddr = currentUser.IpAddress,
                Port = currentUser.Port,
                CmdMainId = (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_SET_CMD,
                SessionId = sessionID,
                CmdSubId = (int)Constants.CMD_SUB_ID_e.CMD_SUB_GET_CNFG,
                ReqFields = reqFieldsValue
            };

            // TODO retry logic is unused; remove at will
            int retryCount = 0;
            const int maxRetries = 0;
            const int delayMilliseconds = 3000;
            string getConfig_Response;
            do
            {
                if (_commandSocket is null || !_commandSocket.ClientSocket.Connected)
                {
                    _commandSocket = new(getConfig_CommandModel, _linuxSocketLogger);

                    await _commandSocket.ConnectAsync(cancellationToken);
                }

                var getConfig_Command = CommandBuilder.BuildCommandString(getConfig_CommandModel, _commandBuilderLogger);
                await _commandSocket.SendDataAsync(getConfig_Command, cancellationToken);

                getConfig_Response = await _commandSocket.ReceiveResponseAsync(cancellationToken);
                getConfig_CommandModel.RspValues = CommandBuilder.GetResponseArray(getConfig_Response);

                if (getConfig_CommandModel.RspValues.Length >= 4)
                    break;

                retryCount++;
                if (retryCount < maxRetries)
                    await Task.Delay(delayMilliseconds);
            }
            while (retryCount < maxRetries);

            // TODO check if below line is necessary
            getConfig_CommandModel.RspValues = CommandBuilder.GetResponseArray(getConfig_Response);

            return CommandBuilder.ParseResponseString(getConfig_CommandModel, getConfig_CommandModel.RspValues[3], _commandBuilderLogger, _xmlParserLogger);
        }
    }
}