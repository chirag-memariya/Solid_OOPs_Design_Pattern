using System.Xml;
using WebApi.Models;

namespace WebApi.Utils
{
    public class XMLParser(string str, ILogger<XMLParser> logger)
    {
        private readonly string xmlString = str;
        private readonly XmlDocument parser = new();
        private readonly ILogger<XMLParser> _logger = logger;

        public List<IEntity> parseDeviceList()
        {
            List<IEntity> devices = [];
            parser.LoadXml(xmlString);
            XmlNodeList deviceNodes = parser.DocumentElement.ChildNodes;

            foreach (XmlNode device in deviceNodes)
            {
                Device newDevice = new()
                {
                    DevId = Convert.ToInt32(device.Attributes["DevId"].Value),
                    DevName = device.Attributes["DevName"].Value,
                    RSId = Convert.ToInt32(device.Attributes["RSId"].Value),
                    DType = Convert.ToInt32(device.Attributes["DType"].Value),
                    FosID = Convert.ToInt32(device.Attributes["FosID"].Value),
                    DevUnderMaintenance = Convert.ToInt32(device.Attributes["DevUnderMaintenance"].Value),
                    Reason = device.Attributes["Reason"].Value
                };

                _logger.LogInformation("Device" + newDevice.DevName);

                devices.Add(newDevice.GetEntity());
            }

            return devices;
        }

        public List<IEntity> parseCameraList()
        {
            List<IEntity> cameraList = [];
            parser.LoadXml(xmlString);
            XmlNodeList cameraNodes = parser.DocumentElement.ChildNodes;

            foreach (XmlNode cameraNode in cameraNodes)
            {
                if (cameraNode != null)
                {
                    Camera newCamera = new()
                    {
                        SeqNo = Convert.ToInt32(cameraNode.Attributes["SeqNo"].Value),
                        RSId = Convert.ToInt32(cameraNode.Attributes["RSId"].Value),
                        FoSID = Convert.ToInt32(cameraNode.Attributes["FoSID"].Value),
                        lLPRDetMode = Convert.ToInt32(cameraNode.SelectSingleNode("lLPRDetMode")?.InnerText),
                        IUnauthorizedDetMode = Convert.ToInt32(cameraNode.SelectSingleNode("IUnauthorizedDetMode")?.InnerText),
                        CID = Convert.ToInt32(cameraNode.SelectSingleNode("CID")?.InnerText),
                        CName = cameraNode.SelectSingleNode("CName")?.InnerText,
                        CType = Convert.ToInt32(cameraNode.SelectSingleNode("CType")?.InnerText),
                        DeviceID = Convert.ToInt32(cameraNode.SelectSingleNode("DeviceID")?.InnerText),
                        LogGrpID = Convert.ToInt32(cameraNode.SelectSingleNode("LogGrpID")?.InnerText),
                        IsMicro = Convert.ToInt32(cameraNode.SelectSingleNode("IsMicro")?.InnerText),
                        IsPTZ = Convert.ToInt32(cameraNode.SelectSingleNode("IsPTZ")?.InnerText),
                        lLPR = Convert.ToInt32(cameraNode.SelectSingleNode("lLPR")?.InnerText),
                        lFD = Convert.ToInt32(cameraNode.SelectSingleNode("lFD")?.InnerText),
                        lPC = Convert.ToInt32(cameraNode.SelectSingleNode("lPC")?.InnerText),
                        lVC = Convert.ToInt32(cameraNode.SelectSingleNode("lVC")?.InnerText),
                        lTG = Convert.ToInt32(cameraNode.SelectSingleNode("lTG")?.InnerText),
                        IsAnalog = Convert.ToInt32(cameraNode.SelectSingleNode("IsAnalog")?.InnerText),
                        lImproper = Convert.ToInt32(cameraNode.SelectSingleNode("lImproper")?.InnerText),
                        IUnauthorized = Convert.ToInt32(cameraNode.SelectSingleNode("IUnauthorized")?.InnerText),
                        lProhibited = Convert.ToInt32(cameraNode.SelectSingleNode("lProhibited")?.InnerText),
                        lPremisesPeople = Convert.ToInt32(cameraNode.SelectSingleNode("lPremisesPeople")?.InnerText),
                        lPremisesVehicle = Convert.ToInt32(cameraNode.SelectSingleNode("lPremisesVehicle")?.InnerText),
                        IWrongWay = Convert.ToInt32(cameraNode.SelectSingleNode("IWrongWay")?.InnerText),
                        Address = cameraNode.SelectSingleNode("Address")?.InnerText,
                        LandlineNo = cameraNode.SelectSingleNode("LandlineNo")?.InnerText,
                        MobNo = cameraNode.SelectSingleNode("MobNo")?.InnerText,
                        DevUnderMaintenance = Convert.ToInt32(cameraNode.SelectSingleNode("DevUnderMaintenance")?.InnerText),
                        Reason = cameraNode.SelectSingleNode("Reason")?.InnerText
                    };

                    cameraList.Add(newCamera.GetEntity());
                }
            }

            return cameraList;
        }
        public List<IEntity> parseRecordingServerList()
        {
            List<IEntity> recordingServerList = [];
            parser.LoadXml(xmlString);

            XmlNodeList serverNodes = parser.DocumentElement.ChildNodes;

            _logger.LogInformation("Server List");

            foreach (XmlNode serverNode in serverNodes)
            {
                if (serverNode != null)
                {
                    RecordingServer newServer = new()
                    {
                        ID = Convert.ToInt32(serverNode.Attributes["ID"].Value),
                        IPAdd = serverNode.SelectSingleNode("IPAdd")?.InnerText ?? "",
                        RSName = serverNode.SelectSingleNode("RSName")?.InnerText ?? "",
                        Port = int.Parse(serverNode.SelectSingleNode("Port")?.InnerText ?? ""),
                        FIP = serverNode.SelectSingleNode("FIP")?.InnerText ?? "",
                        FPRT = int.Parse(serverNode.SelectSingleNode("FPRT")?.InnerText ?? ""),
                        BNAT = int.Parse(serverNode.SelectSingleNode("BNAT")?.InnerText ?? ""),
                        FIP2 = serverNode.SelectSingleNode("FIP2")?.InnerText ?? "",
                        FPRT2 = int.Parse(serverNode.SelectSingleNode("FPRT2")?.InnerText ?? ""),
                        SSLPort = int.Parse(serverNode.SelectSingleNode("SSLPort")?.InnerText ?? ""),
                        Multicast = int.Parse(serverNode.SelectSingleNode("Multicast")?.InnerText ?? "")
                    };

                    recordingServerList.Add(newServer.GetEntity());
                }
            }
            return recordingServerList;
        }
    }
}