namespace WebApi.Models
{
    public class CommandModel
    {
        public string? IpAddr { get; set; }
        public int Port { get; set; }
        public string? SessionId { get; set; }

        public int CmdMainId { get; set; }
        public int CmdSubId { get; set; }

        public int MediaClientType { get; set; }
        public byte SaveSocket { get; set; }
        public int CmdOpErr { get; set; }

        public string[]? ReqFields { get; set; }
        public string[]? RspValues { get; set; }
    }
}