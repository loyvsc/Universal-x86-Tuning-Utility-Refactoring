using ApplicationCore.Enums;

namespace ApplicationCore.Interfaces;

public interface IBatteryInfoService
{
    public decimal GetBatteryRate();
    public BatteryStatus GetBatteryStatus();
    public decimal ReadFullChargeCapacity();
    public decimal ReadDesignCapacity();
    public int GetBatteryCycle();
    public decimal GetBatteryHealth();
}