namespace WebApi.Models;

public record PlaybackSessionDetails
{
    public decimal PlaybackSpeed { get; set; }
    public ulong FramesSentCountAtCurrentSpeed { get; set; }
    public ulong FramesSentCountAtPreviousSpeeds { get; set; }
    public DateTime PreviousSpeed_LastFrame_MaxAheadTime { get; set; }
    public DateTime CurrentSpeed_LastCalculated_MaxAheadTime { get; set; }

    public PlaybackSessionDetails(decimal playbackSpeed = 0,
        ulong framesSentCountAtCurrentSpeed = 0,
        ulong framesSentCountAtPreviousSpeeds = 0,
        DateTime? previousSpeed_LastFrame_MaxAheadTime = null)
    {
        PlaybackSpeed = playbackSpeed;
        FramesSentCountAtCurrentSpeed = framesSentCountAtCurrentSpeed;
        FramesSentCountAtPreviousSpeeds = framesSentCountAtPreviousSpeeds;
        PreviousSpeed_LastFrame_MaxAheadTime = previousSpeed_LastFrame_MaxAheadTime ?? DateTime.MinValue;
    }
}