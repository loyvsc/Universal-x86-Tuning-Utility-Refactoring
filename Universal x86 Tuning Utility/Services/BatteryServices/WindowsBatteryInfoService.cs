using System;
using System.Management;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace Universal_x86_Tuning_Utility.Services.BatteryServices;

public class WindowsBatteryInfoService : IBatteryInfoService, IDisposable
{
    private readonly ILogger<WindowsBatteryInfoService> _logger;
    private readonly ManagementObjectSearcher _batteryInfoSearcher;

    public WindowsBatteryInfoService(ILogger<WindowsBatteryInfoService> logger)
    {
        _logger = logger;
        _batteryInfoSearcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryStatus");
    }
    
    public decimal GetBatteryRate()
    {
        try
        {
            foreach (var obj in _batteryInfoSearcher.Get())
            {
                var chargeRate = Convert.ToDecimal(obj["ChargeRate"]);
                var dischargeRate = Convert.ToDecimal(obj["DischargeRate"]);

                return chargeRate > 0 ? chargeRate : dischargeRate;
            }
        
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when requesting battery rate");
            throw;
        }
    }
    
    public BatteryStatus GetBatteryStatus()
    {
        try
        {
            using (var batteryClass = new ManagementClass("Win32_Battery"))
            {
                using (var batteries = batteryClass.GetInstances())
                {
                    foreach (var battery in batteries)
                    {
                        if (battery["BatteryStatus"] is ushort batteryStatus)
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
            }

            return BatteryStatus.Unknown;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when requesting battery status");
            throw;
        }
    }

    public decimal ReadFullChargeCapacity()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryFullChargedCapacity"))
            {
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToDecimal(obj["FullChargedCapacity"]);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when requesting battery full charge capacity");
            throw;
        }
    }

    public decimal ReadDesignCapacity()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryStaticData"))
            {
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToDecimal(obj["DesignedCapacity"]);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when requesting battery design capacity");
            throw;
        }
    }

    public int GetBatteryCycle()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM BatteryCycleCount"))
            {
                foreach (var queryObj in searcher.Get())
                {
                    return Convert.ToInt32(queryObj["CycleCount"]);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred when requesting battery design capacity");
            throw;
        }
    }

    public decimal GetBatteryHealth()
    {
        try
        {
            var designCap = ReadDesignCapacity();
            var fullCap = ReadFullChargeCapacity();

            var health = fullCap / designCap;

            return health;
        }
        catch
        {
            _logger.LogError("Error occurred when requesting battery health");
            throw;
        }
    }

    public void Dispose()
    {
        _batteryInfoSearcher.Dispose();
    }
}