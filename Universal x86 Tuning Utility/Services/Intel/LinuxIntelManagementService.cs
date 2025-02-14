using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.Intel;

public class LinuxIntelManagementService : IIntelManagementService
{
    public void ChangeTdpAll(int pl)
    {
        throw new System.NotImplementedException();
    }

    public void ChangePowerBalance(int value, IntelPowerBalanceUnit powerBalanceUnit)
    {
        throw new System.NotImplementedException();
    }

    public void ChangeVoltageOffset(int value, IntelVoltagePlan voltagePlan)
    {
        throw new System.NotImplementedException();
    }

    public void ChangeClockRatioOffset(int[] clockRatios)
    {
        throw new System.NotImplementedException();
    }

    public int[] ReadClockRatios()
    {
        throw new System.NotImplementedException();
    }

    public void SetGpuClock(int newGpuClock)
    {
        throw new System.NotImplementedException();
    }

    public void DetermineCpu()
    {
        throw new System.NotImplementedException();
    }
}