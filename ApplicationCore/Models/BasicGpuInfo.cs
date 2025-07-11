using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class BasicGpuInfo
{
    public GpuManufacturer Manufacturer { get; }
    public string Name { get; }

    public BasicGpuInfo(GpuManufacturer manufacturer, string name)
    {
        Manufacturer = manufacturer;
        Name = name;
    }
}