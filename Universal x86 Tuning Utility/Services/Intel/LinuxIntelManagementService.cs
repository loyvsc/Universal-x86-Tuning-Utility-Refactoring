using System.Threading.Tasks;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.Intel;

public class LinuxIntelManagementService : IIntelManagementService
{
    public Task ChangeTdpAll(int pl)
    {
        throw new System.NotImplementedException();
    }

    public Task ChangePowerBalance(int value, IntelPowerBalanceUnit powerBalanceUnit)
    {
        throw new System.NotImplementedException();
    }

    public void ChangeVoltageOffset(int value, IntelVoltagePlan voltagePlan)
    {
        throw new System.NotImplementedException();
    }

    public Task ChangeClockRatioOffset(int[] clockRatios)
    {
        throw new System.NotImplementedException();
    }

    public Task<int[]> ReadClockRatios()
    {
        throw new System.NotImplementedException();
    }

    public Task SetGpuClock(int newGpuClock)
    {
        throw new System.NotImplementedException();
    }

    public Task DetermineCpu()
    {
        throw new System.NotImplementedException();
    }
}