using WebApi.Utils;

namespace WebApi.Models
{
    public class RecordingServer : IEntity
    {
        public int ID { get; set; }
        public string RSName { get; set; }
        public string IPAdd { get; set; }

        public int Port { get; set; }
        public string FIP { get; set; }
        public int FPRT { get; set; }
        public int BNAT { get; set; }
        public string FIP2 { get; set; }
        public int FPRT2 { get; set; }
        public int SSLPort { get; set; }
        public int Multicast { get; set; }
        public IEntity GetEntity()
        {
            return this;
        }
    }
}