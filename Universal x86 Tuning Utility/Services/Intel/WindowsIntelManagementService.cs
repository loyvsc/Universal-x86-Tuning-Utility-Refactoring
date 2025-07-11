using System;
using System.IO;
using System.Threading.Tasks;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.Win32;

namespace Universal_x86_Tuning_Utility.Services.Intel;

public class WindowsIntelManagementService : IIntelManagementService
{
    private readonly ICliService _cliService;
    private const string ProcessMsr = @".\Assets\Intel\MSR\msr-cmd.exe";
    private const string ProcessKx = @".\Assets\Intel\KX\KX.exe";
    private readonly object _lock = new();

    public WindowsIntelManagementService(ICliService cliService)
    {
        _cliService = cliService;
    }

    public async Task ChangeTdpAll(int pl)
    {
        RunIntelTDPChangeMSR(pl, pl);
        await RunIntelTDPChangeMMIOKX(pl, pl); 
    }

    public async Task ChangePowerBalance(int value, IntelPowerBalanceUnit powerBalanceUnit)
    {
        if (value is < 0 or > 31) throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 31");
        switch (powerBalanceUnit)
        {
            case IntelPowerBalanceUnit.Cpu:
            {
                await ChangePowerBalance("0x0000063a 0x00000000", value);
                break;
            }
            case IntelPowerBalanceUnit.Gpu:
            {
                await ChangePowerBalance("0x00000642 0x00000000", value);
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

        _cliService.RunProcess(ProcessMsr, commandArguments, false);
    }

    public async Task ChangeClockRatioOffset(int[] clockRatios)
    {
        string hexValue = string.Empty;
        foreach (var clockRatio in clockRatios)
        {
            hexValue += clockRatio.ToString("X2");
        }

        string commandArguments = $"-s write 0x1AD 0x0 0x{hexValue};";
        await _cliService.RunProcess(ProcessMsr, commandArguments, false);
    }

    //refactor this
    public async Task<int[]> ReadClockRatios()
    {
        string output = await _cliService.RunProcess(ProcessMsr, "read 0x1AD;", true);

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
    
    public async Task SetGpuClock(int newGpuClock)
    {
        var clockHex = ConvertClockToHexMMIO(newGpuClock);
        var commandArguments = "/wrmem8 " + _mchbar + "5994 " + clockHex;

        await _cliService.RunProcess(ProcessKx, commandArguments, true);
    }

    private async Task RunIntelTDPChangeMMIOKX(int pl1Tdp, int pl2Tdp)
    {
        if (pl1Tdp < 1) throw new ArgumentOutOfRangeException(nameof(pl1Tdp), "Pl1 tdp must be greater than zero");
        if (pl2Tdp < 1) throw new ArgumentOutOfRangeException(nameof(pl2Tdp), "Pl2 tdp must be greater than zero");
        
        var pl1TdpHex = ConvertTDPToHexMMIO(pl1Tdp);
        var pl2TdpHex = ConvertTDPToHexMMIO(pl2Tdp);

        var commandArgumentsPl1 = "/wrmem16 " + _mchbar + "a0 0x" + pl1TdpHex;
        await _cliService.RunProcess(ProcessKx, commandArgumentsPl1, true);
        
        var commandArgumentsPl2 = "/wrmem16 " + _mchbar + "a4 0x" + pl2TdpHex;
        await _cliService.RunProcess(ProcessKx, commandArgumentsPl2, true);
    }

    private void RunIntelTDPChangeMSR(int pl1Tdp, int pl2Tdp)
    {
        if (pl1Tdp < 1) throw new ArgumentOutOfRangeException(nameof(pl1Tdp), "Pl1 tdp must be greater than zero");
        if (pl2Tdp < 1) throw new ArgumentOutOfRangeException(nameof(pl2Tdp), "Pl2 tdp must be greater than zero");
        
        var pl1TdpHex = ConvertTDPToHexMSR(pl1Tdp);
        var pl2TdpHex = ConvertTDPToHexMSR(pl2Tdp);
        
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
        
        lock (_lock)
        {
            var commandArguments = "-s write 0x610 0x00438" + pl2TdpHex + " 0x00dd8" + pl1TdpHex;
            _cliService.RunProcess(ProcessMsr, commandArguments, false);
        }
    }
    
    private async Task ChangePowerBalance(string address, int value)
    {
        var hexValue = "0x" + value.ToString("X");
        var commandArguments = "-s write " + address + " " + hexValue;
        
        await _cliService.RunProcess(ProcessMsr, commandArguments, false);
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

    public async Task DetermineCpu()
    {
        CheckDriverBlockRegistry();
        await DetermineIntelMCHBAR();
    }

    private string _mchbar = string.Empty;

    private async Task DetermineIntelMCHBAR()
    {
        if (!File.Exists(ProcessKx)) return;

        string output = await _cliService.RunProcess(ProcessKx, "/RdPci32 0 0 0 0x48", true);

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