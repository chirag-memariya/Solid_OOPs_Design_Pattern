using WebApi.Models;
namespace WebApi.Utils
{
    public class CommandBuilder
    {
        static readonly string[] cmdMainArray =
            [
                "REQ_LOG",
                "REQ_CHNL",
                "SET_CMD",
                "REQ_CON",
                "REQ_POL"
            ];

        public static string BuildCommandString(CommandModel commandParams, ILogger<CommandBuilder> _logger)
        {
            string result = string.Empty;

            switch (commandParams.CmdMainId)
            {
                case (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_LOG:
                    {

                        Guid newGuid = Guid.NewGuid();

                        result += Constants.SOM + cmdMainArray[(int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_LOG]
                                + Constants.FSP + Constants.SMART_CODE + Constants.FSP + commandParams.ReqFields[0]
                                + Constants.FSP + commandParams.ReqFields[1] + Constants.FSP + commandParams.MediaClientType + Constants.FSP + commandParams.ReqFields[2] + Constants.FSP + newGuid.ToString() + Constants.FSP + Constants.EOM;

                        break;
                    }

                case (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_SET_CMD:
                    {
                        result += Constants.SOM + cmdMainArray[(int)Constants.CMD_MAIN_ID_e.CMD_MAIN_SET_CMD]
                                + Constants.FSP + commandParams.SessionId + Constants.FSP + commandParams.CmdSubId + Constants.FSP;

                        if (commandParams.ReqFields != null)
                        {
                            for (int itr = 0; itr < commandParams.ReqFields.Length; itr++)
                            {
                                result += commandParams.ReqFields[itr] + Constants.FSP;
                            }
                        }

                        result += Constants.EOM;
                        break;
                    }

                case (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CON:
                    {
                        result += Constants.SOM + cmdMainArray[(int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CON] + Constants.FSP
                                + Constants.SMART_CODE + Constants.FSP + commandParams.MediaClientType + Constants.FSP
                                + commandParams.ReqFields[0] + Constants.FSP + commandParams.ReqFields[1] + Constants.FSP + Constants.EOM;
                        break;
                    }
                case (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CHNL:
                    {
                        result = Constants.SOM + cmdMainArray[(int)Constants.CMD_MAIN_ID_e.CMD_MAIN_REQ_CHNL]
                        + Constants.FSP + commandParams.ReqFields[0] + Constants.FSP + commandParams.ReqFields[1] + Constants.FSP
                        + commandParams.ReqFields[2] + Constants.FSP + Constants.EOM;

                        result += Constants.EOM;
                        break;
                    }
                default:
                    break;
            }


            _logger.LogInformation(result);

            return result;
        }

        public static string[] GetResponseArray(string responseString)
        {
            return responseString.Split((char)Constants.FSP);
        }

        public static List<IEntity> ParseResponseString(CommandModel commandParams, string responseString, ILogger<CommandBuilder> _logger, ILogger<XMLParser> xmlParserLogger)
        {
            switch (commandParams.CmdMainId)
            {
                case (int)Constants.CMD_MAIN_ID_e.CMD_MAIN_SET_CMD:
                    {
                        XMLParser xmlParser = new(responseString, xmlParserLogger);

                        // Parse RecordingServer List
                        if (commandParams.ReqFields[0] == Constants.CST_RS_CNFG_CMD_VALUE.ToString())
                        {
                            return xmlParser.parseRecordingServerList();
                        }

                        // Parse Device List
                        if (commandParams.ReqFields[0] == Constants.CST_DEVICE_CNFG_CMD_VALUE.ToString())
                        {
                            return xmlParser.parseDeviceList();
                        }

                        // Parse Camera List
                        if (commandParams.ReqFields[0] == Constants.CST_CAMERA_CNFG_CMD_VALUE.ToString())
                        {
                            return xmlParser.parseCameraList();
                        }

                        return [];
                    }
                default:
                    return [];
            }
        }
    }
}