using System.Collections.Generic;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.Extensions.Logging;

namespace Universal_x86_Tuning_Utility.Services.SystemInfoServices;

public class LinuxSystemInfoService : ISystemInfoService
{
    public void AnalyzeSystem()
    {
        throw new System.NotImplementedException();
    }

    public int NvidiaGpuCount { get; }
    public int RadeonGpuCount { get; }
    public CpuInfo Cpu { get; }
    public RamInfo Ram { get; }
    public LaptopInfo? LaptopInfo { get; }
    public string Manufacturer { get; }
    public string Product { get; }
    public string SystemName { get; }
    public bool IsGPUPresent(string gpuName)
    {
        throw new System.NotImplementedException();
    }

    public decimal GetBatteryRate()
    {
        throw new System.NotImplementedException();
    }

    public BatteryStatus GetBatteryStatus()
    {
        throw new System.NotImplementedException();
    }

    public decimal ReadFullChargeCapacity()
    {
        throw new System.NotImplementedException();
    }

    public decimal ReadDesignCapacity()
    {
        throw new System.NotImplementedException();
    }

    public int GetBatteryCycle()
    {
        throw new System.NotImplementedException();
    }

    public decimal GetBatteryHealth()
    {
        throw new System.NotImplementedException();
    }

    public string GetBigLITTLE(int cores, double l2)
    {
        throw new System.NotImplementedException();
    }
}