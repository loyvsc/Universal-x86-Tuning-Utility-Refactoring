using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Runtime.Intrinsics.X86;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.Extensions.Logging;
using Universal_x86_Tuning_Utility.Services.Amd;

namespace Universal_x86_Tuning_Utility.Services.SystemInfoServices;

// todo: move cpu info to CpuInfo
public class WindowsSystemInfoService : ISystemInfoService, IDisposable
{
    private readonly ILogger<WindowsSystemInfoService> _logger;
    private readonly IIntelManagementService _intelManagementService;

    private readonly ManagementObjectSearcher _baseboardSearcher;
    private readonly ManagementObjectSearcher _motherboardSearcher;
    private readonly ManagementObjectSearcher _systemInfoSearcher;
    private readonly ManagementObjectSearcher _processorInfoSearcher;
    private readonly ManagementObjectSearcher _batteryInfoSearcher;
    private readonly ManagementObjectSearcher _memoryInfoSearcher;
    
    private readonly ManagementEventWatcher _installDeviceEventWatcher;
    private readonly ManagementEventWatcher _uninstallDeviceEventWatcher;
    
    public int NvidiaGpuCount { get; private set; }
    public int RadeonGpuCount { get; private set; }
    public CpuInfo Cpu { get; }
    public RamInfo Ram { get; }
    public LaptopInfo? LaptopInfo { get; private set; }
    public List<string> GpuNames { get; }
    
    public WindowsSystemInfoService(ILogger<WindowsSystemInfoService> logger,
                                    IIntelManagementService intelManagementService)
    {
        _logger = logger;
        _intelManagementService = intelManagementService;
        _baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
        _motherboardSearcher = 
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_MotherboardDevice");
        _systemInfoSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystemProduct");
        _processorInfoSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
        _batteryInfoSearcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryStatus");
        _memoryInfoSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory");
        
        _installDeviceEventWatcher = new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
        _installDeviceEventWatcher.EventArrived += OnNewDeviceInstalled;
        _uninstallDeviceEventWatcher = new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
        _uninstallDeviceEventWatcher.EventArrived += OnDeviceUninstalled;
        
        Cpu = new CpuInfo();
        Ram = new RamInfo();
        GpuNames = new List<string>();
    }

    private void OnDeviceUninstalled(object sender, EventArrivedEventArgs e) //todo: test this
    {
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'DISPLAY'"))
        {
            foreach (var device in searcher.Get())
            {
                if (device["Name"] is string name)
                {
                    GpuNames.Remove(name);
                }
            }
        }
    }

    private void OnNewDeviceInstalled(object sender, EventArrivedEventArgs e) //todo: test this
    {
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'DISPLAY'"))
        {
            foreach (var device in searcher.Get())
            {
                if (device["Name"] is string name)
                {
                    GpuNames.Add(name);
                }
            }
        }
    }

    public void AnalyzeSystem()
    {
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
        {
            foreach (var videoController in searcher.Get())
            {
                if (videoController["Name"] is string name)
                {
                    GpuNames.Add(name);
                    if (name.Contains("Radeon"))
                    {
                        RadeonGpuCount++;
                    }
                    else if (name.Contains("NVIDIA"))
                    {
                        NvidiaGpuCount++;
                    }
                }
            }
        }

        try //todo try find another solution for getting information
        {
            var processorIdentifier = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");

            var words = processorIdentifier.Split(' ');

            var familyIndex = Array.IndexOf(words, "Family") + 1;
            var modelIndex = Array.IndexOf(words, "Model") + 1;
            var steppingIndex = Array.IndexOf(words, "Stepping") + 1;

            Cpu.Family = int.Parse(words[familyIndex]);
            Cpu.Model = int.Parse(words[modelIndex]);
            Cpu.Stepping = int.Parse(words[steppingIndex].TrimEnd(','));
            
            foreach (var cpuInfo in _processorInfoSearcher.Get()) //todo test this shit
            {
                Cpu.Name = cpuInfo["Name"].ToString();
                Cpu.Description = cpuInfo["Description"].ToString();
                Cpu.CoresCount = Convert.ToInt32(cpuInfo["NumberOfCores"]);
                Cpu.LogicalCoresCount = Convert.ToInt32(cpuInfo["NumberOfLogicalProcessors"]);
                Cpu.L2Size = Convert.ToDouble(cpuInfo["L2CacheSize"]) / 1024;
                Cpu.L3Size = Convert.ToDouble(cpuInfo["L3CacheSize"]) / 1024;
                Cpu.BaseClock = cpuInfo["MaxClockSpeed"].ToString(); //todo: check why is base clock here
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when analyzing cpu information");
        }

        Cpu.CodeName = GetCodename();
        
        List<string> supportedInstructions = [];
        if (IsMMXSupported()) supportedInstructions.Add("MMX");
        if (Sse.IsSupported) supportedInstructions.Add("SSE");
        if (Sse2.IsSupported) supportedInstructions.Add("SSE2");
        if (Sse3.IsSupported) supportedInstructions.Add("SSE3");
        if (Ssse3.IsSupported) supportedInstructions.Add("SSSE3");
        if (Sse41.IsSupported) supportedInstructions.Add("SSE4.1");
        if (Sse42.IsSupported) supportedInstructions.Add("SSE4.2");
        if (IsEM64TSupported()) supportedInstructions.Add("EM64T");
        if (Environment.Is64BitProcess) supportedInstructions.Add("x86-64");
        if (IsVirtualizationEnabled())
        {
            supportedInstructions.Add(
                Cpu.Manufacturer == ApplicationCore.Enums.Manufacturer.Intel 
                ? "VT-x" 
                : "AMD-V");
        }
        if (Aes.IsSupported) supportedInstructions.Add("AES");
        if (Avx.IsSupported) supportedInstructions.Add("AVX");
        if (Avx2.IsSupported) supportedInstructions.Add("AVX2");
        if (CheckAVX512Support()) supportedInstructions.Add("AVX512");
        if (Fma.IsSupported) supportedInstructions.Add("FMA3");

        Cpu.SupportedInstructions = new ReadOnlyCollection<string>(supportedInstructions);

        try
        {
            AnalyzeRam();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when analyzing ram information");
        }

        if (Cpu.Name.Contains("Intel"))
        {
            Cpu.Manufacturer = ApplicationCore.Enums.Manufacturer.Intel;
            _intelManagementService.DetermineCpu();
        }
        else
        {
            switch (Cpu.RyzenGeneration)
            {
                case RyzenGenerations.Zen1_2:
                {
                    Cpu.RyzenFamily = Cpu.Model switch
                    {
                        1 => RyzenFamily.SummitRidge,
                        8 => RyzenFamily.PinnacleRidge,
                        17 or 18 => RyzenFamily.RavenRidge,
                        24 => RyzenFamily.Picasso,
                        32 => RyzenFamily.Dali,
                        80 => RyzenFamily.FireFlight,
                        96 => RyzenFamily.Renoir,
                        104 => RyzenFamily.Lucienne,
                        113 => RyzenFamily.Matisse,
                        114 or 145 => RyzenFamily.VanGogh,
                        160 => RyzenFamily.Mendocino
                    };
                    if (Cpu.Model == 32 &&
                        (Cpu.Name.Contains("15e") || Cpu.Name.Contains("15Ce") || Cpu.Name.Contains("20e")))
                    {
                        Cpu.RyzenFamily = RyzenFamily.Pollock;
                    }

                    break;
                }
                case RyzenGenerations.Zen3_4:
                {
                    Cpu.RyzenFamily = Cpu.Model switch
                    {
                        33 => RyzenFamily.Vermeer,
                        63 or 68 => RyzenFamily.Rembrandt,
                        80 => Cpu.Name.Contains("25") || Cpu.Name.Contains("75") || Cpu.Name.Contains("30")
                            ? RyzenFamily.Barcelo
                            : RyzenFamily.Cezanne,
                        116 => RyzenFamily.PhoenixPoint,
                        120 => RyzenFamily.PhoenixPoint2,
                        117 => RyzenFamily.HawkPoint
                    };
                    if (Cpu.Model == 97)
                    {
                        Cpu.RyzenFamily =
                            Cpu.Name.Contains("HX") ? RyzenFamily.DragonRange : RyzenFamily.Raphael;
                    }

                    break;
                }
                case RyzenGenerations.Zen5_6:
                {
                    Cpu.RyzenFamily = Cpu.Model switch
                    {
                        32 or 36 => RyzenFamily.StrixPoint,
                        68 => RyzenFamily.GraniteRidge,
                        112 => RyzenFamily.StrixHalo,
                        _ => RyzenFamily.StrixPoint2
                    };
                    break;
                }
            }

            Cpu.AmdProcessorType = Cpu.RyzenFamily is RyzenFamily.SummitRidge
                or RyzenFamily.PinnacleRidge
                or RyzenFamily.Matisse
                or RyzenFamily.Vermeer
                or RyzenFamily.Raphael
                or RyzenFamily.DragonRange
                or RyzenFamily.GraniteRidge
                ? AmdProcessorType.Desktop
                : AmdProcessorType.Apu;

            Addresses.SetAddresses(Cpu);
        }

        var product = Product.ToLower();
        if (product.Contains("laptop"))
        {
            LaptopInfo = new LaptopInfo()
            {
                IsAsus = product.Contains("rog")
                         || product.Contains("tuf")
                         || product.Contains("ally")
                         || product.Contains("flow")
                         || product.Contains("vivobook")
                         || product.Contains("zenbook")
            };
        }
    }

    private bool IsVirtualizationEnabled()
    {
        foreach (var queryObj in _processorInfoSearcher.Get())
        {
            // Check if virtualization is enabled
            if (queryObj["VirtualizationFirmwareEnabled"] is bool)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsEM64TSupported()
    {
        using (var mo = new ManagementObject("Win32_Processor.DeviceID='CPU0'"))
        {
            var i = (ushort)mo["Architecture"];

            return i == 9;
        }
    }

    private bool CheckAVX512Support()
    {
        try
        {
            if (Cpu.Manufacturer != ApplicationCore.Enums.Manufacturer.Intel &&
                Cpu.RyzenFamily < RyzenFamily.Raphael)
            {
                return false;
            }
            return NativeMethods.IsProcessorFeaturePresent(NativeMethods.PF_AVX512F_INSTRUCTIONS_AVAILABLE);
        }
        catch
        {
            return false;
        }
    }

    private bool IsMMXSupported()
    {
        if (Environment.Is64BitProcess)
        {
            // For 64-bit processes, MMX is always supported on Windows.
            return true;
        }
        
        // For 32-bit processes, check for MMX support on Windows.
        return NativeMethods.IsProcessorFeaturePresent(NativeMethods.PF_MMX_INSTRUCTIONS_AVAILABLE);
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
                Model = queryObj["PartNumber"].ToString()!,
                Capacity = Convert.ToDouble(queryObj["Capacity"]),
                Speed = Convert.ToInt32(queryObj["ConfiguredClockSpeed"])
            };
            
            modules.Add(module);
            type = Convert.ToInt32(queryObj["SMBIOSMemoryType"]);
            width += Convert.ToInt32(queryObj["DataWidth"]);
        }

        if (width > 128 && Cpu.RyzenFamily != RyzenFamily.Unknown)
        {
            switch (Cpu.RyzenFamily)
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
                    if (Cpu.RyzenFamily < RyzenFamily.StrixPoint2)
                    {
                        width = 128;
                    }
                    break;
                }
            }
        }

        Ram.Modules = modules.ToArray();
        Ram.Capacity = modules.Sum(module => module.Capacity);
        Ram.Width = width;
        
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

    //todo try store gpu names in collection and subscribe to events on plug in or out video controller
    public bool IsGPUPresent(string gpuName)
    {
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
        {
            foreach (var result in searcher.Get())
            {
                var name = result["Name"]?.ToString();
                if (!string.IsNullOrEmpty(name) && name.Contains(gpuName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    public string Manufacturer
    {
        get
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
        }
    }

    public string Product
    {
        get
        {
            foreach (var queryObj in _systemInfoSearcher.Get())
            {
                var manufacturerName = queryObj["Manufacturer"].ToString();
                if (manufacturerName != null)
                {
                    return manufacturerName;
                }
            }

            return string.Empty;
        }
    }

    public string SystemName
    {
        get
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
        }
    }

    public decimal GetBatteryRate()
    {
        foreach (var obj in _batteryInfoSearcher.Get())
        {
            var chargeRate = Convert.ToDecimal(obj["ChargeRate"]);
            var dischargeRate = Convert.ToDecimal(obj["DischargeRate"]);
                
            return chargeRate > 0 ? chargeRate : dischargeRate;
        }
        
        return 0;
    }
    
    public BatteryStatus GetBatteryStatus()
    {
        var batteryClass = new ManagementClass("Win32_Battery");
        var batteries = batteryClass.GetInstances();
        
        foreach (var battery in batteries)
        {
            if (battery["BatteryStatus"] is ushort batteryStatus)
            {
                return batteryStatus switch
                {
                    1 => BatteryStatus.Discharging,
                    3 => BatteryStatus.FullCharged,
                    4 or 5 => BatteryStatus.Low,
                    2 or 7 or 8 or 9 or 11 => BatteryStatus.Charging,
                    _ => BatteryStatus.Unknown
                };
            }
        }

        return BatteryStatus.Unknown;
    }

    public decimal ReadFullChargeCapacity()
    {
        using (var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryFullChargedCapacity"))
        {
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            { 
                return Convert.ToDecimal(obj["FullChargedCapacity"]);
            }
        }

        return 0;
    }

    public decimal ReadDesignCapacity()
    {
        using (var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryStaticData"))
        {
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                return Convert.ToDecimal(obj["DesignedCapacity"]);
            }
        }

        return 0;
    }

    public int GetBatteryCycle()
    {
        using (var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryCycleCount"))
        {
            foreach (var queryObj in searcher.Get())
            {
                return Convert.ToInt32(queryObj["CycleCount"]);
            }
        }

        return 0;
    }

    public decimal GetBatteryHealth()
    {
        var designCap = ReadDesignCapacity();
        var fullCap = ReadFullChargeCapacity();

        var health = fullCap / designCap;

        return health;
    }

    private enum CacheLevel : ushort
    {
        Level1 = 3,
        Level2 = 4,
        Level3 = 5
    }

    public List<uint> GetCacheSize(ApplicationCore.Enums.CacheLevel level) //todo: test
    {
        var searchLevel = level switch
        {
            ApplicationCore.Enums.CacheLevel.L1 => (ushort) CacheLevel.Level1,
            ApplicationCore.Enums.CacheLevel.L2 => (ushort) CacheLevel.Level2,
            ApplicationCore.Enums.CacheLevel.L3 => (ushort)CacheLevel.Level3,
        };
        
        var mc = new ManagementClass("Win32_CacheMemory");
        var moc = mc.GetInstances();

        return moc
            .Cast<ManagementObject>()
            .Where(p => (ushort)p.Properties["Level"].Value == searchLevel)
            .Select(p => (uint)p.Properties["MaxCacheSize"].Value)
            .ToList();
    }
    
    private string GetCodename()
    {
        if (Cpu.Manufacturer == ApplicationCore.Enums.Manufacturer.Intel)
        {
            if (Cpu.Name.Contains("6th")) return "Skylake";
            if (Cpu.Name.Contains("7th")) return "Kaby Lake";
            if (Cpu.Name.Contains("8th") && Cpu.Name.Contains("G")) return "Kaby Lake";
            else if (Cpu.Name.Contains("8121U") || Cpu.Name.Contains("8114Y")) return "Cannon Lake";
            else if (Cpu.Name.Contains("8th")) return "Coffee Lake";
            if (Cpu.Name.Contains("9th")) return "Coffee Lake";
            if (Cpu.Name.Contains("10th") && Cpu.Name.Contains("G")) return "Ice Lake";
            else if (Cpu.Name.Contains("10th")) return "Comet Lake";
            if (Cpu.Name.Contains("11th"))
            {
                if (Cpu.Name.Contains('G') || Cpu.Name.Contains('U') || Cpu.Name.Contains('H') || Cpu.Name.Contains("KB"))
                {
                    return "Tiger Lake";
                }
            }
            else
            {
                return "Rocket Lake";
            }
            if (Cpu.Name.Contains("12th")) return "Alder Lake";
            if (Cpu.Name.Contains("13th") || Cpu.Name.Contains("14th") ||
                Cpu.Name.Contains("Core") && Cpu.Name.Contains("100") && !Cpu.Name.Contains("th")) return "Raptor Lake";
            if (Cpu.Name.Contains("Core") && Cpu.Name.Contains("Ultra") && Cpu.Name.Contains("100")) return "Meteor Lake";
        }
        else
        {
            return Cpu.RyzenFamily switch
            {
                RyzenFamily.SummitRidge => "Summit Ridge",
                RyzenFamily.PinnacleRidge => "Pinnacle Ridge",
                RyzenFamily.RavenRidge => "Raven Ridge",
                RyzenFamily.Dali => "Dali",
                RyzenFamily.Pollock => "Pollock",
                RyzenFamily.Picasso => "Picasso",
                RyzenFamily.FireFlight => "Fire Flight",
                RyzenFamily.Matisse => "Matisse",
                RyzenFamily.Renoir => "Renoir",
                RyzenFamily.Lucienne => "Lucienne",
                RyzenFamily.VanGogh => "Van Gogh",
                RyzenFamily.Mendocino => "Mendocino",
                RyzenFamily.Vermeer => "Vermeer",
                RyzenFamily.Barcelo => "Barcelo",
                RyzenFamily.Cezanne => "Cezanne",
                RyzenFamily.Rembrandt => "Rembrandt",
                RyzenFamily.Raphael => "Raphael",
                RyzenFamily.DragonRange => "Dragon Range",
                RyzenFamily.PhoenixPoint => "Phoenix Point",
                RyzenFamily.PhoenixPoint2 => "Phoenix Point 2",
                RyzenFamily.HawkPoint => "Hawk Point",
                RyzenFamily.SonomaValley => "Sonoma Valley",
                RyzenFamily.GraniteRidge => "Granite Ridge",
                RyzenFamily.FireRange => "Fire Range",
                RyzenFamily.StrixPoint => "Strix Point",
                RyzenFamily.StrixPoint2 => "Strix Point 2 / Kraken",
                RyzenFamily.StrixHalo => "Strix Halo",
                _ => ""
            };
        }

        return "";
    }

    // todo mvoe to analyze system
    public string GetBigLITTLE(int cores, double l2)
    {
        int bigCores = 0;
        int smallCores = 0;
        if (Cpu.Manufacturer == ApplicationCore.Enums.Manufacturer.Intel)
        {
            //if (CPUName.Contains("12th") || CPUName.Contains("13th") || CPUName.Contains("14th") || CPUName.Contains("Core") && CPUName.Contains("1000") && !CPUName.Contains("i"))
            //{
            //    if (l2 % 1.25 == 0) bigCores = (int)(l2 / 1.25);
            //    else if (l2 % 2 == 0) bigCores = (int)(l2 / 2);

            //    smallCores = cores - bigCores;

            //    if (smallCores > 0)
            //    {
            //        if (CPUName.Contains("Ultra") && CPUName.Contains("100")) return $"{cores} ({bigCores} Performance Cores + {smallCores - 2} Efficiency Cores + 2 LP Efficiency Cores)";
            //        else return $"{cores} ({bigCores} Performance Cores + {smallCores} Efficiency Cores)";
            //    }
            //    else return cores.ToString();
            //}
        }

        if (Cpu.Name.Contains("7545U") && Cpu.RyzenFamily == RyzenFamily.PhoenixPoint2 ||
            Cpu.Name.Contains("Z1") && Cpu.RyzenFamily == RyzenFamily.PhoenixPoint2 || Cpu.Name.Contains("7440U"))
        {
            bigCores = Cpu.Name.Contains("7440U") ? 0 : 2;
            smallCores = cores - bigCores;
            return $"{cores} ({bigCores} Prime Cores + {smallCores} Compact Cores)";
        }
            
        return cores.ToString();
    }
    
    public void Dispose()
    {
        _installDeviceEventWatcher.Stop();
        _installDeviceEventWatcher.Dispose();
        _baseboardSearcher.Dispose();
        _motherboardSearcher.Dispose();
        _systemInfoSearcher.Dispose();
        _processorInfoSearcher.Dispose();
        _batteryInfoSearcher.Dispose();
    }
}

public static class NativeMethods
{
    // Import the CPUID intrinsic (Intel x86 instruction)
    [System.Runtime.InteropServices.DllImport("cpuid_x64.dll")]
    public static extern void Cpuid(int leafNumber, int subleafNumber, ref int eax, ref int ebx, ref int ecx,
        ref int edx);

    public const int PF_MMX_INSTRUCTIONS_AVAILABLE = 3;
    public const int PF_AVX512F_INSTRUCTIONS_AVAILABLE = 49;

    // Import the GetSystemInfo function (Windows API) to check MMX support.
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

    // Helper struct for GetSystemInfo function.
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct SYSTEM_INFO
    {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public IntPtr lpMinimumApplicationAddress;
        public IntPtr lpMaximumApplicationAddress;
        public IntPtr dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
    }

    // Helper method to check MMX support on Windows.
    public static bool IsProcessorFeaturePresent(int processorFeature)
    {
        GetSystemInfo(out SYSTEM_INFO sysInfo);
        return (sysInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL ||
                sysInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64) &&
               (sysInfo.wProcessorLevel & processorFeature) != 0;
    }

    private const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
    private const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
}