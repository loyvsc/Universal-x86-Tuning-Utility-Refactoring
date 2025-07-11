using ApplicationCore.Enums;
using ApplicationCore.Enums.Laptop;

namespace ApplicationCore.Models.LaptopInfo;

public abstract class LaptopInfoBase
{
    public LaptopBrand Brand { get; init; }
}