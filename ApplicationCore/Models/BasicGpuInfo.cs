using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class BasicGpuInfo
{
    public uint? Id { get; }
    public GpuManufacturer Manufacturer { get; }
    public string Name { get; }
    public int? RopCount { get; }
    public int? TmusCount { get; }
    public int? ShadersCount { get; }
    
    /// <summary>
    /// In megabytes
    /// </summary>
    public int? MemorySize { get; }

    public BasicGpuInfo(GpuManufacturer manufacturer, string name)
    {
        Manufacturer = manufacturer;
        Name = name;
    }

    public BasicGpuInfo(uint id, GpuManufacturer manufacturer, string name, int ropCount, int tmusCount, int shadersCount, int memorySize)
    {
        Id = id;
        Manufacturer = manufacturer;
        Name = name;
        RopCount = ropCount;
        TmusCount = tmusCount;
        ShadersCount = shadersCount;
        MemorySize = memorySize;
    }
}