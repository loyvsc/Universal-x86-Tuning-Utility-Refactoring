using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Serilog;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxFanControlService : IFanControlService, IDisposable
{
    public int MaxFanSpeed { get; private set; } = 100;
    public int MinFanSpeed { get; private set; }
    public int MinFanSpeedPercentage { get; private set; } = 25;

    public double FanSpeed { get; private set; }

    public bool IsFanControlEnabled { get; private set; }

    public bool IsFanEnabled => CheckFanEnabled();
    
    private ushort FanToggleAddress;
    private ushort FanChangeAddress;

    private byte EnableToggleAddress;
    private byte DisableToggleAddress;

    private const string FanConfigsFolderPath = "/usr/share/universal-x86-tuning-utility/Fan Configs";

    private readonly ILogger _logger;
    private readonly ISystemInfoService _systemInfoService;

    public LinuxFanControlService(ILogger logger, ISystemInfoService systemInfoService)
    {
        _logger = logger;
        _systemInfoService = systemInfoService;
    }

    public void UpdateAddresses()
    {
        string path = $"{FanConfigsFolderPath}/{_systemInfoService.Manufacturer.ToUpper()}_{_systemInfoService.Product.ToUpper()}.json";

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var dataForDevice = JsonSerializer.Deserialize<FanData>(json);

            MinFanSpeed = dataForDevice.MinFanSpeed;
            MaxFanSpeed = dataForDevice.MaxFanSpeed;
            MinFanSpeedPercentage = dataForDevice.MinFanSpeedPercentage;
            FanToggleAddress = Convert.ToUInt16(dataForDevice.FanControlAddress, 16);
            FanChangeAddress = Convert.ToUInt16(dataForDevice.FanSetAddress, 16);
            EnableToggleAddress = Convert.ToByte(dataForDevice.EnableToggleAddress, 16);
            DisableToggleAddress = Convert.ToByte(dataForDevice.DisableToggleAddress, 16);
        }
    }

    private bool CheckFanEnabled()
    {
        try
        {
            byte currentValue = ReadECByte(FanToggleAddress);
            return currentValue == EnableToggleAddress;
        }
        catch
        {
            return false;
        }
    }

    public void EnableFanControl()
    {
        try
        {
            WriteECByte(FanToggleAddress, EnableToggleAddress);
            IsFanControlEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error occurred when enabling fan control: {ex.Message}");
            throw new Exception($"Error occurred when enabling fan control. See inner exception for details", ex);
        }
    }

    public void DisableFanControl()
    {
        try
        {
            WriteECByte(FanToggleAddress, DisableToggleAddress);
            IsFanControlEnabled = false;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error occurred when disabling fan control: {ex.Message}");
            throw new Exception($"Error occurred when disabling fan control. See inner exception for details", ex);
        }
    }

    public void SetFanSpeed(int speedPercentage)
    {
        if (speedPercentage < MinFanSpeedPercentage && speedPercentage > 0)
        {
            speedPercentage = MinFanSpeedPercentage;
        }

        try
        {
            byte setValue = (byte)Math.Round((double)speedPercentage / 100 * MaxFanSpeed, 0);
            WriteECByte(FanChangeAddress, setValue);
            FanSpeed = speedPercentage;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error occurred when setting fan speed: {ex.Message}");
            throw new Exception("Error occurred when setting fan speed. See inner exception for details", ex);
        }
    }

    public void ReadFanSpeed()
    {
        try
        {
            byte returnValue = ReadECByte(FanChangeAddress);
            double fanPercentage = Math.Round(100 * (Convert.ToDouble(returnValue) / MaxFanSpeed), 0);
            FanSpeed = fanPercentage;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error occurred when reading fan speed: {ex.Message}");
            throw new Exception("Error occurred when reading fan speed. See inner exception for details", ex);
        }
    }
    
    private byte ReadECByte(ushort address)
    {
        try
        {
            string hexAddress = address.ToString("X4");
            
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ec_access",
                    Arguments = $"read {hexAddress}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                return Convert.ToByte(output.Trim(), 16);
            }

            throw new COMException($"Error reading {hexAddress} using ec_access. ExitCode: {proc.ExitCode}", proc.ExitCode);
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred when reading byte. See inner exception for details.", ex);
        }
    }

    private void WriteECByte(ushort address, byte value)
    {
        try
        {
            string hexAddress = address.ToString("X4");
            string hexValue = value.ToString("X2");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ec_access",
                    Arguments = $"write {hexAddress} {hexValue}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                throw new COMException($"Error writing {hexValue} to {hexAddress} using ec_access. ExitCode: {proc.ExitCode}", proc.ExitCode);
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred when writing byte. See inner exception for details.", ex);
        }
    }

    public void Dispose()
    {
        if (IsFanControlEnabled)
        {
            DisableFanControl();
        }
    }
}