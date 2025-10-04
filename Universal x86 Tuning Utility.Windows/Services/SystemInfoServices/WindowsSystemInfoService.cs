using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using ApplicationCore.Enums;
using ApplicationCore.Enums.Laptop;
using ApplicationCore.Extensions;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Models.LaptopInfo;
using ApplicationCore.Utilities;
using DynamicData;
using FluentAvalonia.Core;
using Universal_x86_Tuning_Utility.Helpers;
using Universal_x86_Tuning_Utility.Windows.Interfaces;
using Universal_x86_Tuning_Utility.Windows.Services.Amd.Windows;
using Ols = OpenLibSys_Mem.Ols;

namespace Universal_x86_Tuning_Utility.Windows.Services.SystemInfoServices;

public class WindowsSystemInfoService : ISystemInfoService, IDisposable
{
    private readonly Serilog.ILogger _logger;
    private readonly IIntelManagementService _intelManagementService;

    private readonly ManagementObjectSearcher _baseboardSearcher;
    private readonly ManagementObjectSearcher _motherboardSearcher;
    private readonly ManagementObjectSearcher _systemInfoSearcher;
    private readonly ManagementObjectSearcher _processorInfoSearcher;
    private readonly ManagementObjectSearcher _memoryInfoSearcher;
    private readonly ManagementObjectSearcher _displayInfoSearcher;
    
    private readonly IDisposable _installDeviceSubscription;
    private readonly IDisposable _uninstallDeviceEventWatcher;
    
    public CpuInfo Cpu { get; private set; }
    public RamInfo Ram { get; }
    public LaptopInfoBase? LaptopInfo { get; private set; }
    public IReadOnlyCollection<BasicGpuInfo> Gpus => _gpus.AsReadOnly();
    
    private readonly List<BasicGpuInfo> _gpus = new List<BasicGpuInfo>();
    
    public WindowsSystemInfoService(Serilog.ILogger logger,
                                    IIntelManagementService intelManagementService,
                                    IManagementEventService managementEventService)
    {
        _logger = logger;
        _intelManagementService = intelManagementService;
        _baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
        _motherboardSearcher = 
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_MotherboardDevice");
        _systemInfoSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem");
        _processorInfoSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
        _memoryInfoSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory");
        _displayInfoSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'DISPLAY'");
        
        _installDeviceSubscription =
            managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2")
                .Subscribe(OnNewDeviceInstalled);
        
        _uninstallDeviceEventWatcher =
            managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3")
                .Subscribe(OnDeviceUninstalled);

        Manufacturer = new Lazy<string>(() =>
        {
            foreach (var queryObj in _baseboardSearcher.Get())
            {
                var manufacturerName = queryObj["Manufacturer"].ToString();
                if (manufacturerName != null)
                {
                    return manufacturerName;
                }
            }

            return string.Empty;
        });

        Product = new Lazy<string>(() =>
        {
            foreach (var queryObj in _systemInfoSearcher.Get())
            {
                var manufacturerName = queryObj["Name"].ToString();
                if (manufacturerName != null)
                {
                    return manufacturerName;
                }
            }

            return string.Empty;
        });

        SystemName = new Lazy<string>(() =>
        {
            foreach (var queryObj in _motherboardSearcher.Get())
            {
                var manufacturerName = queryObj["SystemName"].ToString();
                if (manufacturerName != null)
                {
                    return manufacturerName;
                }
            }

            return string.Empty;
        });
        
        Ram = new RamInfo();
        
        ReAnalyzeSystem();
    }

    private void OnDeviceUninstalled(EventArrivedEventArgs e)
    {
        foreach (var device in _displayInfoSearcher.Get())
        {
            if (device["Name"] is string name)
            {
                var gpuToRemove = _gpus.FirstOrDefault(x => x.Name == name);
                if (gpuToRemove != null)
                {
                    _gpus.Remove(gpuToRemove);
                }
            }
        }
    }

    private void OnNewDeviceInstalled(EventArrivedEventArgs e)
    {
        foreach (var device in _displayInfoSearcher.Get())
        {
            if (device["Name"] is string name && _gpus.FirstOrDefault(x => x.Name == name) == null)
            {
                var gpuName = name.Split(' ');
                if (gpuName.Length != 0)
                {
                    if (Enum.TryParse<GpuManufacturer>(gpuName[0], true, out var gpuManufacturer))
                    {
                        _gpus.Add(new BasicGpuInfo(gpuManufacturer, name));
                        continue;
                    }
                
                    _gpus.Add(new BasicGpuInfo(GpuManufacturer.Unknown, name));
                }
            }
        }
    }

    private void InitializeBasicGpuInfo()
    {
        foreach (var device in _displayInfoSearcher.Get())
        {
            if (device["Name"] is string name)
            {
                var gpuName = name.Split(' ');
                if (gpuName.Length != 0)
                {
                    if (Enum.TryParse<GpuManufacturer>(gpuName[0], true, out var gpuManufacturer))
                    {
                        _gpus.Add(new BasicGpuInfo(gpuManufacturer, name));
                        continue;
                    }
                
                    _gpus.Add(new BasicGpuInfo(GpuManufacturer.Unknown, name));
                }
            }
        }
    }

    public void ReAnalyzeSystem()
    {
        try
        {
            InitializeBasicGpuInfo();
            
            var processorIdentifier = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")!;

            var words = processorIdentifier.Split(' ');

            var familyIndex = Array.IndexOf(words, "Family") + 1;
            var modelIndex = Array.IndexOf(words, "Model") + 1;
            var steppingIndex = Array.IndexOf(words, "Stepping") + 1;

            var family = int.Parse(words[familyIndex]);
            var model = int.Parse(words[modelIndex]);
            var stepping = int.Parse(words[steppingIndex].TrimEnd(','));

            var cpuInfo = (ManagementBaseObject) _processorInfoSearcher.Get().ElementAt(0);

            var name = cpuInfo["Name"]?.ToString()?.Trim() ?? string.Empty;
            var description = cpuInfo["Description"]?.ToString()?.Trim() ?? string.Empty;
            var coresCount = Convert.ToInt32(cpuInfo["NumberOfCores"]);
            var logicalCoresCount = Convert.ToInt32(cpuInfo["NumberOfLogicalProcessors"]);
            var baseClock = Convert.ToInt32(cpuInfo["MaxClockSpeed"]);

            var l1Size = GetCacheSize(ApplicationCore.Enums.CacheLevel.L1);
            var l22Size = GetCacheSize(ApplicationCore.Enums.CacheLevel.L2);
            var l3Size = GetCacheSize(ApplicationCore.Enums.CacheLevel.L3);
            
            Cpu = CpuInfoFactory.Create(family: family, 
                model: model, 
                stepping: stepping,
                name: name, 
                description: description, 
                coresCount: coresCount,
                logicalCoresCount: logicalCoresCount,
                maxClockSpeed: baseClock,
                l1Size: l1Size,
                l2Size: l22Size,
                l3Size: l3Size);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when analyzing cpu information");
        }

        if (Cpu is RyzenCpuInfo)
        {
            Addresses.SetAddresses(Cpu);
        }
        else if (Cpu is IntelCpuInfo)
        {
            _intelManagementService.DetermineCpu();
        }

        try
        {
            AnalyzeRam();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when analyzing ram information");
        }

        LaptopInfo = LaptopInfoFactory.Create(Manufacturer.Value.ToLower(), Product.Value.ToLower());
    }

    private void AnalyzeRam()
    {
        int type = 0;
        int width = 0;
        List<RamModule> modules = [];
        
        foreach (var queryObj in _memoryInfoSearcher.Get())
        {
            var module = new RamModule()
            {
                Producer = queryObj["Manufacturer"].ToString()!,
                Model = queryObj["PartNumber"].ToString()?.Trim(),
                Capacity = Convert.ToDouble(queryObj["Capacity"]) / 1073741824, // 1073741824 - gigabyte in bytes
                Speed = Convert.ToInt32(queryObj["ConfiguredClockSpeed"])
            };
            modules.Add(module);
            type = Convert.ToInt32(queryObj["SMBIOSMemoryType"]);
            width += Convert.ToInt32(queryObj["DataWidth"]);
        }

        if (width > 128 && Cpu is RyzenCpuInfo ryzenCpuInfo && ryzenCpuInfo.RyzenFamily != RyzenFamily.Unknown)
        {
            switch (ryzenCpuInfo.RyzenFamily)
            {
                case RyzenFamily.StrixHalo:
                {
                    if (width > 256)
                    {
                        width = 256;
                    }
                    break;
                }
                case RyzenFamily.Mendocino:
                {
                    width = 64;
                    break;
                }
                default:
                {
                    if (ryzenCpuInfo.RyzenFamily < RyzenFamily.StrixPoint2)
                    {
                        width = 128;
                    }
                    break;
                }
            }
        }

        if (modules.Count != 0)
        {
            Ram.Speed = modules[0].Speed;
            Ram.Modules = modules.ToArray();
            Ram.Capacity = modules.Sum(module => module.Capacity);
        }
        
        Ram.Width = width;

        Ram.Timings = Cpu.Manufacturer == ApplicationCore.Enums.Manufacturer.AMD 
            ? GetRyzenTimings()
            : GetIntelTimings();
        
        Ram.Type = type switch
        {
            20 => RamType.DDR,
            21 => RamType.DDR2,
            24 => RamType.DDR3,
            26 => RamType.DDR4,
            30 => RamType.LPDDR4,
            34 => RamType.DDR5,
            35 => RamType.LPDDR5,
            _ => RamType.Unknown
        };
    }

    private IntelMemoryTimings GetIntelTimings()
    {
        var intelTimings = new IntelMemoryTimings();
        int SMUDelay = 10;
    
        var ols = new Ols();
        if (ols.Status != Ols.OlsStatus.NO_ERROR || ols.DllStatus != Ols.OlsDllStatus.OLS_DLL_NO_ERROR) 
            throw new ApplicationException($"Ols initialization error. OlsStatus={ols.Status.ToString()}; DllStatus={ols.DllStatus.ToString()}");
    
        uint someOffset = ReadDword(0x50200, ols, SMUDelay) == 0x300 ? 0x100000u : 0u;
        
        uint DramConfiguration = ReadDword(0x00050200 + someOffset, ols, SMUDelay);
    
        uint DramTiming1 = ReadDword(0x00050204 + someOffset, ols, SMUDelay);
        uint DramTiming2 = ReadDword(0x00050208 + someOffset, ols, SMUDelay);
        uint DramTiming3 = ReadDword(0x0005020C + someOffset, ols, SMUDelay);
        uint DramTiming4 = ReadDword(0x00050210 + someOffset, ols, SMUDelay);
        uint DramTiming5 = ReadDword(0x00050214 + someOffset, ols, SMUDelay);
        uint DramTiming6 = ReadDword(0x00050218 + someOffset, ols, SMUDelay);
        uint DramTiming12 = ReadDword(0x00050230 + someOffset, ols, SMUDelay);
        uint DramTiming13 = ReadDword(0x00050234 + someOffset, ols, SMUDelay);
        uint DramTiming20 = ReadDword(0x00050250 + someOffset, ols, SMUDelay);
        uint DramTiming21 = ReadDword(0x00050254 + someOffset, ols, SMUDelay);
        uint DramTiming22 = ReadDword(0x00050258 + someOffset, ols, SMUDelay);
    
        intelTimings.tRCDWR = (DramTiming1 & 0x3F000000) >> 24;
        intelTimings.tRCDRD = (DramTiming1 & 0x3F0000) >> 16;
        intelTimings.tRAS = (DramTiming1 & 0x7F00) >> 8;
        intelTimings.tCL = DramTiming1 & 0x3F;
    
        intelTimings.tRP = (DramTiming2 & 0x3F0000) >> 16;
        intelTimings.tRC = DramTiming2 & 0xFF;
    
        intelTimings.tRTP = (DramTiming3 & 0x1F000000) >> 24;
        intelTimings.tRRDL = (DramTiming3 & 0x1F00) >> 8;
        intelTimings.tRRDS = DramTiming3 & 0x1F;
    
        intelTimings.tFAW = DramTiming4 & 0x7F;
    
        intelTimings.tWTRL = (DramTiming5 & 0x7F0000) >> 16;
        intelTimings.tWTRS = (DramTiming5 & 0x1F00) >> 8;
        intelTimings.tCWL = DramTiming5 & 0x3F;
    
        intelTimings.tWR = DramTiming6 & 0x7F;
    
        intelTimings.tREF = DramTiming12 & 0xFFFF;
        
        var memClock = DramConfiguration & 0x7F;
        float MEMCLKTRxx = memClock / 3.0f * 100;
        intelTimings.tREFCT = (uint)(1000 / MEMCLKTRxx * intelTimings.tREF);
    
        intelTimings.tMODPDA = (DramTiming13 & 0x3F000000) >> 24;
        intelTimings.tMRDPDA = (DramTiming13 & 0x3F0000) >> 16;
        intelTimings.tMOD = (DramTiming13 & 0x3F00) >> 8;
        intelTimings.tMRD = DramTiming13 & 0x3F;
    
        intelTimings.tSTAG = (DramTiming20 & 0xFF0000) >> 16;
    
        intelTimings.tCKE = (DramTiming21 & 0x1F000000) >> 24;
    
        intelTimings.tRDDATA = DramTiming22 & 0x7F;
    
        uint tRFCTiming0 = ReadDword(0x00050260 + someOffset, ols, SMUDelay);
        uint tRFCTiming1 = ReadDword(0x00050264 + someOffset, ols, SMUDelay);
    
        uint tRFCTiming;
        if (tRFCTiming0 == tRFCTiming1)
        {
            tRFCTiming = tRFCTiming0;
        }
        else if (tRFCTiming0 == 0x21060138)
        {
            tRFCTiming = tRFCTiming1;
        }
        else
        {
            tRFCTiming = tRFCTiming0;
        }
        
        intelTimings.tRFC = tRFCTiming & 0x7FF;

        return intelTimings;
    }
    
    /// <summary>
    /// Retrieves and calculates all memory timings using the OpenLibSys API.
    /// The method does not change any of the PCI/HEX IDs.
    /// </summary>
    private RyzenMemoryTimings GetRyzenTimings()
    {
        var ryzenMemory = new RyzenMemoryTimings();
        bool SMUSlow = false;
        int SMUDelay = SMUSlow ? 60 : 10;
    
        Ols ols = new Ols();
        if (ols.Status != Ols.OlsStatus.NO_ERROR || ols.DllStatus != Ols.OlsDllStatus.OLS_DLL_NO_ERROR)
            throw new ApplicationException($"Ols initialization error. OlsStatus={ols.Status.ToString()}; DllStatus={ols.DllStatus.ToString()}");
    
        uint eax = 0, ebx = 0, ecx = 0, edx = 0;
        ols.CpuidPx(0x80000001, ref eax, ref ebx, ref ecx, ref edx, (UIntPtr)0x01);
        uint CPUFMS = eax & 0xFFFF00;
    
        uint SMUORG = ols.ReadPciConfigDword(0x00, 0xB8);
        Thread.Sleep(SMUDelay);
    
        uint someOffset = ReadDword(0x50200, ols, SMUDelay) == 0x300 ? 0x100000u : 0u;
    
        uint BGS = ReadDword(0x00050058 + someOffset, ols, SMUDelay);
        uint BGSA = ReadDword(0x000500D0 + someOffset, ols, SMUDelay);
        uint DramConfiguration = ReadDword(0x00050200 + someOffset, ols, SMUDelay);
    
        uint DramTiming1 = ReadDword(0x00050204 + someOffset, ols, SMUDelay);
        uint DramTiming2 = ReadDword(0x00050208 + someOffset, ols, SMUDelay);
        uint DramTiming3 = ReadDword(0x0005020C + someOffset, ols, SMUDelay);
        uint DramTiming4 = ReadDword(0x00050210 + someOffset, ols, SMUDelay);
        uint DramTiming5 = ReadDword(0x00050214 + someOffset, ols, SMUDelay);
        uint DramTiming6 = ReadDword(0x00050218 + someOffset, ols, SMUDelay);
        uint DramTiming7 = ReadDword(0x0005021C + someOffset, ols, SMUDelay);
        uint DramTiming8 = ReadDword(0x00050220 + someOffset, ols, SMUDelay);
        uint DramTiming9 = ReadDword(0x00050224 + someOffset, ols, SMUDelay);
        uint DramTiming10 = ReadDword(0x00050228 + someOffset, ols, SMUDelay);
        uint DramTiming12 = ReadDword(0x00050230 + someOffset, ols, SMUDelay);
        uint DramTiming13 = ReadDword(0x00050234 + someOffset, ols, SMUDelay);
        uint DramTiming20 = ReadDword(0x00050250 + someOffset, ols, SMUDelay);
        uint DramTiming21 = ReadDword(0x00050254 + someOffset, ols, SMUDelay);
        uint DramTiming22 = ReadDword(0x00050258 + someOffset, ols, SMUDelay);
    
        uint tRFCTiming0 = ReadDword(0x00050260 + someOffset, ols, SMUDelay);
        uint tRFCTiming1 = ReadDword(0x00050264 + someOffset, ols, SMUDelay);
        uint tSTAGTiming0 = ReadDword(0x00050270 + someOffset, ols, SMUDelay);
        uint tSTAGTiming1 = ReadDword(0x00050274 + someOffset, ols, SMUDelay);
        uint DramTiming35 = ReadDword(0x0005028C + someOffset, ols, SMUDelay);
    
        uint tRFCTiming, tSTAGTiming;
        if (tRFCTiming0 == tRFCTiming1)
        {
            tRFCTiming = tRFCTiming0;
            tSTAGTiming = tSTAGTiming0;
        }
        else if (tRFCTiming0 == 0x21060138)
        {
            tRFCTiming = tRFCTiming1;
            tSTAGTiming = tSTAGTiming1;
        }
        else
        {
            tRFCTiming = tRFCTiming0;
            tSTAGTiming = tSTAGTiming0;
        }
    
        ryzenMemory.BGS = (BGS != 0x87654321);
        ryzenMemory.BGSA = (BGSA == 0x111107F1);
        ryzenMemory.Preamble2T = ((DramConfiguration & 0x1000) >> 12) != 0;
        ryzenMemory.GDM = ((DramConfiguration & 0x800) >> 11) != 0;
        ryzenMemory.CommandRate = ((DramConfiguration & 0x400) >> 10) != 0 ? 2 : 1;
        var memClock = DramConfiguration & 0x7F;
        float MEMCLKTRxx = memClock / 3.0f * 100;
    
        ryzenMemory.tRCDWR = (DramTiming1 & 0x3F000000) >> 24;
        ryzenMemory.tRCDRD = (DramTiming1 & 0x3F0000) >> 16;
        ryzenMemory.tRAS = (DramTiming1 & 0x7F00) >> 8;
        ryzenMemory.tCL = DramTiming1 & 0x3F;
    
        ryzenMemory.tRPPB = (DramTiming2 & 0x3F000000) >> 24;
        ryzenMemory.tRP = (DramTiming2 & 0x3F0000) >> 16;
        ryzenMemory.tRCPB = (DramTiming2 & 0xFF00) >> 8;
        ryzenMemory.tRC = DramTiming2 & 0xFF;
    
        ryzenMemory.tRTP = (DramTiming3 & 0x1F000000) >> 24;
        ryzenMemory.tRRDDLR = (DramTiming3 & 0x1F0000) >> 16;
        ryzenMemory.tRRDL = (DramTiming3 & 0x1F00) >> 8;
        ryzenMemory.tRRDS = DramTiming3 & 0x1F;
    
        ryzenMemory.tFAWDLR = (DramTiming4 & 0x7E000000) >> 25;
        ryzenMemory.tFAWSLR = (DramTiming4 & 0xFC0000) >> 18;
        ryzenMemory.tFAW = DramTiming4 & 0x7F;
    
        ryzenMemory.tWTRL = (DramTiming5 & 0x7F0000) >> 16;
        ryzenMemory.tWTRS = (DramTiming5 & 0x1F00) >> 8;
        ryzenMemory.tCWL = DramTiming5 & 0x3F;
    
        ryzenMemory.tWR = DramTiming6 & 0x7F;
    
        ryzenMemory.tRCPage = (DramTiming7 & 0xFFF00000) >> 20;
    
        ryzenMemory.tRDRDBAN = (DramTiming8 & 0xC0000000) >> 30;
        ryzenMemory.tRDRDSCL = (DramTiming8 & 0x3F000000) >> 24;
        ryzenMemory.tRDRDSCDLR = (DramTiming8 & 0xF00000) >> 20;
        ryzenMemory.tRDRDSC = (DramTiming8 & 0xF0000) >> 16;
        ryzenMemory.tRDRDSD = (DramTiming8 & 0xF00) >> 8;
        ryzenMemory.tRDRDDD = DramTiming8 & 0xF;
    
        ryzenMemory.tWRWRBAN = (DramTiming9 & 0xC0000000) >> 30;
        ryzenMemory.tWRWRSCL = (DramTiming9 & 0x3F000000) >> 24;
        ryzenMemory.tWRWRSCDLR = (DramTiming9 & 0xF00000) >> 20;
        ryzenMemory.tWRWRSC = (DramTiming9 & 0xF0000) >> 16;
        ryzenMemory.tWRWRSD = (DramTiming9 & 0xF00) >> 8;
        ryzenMemory.tWRWRDD = DramTiming9 & 0xF;
    
        ryzenMemory.tWRRDSCDLR = (DramTiming10 & 0x1F0000) >> 16;
        ryzenMemory.tRDWR = (DramTiming10 & 0x1F00) >> 8;
        ryzenMemory.tWRRD = DramTiming10 & 0xF;
    
        ryzenMemory.tREF = DramTiming12 & 0xFFFF;
        ryzenMemory.tREFCT = (uint)(1000 / MEMCLKTRxx * ryzenMemory.tREF);
    
        ryzenMemory.tMODPDA = (DramTiming13 & 0x3F000000) >> 24;
        ryzenMemory.tMRDPDA = (DramTiming13 & 0x3F0000) >> 16;
        ryzenMemory.tMOD = (DramTiming13 & 0x3F00) >> 8;
        ryzenMemory.tMRD = DramTiming13 & 0x3F;
    
        ryzenMemory.tSTAG = (DramTiming20 & 0xFF0000) >> 16;
    
        ryzenMemory.tCKE = (DramTiming21 & 0x1F000000) >> 24;
    
        ryzenMemory.tPHYWRD = (DramTiming22 & 0x7000000) >> 24;
        ryzenMemory.tPHYRDLAT = (DramTiming22 & 0x3F0000) >> 16;
        ryzenMemory.tPHYWRLAT = (DramTiming22 & 0x1F00) >> 8;
        ryzenMemory.tRDDATA = DramTiming22 & 0x7F;
    
        ryzenMemory.tRFC4 = (tRFCTiming & 0xFFC00000) >> 22;
        ryzenMemory.tRFC4CT = (uint)(1000 / MEMCLKTRxx * ryzenMemory.tRFC4);
    
        ryzenMemory.tRFC2 = (tRFCTiming & 0x3FF800) >> 11;
        ryzenMemory.tRFC2CT = (uint)(1000 / MEMCLKTRxx * ryzenMemory.tRFC2);
    
        ryzenMemory.tRFC = tRFCTiming & 0x7FF;
        ryzenMemory.tRFCCT = (uint)(1000 / MEMCLKTRxx * ryzenMemory.tRFC);
    
        ryzenMemory.tSTAG4LR = (tSTAGTiming & 0x1FF00000) >> 20;
        ryzenMemory.tSTAG2LR = (tSTAGTiming & 0x7FC00) >> 10;
        ryzenMemory.tSTAGLR = tSTAGTiming & 0x1FF;
    
        ryzenMemory.tWRMPR = (DramTiming35 & 0x3F000000) >> 24;
    
        uint eax2 = 0, ebx2 = 0, ecx2 = 0, edx2 = 0;
        ols.CpuidPx(0x80000001, ref eax2, ref ebx2, ref ecx2, ref edx2, (UIntPtr)0x01);
        eax2 &= 0xFFFF00;
        ebx2 = (ebx2 & 0xF0000000) >> 28;
        uint someOffset2 = 0;
        if (ebx2 == 7)
            someOffset2 = 0x2180;
        else if (ebx2 == 2)
            someOffset2 = 0x100;
        else
            someOffset2 = 0x00;
    
        // The below code causes system hangs on PHX/HWK systems
        //if (eax2 == 0x810F00 || (ebx2 == 1 || ebx2 == 3 || ebx2 == 4))
        //{
        //    mem.ProcODT = "N/A";
        //    mem.RttNom = "N/A";
        //    mem.RttWr = "N/A";
        //    mem.RttPark = "N/A";
        //    mem.AddrCmdSetup = "N/A";
        //    mem.CsOdtSetup = "N/A";
        //    mem.CkeSetup = "N/A";
        //    mem.ClkDrvStrength = "N/A";
        //    mem.AddrCmdDrvStrength = "N/A";
        //    mem.CsOdtDrvStrength = "N/A";
        //    mem.CkeDrvStrength = "N/A";
        //}
        //else
        //{
        //    ols.WritePciConfigDword(0x00, 0xB8, 0x3B10528);
        //    ols.WritePciConfigDword(0x00, 0xBC, 0x2C);
        //    ols.WritePciConfigDword(0x00, 0xB8, 0x3B1059C);
        //    uint x = ols.ReadPciConfigDword(0, 0xBC);
        //    ulong num26 = x - someOffset2;
        //    Ols.IsInpOutDriverOpen2();
        //    uint num27 = 0xB1;
        //    uint physLong1 = ols.GetPhysLong(new UIntPtr(num26 + num27));
        //    uint num28 = 0xB5;
        //    uint physLong2 = ols.GetPhysLong(new UIntPtr(num26 + num28));
        //    uint num29 = 0xBA;
        //    uint physLong3 = ols.GetPhysLong(new UIntPtr(num26 + num29));
    
        //    uint addrCmdSetup = physLong1 & 0xFF;
        //    switch (addrCmdSetup)
        //    {
        //        case 0: mem.AddrCmdSetup = "0/0"; break;
        //        case 1: mem.AddrCmdSetup = "0/1"; break;
        //        case 2: mem.AddrCmdSetup = "0/2"; break;
        //        case 3: mem.AddrCmdSetup = "0/3"; break;
        //        case 4: mem.AddrCmdSetup = "0/4"; break;
        //        case 5: mem.AddrCmdSetup = "0/5"; break;
        //        case 6: mem.AddrCmdSetup = "0/6"; break;
        //        case 7: mem.AddrCmdSetup = "0/7"; break;
        //        case 8: mem.AddrCmdSetup = "0/8"; break;
        //        case 9: mem.AddrCmdSetup = "0/9"; break;
        //        case 10: mem.AddrCmdSetup = "0/10"; break;
        //        case 11: mem.AddrCmdSetup = "0/11"; break;
        //        case 12: mem.AddrCmdSetup = "0/12"; break;
        //        case 13: mem.AddrCmdSetup = "0/13"; break;
        //        case 14: mem.AddrCmdSetup = "0/14"; break;
        //        case 15: mem.AddrCmdSetup = "0/15"; break;
        //        case 16: mem.AddrCmdSetup = "0/16"; break;
        //        case 17: mem.AddrCmdSetup = "0/17"; break;
        //        case 18: mem.AddrCmdSetup = "0/18"; break;
        //        case 19: mem.AddrCmdSetup = "0/19"; break;
        //        case 20: mem.AddrCmdSetup = "0/20"; break;
        //        case 21: mem.AddrCmdSetup = "0/21"; break;
        //        case 22: mem.AddrCmdSetup = "0/22"; break;
        //        case 23: mem.AddrCmdSetup = "0/23"; break;
        //        case 24: mem.AddrCmdSetup = "0/24"; break;
        //        case 25: mem.AddrCmdSetup = "0/25"; break;
        //        case 26: mem.AddrCmdSetup = "0/26"; break;
        //        case 27: mem.AddrCmdSetup = "0/27"; break;
        //        case 28: mem.AddrCmdSetup = "0/28"; break;
        //        case 29: mem.AddrCmdSetup = "0/29"; break;
        //        case 30: mem.AddrCmdSetup = "0/30"; break;
        //        case 31: mem.AddrCmdSetup = "0/31"; break;
        //        case 32: mem.AddrCmdSetup = "1/0"; break;
        //        case 33: mem.AddrCmdSetup = "1/1"; break;
        //        case 34: mem.AddrCmdSetup = "1/2"; break;
        //        case 35: mem.AddrCmdSetup = "1/3"; break;
        //        case 36: mem.AddrCmdSetup = "1/4"; break;
        //        case 37: mem.AddrCmdSetup = "1/5"; break;
        //        case 38: mem.AddrCmdSetup = "1/6"; break;
        //        case 39: mem.AddrCmdSetup = "1/7"; break;
        //        case 40: mem.AddrCmdSetup = "1/8"; break;
        //        case 41: mem.AddrCmdSetup = "1/9"; break;
        //        case 42: mem.AddrCmdSetup = "1/10"; break;
        //        case 43: mem.AddrCmdSetup = "1/11"; break;
        //        case 44: mem.AddrCmdSetup = "1/12"; break;
        //        case 45: mem.AddrCmdSetup = "1/13"; break;
        //        case 46: mem.AddrCmdSetup = "1/14"; break;
        //        case 47: mem.AddrCmdSetup = "1/15"; break;
        //        case 48: mem.AddrCmdSetup = "1/16"; break;
        //        case 49: mem.AddrCmdSetup = "1/17"; break;
        //        case 50: mem.AddrCmdSetup = "1/18"; break;
        //        case 51: mem.AddrCmdSetup = "1/19"; break;
        //        case 52: mem.AddrCmdSetup = "1/20"; break;
        //        case 53: mem.AddrCmdSetup = "1/21"; break;
        //        case 54: mem.AddrCmdSetup = "1/22"; break;
        //        case 55: mem.AddrCmdSetup = "1/23"; break;
        //        case 56: mem.AddrCmdSetup = "1/24"; break;
        //        case 57: mem.AddrCmdSetup = "1/25"; break;
        //        case 58: mem.AddrCmdSetup = "1/26"; break;
        //        case 59: mem.AddrCmdSetup = "1/27"; break;
        //        case 60: mem.AddrCmdSetup = "1/28"; break;
        //        case 61: mem.AddrCmdSetup = "1/29"; break;
        //        case 62: mem.AddrCmdSetup = "1/30"; break;
        //        case 63: mem.AddrCmdSetup = "1/31"; break;
        //    }
    
        //    uint csOdtSetup = (physLong1 & 0xFF00) >> 8;
        //    switch (csOdtSetup)
        //    {
        //        case 0: mem.CsOdtSetup = "0/0"; break;
        //        case 1: mem.CsOdtSetup = "0/1"; break;
        //        case 2: mem.CsOdtSetup = "0/2"; break;
        //        case 3: mem.CsOdtSetup = "0/3"; break;
        //        case 4: mem.CsOdtSetup = "0/4"; break;
        //        case 5: mem.CsOdtSetup = "0/5"; break;
        //        case 6: mem.CsOdtSetup = "0/6"; break;
        //        case 7: mem.CsOdtSetup = "0/7"; break;
        //        case 8: mem.CsOdtSetup = "0/8"; break;
        //        case 9: mem.CsOdtSetup = "0/9"; break;
        //        case 10: mem.CsOdtSetup = "0/10"; break;
        //        case 11: mem.CsOdtSetup = "0/11"; break;
        //        case 12: mem.CsOdtSetup = "0/12"; break;
        //        case 13: mem.CsOdtSetup = "0/13"; break;
        //        case 14: mem.CsOdtSetup = "0/14"; break;
        //        case 15: mem.CsOdtSetup = "0/15"; break;
        //        case 16: mem.CsOdtSetup = "0/16"; break;
        //        case 17: mem.CsOdtSetup = "0/17"; break;
        //        case 18: mem.CsOdtSetup = "0/18"; break;
        //        case 19: mem.CsOdtSetup = "0/19"; break;
        //        case 20: mem.CsOdtSetup = "0/20"; break;
        //        case 21: mem.CsOdtSetup = "0/21"; break;
        //        case 22: mem.CsOdtSetup = "0/22"; break;
        //        case 23: mem.CsOdtSetup = "0/23"; break;
        //        case 24: mem.CsOdtSetup = "0/24"; break;
        //        case 25: mem.CsOdtSetup = "0/25"; break;
        //        case 26: mem.CsOdtSetup = "0/26"; break;
        //        case 27: mem.CsOdtSetup = "0/27"; break;
        //        case 28: mem.CsOdtSetup = "0/28"; break;
        //        case 29: mem.CsOdtSetup = "0/29"; break;
        //        case 30: mem.CsOdtSetup = "0/30"; break;
        //        case 31: mem.CsOdtSetup = "0/31"; break;
        //        case 32: mem.CsOdtSetup = "1/0"; break;
        //        case 33: mem.CsOdtSetup = "1/1"; break;
        //        case 34: mem.CsOdtSetup = "1/2"; break;
        //        case 35: mem.CsOdtSetup = "1/3"; break;
        //        case 36: mem.CsOdtSetup = "1/4"; break;
        //        case 37: mem.CsOdtSetup = "1/5"; break;
        //        case 38: mem.CsOdtSetup = "1/6"; break;
        //        case 39: mem.CsOdtSetup = "1/7"; break;
        //        case 40: mem.CsOdtSetup = "1/8"; break;
        //        case 41: mem.CsOdtSetup = "1/9"; break;
        //        case 42: mem.CsOdtSetup = "1/10"; break;
        //        case 43: mem.CsOdtSetup = "1/11"; break;
        //        case 44: mem.CsOdtSetup = "1/12"; break;
        //        case 45: mem.CsOdtSetup = "1/13"; break;
        //        case 46: mem.CsOdtSetup = "1/14"; break;
        //        case 47: mem.CsOdtSetup = "1/15"; break;
        //        case 48: mem.CsOdtSetup = "1/16"; break;
        //        case 49: mem.CsOdtSetup = "1/17"; break;
        //        case 50: mem.CsOdtSetup = "1/18"; break;
        //        case 51: mem.CsOdtSetup = "1/19"; break;
        //        case 52: mem.CsOdtSetup = "1/20"; break;
        //        case 53: mem.CsOdtSetup = "1/21"; break;
        //        case 54: mem.CsOdtSetup = "1/22"; break;
        //        case 55: mem.CsOdtSetup = "1/23"; break;
        //        case 56: mem.CsOdtSetup = "1/24"; break;
        //        case 57: mem.CsOdtSetup = "1/25"; break;
        //        case 58: mem.CsOdtSetup = "1/26"; break;
        //        case 59: mem.CsOdtSetup = "1/27"; break;
        //        case 60: mem.CsOdtSetup = "1/28"; break;
        //        case 61: mem.CsOdtSetup = "1/29"; break;
        //        case 62: mem.CsOdtSetup = "1/30"; break;
        //        case 63: mem.CsOdtSetup = "1/31"; break;
        //    }
    
        //    uint ckeSetup = (physLong1 & 0xFF0000) >> 16;
        //    switch (ckeSetup)
        //    {
        //        case 0: mem.CkeSetup = "0/0"; break;
        //        case 1: mem.CkeSetup = "0/1"; break;
        //        case 2: mem.CkeSetup = "0/2"; break;
        //        case 3: mem.CkeSetup = "0/3"; break;
        //        case 4: mem.CkeSetup = "0/4"; break;
        //        case 5: mem.CkeSetup = "0/5"; break;
        //        case 6: mem.CkeSetup = "0/6"; break;
        //        case 7: mem.CkeSetup = "0/7"; break;
        //        case 8: mem.CkeSetup = "0/8"; break;
        //        case 9: mem.CkeSetup = "0/9"; break;
        //        case 10: mem.CkeSetup = "0/10"; break;
        //        case 11: mem.CkeSetup = "0/11"; break;
        //        case 12: mem.CkeSetup = "0/12"; break;
        //        case 13: mem.CkeSetup = "0/13"; break;
        //        case 14: mem.CkeSetup = "0/14"; break;
        //        case 15: mem.CkeSetup = "0/15"; break;
        //        case 16: mem.CkeSetup = "0/16"; break;
        //        case 17: mem.CkeSetup = "0/17"; break;
        //        case 18: mem.CkeSetup = "0/18"; break;
        //        case 19: mem.CkeSetup = "0/19"; break;
        //        case 20: mem.CkeSetup = "0/20"; break;
        //        case 21: mem.CkeSetup = "0/21"; break;
        //        case 22: mem.CkeSetup = "0/22"; break;
        //        case 23: mem.CkeSetup = "0/23"; break;
        //        case 24: mem.CkeSetup = "0/24"; break;
        //        case 25: mem.CkeSetup = "0/25"; break;
        //        case 26: mem.CkeSetup = "0/26"; break;
        //        case 27: mem.CkeSetup = "0/27"; break;
        //        case 28: mem.CkeSetup = "0/28"; break;
        //        case 29: mem.CkeSetup = "0/29"; break;
        //        case 30: mem.CkeSetup = "0/30"; break;
        //        case 31: mem.CkeSetup = "0/31"; break;
        //        case 32: mem.CkeSetup = "1/0"; break;
        //        case 33: mem.CkeSetup = "1/1"; break;
        //        case 34: mem.CkeSetup = "1/2"; break;
        //        case 35: mem.CkeSetup = "1/3"; break;
        //        case 36: mem.CkeSetup = "1/4"; break;
        //        case 37: mem.CkeSetup = "1/5"; break;
        //        case 38: mem.CkeSetup = "1/6"; break;
        //        case 39: mem.CkeSetup = "1/7"; break;
        //        case 40: mem.CkeSetup = "1/8"; break;
        //        case 41: mem.CkeSetup = "1/9"; break;
        //        case 42: mem.CkeSetup = "1/10"; break;
        //        case 43: mem.CkeSetup = "1/11"; break;
        //        case 44: mem.CkeSetup = "1/12"; break;
        //        case 45: mem.CkeSetup = "1/13"; break;
        //        case 46: mem.CkeSetup = "1/14"; break;
        //        case 47: mem.CkeSetup = "1/15"; break;
        //        case 48: mem.CkeSetup = "1/16"; break;
        //        case 49: mem.CkeSetup = "1/17"; break;
        //        case 50: mem.CkeSetup = "1/18"; break;
        //        case 51: mem.CkeSetup = "1/19"; break;
        //        case 52: mem.CkeSetup = "1/20"; break;
        //        case 53: mem.CkeSetup = "1/21"; break;
        //        case 54: mem.CkeSetup = "1/22"; break;
        //        case 55: mem.CkeSetup = "1/23"; break;
        //        case 56: mem.CkeSetup = "1/24"; break;
        //        case 57: mem.CkeSetup = "1/25"; break;
        //        case 58: mem.CkeSetup = "1/26"; break;
        //        case 59: mem.CkeSetup = "1/27"; break;
        //        case 60: mem.CkeSetup = "1/28"; break;
        //        case 61: mem.CkeSetup = "1/29"; break;
        //        case 62: mem.CkeSetup = "1/30"; break;
        //        case 63: mem.CkeSetup = "1/31"; break;
        //    }
    
        //    uint clkDrvStrength = (physLong1 & 0xFF000000) >> 24;
        //    if (clkDrvStrength <= 7)
        //    {
        //        switch (clkDrvStrength)
        //        {
        //            case 0: mem.ClkDrvStrength = "120.0Ω"; break;
        //            case 1: mem.ClkDrvStrength = "60.0Ω"; break;
        //            case 3: mem.ClkDrvStrength = "40.0Ω"; break;
        //            case 7: mem.ClkDrvStrength = "30.0Ω"; break;
        //        }
        //    }
        //    else if (clkDrvStrength != 15)
        //    {
        //        if (clkDrvStrength == 31)
        //            mem.ClkDrvStrength = "20.0Ω";
        //    }
        //    else
        //        mem.ClkDrvStrength = "24.0Ω";
    
        //    uint addrCmdDrvStrength = physLong2 & 0xFF;
        //    if (addrCmdDrvStrength <= 7)
        //    {
        //        switch (addrCmdDrvStrength)
        //        {
        //            case 0: mem.AddrCmdDrvStrength = "120.0Ω"; break;
        //            case 1: mem.AddrCmdDrvStrength = "60.0Ω"; break;
        //            case 3: mem.AddrCmdDrvStrength = "40.0Ω"; break;
        //            case 7: mem.AddrCmdDrvStrength = "30.0Ω"; break;
        //        }
        //    }
        //    else if (addrCmdDrvStrength != 15)
        //    {
        //        if (addrCmdDrvStrength == 31)
        //            mem.AddrCmdDrvStrength = "20.0Ω";
        //    }
        //    else
        //        mem.AddrCmdDrvStrength = "24.0Ω";
    
        //    uint csOdtDrvStrength = (physLong2 & 0xFF00) >> 8;
        //    if (csOdtDrvStrength <= 7)
        //    {
        //        switch (csOdtDrvStrength)
        //        {
        //            case 0: mem.CsOdtDrvStrength = "120.0Ω"; break;
        //            case 1: mem.CsOdtDrvStrength = "60.0Ω"; break;
        //            case 3: mem.CsOdtDrvStrength = "40.0Ω"; break;
        //            case 7: mem.CsOdtDrvStrength = "30.0Ω"; break;
        //        }
        //    }
        //    else if (csOdtDrvStrength != 15)
        //    {
        //        if (csOdtDrvStrength == 31)
        //            mem.CsOdtDrvStrength = "20.0Ω";
        //    }
        //    else
        //        mem.CsOdtDrvStrength = "24.0Ω";
    
        //    uint ckeDrvStrength = (physLong2 & 0xFF0000) >> 16;
        //    if (ckeDrvStrength <= 7)
        //    {
        //        switch (ckeDrvStrength)
        //        {
        //            case 0: mem.CkeDrvStrength = "120.0Ω"; break;
        //            case 1: mem.CkeDrvStrength = "60.0Ω"; break;
        //            case 3: mem.CkeDrvStrength = "40.0Ω"; break;
        //            case 7: mem.CkeDrvStrength = "30.0Ω"; break;
        //        }
        //    }
        //    else if (ckeDrvStrength != 15)
        //    {
        //        if (ckeDrvStrength == 31)
        //            mem.CkeDrvStrength = "20.0Ω";
        //    }
        //    else
        //        mem.CkeDrvStrength = "24.0Ω";
    
        //    uint rttNom = physLong3 & 0xFF;
        //    switch (rttNom)
        //    {
        //        case 0: mem.RttNom = "Disabled"; break;
        //        case 1: mem.RttNom = "60.0Ω"; break;
        //        case 2: mem.RttNom = "120.0Ω"; break;
        //        case 3: mem.RttNom = "40.0Ω"; break;
        //        case 4: mem.RttNom = "240.0Ω"; break;
        //        case 5: mem.RttNom = "48.0Ω"; break;
        //        case 6: mem.RttNom = "80.0Ω"; break;
        //        case 7: mem.RttNom = "34.3Ω"; break;
        //    }
        //    uint rttWr = (physLong3 & 0xFF00) >> 8;
        //    switch (rttWr)
        //    {
        //        case 0: mem.RttWr = "Disabled"; break;
        //        case 1: mem.RttWr = "120.0Ω"; break;
        //        case 2: mem.RttWr = "240.0Ω"; break;
        //        case 3: mem.RttWr = "Hi-Z"; break;
        //        case 4: mem.RttWr = "80.0Ω"; break;
        //    }
        //    uint rttPark = (physLong3 & 0xFF0000) >> 16;
        //    switch (rttPark)
        //    {
        //        case 0: mem.RttPark = "Disabled"; break;
        //        case 1: mem.RttPark = "60.0Ω"; break;
        //        case 2: mem.RttPark = "120.0Ω"; break;
        //        case 3: mem.RttPark = "40.0Ω"; break;
        //        case 4: mem.RttPark = "240.0Ω"; break;
        //        case 5: mem.RttPark = "48.0Ω"; break;
        //        case 6: mem.RttPark = "80.0Ω"; break;
        //        case 7: mem.RttPark = "34.3Ω"; break;
        //    }
        //    uint procODT = (physLong3 & 0xFF000000) >> 24;
        //    if (procODT == 8) mem.ProcODT = "120.0Ω";
        //    else if (procODT == 9) mem.ProcODT = "96.0Ω";
        //    else if (procODT == 10) mem.ProcODT = "80.0Ω";
        //    else if (procODT == 11) mem.ProcODT = "68.6Ω";
        //    else if (procODT == 24) mem.ProcODT = "60.0Ω";
        //    else if (procODT == 25) mem.ProcODT = "53.3Ω";
        //    else if (procODT == 26) mem.ProcODT = "48.0Ω";
        //    else if (procODT == 27) mem.ProcODT = "43.6Ω";
        //    else if (procODT == 56) mem.ProcODT = "40.0Ω";
        //    else if (procODT == 57) mem.ProcODT = "36.9Ω";
        //    else if (procODT == 58) mem.ProcODT = "34.3Ω";
        //    else if (procODT == 59) mem.ProcODT = "32.0Ω";
        //    else if (procODT == 62) mem.ProcODT = "30.0Ω";
        //    else if (procODT == 63) mem.ProcODT = "28.2Ω";
        //    else mem.ProcODT = "N/A";
        //}
    
        ols.WritePciConfigDword(0x0, 0xB8, 0x3B10528);
        ols.WritePciConfigDword(0x0, 0xBC, 0x02);
        ols.WritePciConfigDword(0x00, 0xB8, SMUORG);
        Thread.Sleep(SMUDelay);
    
        ols.Dispose();
        return ryzenMemory;
    }
    
    /// <summary>
    /// Helper method to write a PCI config dword and then read it back.
    /// </summary>
    private uint ReadDword(uint value, Ols ols, int delay)
    {
        ols.WritePciConfigDword(0x00, 0xB8, value);
        Thread.Sleep(delay);
        return ols.ReadPciConfigDword(0x00, 0xBC);
    }

    public Lazy<string> Manufacturer { get; }

    public Lazy<string> Product { get; }

    public Lazy<string> SystemName { get; }

    private enum CacheLevel
    {
        Level1 = 3,
        Level2 = 4,
        Level3 = 5  
    }

    private uint GetCacheSize(ApplicationCore.Enums.CacheLevel level)
    {
        var searchLevel = level switch
        {
            ApplicationCore.Enums.CacheLevel.L1 => (ushort) CacheLevel.Level1,
            ApplicationCore.Enums.CacheLevel.L2 => (ushort) CacheLevel.Level2,
            ApplicationCore.Enums.CacheLevel.L3 => (ushort) CacheLevel.Level3,
        };
        
        using (var mc = new ManagementClass("Win32_CacheMemory"))
        {
            using (var moc = mc.GetInstances())
            {
                return moc
                    .Cast<ManagementObject>()
                    .Where(p => (ushort)p.Properties["Level"].Value == searchLevel)
                    .Select(p => (uint)p.Properties["MaxCacheSize"].Value)
                    .FirstOrDefault();
            }
        }
    }
    
    public void Dispose()
    {
        _installDeviceSubscription.Dispose();
        _uninstallDeviceEventWatcher.Dispose();
        
        _displayInfoSearcher.Dispose();
        _baseboardSearcher.Dispose();
        _motherboardSearcher.Dispose();
        _systemInfoSearcher.Dispose();
        _processorInfoSearcher.Dispose();
        _memoryInfoSearcher.Dispose();
    }
}