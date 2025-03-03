namespace ApplicationCore.Models;

public class IntelMemoryTimings : MemoryTimings
{
    public uint tSTAG { get; set; }
    public uint tSTAG4LR { get; set; }
    public uint tSTAG2LR { get; set; }
    public uint tSTAGLR { get; set; }
    public uint tWRMPR { get; set; }
    public uint tMODPDA { get; set; }
    public uint tMRDPDA { get; set; }
}