using ApplicationCore.Enums;
using ApplicationCore.Events;

namespace ApplicationCore.Interfaces;

public interface IPowerPlanService : IDisposable
{
    public event PowerModeChangedEventHandler PowerModeChanged;
    
    public PowerPlan CurrentPowerPlan { get; }
    
    public void SetPowerPlan(PowerPlan powerPlan);
}