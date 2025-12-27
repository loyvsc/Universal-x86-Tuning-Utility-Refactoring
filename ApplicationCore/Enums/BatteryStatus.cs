namespace ApplicationCore.Enums;

[Flags]
public enum BatteryStatus
{
    NoSystemBattery,
    Low,
    Charging,
    Discharging,
    FullCharged,
}