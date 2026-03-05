using ReactiveUI;

namespace Universal_x86_Tuning_Utility.Models;

public class BatteryModel : ReactiveObject
{
    private string _batteryHealth;
    private string _batteryCycle;
    private string _batteryCapacity;
    private string _batteryChargeRate;
    
    public string DeviceId { get; set; }
    public int Index { get; set; }
    
    public string BatteryHealth
    {
        get => _batteryHealth;
        set => this.RaiseAndSetIfChanged(ref _batteryHealth, value);
    }

    public string BatteryCycle
    {
        get => _batteryCycle;
        set => this.RaiseAndSetIfChanged(ref _batteryCycle, value);
    }

    public string BatteryCapacity
    {
        get => _batteryCapacity;
        set => this.RaiseAndSetIfChanged(ref _batteryCapacity, value);
    }

    public string BatteryChargeRate
    {
        get => _batteryChargeRate;
        set => this.RaiseAndSetIfChanged(ref _batteryChargeRate, value);
    }
}