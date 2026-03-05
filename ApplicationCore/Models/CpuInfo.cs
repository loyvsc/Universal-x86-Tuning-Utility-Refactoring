using System.Collections.ObjectModel;
using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class CpuInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string CodeName { get; set; }

    public int Family { get; set; }
    public int Model { get; set; }
    public int Stepping { get; set; }

    public Manufacturer Manufacturer { get; set; }
    
    public int CoresCount { get; set; }
    public int LogicalCoresCount { get; set; }

    public string? BigLITTLEInfo => _bigLittleInfoLazy.Value;

    public int BaseClock { get; set; }

    public ProcessorType ProcessorType => _processorTypeLazy.Value;
    
    public double L1Size { get; set; }
    public double L2Size { get; set; }
    public double L3Size { get; set; }

    public IReadOnlyCollection<string> SupportedInstructions { get; set; } = ReadOnlyCollection<string>.Empty;

    private readonly Lazy<string?> _bigLittleInfoLazy;
    private readonly Lazy<ProcessorType> _processorTypeLazy;

    protected CpuInfo()
    {
        _bigLittleInfoLazy = new Lazy<string?>(() =>
        {
            int bigCores = 0;
            int smallCores = 0;

            if (this is IntelCpuInfo intelInfo)
            {
                if (intelInfo.IntelFamily >= IntelFamily.Alderlake)
                {
                    if (L2Size % 1.25 == 0) bigCores = (int)(L2Size / 1.25);
                    else if (L2Size % 2 == 0) bigCores = (int)(L2Size / 2);

                    smallCores = CoresCount - bigCores;

                    if (smallCores > 0)
                    {
                        if (intelInfo.IntelFamily == IntelFamily.Meteorlake)
                        {
                            return $"{CoresCount} ({bigCores} Performance Cores + {smallCores - 2} Efficiency Cores + 2 LP Efficiency Cores)";
                        }

                        return $"{CoresCount} ({bigCores} Performance Cores + {smallCores} Efficiency Cores)";
                    }
                }
            }
            else if (this is RyzenCpuInfo ryzenInfo)
            {
                if (Name.Contains("7545U") && ryzenInfo.RyzenFamily == RyzenFamily.PhoenixPoint2 ||
                    Name.Contains("Z1") && ryzenInfo.RyzenFamily == RyzenFamily.PhoenixPoint2 ||
                    Name.Contains("7440U"))
                {
                    bigCores = Name.Contains("7440U") ? 0 : 2;
                    smallCores = CoresCount - bigCores;
                    return $"{CoresCount} ({bigCores} Prime Cores + {smallCores} Compact Cores)";
                }
            }

            return null;
        });

        _processorTypeLazy = new Lazy<ProcessorType>(() =>
        {
            if (this is RyzenCpuInfo ryzenCpuInfo)
            {
                return ryzenCpuInfo.RyzenFamily is RyzenFamily.SummitRidge
                    or RyzenFamily.PinnacleRidge
                    or RyzenFamily.Matisse
                    or RyzenFamily.Vermeer
                    or RyzenFamily.Raphael
                    or RyzenFamily.DragonRange
                    or RyzenFamily.GraniteRidge
                    ? ProcessorType.Desktop
                    : ProcessorType.Apu;
            }
            else if (this is IntelCpuInfo intelCpuInfo)
            {
                return intelCpuInfo.Name.Contains('U') ||
                       intelCpuInfo.Name.Contains('H') ||
                       intelCpuInfo.Name.Contains("HQ") ||
                       intelCpuInfo.Name.Contains("HK") ||
                       intelCpuInfo.Name.Contains('U') ||
                       intelCpuInfo.Name.Contains('Y') ||
                       intelCpuInfo.Name.Contains('M') ||
                       intelCpuInfo.Name.Contains("MQ") ||
                       intelCpuInfo.Name.Contains("QM") ||
                       intelCpuInfo.Name.Contains('G') ||
                       intelCpuInfo.Name.Contains('P') ||
                       intelCpuInfo.Name.Contains("EQ") ||
                       intelCpuInfo.Name.Contains('E') 
                    ? ProcessorType.Laptop 
                    : ProcessorType.Desktop;
            }

            return ProcessorType.Unknown;
        });
    }
}