using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ApplicationCore.Models.LaptopInfo;

public partial class FrameworkLaptopInfo : LaptopInfoBase
{
    [AllowedValues(12, 13, 16)]
    public int LaptopSeries { get; }
    public string CpuSeries { get; }
    
    public FrameworkLaptopInfo(int laptopSeries, string cpuSeries)
    {
        LaptopSeries = laptopSeries;
        CpuSeries = cpuSeries;
    }

    public static FrameworkLaptopInfo Parse(string productInfo)
    {
        if (string.IsNullOrWhiteSpace(productInfo))
            throw new ArgumentNullException(nameof(productInfo));

        var cpuSeries = CpuSeriesRegex().Match(productInfo).Value
            .Replace("amd", "", StringComparison.InvariantCultureIgnoreCase)
            .Replace("ryzen", "", StringComparison.InvariantCultureIgnoreCase)
            .Trim();

        var data = productInfo.Trim('(', ')', ' ').Split(' ');

        if (data.Length < 2)
            throw new FormatException();

        var laptopSeries = data[1];
        
        if (int.TryParse(laptopSeries, out var series))
        {
            return new FrameworkLaptopInfo(series, cpuSeries);
        }

        throw new FormatException();
    }

    public static bool TryPrase(string productInfo, out FrameworkLaptopInfo? result)
    {
        try
        {
            result = Parse(productInfo);
        }
        catch
        {
            result = null;
        }
        
        return result != null;
    }

    [GeneratedRegex(@"\[(.*?)\]")]
    private static partial Regex CpuSeriesRegex();
}