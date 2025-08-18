using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class BatteryInfo
{
    public string DeviceId { get; }
    public Lazy<decimal> Rate { get; }
    public Lazy<BatteryStatus> Status { get; }
    public Lazy<decimal> FullChargeCapacity { get; }
    public Lazy<decimal> DesignCapacity { get; }
    public Lazy<int> CycleCount { get; }
    public Lazy<decimal> Health { get; }

    public BatteryInfo(string deviceId, 
        Func<decimal> rate, Func<BatteryStatus> status, 
        Func<decimal> fullChargeCapacity, Func<decimal> designCapacity,
        Func<int> cycleCount, Func<decimal> health)
    {
        DeviceId = deviceId;
        Rate = new Lazy<decimal>(rate);
        Status = new Lazy<BatteryStatus>(status);
        FullChargeCapacity = new Lazy<decimal>(fullChargeCapacity);
        DesignCapacity = new Lazy<decimal>(designCapacity);
        CycleCount = new Lazy<int>(cycleCount);
        Health = new Lazy<decimal>(health);
    }
}