using System.Collections.ObjectModel;
using ApplicationCore.Enums;
using ApplicationCore.Utilities;

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

    public string BaseClock { get; set; }
    
    public ProcessorType ProcessorType { get; set; } = ProcessorType.Unknown;
    
    public double L1Size { get; set; }
    public double L2Size { get; set; }
    public double L3Size { get; set; }

    public ReadOnlyCollection<string> SupportedInstructions { get; set; } = new(Array.Empty<string>());
    
    public RyzenFamily RyzenFamily { get; set; } = RyzenFamily.Unknown;
    public RyzenGenerations RyzenGeneration { get; set; } = RyzenGenerations.Unknown;
}