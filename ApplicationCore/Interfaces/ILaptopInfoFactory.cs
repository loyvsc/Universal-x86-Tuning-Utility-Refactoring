using ApplicationCore.Models.LaptopInfo;

namespace ApplicationCore.Interfaces;

public interface ILaptopInfoFactory
{
    public LaptopInfoBase? Create();
}