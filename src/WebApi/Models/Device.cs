using WebApi.Utils;

namespace WebApi.Models
{
    public class Device : IEntity
    {
        public int DevId { get; set; }
        public string DevName { get; set; }
        public int RSId { get; set; }
        public int DType { get; set; }
        public int FosID { get; set; }
        public int DevUnderMaintenance { get; set; }
        public string Reason { get; set; }
        public IEntity GetEntity()
        {
            return this;
        }
    }

}