using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface ISystemInfoService
{
    public void AnalyzeSystem();
    
    public int NvidiaGpuCount { get; }
    public int RadeonGpuCount { get; }
    public CpuInfo CpuInfo { get; }
    public LaptopInfo? LaptopInfo { get; }
    
    public string Manufacturer { get; }
    public string Product { get; }
    public string SystemName { get; }
    public bool IsGPUPresent(string gpuName);
    public decimal GetBatteryRate();
    public decimal ReadFullChargeCapacity();
    public decimal ReadDesignCapacity();
    public int GetBatteryCycle();
    public decimal GetBatteryHealth();

    public List<uint> GetCacheSize(ApplicationCore.Enums.CacheLevel level);

    public string GetCodename();
    public string GetBigLITTLE(int cores, double l2);
    public string GetInstructionSets();
}