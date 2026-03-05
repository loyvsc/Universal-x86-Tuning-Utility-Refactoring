using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class IntelCpuInfo : CpuInfo
{
    public IntelFamily IntelFamily { get; set; }

    public IntelCpuInfo()
    {
        Manufacturer = Manufacturer.Intel;
    }
}