using System;
using System.Collections.Generic;
using System.Management;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Universal_x86_Tuning_Utility.Windows.Extensions;
using Universal_x86_Tuning_Utility.Windows.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsBatteryInfoService : IBatteryInfoService, IDisposable
{
    private readonly Serilog.ILogger _logger;
    private readonly ManagementObjectSearcher _batteryStatusSearcher;
    private readonly ManagementObjectSearcher _batteryStaticDataSearcher;
    private readonly ManagementObjectSearcher _batteryFullChargedCapacitySearcher;
    private readonly ManagementObjectSearcher _batteryCycleSearcher;

    private readonly IDisposable _installDeviceSubscription;
    private readonly IDisposable _uninstallDeviceEventWatcher;

    private readonly Lazy<List<BatteryInfo>> _batteryInfo;

    public WindowsBatteryInfoService(Serilog.ILogger logger, IManagementEventService managementEventService)
    {
        _logger = logger;

        _batteryInfo = new Lazy<List<BatteryInfo>>(() => GetData());
        
        _batteryStatusSearcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM BatteryStatus");
        _batteryStaticDataSearcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM BatteryStaticData");
        _batteryFullChargedCapacitySearcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM BatteryFullChargedCapacity");
        _batteryCycleSearcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM BatteryCycleCount");
        
        _installDeviceSubscription =
            managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2")
                .Subscribe(OnDeviceChanged);
        
        _uninstallDeviceEventWatcher =
            managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3")
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
            _logger.Error(ex, "Error occurred when device changed");
        }
    }

    private List<BatteryInfo> GetData()
    {
        try
        {
            var batteries = new List<BatteryInfo>();
            
            foreach (var obj in _batteryStaticDataSearcher.Get())
            {
                var deviceId = obj.Get<string>("InstanceName");

                if (!string.IsNullOrEmpty(deviceId))
                {
                    var batteryInfo = new BatteryInfo(
                        deviceId: deviceId,
                        rate: () => GetBatteryRate(deviceId),
                        status: () => GetBatteryStatus(deviceId),
                        fullChargeCapacity: () => GetFullChargeCapacity(deviceId),
                        designCapacity: () => GetDesignCapacity(deviceId),
                        cycleCount: () => GetBatteryCycle(deviceId),
                        health: () => GetBatteryHealth(deviceId));

                    batteries.Add(batteryInfo);
                }
            }

            return batteries;
        }
        catch (ManagementException mEx)
        {
            if (mEx.ErrorCode != ManagementStatus.InvalidClass)
            {
                _logger.Error(mEx, "Error occurred when requesting battery info");
            }
            return [];
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery info");
            return [];
        }
    }

    public decimal GetBatteryRate(string? deviceId = null)
    {
        try
        {
            var targetBattery = _batteryStatusSearcher.Find(x =>
            {
                var batteryDeviceId = x.Get<string>("InstanceName");
                return string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId?.Contains(deviceId) == true;
            });

            if (targetBattery != null)
            {
                var chargeRate = targetBattery.Get<decimal>("ChargeRate");
                var dischargeRate = targetBattery.Get<decimal>("DischargeRate");

                return chargeRate > 0 ? chargeRate : -dischargeRate;
            }
        }
        catch (ManagementException mEx)
        {
            if (mEx.ErrorCode != ManagementStatus.InvalidClass)
            {
                _logger.Error(mEx, "Error occurred when requesting battery rate");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery rate");
        }
        
        return 0;
    }
    
    public BatteryStatus GetBatteryStatus(string? deviceId = null)
    {
        try
        {
            var targetBattery = _batteryCycleSearcher.Find(x =>
            {
                var batteryDeviceId = x.Get<string>("InstanceName");
                return string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId?.Contains(deviceId) == true;
            });
            if (targetBattery != null)
            {
                var batteryDeviceId = targetBattery.Get<string>("InstanceName");
                
                var fullChargeCapacity = _batteryFullChargedCapacitySearcher
                    .Find(x => x.Properties["InstanceName"].Value.ToString() == batteryDeviceId)
                    ?.Get<decimal>("FullChargedCapacity") ?? 0;
                var remainingCapacity = targetBattery.Get<decimal>("RemainingCapacity");
                var chargingRate = targetBattery.Get<decimal>("ChargingRate");
                var dischargeRate = targetBattery.Get<decimal>("DischargeRate");
                    
                if (chargingRate == 0 && dischargeRate == 0) return BatteryStatus.FullCharged;
                if (chargingRate > 0) return BatteryStatus.Charging;
                if (remainingCapacity <= fullChargeCapacity * 0.15M) return BatteryStatus.Low;
                if (chargingRate < 0) return BatteryStatus.Discharging;
            }
        }
        catch (ManagementException mEx)
        {
            if (mEx.ErrorCode != ManagementStatus.InvalidClass)
            {
                _logger.Error(mEx, "Error occurred when requesting battery status");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery status");
        }
        
        return BatteryStatus.NoSystemBattery;
    }

    public decimal GetFullChargeCapacity(string? deviceId = null)
    {
        try
        {
            return _batteryFullChargedCapacitySearcher.Find(x => 
                {
                    var batteryDeviceId = x.Get<string>("InstanceName");
                    return string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId?.Contains(deviceId) == true;
                })
                ?.Get<decimal>("FullChargedCapacity") ?? 0;
        }
        catch (ManagementException mEx)
        {
            if (mEx.ErrorCode != ManagementStatus.InvalidClass)
            {
                _logger.Error(mEx, "Error occurred when requesting battery full charge capacity");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery full charge capacity");
        }
        return 0;
    }

    public decimal GetDesignCapacity(string? deviceId = null)
    {
        try
        {
            return _batteryStaticDataSearcher.Find(x => 
                {
                    var batteryDeviceId = x.Get<string>("InstanceName");
                    return string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId?.Contains(deviceId) == true;
                })
                ?.Get<decimal>("DesignedCapacity") ?? 0;
        }
        catch (ManagementException mEx)
        {
            if (mEx.ErrorCode != ManagementStatus.InvalidClass)
            {
                _logger.Error(mEx, "Error occurred when requesting battery design capacity");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery design capacity");
        }
        return 0;
    }

    public int GetBatteryCycle(string? deviceId = null)
    {
        try
        {
            return _batteryCycleSearcher.Find(x => 
                {
                    var batteryDeviceId = x.Get<string>("InstanceName");
                    return string.IsNullOrWhiteSpace(deviceId) || batteryDeviceId?.Contains(deviceId) == true;
                })
                ?.Get<int>("CycleCount") ?? 0;
        }
        catch (ManagementException mEx)
        {
            if (mEx.ErrorCode != ManagementStatus.InvalidClass)
            {
                _logger.Error(mEx, "Error occurred when requesting battery cycle count");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery cycle count");
        }
        return 0;
    }

    public decimal GetBatteryHealth(string? deviceId = null)
    {
        try
        {
            var designCap = GetDesignCapacity(deviceId);
            var fullCap = GetFullChargeCapacity(deviceId);

            if (designCap > 0)
            {
                var health = fullCap / designCap;
                return health;
            }

            return 0;
        }
        catch (ManagementException mEx)
        {
            if (mEx.ErrorCode != ManagementStatus.InvalidClass)
            {
                _logger.Error(mEx, "Error occurred when requesting battery health");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when requesting battery health");
        }
        return 0;
    }

    public void Dispose()
    {
        _batteryStatusSearcher.Dispose();
        _batteryStaticDataSearcher.Dispose();
        _batteryFullChargedCapacitySearcher.Dispose();
        _batteryCycleSearcher.Dispose();
        _installDeviceSubscription.Dispose();
        _uninstallDeviceEventWatcher.Dispose();
    }
}