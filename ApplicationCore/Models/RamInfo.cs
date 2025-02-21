using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class RamInfo
{
    public RamType Type { get; set; }
    public int Width { get; set; }
    public int CountOfModules { get; set; }
    public double Capacity { get; set; }
    public double Speed { get; set; }
    
    public RamModule[] Modules { get; set; }
}