using System;
using System.Collections.Generic;
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
    private readonly ManagementEventWatcher _installDeviceEventWatcher;
    private readonly ManagementEventWatcher _uninstallDeviceEventWatcher;
    
    public int NvidiaGpuCount { get; private set; }
    public int RadeonGpuCount { get; private set; }
    public CpuInfo CpuInfo { get; }
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
        
        _installDeviceEventWatcher = new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
        _installDeviceEventWatcher.EventArrived += OnNewDeviceInstalled;
        _uninstallDeviceEventWatcher = new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
        _uninstallDeviceEventWatcher.EventArrived += OnDeviceUninstalled;
        
        CpuInfo = new CpuInfo();
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

            CpuInfo.Family = int.Parse(words[familyIndex]);
            CpuInfo.Model = int.Parse(words[modelIndex]);
            CpuInfo.Stepping = int.Parse(words[steppingIndex].TrimEnd(','));

            foreach (var cpuInfo in _processorInfoSearcher.Get()) //todo test this shit
            {
                CpuInfo.Name = cpuInfo["Name"].ToString();
            }
        }
        catch (ManagementException ex)
        {
            _logger.LogError(ex, "Error occurred when analyzing cpu information");
        }

        if (CpuInfo.Name.Contains("Intel"))
        {
            CpuInfo.Manufacturer = ApplicationCore.Enums.Manufacturer.Intel;
            _intelManagementService.DetermineCpu();
        }
        else
        {
            switch (CpuInfo.RyzenGeneration)
            {
                case RyzenGenerations.Zen1_2:
                {
                    CpuInfo.RyzenFamily = CpuInfo.Model switch
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
                    if (CpuInfo.Model == 32 &&
                        (CpuInfo.Name.Contains("15e") || CpuInfo.Name.Contains("15Ce") || CpuInfo.Name.Contains("20e")))
                    {
                        CpuInfo.RyzenFamily = RyzenFamily.Pollock;
                    }

                    break;
                }
                case RyzenGenerations.Zen3_4:
                {
                    CpuInfo.RyzenFamily = CpuInfo.Model switch
                    {
                        33 => RyzenFamily.Vermeer,
                        63 or 68 => RyzenFamily.Rembrandt,
                        80 => CpuInfo.Name.Contains("25") || CpuInfo.Name.Contains("75") || CpuInfo.Name.Contains("30")
                            ? RyzenFamily.Barcelo
                            : RyzenFamily.Cezanne,
                        116 => RyzenFamily.PhoenixPoint,
                        120 => RyzenFamily.PhoenixPoint2,
                        117 => RyzenFamily.HawkPoint
                    };
                    if (CpuInfo.Model == 97)
                    {
                        CpuInfo.RyzenFamily =
                            CpuInfo.Name.Contains("HX") ? RyzenFamily.DragonRange : RyzenFamily.Raphael;
                    }

                    break;
                }
                case RyzenGenerations.Zen5_6:
                {
                    CpuInfo.RyzenFamily = CpuInfo.Model switch
                    {
                        32 or 36 => RyzenFamily.StrixPoint,
                        68 => RyzenFamily.GraniteRidge,
                        112 => RyzenFamily.StrixHalo,
                        _ => RyzenFamily.StrixPoint2
                    };
                    break;
                }
            }

            CpuInfo.AmdProcessorType = CpuInfo.RyzenFamily is RyzenFamily.SummitRidge
                or RyzenFamily.PinnacleRidge
                or RyzenFamily.Matisse
                or RyzenFamily.Vermeer
                or RyzenFamily.Raphael
                or RyzenFamily.DragonRange
                or RyzenFamily.GraniteRidge
                ? AmdProcessorType.Desktop
                : AmdProcessorType.Apu;

            Addresses.SetAddresses(CpuInfo);
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
    
    public string GetCodename()
    {
        if (CpuInfo.Manufacturer == ApplicationCore.Enums.Manufacturer.Intel)
        {
            if (CpuInfo.Name.Contains("6th")) return "Skylake";
            if (CpuInfo.Name.Contains("7th")) return "Kaby Lake";
            if (CpuInfo.Name.Contains("8th") && CpuInfo.Name.Contains("G")) return "Kaby Lake";
            else if (CpuInfo.Name.Contains("8121U") || CpuInfo.Name.Contains("8114Y")) return "Cannon Lake";
            else if (CpuInfo.Name.Contains("8th")) return "Coffee Lake";
            if (CpuInfo.Name.Contains("9th")) return "Coffee Lake";
            if (CpuInfo.Name.Contains("10th") && CpuInfo.Name.Contains("G")) return "Ice Lake";
            else if (CpuInfo.Name.Contains("10th")) return "Comet Lake";
            if (CpuInfo.Name.Contains("11th"))
            {
                if (CpuInfo.Name.Contains('G') || CpuInfo.Name.Contains('U') || CpuInfo.Name.Contains('H') || CpuInfo.Name.Contains("KB"))
                {
                    return "Tiger Lake";
                }
            }
            else
            {
                return "Rocket Lake";
            }
            if (CpuInfo.Name.Contains("12th")) return "Alder Lake";
            if (CpuInfo.Name.Contains("13th") || CpuInfo.Name.Contains("14th") ||
                CpuInfo.Name.Contains("Core") && CpuInfo.Name.Contains("100") && !CpuInfo.Name.Contains("th")) return "Raptor Lake";
            if (CpuInfo.Name.Contains("Core") && CpuInfo.Name.Contains("Ultra") && CpuInfo.Name.Contains("100")) return "Meteor Lake";
        }
        else
        {
            return CpuInfo.RyzenFamily switch
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
        if (CpuInfo.Manufacturer == ApplicationCore.Enums.Manufacturer.Intel)
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

        if (CpuInfo.Name.Contains("7545U") && CpuInfo.RyzenFamily == RyzenFamily.PhoenixPoint2 ||
            CpuInfo.Name.Contains("Z1") && CpuInfo.RyzenFamily == RyzenFamily.PhoenixPoint2 || CpuInfo.Name.Contains("7440U"))
        {
            bigCores = CpuInfo.Name.Contains("7440U") ? 0 : 2;
            smallCores = cores - bigCores;
            return $"{cores} ({bigCores} Prime Cores + {smallCores} Compact Cores)";
        }
            
        return cores.ToString();
    }

    public string GetInstructionSets()
    {
        List<string> supportedInstrictions = new();
        if (IsMMXSupported()) supportedInstrictions.Add("MMX");
        if (Sse.IsSupported) supportedInstrictions.Add("SSE");
        if (Sse2.IsSupported) supportedInstrictions.Add("SSE2");
        if (Sse3.IsSupported) supportedInstrictions.Add("SSE3");
        if (Ssse3.IsSupported) supportedInstrictions.Add("SSSE3");
        if (Sse41.IsSupported) supportedInstrictions.Add("SSE4.1");
        if (Sse42.IsSupported) supportedInstrictions.Add("SSE4.2");
        if (IsEM64TSupported()) supportedInstrictions.Add("EM64T");
        if (Environment.Is64BitProcess) supportedInstrictions.Add("x86-64");
        if (IsVirtualizationEnabled())
        {
            supportedInstrictions.Add(CpuInfo.Manufacturer == ApplicationCore.Enums.Manufacturer.Intel ? "VT-x" : "AMD-V");
        }
        if (Aes.IsSupported) supportedInstrictions.Add("AES");
        if (Avx.IsSupported) supportedInstrictions.Add("AVX");
        if (Avx2.IsSupported) supportedInstrictions.Add("AVX2");
        if (CheckAVX512Support()) supportedInstrictions.Add("AVX512");
        if (Fma.IsSupported) supportedInstrictions.Add("FMA3");
        
        return string.Join(", ", supportedInstrictions);
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
            if (CpuInfo.Manufacturer != ApplicationCore.Enums.Manufacturer.Intel &&
                CpuInfo.RyzenFamily < RyzenFamily.Raphael)
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