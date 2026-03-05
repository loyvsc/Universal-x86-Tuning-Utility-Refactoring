using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class RamInfo
{
    public RamType Type { get; set; }
    public int Width { get; set; }
    public double Capacity { get; set; }
    public double Speed { get; set; }
    public MemoryTimings Timings { get; set; }
    
    public ICollection<RamModule> Modules { get; set; }

    public RamInfo()
    {
        Timings = new MemoryTimings();
        Modules = new List<RamModule>();
    }
}