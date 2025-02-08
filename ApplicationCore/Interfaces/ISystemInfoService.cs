using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface ISystemInfoService
{
    public void AnalyzeSystem();
    
    public int NvidiaGpuCount { get; }
    public int RadeonGpuCount { get; }
    public CpuInfo CpuInfo { get; set; }
}