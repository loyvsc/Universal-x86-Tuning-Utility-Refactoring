using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.Extensions.Logging;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;
using static NvAPIWrapper.Native.GPU.Structures.PerformanceStates20InfoV1;

namespace Universal_x86_Tuning_Utility.Services.GPUs.NVIDIA;

public class WindowsNvidiaGpuService : INvidiaGpuService
{
    private const int MinCoreOffset = -900;
    private const int MaxCoreOffset = 4000;
    
    private const int MinClockLimit = 400;
    private const int MaxClockLimit = 4000;
    
    private const int MinMemoryOffset = -900;
    private const int MaxMemoryOffset = 4000;
    
    private readonly ILogger<WindowsNvidiaGpuService> _logger;
    private readonly Lazy<Dictionary<string, int>> _ropCountDictionary = new Lazy<Dictionary<string, int>>(() =>
    {
        var data = File.ReadAllText(@"./Assets/nvidiaGpusData.json");
        var deserializedData = JsonSerializer.Deserialize<Dictionary<string, int>>(data);
        return deserializedData ?? new Dictionary<string, int>();
    });
    
    public WindowsNvidiaGpuService(ILogger<WindowsNvidiaGpuService> logger)
    {
        _logger = logger;
    }
    
    /// <exception cref="AggregateException">Throws when no nvidia gpu installed or failed to set clocks</exception>
    public void SetClocks(int core, int memory, int coreVoltage = 0)
    {
        if (core is < MinCoreOffset or > MaxCoreOffset)
            throw new ArgumentOutOfRangeException(nameof(core), "Core clock must be between MinCoreOffset and MaxCoreOffset");
        
        if (memory is < MinMemoryOffset or > MaxMemoryOffset)
            throw new ArgumentOutOfRangeException(nameof(memory), "Memory clock must be between MinMemoryOffset and MaxMemoryOffset");

        var internalGpu = PhysicalGPU.GetPhysicalGPUs().FirstOrDefault();

        if (internalGpu == null) throw new AggregateException("No Nvidia gpu found");
        
        try
        {
            var coreClock = new PerformanceStates20ClockEntryV1(PublicClockDomain.Graphics, new PerformanceStates20ParameterDelta(core * 1000));
            var memoryClock = new PerformanceStates20ClockEntryV1(PublicClockDomain.Memory, new PerformanceStates20ParameterDelta(memory * 1000));
                
            PerformanceStates20ClockEntryV1[] clocks =
            {
                coreClock, memoryClock
            };
            
            PerformanceStates20BaseVoltageEntryV1[] voltages = { };
            if (coreVoltage != 0)
            {
                var voltageEntry = new PerformanceStates20BaseVoltageEntryV1(
                    PerformanceVoltageDomain.Core,
                    new PerformanceStates20ParameterDelta(coreVoltage));
                
                voltages = new[]
                {
                    voltageEntry
                };
            }
            
            PerformanceState20[] performanceStates =
            {
                new(PerformanceStateId.P0_3DPerformance, clocks, voltages)
            };

            var overclock = new PerformanceStates20InfoV1(performanceStates, 2, 0);

            GPUApi.SetPerformanceStates20(internalGpu.Handle, overclock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set clocks");
            throw new AggregateException("Failed to set clocks");
        }
    }

    public int MaxGpuClock
    {
        get
        {
            var internalGpu = PhysicalGPU.GetPhysicalGPUs().FirstOrDefault();
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
        set
        {
            if (value is < MinClockLimit or >= MaxClockLimit)
            {
                if (MaxGpuClock != value)
                {
                    if (value > 0)
                    {
                        RunPowershellCommand($"nvidia-smi -lgc 0,{value}");
                    }
                    else
                    {
                        RunPowershellCommand("nvidia-smi -rgc");
                    }
                }
            }
        }
    }
    
    private void RunPowershellCommand(string script)
    {
        RunCmd("powershell", script);
    }

    private void RunCmd(string name, string args)
    {
        var cmd = new Process();
        cmd.StartInfo.UseShellExecute = false;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        cmd.StartInfo.FileName = name;
        cmd.StartInfo.Arguments = args;
        cmd.Start();

        cmd.WaitForExit();
    }

    public IReadOnlyCollection<CheckIsGpuOriginalResult> CheckIsGpusOriginal()
    {
        var gpus = PhysicalGPU.GetPhysicalGPUs();
        
        var results = new List<CheckIsGpuOriginalResult>();
        for (var i = 0; i < gpus.Length; i++)
        {
            var gpu = gpus[i];
            var gpuName = gpu.ArchitectInformation.PhysicalGPU.ToString();

            var expectedRopCount = GetRopCount(gpuName);
            var actualRopCount = gpu.ArchitectInformation.NumberOfROPs;

            var result = new CheckIsGpuOriginalResult()
            {
                GpuName = gpuName,
                GpuNumber = i + 1,
                IsGpuOriginal = expectedRopCount == actualRopCount,
                ExpectedRopCount = expectedRopCount,
                ActualRopCount = actualRopCount
            };
            results.Add(result);
        }

        return new ReadOnlyCollection<CheckIsGpuOriginalResult>(results);
    }
    
    private int GetRopCount(string gpuName)
    {
        var index = gpuName.IndexOf("Geforce", StringComparison.InvariantCulture);
        if (index != -1)
        {
            gpuName = gpuName.Remove(index, 8);
        }

        return _ropCountDictionary.Value.GetValueOrDefault(gpuName, -1);
    }
}