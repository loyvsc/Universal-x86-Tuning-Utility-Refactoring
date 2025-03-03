namespace ApplicationCore.Models;

/// <summary>
/// The Mem_Timings class holds each memory timing value and contains
/// a reusable method to retrieve all timings.
/// </summary>
public class RyzenMemoryTimings : MemoryTimings
{
    public bool BGS { get; set; }
    public bool BGSA { get; set; }
    public bool Preamble2T { get; set; }
    public bool GDM { get; set; }
    public int CommandRate { get; set; }
    public uint tRPPB { get; set; }
    public uint tRCPB { get; set; }
    public uint tRRDDLR { get; set; }
    public uint tFAWDLR { get; set; }
    public uint tFAWSLR { get; set; }
    public uint tRCPage { get; set; }
    public uint tRDRDBAN { get; set; }
    public uint tRDRDSCL { get; set; }
    public uint tRDRDSCDLR { get; set; }
    public uint tRDRDSC { get; set; }
    public uint tRDRDSD { get; set; }
    public uint tRDRDDD { get; set; }
    public uint tWRWRBAN { get; set; }
    public uint tWRWRSCL { get; set; }
    public uint tWRWRSCDLR { get; set; }
    public uint tWRWRSC { get; set; }
    public uint tWRWRSD { get; set; }
    public uint tWRWRDD { get; set; }
    public uint tWRRDSCDLR { get; set; }
    public uint tRDWR { get; set; }
    public uint tWRRD { get; set; }
    public uint tMODPDA { get; set; }
    public uint tMRDPDA { get; set; }
    public uint tSTAG { get; set; }
    public uint tPHYWRD { get; set; }
    public uint tPHYRDLAT { get; set; }
    public uint tPHYWRLAT { get; set; }
    public uint tRFC4 { get; set; }
    public uint tRFC4CT { get; set; }
    public uint tRFC2 { get; set; }
    public uint tRFC2CT { get; set; }
    public uint tRFCCT { get; set; }
    public uint tSTAG4LR { get; set; }
    public uint tSTAG2LR { get; set; }
    public uint tSTAGLR { get; set; }
    public uint tWRMPR { get; set; }
    
    public string ProcODT { get; set; }
    public string RttNom { get; set; }
    public string RttWr { get; set; }
    public string RttPark { get; set; }
    public string AddrCmdSetup { get; set; }
    public string CsOdtSetup { get; set; }
    public string CkeSetup { get; set; }
    public string ClkDrvStrength { get; set; }
    public string AddrCmdDrvStrength { get; set; }
    public string CsOdtDrvStrength { get; set; }
    public string CkeDrvStrength { get; set; }
}