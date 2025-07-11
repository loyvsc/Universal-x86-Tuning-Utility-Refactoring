using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.BatteryServices;

public class LinuxBatteryInfoService : IBatteryInfoService
{
    public decimal GetBatteryRate()
    {
        throw new System.NotImplementedException();
    }

    public BatteryStatus GetBatteryStatus()
    {
        throw new System.NotImplementedException();
    }

    public decimal ReadFullChargeCapacity()
    {
        throw new System.NotImplementedException();
    }

    public decimal ReadDesignCapacity()
    {
        throw new System.NotImplementedException();
    }

    public int GetBatteryCycle()
    {
        throw new System.NotImplementedException();
    }

    public decimal GetBatteryHealth()
    {
        throw new System.NotImplementedException();
    }
}