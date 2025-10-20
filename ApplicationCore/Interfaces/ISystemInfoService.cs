using ApplicationCore.Enums;
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
    
    public string Manufacturer { get; }
    public string Product { get; }
    public string SystemName { get; }
    
    public ChassisType ChassisType { get; }
}