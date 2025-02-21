using ApplicationCore.Enums;
using ApplicationCore.Events;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.PowerPlanServices;

public class LinuxPowerPlanService : IPowerPlanService
{
    public event PowerModeChangedEventHandler? PowerModeChanged;
    public void SetPowerPlan(PowerPlan powerPlan)
    {
        throw new System.NotImplementedException();
    }
    
    public void Dispose()
    {
        throw new System.NotImplementedException();
    }
}