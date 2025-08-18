using System.Collections.ObjectModel;
using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class RyzenCpuInfo : CpuInfo
{
    public RyzenFamily RyzenFamily { get; set; } = RyzenFamily.Unknown;
    public RyzenGenerations RyzenGeneration { get; set; } = RyzenGenerations.Unknown;
    public RyzenSeries RyzenSeries { get; set; } = RyzenSeries.Unknown;
    
    public RyzenCpuInfo(CpuInfo other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        Name = other.Name;
        Description = other.Description;
        CodeName = other.CodeName;
        Family = other.Family;
        Model = other.Model;
        Stepping = other.Stepping;
        Manufacturer = Manufacturer.AMD;
        CoresCount = other.CoresCount;
        LogicalCoresCount = other.LogicalCoresCount;
        BigLITTLEInfo = other.BigLITTLEInfo;
        BaseClock = other.BaseClock;
        ProcessorType = other.ProcessorType;
        L1Size = other.L1Size;
        L2Size = other.L2Size;
        L3Size = other.L3Size;
        SupportedInstructions = other.SupportedInstructions?.ToList().AsReadOnly() 
                                ?? ReadOnlyCollection<string>.Empty;
    }
}