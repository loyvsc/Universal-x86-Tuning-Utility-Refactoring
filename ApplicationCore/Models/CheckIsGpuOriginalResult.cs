namespace ApplicationCore.Models;

public class CheckIsGpuOriginalResult
{
    public int GpuNumber { get; init; }
    public string GpuName { get; init; }
    public int ExpectedRopCount { get; init; }
    public int ActualRopCount { get; init; }
    public bool IsGpuOriginal { get; init; }
}