using ApplicationCore.Enums;
using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface ISystemInfoService
{
    public void AnalyzeSystem();
    
    public int NvidiaGpuCount { get; }
    public int RadeonGpuCount { get; }
    public CpuInfo Cpu { get; }
    public RamInfo Ram { get; }
    public LaptopInfo? LaptopInfo { get; }
    
    public string Manufacturer { get; }
    public string Product { get; }
    public string SystemName { get; }
    public bool IsGPUPresent(string gpuName);
    public decimal GetBatteryRate();
    public BatteryStatus GetBatteryStatus();
    public decimal ReadFullChargeCapacity();
    public decimal ReadDesignCapacity();
    public int GetBatteryCycle();
    public decimal GetBatteryHealth();

    public string GetBigLITTLE();
}