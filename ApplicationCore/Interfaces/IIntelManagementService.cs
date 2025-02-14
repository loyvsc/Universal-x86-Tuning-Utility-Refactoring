using ApplicationCore.Enums;

namespace ApplicationCore.Interfaces;

public interface IIntelManagementService
{
    public void ChangeTdpAll(int pl);
    public void ChangePowerBalance(int value, IntelPowerBalanceUnit powerBalanceUnit);
    public void ChangeVoltageOffset(int value, IntelVoltagePlan voltagePlan);
    public void ChangeClockRatioOffset(int[] clockRatios);
    public int[] ReadClockRatios();
    public void SetGpuClock(int newGpuClock);
    public void DetermineCpu();
}