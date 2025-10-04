using System.Management;
using System.Runtime.Intrinsics.X86;
using ApplicationCore.Enums;
using ApplicationCore.Extensions;
using ApplicationCore.Models;

namespace ApplicationCore.Utilities;

public static class CpuInfoFactory
{
    public static CpuInfo Create(int family, int model, int stepping,
        string name, string description, int coresCount, int logicalCoresCount, 
        int maxClockSpeed, uint l1Size, uint l2Size, uint l3Size)
    {
        if (name.Contains("Intel", StringComparison.InvariantCultureIgnoreCase))
        {
            var intelFamily = GetIntelFamily(model, stepping);
            
            return new IntelCpuInfo()
            {
                Family = family,
                Model = model,
                Stepping = stepping,
                Name = name,
                Description = description,
                CoresCount = coresCount,
                LogicalCoresCount = logicalCoresCount,
                BaseClock = maxClockSpeed,
                IntelFamily = intelFamily,
                CodeName = intelFamily.GetDescription(),
                L1Size = l1Size,
                L2Size = l2Size,
                L3Size = l3Size,
                SupportedInstructions = GetSupportedInstructions()
            };
        }
        else if (name.Contains("AMD", StringComparison.InvariantCultureIgnoreCase))
        {
            var ryzenGeneration = GetRyzenGeneration(family);
            var ryzenFamily = GetRyzenFamily(ryzenGeneration, name, model, stepping);

            return new RyzenCpuInfo()
            {
                Family = family,
                Model = model,
                Stepping = stepping,
                Name = name,
                Description = description,
                CoresCount = coresCount,
                LogicalCoresCount = logicalCoresCount,
                BaseClock = maxClockSpeed,
                L1Size = l1Size,
                L2Size = l2Size,
                L3Size = l3Size,
                SupportedInstructions = GetSupportedInstructions(),
                RyzenFamily = ryzenFamily,
                RyzenSeries = GetRyzenSeries(name),
                RyzenGeneration = ryzenGeneration,
                CodeName = ryzenFamily switch
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
                    _ => string.Empty
                }
            };
        }
        else
        {
            return new IntelCpuInfo()
            {
                Family = family,
                Model = model,
                Stepping = stepping,
                Name = name,
                Description = description,
                CoresCount = coresCount,
                LogicalCoresCount = logicalCoresCount,
                BaseClock = maxClockSpeed,
                L1Size = l1Size,
                L2Size = l2Size,
                L3Size = l3Size,
                SupportedInstructions = GetSupportedInstructions()
            };
        }
    }
    
    private static IntelFamily GetIntelFamily(int model, int stepping)
    {
        //REFERENCE: https://github.com/torvalds/linux/blob/master/arch/x86/include/asm/intel-family.h

        switch (model)
        {
            case 0x3C:
            case 0x3F:
            case 0x45:
            case 0x46: return IntelFamily.Haswell;
            case 0x3D:
            case 0x47:
            case 0x4F:
            case 0x56: return IntelFamily.Broadwell;
            case 0x4E:
            case 0x5E:
            case 0x55: return IntelFamily.Skylake;
            case 0x8E:
            {
                return stepping switch
                {
                    9 => IntelFamily.Amberlake,
                    10 => IntelFamily.Coffeelake,
                    11 or 12 => IntelFamily.WhiskeyLake,
                    _ => IntelFamily.Kabylake
                };
            }
            case 0x9E:
            {
                if (stepping is >= 10 and <= 13)
                {
                    return IntelFamily.Coffeelake;
                }

                return IntelFamily.Kabylake;
            }
            case 0xA5:
            case 0xA6: return IntelFamily.Cometlake;
            case 0x66: return IntelFamily.CannonLake;
            case 0x6A:
            case 0x6C:
            case 0x7D:
            case 0x7E:
            case 0x9D: return IntelFamily.Icelake;
            case 0xA7: return IntelFamily.RocketLake;
            case 0x8C:
            case 0x8D: return IntelFamily.Tigerlake;
            case 0x8A: return IntelFamily.Jasperlake;
            case 0x97:
            case 0x9A:
            case 0xBE: return IntelFamily.Alderlake;
            case 0xB7:
            case 0xBA:
            case 0xBF: return IntelFamily.Raptorlake;
            case 0xAC:
            case 0xAA: return IntelFamily.Meteorlake;
            case 0xC6: return IntelFamily.Arrowlake;
            case 0xBD: return IntelFamily.Lunarlake;
            case 0xCC: return IntelFamily.Pantherlake;
            case 0xD5: return IntelFamily.Wildcatlake;
            case 0x01:
            case 0x03: return IntelFamily.Novalake;

            default: return IntelFamily.Unknown;
        }
    }

    private static RyzenFamily GetRyzenFamily(RyzenGeneration generation, string cpuName, int model, int stepping)
    {
        switch (generation)
        {
            case RyzenGeneration.Zen1_2:
            {
                return model switch
                {
                    1 => RyzenFamily.SummitRidge,
                    8 => RyzenFamily.PinnacleRidge,
                    17 or 18 => RyzenFamily.RavenRidge,
                    24 => RyzenFamily.Picasso,
                    32 => cpuName.Contains("15e") || cpuName.Contains("15Ce") || cpuName.Contains("20e") ? RyzenFamily.Pollock : RyzenFamily.Dali,
                    80 => RyzenFamily.FireFlight,
                    96 => RyzenFamily.Renoir,
                    104 => RyzenFamily.Lucienne,
                    113 => RyzenFamily.Matisse,
                    114 or 145 => RyzenFamily.VanGogh,
                    160 => RyzenFamily.Mendocino
                };
            }
            case RyzenGeneration.Zen3_4:
            {
                return model switch
                {
                    33 => RyzenFamily.Vermeer,
                    63 or 68 => RyzenFamily.Rembrandt,
                    80 => cpuName.Contains("25") || cpuName.Contains("75") || cpuName.Contains("30")
                        ? RyzenFamily.Barcelo
                        : RyzenFamily.Cezanne,
                    97 => cpuName.Contains("HX") ? RyzenFamily.DragonRange : RyzenFamily.Raphael,
                    116 => RyzenFamily.PhoenixPoint,
                    120 => RyzenFamily.PhoenixPoint2,
                    117 => RyzenFamily.HawkPoint
                };
            }
            case RyzenGeneration.Zen5_6:
            {
                return model switch
                {
                    32 or 36 => RyzenFamily.StrixPoint,
                    68 => RyzenFamily.GraniteRidge,
                    112 => RyzenFamily.StrixHalo,
                    _ => RyzenFamily.StrixPoint2
                };
            }
            default: return RyzenFamily.Unknown;
        }
    }
    
    private static RyzenSeries GetRyzenSeries(string cpuName)
    {
        var cpuData = cpuName.ToLower().Split(' ');
        var ryzenIndex = Array.IndexOf(cpuData, "ryzen");
        
        if (ryzenIndex != -1 && cpuData.Length >= 2)
        {
            var ryzenSeries = cpuData[ryzenIndex + 1];
            if (int.TryParse(ryzenSeries, out var ryzenSeriesValue))
            {
                return ryzenSeriesValue switch
                {
                    3 => RyzenSeries.Ryzen3,
                    5 => RyzenSeries.Ryzen5,
                    7 => RyzenSeries.Ryzen7,
                    9 => RyzenSeries.Ryzen9,
                    _ => RyzenSeries.Unknown
                };
            }
        }
        
        return RyzenSeries.Unknown;
    }

    private static RyzenGeneration GetRyzenGeneration(int family)
    {
        return family switch
        {
            23 => RyzenGeneration.Zen1_2,
            25 => RyzenGeneration.Zen3_4,
            26 => RyzenGeneration.Zen5_6,
            _ => RyzenGeneration.Unknown
        };
    }
    
    private static IReadOnlyCollection<string> GetSupportedInstructions()
    {
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
            var manufacturer = GetCpuManufacturer();
            supportedInstructions.Add(manufacturer == Manufacturer.Intel ? "VT-x" : "AMD-V");
        }
        if (Aes.IsSupported) supportedInstructions.Add("AES");
        if (Avx.IsSupported) supportedInstructions.Add("AVX");
        if (Avx2.IsSupported) supportedInstructions.Add("AVX2");
        if (CheckAVX512Support()) supportedInstructions.Add("AVX512");
        if (Fma.IsSupported) supportedInstructions.Add("FMA3");
        
        return supportedInstructions.AsReadOnly();
    }

    private static Manufacturer GetCpuManufacturer()
    {
#if WINDOWS
        using (var processorInfoSearcher =
               new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
        {
            foreach (var cpuInfo in processorInfoSearcher.Get())
            {
                var name = cpuInfo["Name"].ToString().Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase))
                    {
                        return Manufacturer.AMD;
                    }
                    if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase))
                    {
                        return Manufacturer.Intel;
                    }
                }
            }
        }
#elif LINUX
        var procInfo = ReadFromFile("/proc/cpuinfo");
        if (procInfo.Contains("AMD", StringComparison.OrdinalIgnoreCase))
        {
            return Manufacturer.AMD;
        }
        if (procInfo.Contains("Intel", StringComparison.OrdinalIgnoreCase))
        {
            return Manufacturer.Intel;
        }
#endif
        return Manufacturer.Unknown;
    }
    
    private static string ReadFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath).Trim();
            return string.IsNullOrEmpty(content) ? string.Empty : content;
        }
        return string.Empty;
    }

    private static bool IsVirtualizationEnabled()
    {
        if (OperatingSystem.IsWindows())
        {
            using (var processorInfoSearcher =
                   new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
            {
                foreach (var queryObj in processorInfoSearcher.Get())
                {
                    // Check if virtualization is enabled
                    if (queryObj["VirtualizationFirmwareEnabled"] is bool)
                    {
                        return true;
                    }
                }
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            var procInfo = ReadFromFile("/proc/cpuinfo");
            if (procInfo.Contains("svm") || procInfo.Contains("vmx"))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEM64TSupported()
    {
#if WINDOWS
        using (var mo = new ManagementObject("Win32_Processor.DeviceID='CPU0'"))
        {
            var i = (ushort)mo["Architecture"];

            return i == 9;
        }
#elif LINUX 
        var procInfo = ReadFromFile("/proc/cpuinfo");
        if (procInfo.Contains("emt64"))
        {
            return true;
        }
#endif
        return false;
    }

    private static bool CheckAVX512Support()
    {
#if WINDOWS
        try
        {
            return NativeMethods.IsProcessorFeaturePresent(NativeMethods.PF_AVX512F_INSTRUCTIONS_AVAILABLE);
        }
        catch
        {
            return false;
        }
#elif LINUX 
        var procInfo = ReadFromFile("/proc/cpuinfo");
        if (procInfo.Contains("avx512"))
        {
            return true;
        }
#endif
        return false;
    }

    private static bool IsMMXSupported()
    {
#if WINDOWS
        if (Environment.Is64BitProcess)
        {
            // For 64-bit processes, MMX is always supported on Windows.
            return true;
        }
        return NativeMethods.IsProcessorFeaturePresent(NativeMethods.PF_MMX_INSTRUCTIONS_AVAILABLE);
#endif
#if LINUX
        var procInfo = ReadFromFile("/proc/cpuinfo");
        if (procInfo.Contains("mmx"))
        {
            return true;
        }
#endif
        return true;
    }

    #if WINDOWS
    internal static class NativeMethods
    {
        public const int PF_MMX_INSTRUCTIONS_AVAILABLE = 3;
        public const int PF_AVX512F_INSTRUCTIONS_AVAILABLE = 49;

        // Import the GetSystemInfo function (Windows API) to check MMX support.
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        // Helper struct for GetSystemInfo function.
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct SYSTEM_INFO
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
    #endif
}