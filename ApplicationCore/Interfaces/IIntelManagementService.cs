using ApplicationCore.Enums;

namespace ApplicationCore.Interfaces;

public interface IIntelManagementService
{
    public Task ChangeTdpAll(int pl);
    public Task ChangePowerBalance(int value, IntelPowerBalanceUnit powerBalanceUnit);
    public void ChangeVoltageOffset(int value, IntelVoltagePlan voltagePlan);
    public Task ChangeClockRatioOffset(int[] clockRatios);
    public Task<int[]> ReadClockRatios();
    public Task SetGpuClock(int newGpuClock);
    public Task DetermineCpu();
}