using System;
using System.Collections.Generic;
using System.IO;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Hardware.Info;
using Serilog;
using Universal_x86_Tuning_Utility.Linux.Interfaces;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxBatteryInfoService : IBatteryInfoService, IDisposable
{
    private readonly ILogger _logger;
    public event Action? BatteryCountChanged;
    public IReadOnlyCollection<BatteryInfo> Batteries => _batteryInfos.AsReadOnly();

    private readonly IDisposable _batteryChangedSubscription;
    private readonly HardwareInfo _hardwareInfo = new HardwareInfo();
    private readonly List<BatteryInfo> _batteryInfos = new List<BatteryInfo>();

    public LinuxBatteryInfoService(ILogger logger, ISysFsEventService sysFsEventService)
    {
        _logger = logger;

        Analyze();
        
        _batteryChangedSubscription =
            sysFsEventService.SubscribeToPath("/sys/class/power_supply")
                .Subscribe(OnBatteryChanged);
    }
    
    private void OnBatteryChanged(FileSystemEventArgs e)
    {
        Analyze();
    }

    private void Analyze()
    {
        try
        {
            for (var i = 0; i < _hardwareInfo.BatteryList.Count; i++)
            {
                var batteryInfo = new BatteryInfo(deviceId: i.ToString(),
                    rate: () => GetBatteryRate(), 
                    status: () => GetBatteryStatus(), 
                    fullChargeCapacity: () => GetFullChargeCapacity(),
                    designCapacity:  () => GetDesignCapacity(),
                    cycleCount: () => GetBatteryCycle(),
                    health: () => GetBatteryHealth());
            
                _batteryInfos.Add(batteryInfo);
            }
            
            if (_batteryInfos.Count != Batteries.Count)
            {
                BatteryCountChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery info");
            throw;
        }
    }
    
    public decimal GetBatteryRate(string? deviceId = null)
    {
        try
        {
            if (int.TryParse(deviceId, out var batteryId))
            {
                _hardwareInfo.RefreshBatteryList();

                if (_hardwareInfo.BatteryList.Count <= batteryId)
                {
                    var battery = _hardwareInfo.BatteryList[batteryId];

                    // return battery.BatteryRate;
                }
            }

            return 0;
        }
        catch
        {
            _logger.Error("Error occurred when requesting battery rate");
            throw;
        }
    }

    public BatteryStatus GetBatteryStatus(string? deviceId = null)
    {
        try
        {
            if (int.TryParse(deviceId, out var batteryId))
            {
                _hardwareInfo.RefreshBatteryList();

                if (_hardwareInfo.BatteryList.Count <= batteryId)
                {
                    var battery = _hardwareInfo.BatteryList[batteryId];

                    return battery.BatteryStatus switch
                    {
                        1 => BatteryStatus.Charging,
                        2 or 4 => BatteryStatus.Low,
                        8 => BatteryStatus.Charging,
                        128 => BatteryStatus.NoSystemBattery,
                        _ => BatteryStatus.Unknown
                    };
                }
            }

            return BatteryStatus.Unknown;
        }
        catch
        {
            _logger.Error("Error occurred when requesting battery status");
            throw;
        }
    }

    public decimal GetFullChargeCapacity(string? deviceId = null)
    {
        try
        {
            if (int.TryParse(deviceId, out var batteryId))
            {
                _hardwareInfo.RefreshBatteryList();

                if (_hardwareInfo.BatteryList.Count <= batteryId)
                {
                    var battery = _hardwareInfo.BatteryList[batteryId];

                    return battery.FullChargeCapacity;
                }
            }

            return 0;
        }
        catch
        {
            _logger.Error("Error occurred when requesting battery full charge capacity");
            throw;
        }
    }

    public decimal GetDesignCapacity(string? deviceId = null)
    {
        try
        {
            if (int.TryParse(deviceId, out var batteryId))
            {
                _hardwareInfo.RefreshBatteryList();

                if (_hardwareInfo.BatteryList.Count <= batteryId)
                {
                    var battery = _hardwareInfo.BatteryList[batteryId];

                    return battery.DesignCapacity;
                }
            }

            return 0;
        }
        catch
        {
            _logger.Error("Error occurred when requesting battery design capacity");
            throw;
        }
    }

    public int GetBatteryCycle(string? deviceId = null)
    {
        try
        {
            if (int.TryParse(deviceId, out var batteryId))
            {
                _hardwareInfo.RefreshBatteryList();

                if (_hardwareInfo.BatteryList.Count <= batteryId)
                {
                    var battery = _hardwareInfo.BatteryList[batteryId];

                    // return battery.BatteryCycle;
                }
            }

            return 0;
        }
        catch
        {
            _logger.Error("Error occurred when requesting battery design capacity");
            throw;
        }
    }

    public decimal GetBatteryHealth(string? deviceId = null)
    {
        try
        {
            if (int.TryParse(deviceId, out var batteryId))
            {
                _hardwareInfo.RefreshBatteryList();

                if (_hardwareInfo.BatteryList.Count <= batteryId)
                {
                    var designCap = GetDesignCapacity();
                    var fullCap = GetFullChargeCapacity();

                    var health = fullCap / designCap;

                    return health;
                }
            }

            return 0;
        }
        catch
        {
            _logger.Error("Error occurred when requesting battery health");
            throw;
        }
    }

    public void Dispose()
    {
        _batteryChangedSubscription.Dispose();
    }
}