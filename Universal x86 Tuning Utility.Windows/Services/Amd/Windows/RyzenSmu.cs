using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Enums;
using ApplicationCore.Models;

[assembly: CLSCompliant(false)]

namespace Universal_x86_Tuning_Utility.Windows.Services.Amd.Windows;

public static class Addresses
{
    private static RyzenCpuInfo _cpuInfo;
    
    public static void SetAddresses(CpuInfo cpuInfo)
    {
        if (cpuInfo is RyzenCpuInfo ryzenCpuInfo)
        {
            _cpuInfo = ryzenCpuInfo;
        
            Smu.SMU_PCI_ADDR = 0x00000000;
            Smu.SMU_OFFSET_ADDR = 0xB8;
            Smu.SMU_OFFSET_DATA = 0xBC;
            
            switch (ryzenCpuInfo.RyzenFamily)
            {
                case RyzenFamily.SummitRidge or RyzenFamily.PinnacleRidge:
                {
                    Socket_AM4_V1();
                    break;
                }
                case RyzenFamily.RavenRidge 
                    or RyzenFamily.Picasso 
                    or RyzenFamily.Dali 
                    or RyzenFamily.Pollock 
                    or RyzenFamily.FireFlight:
                {
                    Socket_FT5_FP5_AM4();
                    break;
                }
                case RyzenFamily.Matisse or RyzenFamily.Vermeer:
                {
                    Socket_AM4_V2();
                    break;
                }
                case RyzenFamily.Renoir
                    or RyzenFamily.Lucienne
                    or RyzenFamily.Cezanne
                    or RyzenFamily.Barcelo:
                {
                    Socket_FP6_AM4();
                    break;
                }
                case RyzenFamily.VanGogh:
                {
                    Socket_FF3();
                    break;
                }
                case RyzenFamily.Mendocino
                    or RyzenFamily.Rembrandt
                    or RyzenFamily.PhoenixPoint
                    or RyzenFamily.PhoenixPoint2
                    or RyzenFamily.HawkPoint
                    or RyzenFamily.StrixPoint
                    or RyzenFamily.StrixPoint2 
                    or RyzenFamily.StrixHalo:
                {
                    Socket_FT6_FP7_FP8();
                    break;
                }
                case RyzenFamily.Raphael
                    or RyzenFamily.DragonRange
                    or RyzenFamily.GraniteRidge:
                {
                    Socket_AM5_V1();
                    break;
                }
            }
            
            WindowsSMUCommands.RyzenAccess.Initialize();
        }
    }

    private static void Socket_FT5_FP5_AM4()
    {
        Smu.MP1_ADDR_MSG = 0x3B10528;
        Smu.MP1_ADDR_RSP = 0x3B10564;
        Smu.MP1_ADDR_ARG = 0x3B10998;

        Smu.PSMU_ADDR_MSG = 0x03B10A20;
        Smu.PSMU_ADDR_RSP = 0x03B10A80;
        Smu.PSMU_ADDR_ARG = 0x03B10A88;

        WindowsSMUCommands.Commands = new List<(string, bool, uint)>
        {
            // Store the commands
            ("stapm-limit",true, 0x1a), // Use MP1 address
            ("stapm-time",true , 0x1e), 
            ("fast-limit",true , 0x1b),
            ("slow-limit",true , 0x1c),
            ("slow-time",true , 0x1d),
            ("tctl-temp",true , 0x1f),
            ("cHTC-temp",false , 0x56), // Use RSMU address
            ("vrm-current",true , 0x20),
            ("vrmmax-current",true , 0x22),
            ("vrmsoc-current",true , 0x21),
            ("vrmsocmax-current",true , 0x23),
            ("prochot-deassertion-ramp",true , 0x25),
            ("pbo-scalar",false , 0x68),
            ("power-saving",true , 0x19),
            ("max-performance",true , 0x18),
            ("oc-clk",false , 0x7d),
            ("per-core-oc-clk",false , 0x7e),
            ("oc-volt",false , 0x7f),
            ("enable-oc",false , 0x69),
            ("disable-oc",false , 0x6a),
            ("max-cpuclk",true, 0x44),
            ("min-cpuclk",true, 0x45),
            ("max-gfxclk",true, 0x46),
            ("min-gfxclk",true, 0x47),
            ("max-socclk-frequency",true, 0x48),
            ("min-socclk-frequency",true, 0x49),
            ("max-fclk-frequency",true, 0x4a),
            ("min-fclk-frequency",true, 0x4b),
            ("max-vcn",true, 0x4c),
            ("min-vcn",true, 0x4d),
            ("max-lclk",true, 0x4e),
            ("min-lclk",true, 0x4f),
        };
    }

    private static void Socket_FP6_AM4()
    {
        Smu.MP1_ADDR_MSG = 0x3B10528;
        Smu.MP1_ADDR_RSP = 0x3B10564;
        Smu.MP1_ADDR_ARG = 0x3B10998;

        Smu.PSMU_ADDR_MSG = 0x03B10A20;
        Smu.PSMU_ADDR_RSP = 0x03B10A80;
        Smu.PSMU_ADDR_ARG = 0x03B10A88;

        WindowsSMUCommands.Commands = new List<(string, bool, uint)>
        {
            // Store the commands
            ("stapm-limit",true , 0x14), // Use MP1 address
            //("stapm-limit",false , 0x31), // Use RSMU address
            ("ppt-limit",false , 0x33),
            ("stapm-time",true , 0x18),
            ("fast-limit",true , 0x15),
            ("slow-limit",true , 0x16),
            ("slow-time",true , 0x17),
            ("tctl-temp",true , 0x19),
            ("cHTC-temp",false , 0x37),
            ("apu-skin-temp",true , 0x38),
            ("vrm-current",true , 0x1a),
            ("vrmmax-current",true , 0x1c),
            ("vrmsoc-current",true , 0x1b),
            ("vrmsocmax-current",true , 0x1d),
            ("prochot-deassertion-ramp",true , 0x20),
            ("gfx-clk",false , 0x89),
            ("dgpu-skin-temp",true , 0x37),
            ("power-saving",true , 0x12),
            ("max-performance",true , 0x11),
            ("pbo-scalar",false , 0x3F),
            ("oc-clk",false , 0x19),
            ("oc-clk",true , 0x31),
            ("per-core-oc-clk",false , 0x1a),
            ("per-core-oc-clk",true , 0x32),
            ("oc-volt",false , 0x1b),
            ("oc-volt",true , 0x33),
            ("set-coall",true , 0x55),
            ("set-coall",false , 0xB1),
            ("set-coper",true , 0x54),
            ("set-cogfx",true , 0x64),
            ("set-cogfx",false , 0x57),
            ("enable-oc",false , 0x17),
            ("enable-oc",true , 0x2f),
            ("disable-oc",false , 0x18),
            ("disable-oc",true , 0x30)
        };
    }

    private static void Socket_FT6_FP7_FP8()
    {
        if(_cpuInfo.RyzenFamily >= RyzenFamily.StrixPoint)
        {
            Smu.MP1_ADDR_MSG = 0x3b10928;
            Smu.MP1_ADDR_RSP = 0x3b10978;
            Smu.MP1_ADDR_ARG = 0x3b10998;

            Smu.PSMU_ADDR_MSG = 0x03B10a20;
            Smu.PSMU_ADDR_RSP = 0x03B10a80;
            Smu.PSMU_ADDR_ARG = 0x03B10a88;
        }
        else 
        {
            Smu.MP1_ADDR_MSG = 0x3B10528;
            Smu.MP1_ADDR_RSP = 0x3B10578;
            Smu.MP1_ADDR_ARG = 0x3B10998;

            Smu.PSMU_ADDR_MSG = 0x03B10a20;
            Smu.PSMU_ADDR_RSP = 0x03B10a80;
            Smu.PSMU_ADDR_ARG = 0x03B10a88;
        }

        WindowsSMUCommands.Commands = new List<(string, bool, uint)>
        {
            // Store the commands
            ("stapm-limit", true, 0x14), // Use MP1 address
            //("stapm-limit", false, 0x31), // Use RSMU address
            ("stapm-time", true, 0x18),
            ("fast-limit", true, 0x15),
            ("fast-limit", false, 0x32),
            ("slow-limit", true, 0x16),
            ("slow-limit", false, 0x33),
            ("slow-limit", false, 0x34),
            ("slow-time", true, 0x17),
            ("tctl-temp", true, 0x19),
            ("cHTC-temp", false, 0x37),
            ("apu-skin-temp", true, 0x33),
            ("vrm-current", true, 0x1a),
            ("vrmmax-current", true, 0x1c),
            ("vrmsoc-current", true, 0x1b),
            ("vrmsocmax-current", true ,0x1d),
            ("prochot-deassertion-ramp", true, 0x1f),
            ("gfx-clk", false, 0x89),
            ("dgpu-skin-temp", true, 0x32),
            ("power-saving", true, 0x12),
            ("max-performance", true, 0x11),
            ("pbo-scalar", false, 0x3E),
            ("oc-clk",  false, 0x19),
            ("per-core-oc-clk", false, 0x1a),
            ("set-coall",   true, 0x4c),
            ("set-coall",   false, 0x5d),
            ("set-coper",   true, 0x4b),
            ("set-cogfx",   false, 0xb7),
            ("enable-oc",   false, 0x17),
            ("disable-oc",  false, 0x18)
        };
    }

    private static void Socket_FF3()
    {
        Smu.MP1_ADDR_MSG = 0x3B10528;
        Smu.MP1_ADDR_RSP = 0x3B10578;
        Smu.MP1_ADDR_ARG = 0x3B10998;

        Smu.PSMU_ADDR_MSG = 0x03B10a20;
        Smu.PSMU_ADDR_RSP = 0x03B10a80;
        Smu.PSMU_ADDR_ARG = 0x03B10a88;

        WindowsSMUCommands.Commands = new List<(string, bool, uint)>
        {
            // Store the commands
            ("stapm-limit",true, 0x14), // Use MP1 address
            //("stapm-limit",false , 0x31), // Use RSMU address
            ("stapm-time",true , 0x18),
            ("fast-limit",true , 0x15),
            ("slow-limit",true , 0x16),
            ("slow-time",true , 0x17),
            ("tctl-temp",true , 0x19),
            ("cHTC-temp",false , 0x37),
            ("apu-skin-temp",true , 0x33),
            ("vrm-current",true , 0x1a),
            ("vrmmax-current",true , 0x1e),
            ("vrmsoc-current",true , 0x1b),
            ("vrmsocmax-current",true , 0x1d),
            ("vrmgfx-current",true , 0x1c),
            ("vrmgfxmax-current",true , 0x1f),
            ("prochot-deassertion-ramp",true , 0x22),
            ("gfx-clk",false , 0x89),
            ("power-saving",true , 0x12),
            ("max-performance",true , 0x11),
            ("set-coall",true , 0x4c),
            ("set-coall",false , 0x5d),
            ("set-coper",true , 0x4b),
            ("set-cogfx",false , 0xb7)
        };
    }

    private static void Socket_AM4_V1()
    {
        Smu.MP1_ADDR_MSG = 0X3B10528;
        Smu.MP1_ADDR_RSP = 0X3B10564;
        Smu.MP1_ADDR_ARG = 0X3B10598;

        Smu.PSMU_ADDR_MSG = 0x03B1051C;
        Smu.PSMU_ADDR_RSP = 0X03B10568;
        Smu.PSMU_ADDR_ARG = 0X03B10590;

        WindowsSMUCommands.Commands = new List<(string, bool, uint)>
        {
            // Store the commands
            ("ppt-limit",false, 0x64), // Use RSMU address
            ("tdc-limit",false , 0x65),
            ("edc-limit",false , 0x66),
            ("tctl-temp",false , 0x68),
            ("pbo-scalar",false , 0x6a),
            ("oc-clk", false, 0x6c),
            ("per-core-oc-clk",false , 0x6d),
            ("oc-volt", false, 0x6e),
            ("enable-oc",true , 0x23),
            ("enable-oc",false , 0x6b),
            ("disable-oc",true , 0x24),
        };
    }

    private static void Socket_AM4_V2()
    {
        Smu.MP1_ADDR_MSG = 0x3B10530;
        Smu.MP1_ADDR_RSP = 0x3B1057C;
        Smu.MP1_ADDR_ARG = 0x3B109C4;

        Smu.PSMU_ADDR_MSG = 0x03B10524;
        Smu.PSMU_ADDR_RSP = 0x03B10570;
        Smu.PSMU_ADDR_ARG = 0x03B10A40;

        WindowsSMUCommands.Commands = new List<(string, bool, uint)>
        {
            // Store the commands
            ("ppt-limit",true, 0x3D), // Use MP1 address
            ("ppt-limit",false, 0x53), // Use RSMU address
            ("tdc-limit",true , 0x3B),
            ("tdc-limit",false , 0x54),
            ("edc-limit",true , 0x3c),
            ("edc-limit",false , 0x55),
            ("tctl-temp",true , 0x3E),
            ("tctl-temp",false , 0x56),
            ("pbo-scalar",false , 0x58),
            ("oc-clk", true, 0x26),
            ("oc-clk", false, 0x5c),
            ("per-core-oc-clk",true , 0x27),
            ("per-core-oc-clk",false , 0x5d),
            ("oc-volt", true, 0x28),
            ("oc-volt", false, 0x61),
            ("set-coall", true, 0x36),
            ("set-coall", false, 0xb),
            ("set-coper", true, 0x35),
            ("enable-oc",true , 0x24),
            ("enable-oc",false , 0x5a),
            ("disable-oc",true , 0x25),
            ("disable-oc",false , 0x5b),
        };
    }

    private static void Socket_AM5_V1()
    {
        Smu.MP1_ADDR_MSG = 0x3B10530;
        Smu.MP1_ADDR_RSP = 0x3B1057C;
        Smu.MP1_ADDR_ARG = 0x3B109C4;

        Smu.PSMU_ADDR_MSG = 0x03B10524;
        Smu.PSMU_ADDR_RSP = 0x03B10570;
        Smu.PSMU_ADDR_ARG = 0x03B10A40;

        WindowsSMUCommands.Commands = new List<(string, bool, uint)>
        {
            // Store the commands
            ("ppt-limit",true, 0x3e), // Use MP1 address
            ("ppt-limit",false, 0x56), // Use RSMU address
            ("tdc-limit",true , 0x3c),
            ("tdc-limit",false , 0x57),
            ("edc-limit",true , 0x3d),
            ("edc-limit",false , 0x58),
            ("tctl-temp",true , 0x3f),
            ("tctl-temp",false , 0x59),
            ("pbo-scalar",false , 0x5b),
            ("oc-clk", false, 0x5f),
            ("per-core-oc-clk",false , 0x60),
            ("oc-volt", false, 0x61),
            ("set-coall", false, 0x7),
            ("set-coper", false, 0x6),
            ("enable-oc",false , 0x5d),
            ("disable-oc",false , 0x5e),
        };
    }
}

public static class WindowsSMUCommands
{
    public static List<(string, bool, uint)> Commands;

    internal static Smu RyzenAccess = new Smu();

    public static void ApplySettings(string commandName, uint value)
    {
        uint[] Args = new uint[6];
        Args[0] = value;

        // Find the command by name
        var matchingCommands = Commands.Where(c => c.Item1 == commandName);
        if (matchingCommands.Count() > 0)
        {
            Task.WaitAll(matchingCommands.Select(command => Task.Run(async () =>
            {
                // Apply the command based on its address
                if (command.Item2)
                    RyzenAccess.SendMp1(command.Item3, ref Args);
                else
                    RyzenAccess.SendRsmu(command.Item3, ref Args);
            })).ToArray());
        }
        else throw new SmuCommandNotFound(commandName);
    }
}

internal class Smu
{
    public enum Status
    {
        BAD = 0x0,
        OK = 0x1,
        FAILED = 0xFF,
        UNKNOWN_CMD = 0xFE,
        CMD_REJECTED_PREREQ = 0xFD,
        CMD_REJECTED_BUSY = 0xFC
    }
    
    private readonly Ols _ryzenNbAccess = new();

    public Smu()
    {
        // Check WinRing0 status
        switch (_ryzenNbAccess.GetDllStatus())
        {
            case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED:
                throw new ApplicationException("WinRing OLS_DRIVER_NOT_LOADED");

            case (uint)Ols.OlsDllStatus.OLS_DLL_UNSUPPORTED_PLATFORM:
                throw new ApplicationException("WinRing OLS_UNSUPPORTED_PLATFORM");

            case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_FOUND:
                throw new ApplicationException("WinRing OLS_DLL_DRIVER_NOT_FOUND");

            case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_UNLOADED:
                throw new ApplicationException("WinRing OLS_DLL_DRIVER_UNLOADED");

            case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK:
                throw new ApplicationException("WinRing DRIVER_NOT_LOADED_ON_NETWORK");

            case (uint)Ols.OlsDllStatus.OLS_DLL_UNKNOWN_ERROR:
                throw new ApplicationException("WinRing OLS_DLL_UNKNOWN_ERROR");
        }

    }

    public void Initialize()
    {
        _amdSmuMutex = new Mutex();
        _ryzenNbAccess.InitializeOls();

        // Check WinRing0 status
        switch (_ryzenNbAccess.GetStatus())
        {
            case (uint)Ols.Status.DLL_NOT_FOUND:
                throw new ApplicationException("WinRing DLL_NOT_FOUND");
            
            case (uint)Ols.Status.DLL_INCORRECT_VERSION:
                throw new ApplicationException("WinRing DLL_INCORRECT_VERSION");
            
            case (uint)Ols.Status.DLL_INITIALIZE_ERROR:
                throw new ApplicationException("WinRing DLL_INITIALIZE_ERROR");
        }
    }

    public void Dispose()
    {
        _ryzenNbAccess.DeinitializeOls();
    }

    public static uint SMU_PCI_ADDR { get; set; }
    public static uint SMU_OFFSET_ADDR { get; set; }
    public static uint SMU_OFFSET_DATA { get; set; }

    public static uint MP1_ADDR_MSG { get; set; }
    public static uint MP1_ADDR_RSP { get; set; }
    public static uint MP1_ADDR_ARG { get; set; }

    public static uint PSMU_ADDR_MSG { get; set; }
    public static uint PSMU_ADDR_RSP { get; set; }
    public static uint PSMU_ADDR_ARG { get; set; }

    private static Mutex _amdSmuMutex;
    private const ushort SMU_TIMEOUT = 8192;

    public Status SendMp1(uint message, ref uint[] arguments)
    {
        return SendMsg(MP1_ADDR_MSG, MP1_ADDR_RSP, MP1_ADDR_ARG, message, ref arguments);
    }

    public Status SendRsmu(uint message, ref uint[] arguments)
    {
        return SendMsg(PSMU_ADDR_MSG, PSMU_ADDR_RSP, PSMU_ADDR_ARG, message, ref arguments);
    }

    public bool SendSmuCommand(uint SMU_ADDR_MSG, uint SMU_ADDR_RSP, uint SMU_ADDR_ARG, uint msg, ref uint[] args)
    {
        return (SendMsg(SMU_ADDR_MSG, SMU_ADDR_RSP, SMU_ADDR_ARG, msg, ref args) == Smu.Status.OK);
    }

    public Status SendMsg(uint SMU_ADDR_MSG, uint SMU_ADDR_RSP, uint SMU_ADDR_ARG, uint msg, ref uint[] args)
    {
        ushort timeout = SMU_TIMEOUT;
        uint[] cmdArgs = new uint[6];
        int argsLength = args.Length;
        uint status = 0;

        if (argsLength > cmdArgs.Length)
            argsLength = cmdArgs.Length;

        for (int i = 0; i < argsLength; ++i)
        {
            cmdArgs[i] = args[i];
        }

        if (_amdSmuMutex.WaitOne(5000))
        {
            // Clear response register
            bool temp;
            do
            {
                temp = SmuWriteReg(SMU_ADDR_RSP, 0);
            }
            while (!temp && --timeout > 0);

            if (timeout == 0)
            {
                _amdSmuMutex.ReleaseMutex();
                SmuReadReg(SMU_ADDR_RSP, ref status);
                return (Status)status;
            }

            // Write data
            for (int i = 0; i < cmdArgs.Length; ++i)
            {
                SmuWriteReg(SMU_ADDR_ARG + (uint)(i * 4), cmdArgs[i]);
            }

            // Send message
            SmuWriteReg(SMU_ADDR_MSG, msg);

            // Wait done
            if (!SmuWaitDone(SMU_ADDR_RSP))
            {
                _amdSmuMutex.ReleaseMutex();
                SmuReadReg(SMU_ADDR_RSP, ref status);
                return (Status)status;
            }

            // Read back args
            for (int i = 0; i < args.Length; ++i)
                SmuReadReg(SMU_ADDR_ARG + (uint)(i * 4), ref args[i]);
        }

        _amdSmuMutex.ReleaseMutex();
        SmuReadReg(SMU_ADDR_RSP, ref status);

        return (Status)status;
    }

    public bool SmuWaitDone(uint SMU_ADDR_RSP)
    {
        bool res;
        ushort timeout = SMU_TIMEOUT;
        uint data = 0;

        do
            res = SmuReadReg(SMU_ADDR_RSP, ref data);
        while ((!res || data != 1) && --timeout > 0);

        if (timeout == 0 || data != 1) res = false;

        return res;
    }


    private bool SmuWriteReg(uint addr, uint data)
    {
        if (_ryzenNbAccess.WritePciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_ADDR, addr) == 1)
        {
            return _ryzenNbAccess.WritePciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_DATA, data) == 1;
        }
        return false;
    }

    private bool SmuReadReg(uint addr, ref uint data)
    {
        if (_ryzenNbAccess.WritePciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_ADDR, addr) == 1)
        {
            return _ryzenNbAccess.ReadPciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_DATA, ref data) == 1;
        }
        return false;
    }
}

public class SmuCommandNotFound : Exception
{
    public string Command { get; }
    
    public SmuCommandNotFound(string command, string? message = null) : base(message)
    {
        Command = command;
    }
}