using ApplicationCore.Enums;
using ApplicationCore.Enums.Laptop;
using ApplicationCore.Extensions;
using ApplicationCore.Interfaces;
using ApplicationCore.Models.LaptopInfo;

namespace ApplicationCore.Utilities;

public class LaptopInfoFactory : ILaptopInfoFactory
{
    private readonly IDeviceManagerService _deviceManagerService;
    private readonly ISystemInfoService _systemInfoService;

    public LaptopInfoFactory(ISystemInfoService systemInfoService, IDeviceManagerService deviceManagerService)
    {
        _deviceManagerService = deviceManagerService;
        _systemInfoService = systemInfoService;
    }
    
    public LaptopInfoBase? Create()
    {
        var manufacturer = _systemInfoService.Manufacturer.Trim().ToLower();
        if (_systemInfoService.ChassisType.IsLaptop())
        {
            var product = _systemInfoService.Product.Trim().ToLower();
            if (manufacturer.Contains("asus"))
            {
                if (product.Contains("rog"))
                {
                    var rogSeries = AsusRogSeries.Basic;
                    if (product.Contains("ally"))
                    {
                        return new PortableConsoleInfo(PortableConsoleManufacturer.Asus);
                    }
                    else if (product.Contains("flow"))
                    {
                        rogSeries = AsusRogSeries.Flow;
                    }

                    return new AsusRogLaptopInfo(rogSeries);
                }

                if (product.Contains("tuf"))
                {
                    return new AsusLaptopInfo(AsusLaptopSeries.TUF);
                }
                if (product.Contains("vivobook"))
                {
                    return new AsusLaptopInfo(AsusLaptopSeries.VivoBook);
                }
                if (product.Contains("zenbook"))
                {
                    return new AsusLaptopInfo(AsusLaptopSeries.ZenBook);
                }
            }
            else if (manufacturer.Contains("framework") && FrameworkLaptopInfo.TryPrase(product, out var laptopInfo))
            {
                return laptopInfo;
            }
            
            return new BasicLaptopInfo();
        }

        var portableConsoleManufacturer = GetPortableConsoleManufacturer(manufacturer);
        
        if (portableConsoleManufacturer != PortableConsoleManufacturer.Unknown)
        {
            switch (portableConsoleManufacturer)
            {
                case PortableConsoleManufacturer.Valve:
                {
                    if (_deviceManagerService.Contains(x =>
                            x.DeviceID.Contains("VID_28DE") || x.DeviceID.Contains("Valve")))
                    {
                        return new PortableConsoleInfo(portableConsoleManufacturer);
                    }
                    break;
                }
                case PortableConsoleManufacturer.Gpd:
                {
                    break;
                }
                default: return new PortableConsoleInfo(portableConsoleManufacturer);
            }
        }

        return null;
    }
    
    private PortableConsoleManufacturer GetPortableConsoleManufacturer(string manufacturer)
    {
        var manufacturerValues = manufacturer.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var value in manufacturerValues)
        {
            if (Enum.TryParse(value, true, out PortableConsoleManufacturer consoleManufacturer))
            {
                return consoleManufacturer;
            }
        }

        return PortableConsoleManufacturer.Unknown;
    }
}