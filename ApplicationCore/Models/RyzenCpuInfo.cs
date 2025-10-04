using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class RyzenCpuInfo : CpuInfo
{
    public RyzenFamily RyzenFamily { get; set; }
    public RyzenGeneration RyzenGeneration { get; set; }
    public RyzenSeries RyzenSeries { get; set; }
}