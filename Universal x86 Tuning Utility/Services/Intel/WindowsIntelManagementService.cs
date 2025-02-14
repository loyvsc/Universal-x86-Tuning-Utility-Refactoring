using System;
using System.IO;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.Win32;

namespace Universal_x86_Tuning_Utility.Services.Intel;

public class WindowsIntelManagementService : IIntelManagementService
{
    private const string ProcessMsr = @".\Assets\Intel\MSR\msr-cmd.exe";
    private const string ProcessKx = @".\Assets\Intel\KX\KX.exe";
    private readonly object _lock = new();

    public void ChangeTdpAll(int pl)
    {
        RunIntelTDPChangeMSR(pl, pl);
        RunIntelTDPChangeMMIOKX(pl, pl); 
    }

    public void ChangePowerBalance(int value, IntelPowerBalanceUnit powerBalanceUnit)
    {
        if (value is < 0 or > 31) throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 31");
        switch (powerBalanceUnit)
        {
            case IntelPowerBalanceUnit.Cpu:
            {
                ChangePowerBalance("0x0000063a 0x00000000", value);
                break;
            }
            case IntelPowerBalanceUnit.Gpu:
            {
                ChangePowerBalance("0x00000642 0x00000000", value);
                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(powerBalanceUnit), powerBalanceUnit, null);
        }
    }

    public void ChangeVoltageOffset(int value, IntelVoltagePlan voltagePlan)
    {
        string commandArguments = voltagePlan switch
        {
            IntelVoltagePlan.Cpu => $"-s write 0x150 0x80000011 0x{ConvertVoltageToHexMsr(value)};",
            IntelVoltagePlan.Gpu => $"-s write 0x150 0x80000111 0x{ConvertVoltageToHexMsr(value)};", // iGPU
            IntelVoltagePlan.Cache => $"-s write 0x150 0x80000211 0x{ConvertVoltageToHexMsr(value)};",
            IntelVoltagePlan.SA => $"-s write 0x150 0x80000411 0x{ConvertVoltageToHexMsr(value)};",
            _ => throw new ArgumentOutOfRangeException(nameof(voltagePlan), voltagePlan, null)
        };

        RunCliHelper.RunCommand(commandArguments, false, ProcessMsr);
    }

    public void ChangeClockRatioOffset(int[] clockRatios)
    {
        string hexValue = "";
        foreach (var clockRatio in clockRatios)
        {
            hexValue += clockRatio.ToString("X2");
        }

        string commandArguments = $"-s write 0x1AD 0x0 0x{hexValue};";
        RunCli.RunCommand(commandArguments, false, ProcessMsr);
    }

    //refactor this
    public int[] ReadClockRatios()
    {
        string output = RunCli.RunCommand("read 0x1AD;", true, ProcessMsr);

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length < 2) return Array.Empty<int>();
        
        string secondLine = lines[1];
        var parts = secondLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2) return Array.Empty<int>();
        
        string hexValue = parts[^1].Substring(2);
        
        int numberOfParts = hexValue.Length / 2;
        var hexParts = new string[numberOfParts];
        int[] intParts = new int[numberOfParts];

        for (int i = 0; i < numberOfParts; i++)
        {
            hexParts[i] = hexValue.Substring(i * 2, 2);
            intParts[i] = Convert.ToInt32(hexParts[i], 16);
        }

        return intParts;
    }
    
    public void SetGpuClock(int newGpuClock)
    {
        var clockHex = ConvertClockToHexMMIO(newGpuClock);
        var commandArguments = "/wrmem8 " + _mchbar + "5994 " + clockHex;

        RunCli.RunCommand(commandArguments, true, ProcessKx);
    }

    private void RunIntelTDPChangeMMIOKX(int pl1Tdp, int pl2Tdp)
    {
        if (pl1Tdp < 1) throw new ArgumentOutOfRangeException(nameof(pl1Tdp), "Pl1 tdp must be greater than zero");
        if (pl2Tdp < 1) throw new ArgumentOutOfRangeException(nameof(pl2Tdp), "Pl1 tdp must be greater than zero");
        
        var pl1TdpHex = ConvertTDPToHexMMIO(pl1Tdp);
        var pl2TdpHex = ConvertTDPToHexMMIO(pl2Tdp);

        var commandArgumentsPl1 = "/wrmem16 " + _mchbar + "a0 0x" + pl1TdpHex;
        RunCli.RunCommand(commandArgumentsPl1, true, ProcessKx);
        
        var commandArgumentsPl2 = "/wrmem16 " + _mchbar + "a4 0x" + pl2TdpHex;
        RunCli.RunCommand(commandArgumentsPl2, true, ProcessKx);
    }

    private void RunIntelTDPChangeMSR(int pl1Tdp, int pl2Tdp)
    {
        if (pl1Tdp < 1) throw new ArgumentOutOfRangeException(nameof(pl1Tdp), "Pl1 tdp must be greater than zero");
        if (pl2Tdp < 1) throw new ArgumentOutOfRangeException(nameof(pl2Tdp), "Pl1 tdp must be greater than zero");
        
        var pl1TdpHex = ConvertTDPToHexMSR(pl1Tdp);
        var pl2TdpHex = ConvertTDPToHexMSR(pl2Tdp);
        
        lock (_lock)
        {
            if (pl1TdpHex.Length < 3)
            {
                if (pl1TdpHex.Length == 1)
                {
                    pl1TdpHex = "00" + pl1TdpHex;
                }

                if (pl1TdpHex.Length == 2)
                {
                    pl1TdpHex = "0" + pl1TdpHex;
                }
            }
            if (pl2TdpHex.Length < 3)
            {
                if (pl2TdpHex.Length == 1)
                {
                    pl2TdpHex = "00" + pl2TdpHex;
                }

                if (pl2TdpHex.Length == 2)
                {
                    pl2TdpHex = "0" + pl2TdpHex;
                }
            }

            var commandArguments = "-s write 0x610 0x00438" + pl2TdpHex + " 0x00dd8" + pl1TdpHex;
            RunCli.RunCommand(commandArguments, false, ProcessMsr);
        }
    }
    
    //todo: add this to interface and create new functional
    private void ChangePowerBalance(string address, int value)
    {
        var hexValue = "0x" + value.ToString("X");
        var commandArguments = "-s write " + address + " " + hexValue;
        
        RunCli.RunCommand(commandArguments, false, ProcessMsr);
    }

    private void CheckDriverBlockRegistry()
    {
        var registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\CI\Config", true);
        if (registryKey != null)
        {
            if (registryKey.GetValue("VulnerableDriverBlocklistEnable") is "1")
            {
                registryKey.SetValue("VulnerableDriverBlocklistEnable", "0", RegistryValueKind.String);
            }
            registryKey.Close();
        }
    }

    public void DetermineCpu()
    {
        CheckDriverBlockRegistry();
        DetermineIntelMCHBAR();
    }

    private string _mchbar = string.Empty;

    private void DetermineIntelMCHBAR()
    {
        if (!File.Exists(ProcessKx)) return;

        string output = RunCli.RunCommand("/RdPci32 0 0 0 0x48", true, ProcessKx);

        int index = output.IndexOf("Return", StringComparison.InvariantCulture);
        
        if (index != -1)
        {
            string mchbarValue = output.Substring(index + 7);
            _mchbar = "0x" + long.Parse(mchbarValue).ToString("X2").Substring(0, 4);
        }
    }

    private string ConvertTDPToHexMMIO(int tdp)
    {
        int newTdp = tdp * 1000 / 125 + 32768;
        return newTdp.ToString("X");
    }

    private string ConvertTDPToHexMSR(int tdp)
    {
        int newTdp = tdp * 8;
        return newTdp.ToString("X");
    }

    private string ConvertVoltageToHexMsr(int volt)
    {
        double hex = volt * 1.024;
        int result = (int)Math.Round(hex) << 21;
        return result.ToString("X");
    }
    
    private string ConvertClockToHexMMIO(int value)
    {
        value /= 50;
        return "0x" + value.ToString("X2");
    }
}