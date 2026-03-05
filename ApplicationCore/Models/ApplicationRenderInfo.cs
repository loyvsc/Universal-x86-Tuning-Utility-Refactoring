namespace ApplicationCore.Models;

public class ApplicationRenderInfo
{
    public int ProcessId { get; }
    public string Name { get; } 
    public uint InstantaneousFrames { get; }
    public uint TotalFramesCount { get; }
    public uint AverageFramerate { get; }
    public uint MinFramerate { get; }
    public uint MaxFramerate { get; }
    public uint MinFrameTime { get; }
    public uint MaxFrameTime { get; }
    public uint AverageFrameTime { get; }
    public TimeSpan InstantaneousFrameTime { get; }

    public ApplicationRenderInfo(int processId, string name, uint instantaneousFrames, uint totalFramesCount, uint averageFramerate, uint minFramerate, uint maxFramerate, uint minFrameTime, uint maxFrameTime, uint averageFrameTime, TimeSpan instantaneousFrameTime)
    {
        ProcessId = processId;
        Name = name;
        TotalFramesCount = totalFramesCount;
        AverageFramerate = averageFramerate;
        MinFramerate = minFramerate;
        MaxFramerate = maxFramerate;
        MinFrameTime = minFrameTime;
        MaxFrameTime = maxFrameTime;
        AverageFrameTime = averageFrameTime;
        InstantaneousFrameTime = instantaneousFrameTime;
        InstantaneousFrames = instantaneousFrames;
    }
}