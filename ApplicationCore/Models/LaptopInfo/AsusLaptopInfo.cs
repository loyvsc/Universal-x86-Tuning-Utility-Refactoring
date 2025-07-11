using ApplicationCore.Enums;
using ApplicationCore.Enums.Laptop;

namespace ApplicationCore.Models.LaptopInfo;

public class AsusLaptopInfo : LaptopInfoBase
{
    public AsusLaptopSeries LaptopSeries { get; }

    public AsusLaptopInfo(AsusLaptopSeries laptopSeries)
    {
        Brand = LaptopBrand.ASUS;
        LaptopSeries = laptopSeries;
    }
}