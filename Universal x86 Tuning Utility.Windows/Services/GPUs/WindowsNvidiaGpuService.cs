using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;
using Universal_x86_Tuning_Utility.Helpers;
using Universal_x86_Tuning_Utility.Windows.Helpers;
using static NvAPIWrapper.Native.GPU.Structures.PerformanceStates20InfoV1;

namespace Universal_x86_Tuning_Utility.Windows.Services.GPUs;

public class WindowsNvidiaGpuService : INvidiaGpuService
{
    private const int MinCoreOffset = -900;
    private const int MaxCoreOffset = 4000;
    
    private const int MinClockLimit = 400;
    private const int MaxClockLimit = 4000;
    
    private const int MinMemoryOffset = -900;
    private const int MaxMemoryOffset = 4000;
    
    private readonly Serilog.ILogger _logger;
    private readonly List<BasicGpuInfo> _gpuList = [];
    private readonly Lock _lock = new Lock();
    
    public WindowsNvidiaGpuService(Serilog.ILogger logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<BasicGpuInfo> Gpus
    {
        get
        {
            lock (_lock)
            {
                if (_gpuList.Count == 0)
                {
                    RefreshGpusList();
                }
            }

            return _gpuList;
        }
    }

    public async Task SetMaxGpuClock(uint gpuId, int value)
    {
        if (value is < MinClockLimit or >= MaxClockLimit)
        {
            if (GetMaxGpuClock(gpuId) != value)
            {
                if (value > 0)
                {
                    await ProcessHelpers.RunCmd("powershell", $"nvidia-smi -lgc 0,{value}");
                }
                else
                {
                    await ProcessHelpers.RunCmd("powershell", "nvidia-smi -rgc");
                }
            }
        }
    }

    public int GetMaxGpuClock(uint gpuId)
    {
        var internalGpu = PhysicalGPU.FromGPUId(gpuId);
        if (internalGpu == null) return -1;
        try
        {
            var clockBoostLock = GPUApi.GetClockBoostLock(internalGpu.Handle);
            int limit = (int)clockBoostLock.ClockBoostLocks[0].VoltageInMicroV / 1000;
            return limit;
        }
        catch
        {
            return -1;
        }
    }

    public void RefreshGpusList()
    {
        _gpuList.Clear();
        _gpuList.AddRange(PhysicalGPU.GetPhysicalGPUs().Select(x => new BasicGpuInfo(x.GPUId,
            GpuManufacturer.Nvidia,
            x.FullName,
            x.ArchitectInformation.NumberOfROPs,
            NvidiaHelper.CalculateTMU(x.ArchitectInformation.ShortName, x.ArchitectInformation.NumberOfCores),
            x.ArchitectInformation.NumberOfCores,
            (int) Math.Round(x.MemoryInformation.PhysicalFrameBufferSizeInkB / 1024d))));
    }

    /// <exception cref="AggregateException">Throws when no nvidia gpu installed or failed to set clocks</exception>
    public void SetClocks(uint gpuId, int core, int memory, int coreVoltage = 0)
    {
        if (core is < MinCoreOffset or > MaxCoreOffset)
            throw new ArgumentOutOfRangeException(nameof(core), "Core clock must be between MinCoreOffset and MaxCoreOffset");
        
        if (memory is < MinMemoryOffset or > MaxMemoryOffset)
            throw new ArgumentOutOfRangeException(nameof(memory), "Memory clock must be between MinMemoryOffset and MaxMemoryOffset");

        var internalGpu = PhysicalGPU.FromGPUId(gpuId);

        if (internalGpu == null) throw new AggregateException("No Nvidia gpu found");
        
        try
        {
            var coreClock = new PerformanceStates20ClockEntryV1(PublicClockDomain.Graphics, new PerformanceStates20ParameterDelta(core * 1000));
            var memoryClock = new PerformanceStates20ClockEntryV1(PublicClockDomain.Memory, new PerformanceStates20ParameterDelta(memory * 1000));
                
            PerformanceStates20ClockEntryV1[] clocks = [coreClock, memoryClock];
            
            PerformanceStates20BaseVoltageEntryV1[] voltages = [];
            if (coreVoltage != 0)
            {
                var voltageEntry = new PerformanceStates20BaseVoltageEntryV1(
                    PerformanceVoltageDomain.Core,
                    new PerformanceStates20ParameterDelta(coreVoltage));
                
                voltages = [voltageEntry];
            }
            
            PerformanceState20[] performanceStates = [new(PerformanceStateId.P0_3DPerformance, clocks, voltages)];

            var overclock = new PerformanceStates20InfoV1(performanceStates, 2, 0);

            GPUApi.SetPerformanceStates20(internalGpu.Handle, overclock);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set clocks");
            throw new AggregateException("Failed to set clocks");
        }
    }
}