using System.Collections.ObjectModel;
using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class IntelCpuInfo : CpuInfo
{
    public IntelFamily IntelFamily { get; set; } = IntelFamily.Unknown;

    public IntelCpuInfo(CpuInfo other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        Name = other.Name;
        Description = other.Description;
        CodeName = other.CodeName;
        Family = other.Family;
        Model = other.Model;
        Stepping = other.Stepping;
        Manufacturer = Manufacturer.Intel;
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