namespace ApplicationCore.Models;

public class GpuSpecs
{
    public int RopCount { get; init; }
    public int TmusCount { get; init; }
    public int ShadersCount { get; init; }
    
    /// <summary>
    /// In megabytes
    /// </summary>
    public int MemorySize { get; init; }
}