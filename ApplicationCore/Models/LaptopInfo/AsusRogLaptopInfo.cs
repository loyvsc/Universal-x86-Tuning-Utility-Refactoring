using ApplicationCore.Enums;
using ApplicationCore.Enums.Laptop;

namespace ApplicationCore.Models.LaptopInfo;

public class AsusRogLaptopInfo : AsusLaptopInfo
{
    public AsusRogSeries RogSeries { get; }

    public AsusRogLaptopInfo(AsusRogSeries rogSeries) : base(AsusLaptopSeries.ROG)
    {
        RogSeries = rogSeries;
    }
}