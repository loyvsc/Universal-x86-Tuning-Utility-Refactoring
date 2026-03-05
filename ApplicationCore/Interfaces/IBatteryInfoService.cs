using ApplicationCore.Enums;
using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IBatteryInfoService
{
    public event Action? BatteryCountChanged;
    
    public IReadOnlyCollection<BatteryInfo> Batteries { get; }
    public BatteryInfo? MainBatteryInfo => Batteries?.FirstOrDefault();
    
    /// <param name="deviceId">Target battery DeviceId. if <c>null</c> return info for primary battery</param>
    public decimal GetBatteryRate(string? deviceId = null);
    
    /// <param name="deviceId">Target battery DeviceId. if <c>null</c> return info for primary battery</param>
    public BatteryStatus GetBatteryStatus(string? deviceId = null);
    
    /// <param name="deviceId">Target battery DeviceId. if <c>null</c> return info for primary battery</param>
    public decimal GetFullChargeCapacity(string? deviceId = null);
    
    /// <param name="deviceId">Target battery DeviceId. if <c>null</c> return info for primary battery</param>
    public decimal GetDesignCapacity(string? deviceId = null);
    
    /// <param name="deviceId">Target battery DeviceId. if <c>null</c> return info for primary battery</param>
    public int GetBatteryCycle(string? deviceId = null);
    
    /// <param name="deviceId">Target battery DeviceId. if <c>null</c> return info for primary battery</param>
    public decimal GetBatteryHealth(string? deviceId = null);
}