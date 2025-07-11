using ApplicationCore.Models;
using ApplicationCore.Models.LaptopInfo;

namespace ApplicationCore.Interfaces;

public interface ISystemInfoService
{
    public void ReAnalyzeSystem();
    
    public CpuInfo Cpu { get; }
    public RamInfo Ram { get; }
    public LaptopInfoBase? LaptopInfo { get; }
    public IReadOnlyCollection<BasicGpuInfo> Gpus { get; }
    
    public Lazy<string> Manufacturer { get; }
    public Lazy<string> Product { get; }
    public Lazy<string> SystemName { get; }
}