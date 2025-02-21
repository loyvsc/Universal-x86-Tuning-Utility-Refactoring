namespace ApplicationCore.Enums;

[Flags]
public enum BatteryStatus
{
    NoSystemBattery,
    Unknown,
    Low,
    Charging,
    Discharging,
    FullCharged,
}