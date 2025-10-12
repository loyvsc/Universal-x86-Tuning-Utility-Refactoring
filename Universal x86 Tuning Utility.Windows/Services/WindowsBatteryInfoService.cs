using System;
using System.Collections.Generic;
using System.Management;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Universal_x86_Tuning_Utility.Windows.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsBatteryInfoService : IBatteryInfoService, IDisposable
{
    private readonly Serilog.ILogger _logger;
    private readonly IManagementEventService _managementEventService;
    private readonly ManagementObjectSearcher _batteryInfoSearcher;

    private readonly IDisposable _installDeviceSubscription;
    private readonly IDisposable _uninstallDeviceEventWatcher;

    private readonly Lazy<List<BatteryInfo>> _batteryInfo;

    public WindowsBatteryInfoService(Serilog.ILogger logger, IManagementEventService managementEventService)
    {
        _logger = logger;
        _managementEventService = managementEventService;

        _batteryInfo = new Lazy<List<BatteryInfo>>(() => GetData());
        
        _batteryInfoSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Battery");
        _installDeviceSubscription =
            _managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2")
                .Subscribe(OnDeviceChanged);
        
        _uninstallDeviceEventWatcher =
            _managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3")
                .Subscribe(OnDeviceChanged);
    }
    
    public event Action? BatteryCountChanged;

    public IReadOnlyCollection<BatteryInfo> Batteries => _batteryInfo.Value;

    private void OnDeviceChanged(EventArrivedEventArgs e)
    {
        try
        {
            if (_batteryInfo.IsValueCreated)
            {
                var initialCount = _batteryInfo.Value.Count;
                _batteryInfo.Value.Clear();
                _batteryInfo.Value.AddRange(GetData());
                
                if (_batteryInfo.Value.Count != initialCount)
                {
                    BatteryCountChanged?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when device chan");
            throw;
        }
    }

    private List<BatteryInfo> GetData()
    {
        try
        {
            var batteries = new List<BatteryInfo>();
            foreach (var obj in _batteryInfoSearcher.Get())
            {
                var batteryInfo = new BatteryInfo(deviceId: obj["DeviceID"].ToString(),
                    rate: () => GetBatteryRate(),
                    status: () => GetBatteryStatus(),
                    fullChargeCapacity: () => GetFullChargeCapacity(),
                    designCapacity: () => GetDesignCapacity(),
                    cycleCount: () => GetBatteryCycle(),
                    health: () => GetBatteryHealth());
                batteries.Add(batteryInfo);
            }

            return batteries;
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
            foreach (var obj in _batteryInfoSearcher.Get())
            {
                var batteryDeviceId = obj["DeviceID"].ToString();

                if (string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId == deviceId)
                {
                    var chargeRate = Convert.ToDecimal(obj["ChargeRate"]);
                    var dischargeRate = Convert.ToDecimal(obj["DischargeRate"]);

                    return chargeRate > 0 ? chargeRate : dischargeRate;
                }
            }
        
            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery rate");
            throw;
        }
    }
    
    public BatteryStatus GetBatteryStatus(string? deviceId = null)
    {
        try
        {
            foreach (var obj in _batteryInfoSearcher.Get())
            {
                var batteryDeviceId = obj["DeviceID"].ToString();

                if (string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId == deviceId)
                {
                    if (obj["BatteryStatus"] is ushort batteryStatus)
                    {
                        return batteryStatus switch
                        {
                            1 => BatteryStatus.Discharging,
                            3 => BatteryStatus.FullCharged,
                            4 or 5 => BatteryStatus.Low,
                            2 or 7 or 8 or 9 or 11 => BatteryStatus.Charging,
                            _ => BatteryStatus.Unknown
                        };
                    }
                }
            }

            return BatteryStatus.NoSystemBattery;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery status");
            throw;
        }
    }

    public decimal GetFullChargeCapacity(string? deviceId = null)
    {
        try
        {
            foreach (var obj in _batteryInfoSearcher.Get())
            {
                var batteryDeviceId = obj["DeviceID"].ToString();

                if (string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId == deviceId)
                {
                    return Convert.ToDecimal(obj["FullChargedCapacity"]);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery full charge capacity");
            throw;
        }
    }

    public decimal GetDesignCapacity(string? deviceId = null)
    {
        try
        {
            foreach (var obj in _batteryInfoSearcher.Get())
            {
                var batteryDeviceId = obj["DeviceID"].ToString();

                if (string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId == deviceId)
                {
                    return Convert.ToDecimal(obj["DesignedCapacity"]);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery design capacity");
            throw;
        }
    }

    public int GetBatteryCycle(string? deviceId = null)
    {
        try
        {
            foreach (var obj in _batteryInfoSearcher.Get())
            {
                var batteryDeviceId = obj["DeviceID"].ToString();

                if (string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId == deviceId)
                {
                    return Convert.ToInt32(obj["CycleCount"]);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery design capacity");
            throw;
        }
    }

    /// <param name="deviceId">Target battery DeviceId. if <c>null</c> return info for primary battery</param>
    public decimal GetBatteryHealth(string? deviceId = null)
    {
        try
        {
            foreach (var obj in _batteryInfoSearcher.Get())
            {
                var batteryDeviceId = obj["DeviceID"].ToString();

                if (string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId == deviceId)
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
        _batteryInfoSearcher.Dispose();
        _installDeviceSubscription.Dispose();
        _uninstallDeviceEventWatcher.Dispose();
    }
}