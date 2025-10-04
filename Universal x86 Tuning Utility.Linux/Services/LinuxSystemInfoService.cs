using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Models.LaptopInfo;
using ApplicationCore.Utilities;
using Hardware.Info;
using Serilog;
using Universal_x86_Tuning_Utility.Helpers;

namespace Universal_x86_Tuning_Utility.Linux.Services;

// TODO IMPLEMENT
public class LinuxSystemInfoService : ISystemInfoService
{
    private readonly ILogger _logger;
    
    private readonly List<BasicGpuInfo> _gpus = new List<BasicGpuInfo>();
    private readonly IHardwareInfo _hardwareInfo = new HardwareInfo();
    private readonly IIntelManagementService _intelManagementService;

    public LinuxSystemInfoService(ILogger logger, IIntelManagementService intelManagementService)
    {
        _logger = logger;
        _intelManagementService = intelManagementService;

        _hardwareInfo.RefreshMotherboardList();
        Manufacturer = new Lazy<string>(() =>
        {
            if (_hardwareInfo.MotherboardList.Count != 0)
            {
                return _hardwareInfo.MotherboardList[0].Manufacturer;
            }
            
            return string.Empty;
        });

        Product = new Lazy<string>(() => ReadFromFile("/sys/class/dmi/id/product_name"));

        SystemName = new Lazy<string>(() => ReadFromFile("/etc/hostname"));
        
        Ram = new RamInfo();
        
        ReAnalyzeSystem();
    }
    
    private string ReadFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath).Trim();
            return string.IsNullOrEmpty(content) ? string.Empty : content;
        }
        return string.Empty;
    }
    
    private void InitializeBasicGpuInfo()
    {
        _hardwareInfo.RefreshVideoControllerList();
        foreach (var device in _hardwareInfo.VideoControllerList)
        {
            // if (device["Name"] is string name)
            // {
            //     var gpuName = name.Split(' ');
            //     if (gpuName.Length != 0)
            //     {
            //         if (Enum.TryParse<GpuManufacturer>(gpuName[0], true, out var gpuManufacturer))
            //         {
            //             _gpus.Add(new BasicGpuInfo(gpuManufacturer, name));
            //             continue;
            //         }
            //     
            //         _gpus.Add(new BasicGpuInfo(GpuManufacturer.Unknown, name));
            //     }
            // }
        }
    }
    
    public void ReAnalyzeSystem()
    {
        try
        {
            InitializeBasicGpuInfo();
            
            var processorIdentifier = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");

            var words = processorIdentifier.Split(' ');

            var familyIndex = Array.IndexOf(words, "Family") + 1;
            var modelIndex = Array.IndexOf(words, "Model") + 1;
            var steppingIndex = Array.IndexOf(words, "Stepping") + 1;

            var family = int.Parse(words[familyIndex]);
            var model = int.Parse(words[modelIndex]);
            var stepping = int.Parse(words[steppingIndex].TrimEnd(','));

            _hardwareInfo.RefreshCPUList();
            var firstCpu = _hardwareInfo.CpuList[0];
            
            var name = firstCpu.Name;
            var description = firstCpu.Description;
            var coresCount = Convert.ToInt32(firstCpu.NumberOfCores);
            var logicalCoresCount = Convert.ToInt32(firstCpu.NumberOfLogicalProcessors);
            var baseClock = Convert.ToInt32(firstCpu.MaxClockSpeed);

            var l1Size = GetCacheSize(CacheLevel.L1);
            var l22Size = GetCacheSize(CacheLevel.L2);
            var l3Size = GetCacheSize(CacheLevel.L3);
            
            Cpu = CpuInfoFactory.Create(family: family, 
                model: model, 
                stepping: stepping,
                name: name, 
                description: description, 
                coresCount: coresCount,
                logicalCoresCount: logicalCoresCount,
                maxClockSpeed: baseClock,
                l1Size: l1Size,
                l2Size: l22Size,
                l3Size: l3Size);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when analyzing cpu information");
        }

        if (Cpu is RyzenCpuInfo)
        {
            // Addresses.SetAddresses(Cpu);
        }
        else if (Cpu is IntelCpuInfo)
        {
            _intelManagementService.DetermineCpu();
        }

        try
        {
            AnalyzeRam();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when analyzing ram information");
        }

        LaptopInfo = LaptopInfoFactory.Create(Manufacturer.Value.ToLower(), Product.Value.ToLower());
    }
    
    private void AnalyzeRam()
    {
        int type = 0;
        int width = 0;
        List<RamModule> modules = [];
        
        _hardwareInfo.RefreshMemoryList();
        foreach (var memoryInfo in _hardwareInfo.MemoryList)
        {
            var module = new RamModule()
            {
                Producer = memoryInfo.Manufacturer,
                Model = memoryInfo.PartNumber,
                Capacity = Convert.ToDouble(memoryInfo.Capacity / 1073741824), // 1073741824 - gigabyte in bytes
                Speed = Convert.ToInt32(memoryInfo.Speed),
                // Type = memoryInfo.Type,
                // Width = memoryInfo.Width
            };
            modules.Add(module);
        }

        if (width > 128 && Cpu is RyzenCpuInfo ryzenCpuInfo && ryzenCpuInfo.RyzenFamily != RyzenFamily.Unknown)
        {
            switch (ryzenCpuInfo.RyzenFamily)
            {
                case RyzenFamily.StrixHalo:
                {
                    if (width > 256)
                    {
                        width = 256;
                    }
                    break;
                }
                case RyzenFamily.Mendocino:
                {
                    width = 64;
                    break;
                }
                default:
                {
                    if (ryzenCpuInfo.RyzenFamily < RyzenFamily.StrixPoint2)
                    {
                        width = 128;
                    }
                    break;
                }
            }
        }

        if (modules.Count != 0)
        {
            Ram.Speed = modules[0].Speed;
            Ram.Modules = modules.ToArray();
            Ram.Capacity = modules.Sum(module => module.Capacity);
        }
        
        Ram.Width = width;

        // Ram.Timings = Cpu.Manufacturer == ApplicationCore.Enums.Manufacturer.AMD 
        //     ? GetRyzenTimings()
        //     : GetIntelTimings();
        
        Ram.Type = type switch
        {
            20 => RamType.DDR,
            21 => RamType.DDR2,
            24 => RamType.DDR3,
            26 => RamType.DDR4,
            30 => RamType.LPDDR4,
            34 => RamType.DDR5,
            35 => RamType.LPDDR5,
            _ => RamType.Unknown
        };
    }
    
    private RyzenMemoryTimings GetRyzenTimings()
    {
        var ryzenMemory = new RyzenMemoryTimings();
    
        if (_linuxMsrReader.TryOpen())
            throw new ApplicationException($"Initialization error");
    
        uint eax = 0, ebx = 0, ecx = 0, edx = 0;
        // ols.CpuidPx(0x80000001, ref eax, ref ebx, ref ecx, ref edx, (UIntPtr)0x01);
        uint CPUFMS = eax & 0xFFFF00;
    
        // uint SMUORG = ols.ReadPciConfigDword(0x00, 0xB8);
    
        uint someOffset = ReadDword(0x50200) == 0x300 ? 0x100000u : 0u;
    
        uint BGS = ReadDword(0x00050058 + someOffset);
        uint BGSA = ReadDword(0x000500D0 + someOffset);
        uint DramConfiguration = ReadDword(0x00050200 + someOffset);
    
        uint DramTiming1 = ReadDword(0x00050204 + someOffset);
        uint DramTiming2 = ReadDword(0x00050208 + someOffset);
        uint DramTiming3 = ReadDword(0x0005020C + someOffset);
        uint DramTiming4 = ReadDword(0x00050210 + someOffset);
        uint DramTiming5 = ReadDword(0x00050214 + someOffset);
        uint DramTiming6 = ReadDword(0x00050218 + someOffset);
        uint DramTiming7 = ReadDword(0x0005021C + someOffset);
        uint DramTiming8 = ReadDword(0x00050220 + someOffset);
        uint DramTiming9 = ReadDword(0x00050224 + someOffset);
        uint DramTiming10 = ReadDword(0x00050228 + someOffset);
        uint DramTiming12 = ReadDword(0x00050230 + someOffset);
        uint DramTiming13 = ReadDword(0x00050234 + someOffset);
        uint DramTiming20 = ReadDword(0x00050250 + someOffset);
        uint DramTiming21 = ReadDword(0x00050254 + someOffset);
        uint DramTiming22 = ReadDword(0x00050258 + someOffset);
    
        uint tRFCTiming0 = ReadDword(0x00050260 + someOffset);
        uint tRFCTiming1 = ReadDword(0x00050264 + someOffset);
        uint tSTAGTiming0 = ReadDword(0x00050270 + someOffset);
        uint tSTAGTiming1 = ReadDword(0x00050274 + someOffset);
        uint DramTiming35 = ReadDword(0x0005028C + someOffset);
    
        uint tRFCTiming, tSTAGTiming;
        if (tRFCTiming0 == tRFCTiming1)
        {
            tRFCTiming = tRFCTiming0;
            tSTAGTiming = tSTAGTiming0;
        }
        else if (tRFCTiming0 == 0x21060138)
        {
            tRFCTiming = tRFCTiming1;
            tSTAGTiming = tSTAGTiming1;
        }
        else
        {
            tRFCTiming = tRFCTiming0;
            tSTAGTiming = tSTAGTiming0;
        }
    
        ryzenMemory.BGS = (BGS != 0x87654321);
        ryzenMemory.BGSA = (BGSA == 0x111107F1);
        ryzenMemory.Preamble2T = ((DramConfiguration & 0x1000) >> 12) != 0;
        ryzenMemory.GDM = ((DramConfiguration & 0x800) >> 11) != 0;
        ryzenMemory.CommandRate = ((DramConfiguration & 0x400) >> 10) != 0 ? 2 : 1;
        var memClock = DramConfiguration & 0x7F;
        float MEMCLKTRxx = memClock / 3.0f * 100;
    
        ryzenMemory.tRCDWR = (DramTiming1 & 0x3F000000) >> 24;
        ryzenMemory.tRCDRD = (DramTiming1 & 0x3F0000) >> 16;
        ryzenMemory.tRAS = (DramTiming1 & 0x7F00) >> 8;
        ryzenMemory.tCL = DramTiming1 & 0x3F;
    
        ryzenMemory.tRPPB = (DramTiming2 & 0x3F000000) >> 24;
        ryzenMemory.tRP = (DramTiming2 & 0x3F0000) >> 16;
        ryzenMemory.tRCPB = (DramTiming2 & 0xFF00) >> 8;
        ryzenMemory.tRC = DramTiming2 & 0xFF;
    
        ryzenMemory.tRTP = (DramTiming3 & 0x1F000000) >> 24;
        ryzenMemory.tRRDDLR = (DramTiming3 & 0x1F0000) >> 16;
        ryzenMemory.tRRDL = (DramTiming3 & 0x1F00) >> 8;
        ryzenMemory.tRRDS = DramTiming3 & 0x1F;
    
        ryzenMemory.tFAWDLR = (DramTiming4 & 0x7E000000) >> 25;
        ryzenMemory.tFAWSLR = (DramTiming4 & 0xFC0000) >> 18;
        ryzenMemory.tFAW = DramTiming4 & 0x7F;
    
        ryzenMemory.tWTRL = (DramTiming5 & 0x7F0000) >> 16;
        ryzenMemory.tWTRS = (DramTiming5 & 0x1F00) >> 8;
        ryzenMemory.tCWL = DramTiming5 & 0x3F;
    
        ryzenMemory.tWR = DramTiming6 & 0x7F;
    
        ryzenMemory.tRCPage = (DramTiming7 & 0xFFF00000) >> 20;
    
        ryzenMemory.tRDRDBAN = (DramTiming8 & 0xC0000000) >> 30;
        ryzenMemory.tRDRDSCL = (DramTiming8 & 0x3F000000) >> 24;
        ryzenMemory.tRDRDSCDLR = (DramTiming8 & 0xF00000) >> 20;
        ryzenMemory.tRDRDSC = (DramTiming8 & 0xF0000) >> 16;
        ryzenMemory.tRDRDSD = (DramTiming8 & 0xF00) >> 8;
        ryzenMemory.tRDRDDD = DramTiming8 & 0xF;
    
        ryzenMemory.tWRWRBAN = (DramTiming9 & 0xC0000000) >> 30;
        ryzenMemory.tWRWRSCL = (DramTiming9 & 0x3F000000) >> 24;
        ryzenMemory.tWRWRSCDLR = (DramTiming9 & 0xF00000) >> 20;
        ryzenMemory.tWRWRSC = (DramTiming9 & 0xF0000) >> 16;
        ryzenMemory.tWRWRSD = (DramTiming9 & 0xF00) >> 8;
        ryzenMemory.tWRWRDD = DramTiming9 & 0xF;
    
        ryzenMemory.tWRRDSCDLR = (DramTiming10 & 0x1F0000) >> 16;
        ryzenMemory.tRDWR = (DramTiming10 & 0x1F00) >> 8;
        ryzenMemory.tWRRD = DramTiming10 & 0xF;
    
        ryzenMemory.tREF = DramTiming12 & 0xFFFF;
        ryzenMemory.tREFCT = (uint)(1000 / MEMCLKTRxx * ryzenMemory.tREF);
    
        ryzenMemory.tMODPDA = (DramTiming13 & 0x3F000000) >> 24;
        ryzenMemory.tMRDPDA = (DramTiming13 & 0x3F0000) >> 16;
        ryzenMemory.tMOD = (DramTiming13 & 0x3F00) >> 8;
        ryzenMemory.tMRD = DramTiming13 & 0x3F;
    
        ryzenMemory.tSTAG = (DramTiming20 & 0xFF0000) >> 16;
    
        ryzenMemory.tCKE = (DramTiming21 & 0x1F000000) >> 24;
    
        ryzenMemory.tPHYWRD = (DramTiming22 & 0x7000000) >> 24;
        ryzenMemory.tPHYRDLAT = (DramTiming22 & 0x3F0000) >> 16;
        ryzenMemory.tPHYWRLAT = (DramTiming22 & 0x1F00) >> 8;
        ryzenMemory.tRDDATA = DramTiming22 & 0x7F;
    
        ryzenMemory.tRFC4 = (tRFCTiming & 0xFFC00000) >> 22;
        ryzenMemory.tRFC4CT = (uint)(1000 / MEMCLKTRxx * ryzenMemory.tRFC4);
    
        ryzenMemory.tRFC2 = (tRFCTiming & 0x3FF800) >> 11;
        ryzenMemory.tRFC2CT = (uint)(1000 / MEMCLKTRxx * ryzenMemory.tRFC2);
    
        ryzenMemory.tRFC = tRFCTiming & 0x7FF;
        ryzenMemory.tRFCCT = (uint)(1000 / MEMCLKTRxx * ryzenMemory.tRFC);
    
        ryzenMemory.tSTAG4LR = (tSTAGTiming & 0x1FF00000) >> 20;
        ryzenMemory.tSTAG2LR = (tSTAGTiming & 0x7FC00) >> 10;
        ryzenMemory.tSTAGLR = tSTAGTiming & 0x1FF;
    
        ryzenMemory.tWRMPR = (DramTiming35 & 0x3F000000) >> 24;
    
        uint eax2 = 0, ebx2 = 0, ecx2 = 0, edx2 = 0;
        // ols.CpuidPx(0x80000001, ref eax2, ref ebx2, ref ecx2, ref edx2, (UIntPtr)0x01);
        eax2 &= 0xFFFF00;
        ebx2 = (ebx2 & 0xF0000000) >> 28;
        uint someOffset2 = 0;
        if (ebx2 == 7)
            someOffset2 = 0x2180;
        else if (ebx2 == 2)
            someOffset2 = 0x100;
        else
            someOffset2 = 0x00;
    
        // ols.WritePciConfigDword(0x0, 0xB8, 0x3B10528);
        // ols.WritePciConfigDword(0x0, 0xBC, 0x02);
        // ols.WritePciConfigDword(0x00, 0xB8, SMUORG);
        
        return ryzenMemory;
    }
    
    private IntelMemoryTimings GetIntelTimings()
    {
        var intelTimings = new IntelMemoryTimings();
        int SMUDelay = 10;
    
        if (_linuxMsrReader.TryOpen()) 
            throw new ApplicationException("Initialization error");
    
        uint someOffset = ReadDword(0x50200) == 0x300 ? 0x100000u : 0u;
        
        uint DramConfiguration = ReadDword(0x00050200 + someOffset);
    
        uint DramTiming1 = ReadDword(0x00050204 + someOffset);
        uint DramTiming2 = ReadDword(0x00050208 + someOffset);
        uint DramTiming3 = ReadDword(0x0005020C + someOffset);
        uint DramTiming4 = ReadDword(0x00050210 + someOffset);
        uint DramTiming5 = ReadDword(0x00050214 + someOffset);
        uint DramTiming6 = ReadDword(0x00050218 + someOffset);
        uint DramTiming12 = ReadDword(0x00050230 + someOffset);
        uint DramTiming13 = ReadDword(0x00050234 + someOffset);
        uint DramTiming20 = ReadDword(0x00050250 + someOffset);
        uint DramTiming21 = ReadDword(0x00050254 + someOffset);
        uint DramTiming22 = ReadDword(0x00050258 + someOffset);
    
        intelTimings.tRCDWR = (DramTiming1 & 0x3F000000) >> 24;
        intelTimings.tRCDRD = (DramTiming1 & 0x3F0000) >> 16;
        intelTimings.tRAS = (DramTiming1 & 0x7F00) >> 8;
        intelTimings.tCL = DramTiming1 & 0x3F;
    
        intelTimings.tRP = (DramTiming2 & 0x3F0000) >> 16;
        intelTimings.tRC = DramTiming2 & 0xFF;
    
        intelTimings.tRTP = (DramTiming3 & 0x1F000000) >> 24;
        intelTimings.tRRDL = (DramTiming3 & 0x1F00) >> 8;
        intelTimings.tRRDS = DramTiming3 & 0x1F;
    
        intelTimings.tFAW = DramTiming4 & 0x7F;
    
        intelTimings.tWTRL = (DramTiming5 & 0x7F0000) >> 16;
        intelTimings.tWTRS = (DramTiming5 & 0x1F00) >> 8;
        intelTimings.tCWL = DramTiming5 & 0x3F;
    
        intelTimings.tWR = DramTiming6 & 0x7F;
    
        intelTimings.tREF = DramTiming12 & 0xFFFF;
        
        var memClock = DramConfiguration & 0x7F;
        float MEMCLKTRxx = memClock / 3.0f * 100;
        intelTimings.tREFCT = (uint)(1000 / MEMCLKTRxx * intelTimings.tREF);
    
        intelTimings.tMODPDA = (DramTiming13 & 0x3F000000) >> 24;
        intelTimings.tMRDPDA = (DramTiming13 & 0x3F0000) >> 16;
        intelTimings.tMOD = (DramTiming13 & 0x3F00) >> 8;
        intelTimings.tMRD = DramTiming13 & 0x3F;
    
        intelTimings.tSTAG = (DramTiming20 & 0xFF0000) >> 16;
    
        intelTimings.tCKE = (DramTiming21 & 0x1F000000) >> 24;
    
        intelTimings.tRDDATA = DramTiming22 & 0x7F;
    
        uint tRFCTiming0 = ReadDword(0x00050260 + someOffset);
        uint tRFCTiming1 = ReadDword(0x00050264 + someOffset);
    
        uint tRFCTiming;
        if (tRFCTiming0 == tRFCTiming1)
        {
            tRFCTiming = tRFCTiming0;
        }
        else if (tRFCTiming0 == 0x21060138)
        {
            tRFCTiming = tRFCTiming1;
        }
        else
        {
            tRFCTiming = tRFCTiming0;
        }
        
        intelTimings.tRFC = tRFCTiming & 0x7FF;

        return intelTimings;
    }
    
    private uint ReadDword(uint address)
    {
        if (_linuxMsrReader.TryOpen())
        {
            return _linuxMsrReader.Read(address);
        }

        return 0;
    }

    private readonly LinuxMsrReader _linuxMsrReader = new LinuxMsrReader();

    private uint GetCacheSize(ApplicationCore.Enums.CacheLevel level)
    {
        var firstCpu = _hardwareInfo.CpuList[0];
        
        return level switch
        {
            ApplicationCore.Enums.CacheLevel.L1 => firstCpu.L1DataCacheSize,
            ApplicationCore.Enums.CacheLevel.L2 => firstCpu.L2CacheSize,
            ApplicationCore.Enums.CacheLevel.L3 => firstCpu.L3CacheSize,
        };
    }

    public CpuInfo Cpu { get; private set; }
    public RamInfo Ram { get; private set; }
    public LaptopInfoBase? LaptopInfo { get; private set; }
    public IReadOnlyCollection<BasicGpuInfo> Gpus { get; private set; }
    public Lazy<string> Manufacturer { get; }
    public Lazy<string> Product { get; }
    public Lazy<string> SystemName { get; }
}

public class LinuxMsrReader : IDisposable
{
    private FileStream? _stream;
    private const string Path = "/dev/cpu/{0}/msr";
    private readonly byte[] _buffer = new byte[8];

    public bool TryOpen(int cpuId = 0)
    {
        try
        {
            if (_stream == null || !_stream.CanRead)
            {
                _stream = new FileStream(string.Format(Path, cpuId), FileMode.Open, FileAccess.Read);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public uint Read(ulong msrAddr)
    {
        if (_stream == null)
            throw new Exception("Stream not opened");
        
        _stream.Seek((long)msrAddr, SeekOrigin.Begin);

        try
        {
            int read = _stream.Read(_buffer, 0, 8);
            if (read != 8)
                throw new InvalidDataException();
            return BitConverter.ToUInt32(_buffer, 0);
        }
        finally
        {
            Array.Clear(_buffer);
        }
    }
    
    public void Dispose()
    {
        _stream?.Dispose();
    }
}