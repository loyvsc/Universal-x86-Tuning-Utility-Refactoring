using System;
using System.Collections.Generic;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxBatteryInfoService : IBatteryInfoService
{
    public event Action? BatteryCountChanged;
    public IReadOnlyCollection<BatteryInfo> Batteries { get; }
    public decimal GetBatteryRate(string? deviceId = null)
    {
        throw new System.NotImplementedException();
    }

    public BatteryStatus GetBatteryStatus(string? deviceId = null)
    {
        throw new System.NotImplementedException();
    }

    public decimal GetFullChargeCapacity(string? deviceId = null)
    {
        throw new System.NotImplementedException();
    }

    public decimal GetDesignCapacity(string? deviceId = null)
    {
        throw new System.NotImplementedException();
    }

    public int GetBatteryCycle(string? deviceId = null)
    {
        throw new System.NotImplementedException();
    }

    public decimal GetBatteryHealth(string? deviceId = null)
    {
        throw new System.NotImplementedException();
    }
}