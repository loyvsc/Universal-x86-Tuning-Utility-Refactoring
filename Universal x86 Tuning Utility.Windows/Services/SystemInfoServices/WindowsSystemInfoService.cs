using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Models.LaptopInfo;
using ApplicationCore.Utilities;
using FluentAvalonia.Core;
using Splat;
using Universal_x86_Tuning_Utility.Windows.Extensions;
using Universal_x86_Tuning_Utility.Windows.Interfaces;
using Universal_x86_Tuning_Utility.Windows.Services.Amd.Windows;

namespace Universal_x86_Tuning_Utility.Windows.Services.SystemInfoServices;

public class WindowsSystemInfoService : ISystemInfoService, IDisposable
{
    private readonly Serilog.ILogger _logger;
    private readonly IIntelManagementService _intelManagementService;
    private readonly INvidiaGpuService _nvidiaGpuService;

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
    public LaptopInfoBase? LaptopInfo => _laptopInfo.Value;
    public IReadOnlyCollection<BasicGpuInfo> Gpus => _gpus.AsReadOnly();

    public ChassisType ChassisType => _chassisType.Value;

    private readonly List<BasicGpuInfo> _gpus = new List<BasicGpuInfo>();
    private readonly Lazy<ChassisType> _chassisType;
    private readonly Lazy<string> _manufacturer;
    private readonly Lazy<string> _product;
    private readonly Lazy<string> _systemName;
    private readonly Lazy<LaptopInfoBase?> _laptopInfo;

    public WindowsSystemInfoService(Serilog.ILogger logger,
                                    IIntelManagementService intelManagementService,
                                    INvidiaGpuService nvidiaGpuService,
                                    IManagementEventService managementEventService)
    {
        _logger = logger;
        _intelManagementService = intelManagementService;
        _nvidiaGpuService = nvidiaGpuService;
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

        _chassisType = new Lazy<ChassisType>(() =>
        {
            using (var systemEnclosureSearcher =
                   new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_SystemEnclosure"))
            {
                foreach (var obj in systemEnclosureSearcher.Get())
                {
                    var data = obj.Get<ChassisType[]>("ChassisTypes");

                    if (data?.Length > 0)
                        return data[0];
                }

                return ChassisType.Unknown;
            }
        });

        _laptopInfo = new Lazy<LaptopInfoBase?>(() => Locator.Current.GetService<ILaptopInfoFactory>()?.Create());

        _manufacturer = new Lazy<string>(() =>
        {
            foreach (var queryObj in _baseboardSearcher.Get())
            {
                var manufacturer = queryObj.Get<string>("Manufacturer");
                if (manufacturer != null)
                {
                    return manufacturer;
                }
            }

            return string.Empty;
        });

        _product = new Lazy<string>(() =>
        {
            foreach (var queryObj in _systemInfoSearcher.Get())
            {
                var sb = StringBuilderPool.Rent();
                
                sb.Append(queryObj.Get<string>("SystemFamily"));
                sb.Append(' ');
                sb.Append(queryObj.Get<string>("Model"));
                
                var product = sb.ToString();
                StringBuilderPool.Return(sb);
                return product;
            }

            return string.Empty;
        });

        _systemName = new Lazy<string>(() =>
        {
            foreach (var queryObj in _motherboardSearcher.Get())
            {
                var systemName = queryObj.Get<string>("SystemName");
                if (systemName != null)
                {
                    return systemName;
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
                        switch (gpuManufacturer)
                        {
                            case GpuManufacturer.Nvidia:
                            {
                                _nvidiaGpuService.RefreshGpusList();
                                var newGpus = _nvidiaGpuService.Gpus.Where(x => Gpus.All(y => y.Id != x.Id));
                                _gpus.AddRange(newGpus);
                                
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void InitializeBasicGpuInfo()
    {
        _gpus.AddRange(_nvidiaGpuService.Gpus);
        foreach (var device in _displayInfoSearcher.Get())
        {
            if (device["Name"] is string name)
            {
                var gpuName = name.Split(' ');
                if (gpuName.Length != 0)
                {
                    if (Enum.TryParse<GpuManufacturer>(gpuName[0], true, out var gpuManufacturer))
                    {
                        if (gpuManufacturer == GpuManufacturer.Nvidia)
                            continue;
                        
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

            var cpuInfo = (ManagementBaseObject)_processorInfoSearcher.Get().ElementAt(0);

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
                Producer = queryObj.Get<string>("Manufacturer")?.Trim() ?? string.Empty,
                Model = queryObj.Get<string>("PartNumber")?.Trim() ?? string.Empty,
                Capacity = queryObj.Get<double>("Capacity") / 1073741824, // 1073741824 - gigabyte in bytes
                Speed = queryObj.Get<int>("ConfiguredClockSpeed")
            };
            modules.Add(module);
            type = queryObj.Get<int>("SMBIOSMemoryType");
            width += queryObj.Get<int>("DataWidth");
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
        int smuDelay = 10;

        var ols = new Ols();
        var status = ols.GetStatus();
        var dllStatus = Ols.OlsDllStatus.OLS_DLL_NO_ERROR;
        if (status != Ols.Status.NO_ERROR || ols.GetDllStatus() != (uint) dllStatus)
            throw new ApplicationException(
                $"Ols initialization error. OlsStatus={status.ToString()}; DllStatus={dllStatus.ToString()}");

        uint someOffset = ReadDword(0x50200, ols, smuDelay) == 0x300 ? 0x100000u : 0u;

        uint dramConfiguration = ReadDword(0x00050200 + someOffset, ols, smuDelay);

        uint dramTiming1 = ReadDword(0x00050204 + someOffset, ols, smuDelay);
        uint dramTiming2 = ReadDword(0x00050208 + someOffset, ols, smuDelay);
        uint dramTiming3 = ReadDword(0x0005020C + someOffset, ols, smuDelay);
        uint dramTiming4 = ReadDword(0x00050210 + someOffset, ols, smuDelay);
        uint dramTiming5 = ReadDword(0x00050214 + someOffset, ols, smuDelay);
        uint dramTiming6 = ReadDword(0x00050218 + someOffset, ols, smuDelay);
        uint dramTiming12 = ReadDword(0x00050230 + someOffset, ols, smuDelay);
        uint dramTiming13 = ReadDword(0x00050234 + someOffset, ols, smuDelay);
        uint dramTiming20 = ReadDword(0x00050250 + someOffset, ols, smuDelay);
        uint dramTiming21 = ReadDword(0x00050254 + someOffset, ols, smuDelay);
        uint dramTiming22 = ReadDword(0x00050258 + someOffset, ols, smuDelay);

        intelTimings.tRCDWR = (dramTiming1 & 0x3F000000) >> 24;
        intelTimings.tRCDRD = (dramTiming1 & 0x3F0000) >> 16;
        intelTimings.tRAS = (dramTiming1 & 0x7F00) >> 8;
        intelTimings.tCL = dramTiming1 & 0x3F;

        intelTimings.tRP = (dramTiming2 & 0x3F0000) >> 16;
        intelTimings.tRC = dramTiming2 & 0xFF;

        intelTimings.tRTP = (dramTiming3 & 0x1F000000) >> 24;
        intelTimings.tRRDL = (dramTiming3 & 0x1F00) >> 8;
        intelTimings.tRRDS = dramTiming3 & 0x1F;

        intelTimings.tFAW = dramTiming4 & 0x7F;

        intelTimings.tWTRL = (dramTiming5 & 0x7F0000) >> 16;
        intelTimings.tWTRS = (dramTiming5 & 0x1F00) >> 8;
        intelTimings.tCWL = dramTiming5 & 0x3F;

        intelTimings.tWR = dramTiming6 & 0x7F;

        intelTimings.tREF = dramTiming12 & 0xFFFF;

        var memClock = dramConfiguration & 0x7F;
        float memclktRxx = memClock / 3.0f * 100;
        intelTimings.tREFCT = (uint)(1000 / memclktRxx * intelTimings.tREF);

        intelTimings.tMODPDA = (dramTiming13 & 0x3F000000) >> 24;
        intelTimings.tMRDPDA = (dramTiming13 & 0x3F0000) >> 16;
        intelTimings.tMOD = (dramTiming13 & 0x3F00) >> 8;
        intelTimings.tMRD = dramTiming13 & 0x3F;

        intelTimings.tSTAG = (dramTiming20 & 0xFF0000) >> 16;

        intelTimings.tCKE = (dramTiming21 & 0x1F000000) >> 24;

        intelTimings.tRDDATA = dramTiming22 & 0x7F;

        uint tRfcTiming0 = ReadDword(0x00050260 + someOffset, ols, smuDelay);
        uint tRfcTiming1 = ReadDword(0x00050264 + someOffset, ols, smuDelay);

        uint tRfcTiming;
        if (tRfcTiming0 == tRfcTiming1)
        {
            tRfcTiming = tRfcTiming0;
        }
        else if (tRfcTiming0 == 0x21060138)
        {
            tRfcTiming = tRfcTiming1;
        }
        else
        {
            tRfcTiming = tRfcTiming0;
        }

        intelTimings.tRFC = tRfcTiming & 0x7FF;

        return intelTimings;
    }

    /// <summary>
    /// Retrieves and calculates all memory timings using the OpenLibSys API.
    /// The method does not change any of the PCI/HEX IDs.
    /// </summary>
    private RyzenMemoryTimings GetRyzenTimings()
    {
        var ryzenMemory = new RyzenMemoryTimings();
        bool smuSlow = false;
        int smuDelay = smuSlow ? 60 : 10;

        Ols ols = new Ols();
        var status = ols.GetStatus();
        var dllStatus = Ols.OlsDllStatus.OLS_DLL_NO_ERROR;
        if (status != Ols.Status.NO_ERROR || ols.GetDllStatus() != (uint) dllStatus)
            throw new ApplicationException(
                $"Ols initialization error. OlsStatus={status.ToString()}; DllStatus={dllStatus.ToString()}");

        uint eax = 0, ebx = 0, ecx = 0, edx = 0;
        ols.CpuidPx(0x80000001, ref eax, ref ebx, ref ecx, ref edx, 0x01);

        uint smuorg = ols.ReadPciConfigDword(0x00, 0xB8);
        Thread.Sleep(smuDelay);

        uint someOffset = ReadDword(0x50200, ols, smuDelay) == 0x300 ? 0x100000u : 0u;

        uint bgs = ReadDword(0x00050058 + someOffset, ols, smuDelay);
        uint bgsa = ReadDword(0x000500D0 + someOffset, ols, smuDelay);
        uint dramConfiguration = ReadDword(0x00050200 + someOffset, ols, smuDelay);

        uint dramTiming1 = ReadDword(0x00050204 + someOffset, ols, smuDelay);
        uint dramTiming2 = ReadDword(0x00050208 + someOffset, ols, smuDelay);
        uint dramTiming3 = ReadDword(0x0005020C + someOffset, ols, smuDelay);
        uint dramTiming4 = ReadDword(0x00050210 + someOffset, ols, smuDelay);
        uint dramTiming5 = ReadDword(0x00050214 + someOffset, ols, smuDelay);
        uint dramTiming6 = ReadDword(0x00050218 + someOffset, ols, smuDelay);
        uint dramTiming7 = ReadDword(0x0005021C + someOffset, ols, smuDelay);
        uint dramTiming8 = ReadDword(0x00050220 + someOffset, ols, smuDelay);
        uint dramTiming9 = ReadDword(0x00050224 + someOffset, ols, smuDelay);
        uint dramTiming10 = ReadDword(0x00050228 + someOffset, ols, smuDelay);
        uint dramTiming12 = ReadDword(0x00050230 + someOffset, ols, smuDelay);
        uint dramTiming13 = ReadDword(0x00050234 + someOffset, ols, smuDelay);
        uint dramTiming20 = ReadDword(0x00050250 + someOffset, ols, smuDelay);
        uint dramTiming21 = ReadDword(0x00050254 + someOffset, ols, smuDelay);
        uint dramTiming22 = ReadDword(0x00050258 + someOffset, ols, smuDelay);

        uint tRfcTiming0 = ReadDword(0x00050260 + someOffset, ols, smuDelay);
        uint tRfcTiming1 = ReadDword(0x00050264 + someOffset, ols, smuDelay);
        uint tStagTiming0 = ReadDword(0x00050270 + someOffset, ols, smuDelay);
        uint tStagTiming1 = ReadDword(0x00050274 + someOffset, ols, smuDelay);
        uint dramTiming35 = ReadDword(0x0005028C + someOffset, ols, smuDelay);

        uint tRfcTiming, tStagTiming;
        if (tRfcTiming0 == tRfcTiming1)
        {
            tRfcTiming = tRfcTiming0;
            tStagTiming = tStagTiming0;
        }
        else if (tRfcTiming0 == 0x21060138)
        {
            tRfcTiming = tRfcTiming1;
            tStagTiming = tStagTiming1;
        }
        else
        {
            tRfcTiming = tRfcTiming0;
            tStagTiming = tStagTiming0;
        }

        ryzenMemory.BGS = (bgs != 0x87654321);
        ryzenMemory.BGSA = (bgsa == 0x111107F1);
        ryzenMemory.Preamble2T = ((dramConfiguration & 0x1000) >> 12) != 0;
        ryzenMemory.GDM = ((dramConfiguration & 0x800) >> 11) != 0;
        ryzenMemory.CommandRate = ((dramConfiguration & 0x400) >> 10) != 0 ? 2 : 1;
        var memClock = dramConfiguration & 0x7F;
        float memclktRxx = memClock / 3.0f * 100;

        ryzenMemory.tRCDWR = (dramTiming1 & 0x3F000000) >> 24;
        ryzenMemory.tRCDRD = (dramTiming1 & 0x3F0000) >> 16;
        ryzenMemory.tRAS = (dramTiming1 & 0x7F00) >> 8;
        ryzenMemory.tCL = dramTiming1 & 0x3F;

        ryzenMemory.tRPPB = (dramTiming2 & 0x3F000000) >> 24;
        ryzenMemory.tRP = (dramTiming2 & 0x3F0000) >> 16;
        ryzenMemory.tRCPB = (dramTiming2 & 0xFF00) >> 8;
        ryzenMemory.tRC = dramTiming2 & 0xFF;

        ryzenMemory.tRTP = (dramTiming3 & 0x1F000000) >> 24;
        ryzenMemory.tRRDDLR = (dramTiming3 & 0x1F0000) >> 16;
        ryzenMemory.tRRDL = (dramTiming3 & 0x1F00) >> 8;
        ryzenMemory.tRRDS = dramTiming3 & 0x1F;

        ryzenMemory.tFAWDLR = (dramTiming4 & 0x7E000000) >> 25;
        ryzenMemory.tFAWSLR = (dramTiming4 & 0xFC0000) >> 18;
        ryzenMemory.tFAW = dramTiming4 & 0x7F;

        ryzenMemory.tWTRL = (dramTiming5 & 0x7F0000) >> 16;
        ryzenMemory.tWTRS = (dramTiming5 & 0x1F00) >> 8;
        ryzenMemory.tCWL = dramTiming5 & 0x3F;

        ryzenMemory.tWR = dramTiming6 & 0x7F;

        ryzenMemory.tRCPage = (dramTiming7 & 0xFFF00000) >> 20;

        ryzenMemory.tRDRDBAN = (dramTiming8 & 0xC0000000) >> 30;
        ryzenMemory.tRDRDSCL = (dramTiming8 & 0x3F000000) >> 24;
        ryzenMemory.tRDRDSCDLR = (dramTiming8 & 0xF00000) >> 20;
        ryzenMemory.tRDRDSC = (dramTiming8 & 0xF0000) >> 16;
        ryzenMemory.tRDRDSD = (dramTiming8 & 0xF00) >> 8;
        ryzenMemory.tRDRDDD = dramTiming8 & 0xF;

        ryzenMemory.tWRWRBAN = (dramTiming9 & 0xC0000000) >> 30;
        ryzenMemory.tWRWRSCL = (dramTiming9 & 0x3F000000) >> 24;
        ryzenMemory.tWRWRSCDLR = (dramTiming9 & 0xF00000) >> 20;
        ryzenMemory.tWRWRSC = (dramTiming9 & 0xF0000) >> 16;
        ryzenMemory.tWRWRSD = (dramTiming9 & 0xF00) >> 8;
        ryzenMemory.tWRWRDD = dramTiming9 & 0xF;

        ryzenMemory.tWRRDSCDLR = (dramTiming10 & 0x1F0000) >> 16;
        ryzenMemory.tRDWR = (dramTiming10 & 0x1F00) >> 8;
        ryzenMemory.tWRRD = dramTiming10 & 0xF;

        ryzenMemory.tREF = dramTiming12 & 0xFFFF;
        ryzenMemory.tREFCT = (uint)(1000 / memclktRxx * ryzenMemory.tREF);

        ryzenMemory.tMODPDA = (dramTiming13 & 0x3F000000) >> 24;
        ryzenMemory.tMRDPDA = (dramTiming13 & 0x3F0000) >> 16;
        ryzenMemory.tMOD = (dramTiming13 & 0x3F00) >> 8;
        ryzenMemory.tMRD = dramTiming13 & 0x3F;

        ryzenMemory.tSTAG = (dramTiming20 & 0xFF0000) >> 16;

        ryzenMemory.tCKE = (dramTiming21 & 0x1F000000) >> 24;

        ryzenMemory.tPHYWRD = (dramTiming22 & 0x7000000) >> 24;
        ryzenMemory.tPHYRDLAT = (dramTiming22 & 0x3F0000) >> 16;
        ryzenMemory.tPHYWRLAT = (dramTiming22 & 0x1F00) >> 8;
        ryzenMemory.tRDDATA = dramTiming22 & 0x7F;

        ryzenMemory.tRFC4 = (tRfcTiming & 0xFFC00000) >> 22;
        ryzenMemory.tRFC4CT = (uint)(1000 / memclktRxx * ryzenMemory.tRFC4);

        ryzenMemory.tRFC2 = (tRfcTiming & 0x3FF800) >> 11;
        ryzenMemory.tRFC2CT = (uint)(1000 / memclktRxx * ryzenMemory.tRFC2);

        ryzenMemory.tRFC = tRfcTiming & 0x7FF;
        ryzenMemory.tRFCCT = (uint)(1000 / memclktRxx * ryzenMemory.tRFC);

        ryzenMemory.tSTAG4LR = (tStagTiming & 0x1FF00000) >> 20;
        ryzenMemory.tSTAG2LR = (tStagTiming & 0x7FC00) >> 10;
        ryzenMemory.tSTAGLR = tStagTiming & 0x1FF;

        ryzenMemory.tWRMPR = (dramTiming35 & 0x3F000000) >> 24;

        uint eax2 = 0, ebx2 = 0, ecx2 = 0, edx2 = 0;
        ols.CpuidPx(0x80000001, ref eax2, ref ebx2, ref ecx2, ref edx2, 0x01);

        ols.WritePciConfigDword(0x0, 0xB8, 0x3B10528);
        ols.WritePciConfigDword(0x0, 0xBC, 0x02);
        ols.WritePciConfigDword(0x00, 0xB8, smuorg);
        Thread.Sleep(smuDelay);

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

    public string Manufacturer => _manufacturer.Value;

    public string Product => _product.Value;

    public string SystemName => _systemName.Value;

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
            ApplicationCore.Enums.CacheLevel.L1 => (ushort)CacheLevel.Level1,
            ApplicationCore.Enums.CacheLevel.L2 => (ushort)CacheLevel.Level2,
            ApplicationCore.Enums.CacheLevel.L3 => (ushort)CacheLevel.Level3,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

        using (var mc = new ManagementClass("Win32_CacheMemory"))
        {
            using (var moc = mc.GetInstances())
            {
                return moc
                    .Cast<ManagementObject>()
                    .Where(p => p.Get<ushort>("Level") == searchLevel)
                    .Select(p => p.Get<uint>("MaxCacheSize"))
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