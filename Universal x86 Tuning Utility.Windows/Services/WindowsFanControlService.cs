using System;
using System.IO;
using System.Text.Json;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Universal_x86_Tuning_Utility.Windows.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsFanControlService : IFanControlService
{
    public int MaxFanSpeed { get; private set; } = 100;
    public int MinFanSpeed { get; private set; }
    public int MinFanSpeedPercentage  { get; private set; } = 25;

    public double FanSpeed { get; private set; }

    public bool IsFanControlEnabled { get; private set; }

    public bool IsFanEnabled => _winRingEcManagementService.ECRamRead(_fanToggleAddress) == 0;
    
    private ushort _fanToggleAddress;
    private ushort _fanChangeAddress;

    private byte _enableToggleAddress;
    private byte _disableToggleAddress;

    private ushort _regAddress;
    private ushort _regData;
    
    private readonly ISystemInfoService _systemInfoService;
    private readonly IWinRingEcManagementService _winRingEcManagementService;
    private readonly Serilog.ILogger _logger;

    public WindowsFanControlService(Serilog.ILogger logger, ISystemInfoService systemInfoService, IWinRingEcManagementService winRingEcManagementService)
    {
        _logger = logger;
        _systemInfoService = systemInfoService;
        _winRingEcManagementService = winRingEcManagementService;
    }

    private const string FanConfigsFolderPath = @"\Assets\Fan Configs";

    public void UpdateAddresses()
    {
        string path = $@"{FanConfigsFolderPath}\{_systemInfoService.Manufacturer.ToUpper()}_{_systemInfoService.Product.ToUpper()}.json";

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var dataForDevice = JsonSerializer.Deserialize<FanData>(json);

            if (dataForDevice != null)
            {
                // Access data for the device
                MinFanSpeed = dataForDevice.MinFanSpeed;
                MaxFanSpeed = dataForDevice.MaxFanSpeed;
                MinFanSpeedPercentage = dataForDevice.MinFanSpeedPercentage;
                _fanToggleAddress = Convert.ToUInt16(dataForDevice.FanControlAddress, 16);
                _fanChangeAddress = Convert.ToUInt16(dataForDevice.FanSetAddress, 16);
                _enableToggleAddress = Convert.ToByte(dataForDevice.EnableToggleAddress, 16);
                _disableToggleAddress = Convert.ToByte(dataForDevice.DisableToggleAddress, 16);

                _regAddress = Convert.ToUInt16(dataForDevice.RegAddress, 16);
                _regData = Convert.ToUInt16(dataForDevice.RegData, 16);

                _winRingEcManagementService.RegAddress = _regAddress;
                _winRingEcManagementService.RegData = _regData;
                _logger.Information("Config {manufacturer}_{product} ", _systemInfoService.Manufacturer.ToUpper(), _systemInfoService.Product.ToUpper());
            }
            else
            {
                _logger.Error("Incorrect fan config at {path}", path);
            }
        }
    }

    public void EnableFanControl()
    {
        _winRingEcManagementService.ECRamWrite(_fanToggleAddress, _enableToggleAddress);
        IsFanControlEnabled = true;
        _logger.Information("Fan control enabled");
    }

    public void DisableFanControl()
    {
        _winRingEcManagementService.ECRamWrite(_fanToggleAddress, _disableToggleAddress);
        IsFanControlEnabled = false;
        _logger.Information("Fan control disabled");
    }

    public void SetFanSpeed(int speedPercentage)
    {
        if (speedPercentage < MinFanSpeedPercentage && speedPercentage > 0)
        {
            speedPercentage = MinFanSpeedPercentage;
        }

        byte setValue = (byte)Math.Round((double)speedPercentage / 100 * MaxFanSpeed, 0);
        _winRingEcManagementService.ECRamWrite(_fanChangeAddress, setValue);

        FanSpeed = speedPercentage;
        _logger.Information("Fan speed changed to {fanSpeed}", FanSpeed);
    }

    public void ReadFanSpeed()
    {
        byte returnValue = _winRingEcManagementService.ECRamRead(_fanChangeAddress);

        double fanPercentage = Math.Round(100 * (Convert.ToDouble(returnValue) / MaxFanSpeed), 0);
        FanSpeed = fanPercentage;
        _logger.Information("Fan speed has been read: {fanSpeed}", FanSpeed);
    }

    public void Dispose()
    {
        DisableFanControl();
    }
}