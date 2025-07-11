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
    
    public string? BigLITTLEInfo { get; set; }

    public int BaseClock { get; set; }
    
    public ProcessorType ProcessorType { get; set; } = ProcessorType.Unknown;
    
    public double L1Size { get; set; }
    public double L2Size { get; set; }
    public double L3Size { get; set; }

    public IReadOnlyCollection<string> SupportedInstructions { get; set; } = ReadOnlyCollection<string>.Empty;
    
    public RyzenFamily RyzenFamily { get; set; } = RyzenFamily.Unknown;
    public RyzenGenerations RyzenGeneration { get; set; } = RyzenGenerations.Unknown;
    public RyzenSeries RyzenSeries { get; set; } = RyzenSeries.Unknown;
}