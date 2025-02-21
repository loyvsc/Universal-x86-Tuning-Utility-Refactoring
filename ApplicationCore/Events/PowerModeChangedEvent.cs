using ApplicationCore.Enums;

namespace ApplicationCore.Events;

public delegate void PowerModeChangedEventHandler(PowerModeChangedEventArgs e);

public class PowerModeChangedEventArgs : EventArgs
{
    public BatteryStatus BatteryStatus { get; }
    public PowerMode PowerMode { get; }

    public PowerModeChangedEventArgs(BatteryStatus batteryStatus, PowerMode powerMode)
    {
        BatteryStatus = batteryStatus;
        PowerMode = powerMode;
    }
}