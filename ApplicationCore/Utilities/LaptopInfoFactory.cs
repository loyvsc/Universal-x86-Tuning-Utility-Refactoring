using ApplicationCore.Enums;
using ApplicationCore.Enums.Laptop;
using ApplicationCore.Models.LaptopInfo;

namespace ApplicationCore.Utilities;

public static class LaptopInfoFactory
{
    public static LaptopInfoBase? Create(string manufacturer, string product)
    {
        if (product.Contains("laptop"))
        {
            if (manufacturer.Contains("asus"))
            {
                if (product.Contains("rog"))
                {
                    var rogSeries = AsusRogSeries.Basic;
                    if (product.Contains("ally"))
                    {
                        rogSeries = AsusRogSeries.Ally;
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
            return new PortableConsoleInfo(portableConsoleManufacturer);
        }

        return null;
    }
    
    private static PortableConsoleManufacturer GetPortableConsoleManufacturer(string manufacturer)
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