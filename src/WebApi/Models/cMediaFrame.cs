namespace WebApi.Models;

public class cMediaFrame
{
    public FrameHeader Header { get; set; }
    public Memory<byte> Data { get; set; }

    public cMediaFrame(FrameHeader header, Memory<byte> data)
    {
        Header = header;
        Data = data;
    }
}
