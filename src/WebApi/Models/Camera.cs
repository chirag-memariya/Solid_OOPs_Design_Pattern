
using WebApi.Utils;

public class Camera : IEntity
{
    public int SeqNo { get; set; }
    public int RSId { get; set; }
    public int FoSID { get; set; }
    public int lLPRDetMode { get; set; }
    public int IUnauthorizedDetMode { get; set; }
    public int CID { get; set; }
    public string CName { get; set; }
    public int CType { get; set; }
    public int DeviceID { get; set; }
    public int LogGrpID { get; set; }
    public int IsMicro { get; set; }
    public int IsPTZ { get; set; }
    public int lLPR { get; set; }
    public int lFD { get; set; }
    public int lPC { get; set; }
    public int lVC { get; set; }
    public int lTG { get; set; }
    public int IsAnalog { get; set; }
    public int lImproper { get; set; }
    public int IUnauthorized { get; set; }
    public int lProhibited { get; set; }
    public int lPremisesPeople { get; set; }
    public int lPremisesVehicle { get; set; }
    public int IWrongWay { get; set; }
    public string Address { get; set; }
    public string LandlineNo { get; set; }
    public string MobNo { get; set; }
    public int DevUnderMaintenance { get; set; }
    public string Reason { get; set; }
    public IEntity GetEntity()
    {
        return this;
    }

}