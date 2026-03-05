using ApplicationCore.Enums;

namespace ApplicationCore.Models.LaptopInfo;

public class PortableConsoleInfo : LaptopInfoBase
{
    public PortableConsoleManufacturer Manufacturer { get; set; }

    public PortableConsoleInfo(PortableConsoleManufacturer manufacturer)
    {
        Manufacturer = manufacturer;
    }
}