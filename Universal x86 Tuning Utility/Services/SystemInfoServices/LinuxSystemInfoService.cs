using System;
using System.Collections.Generic;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Models.LaptopInfo;

namespace Universal_x86_Tuning_Utility.Services.SystemInfoServices;

public class LinuxSystemInfoService : ISystemInfoService
{
    public void ReAnalyzeSystem()
    {
        throw new System.NotImplementedException();
    }

    public CpuInfo Cpu { get; }
    public RamInfo Ram { get; }
    public LaptopInfoBase? LaptopInfo { get; }
    public IReadOnlyCollection<BasicGpuInfo> Gpus { get; }
    public Lazy<string> Manufacturer { get; }
    public Lazy<string> Product { get; }
    public Lazy<string> SystemName { get; }
}