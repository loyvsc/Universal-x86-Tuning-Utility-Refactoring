using ApplicationCore.Enums;
using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class PowerMode : NotifyPropertyChangedBase
{
    private PowerPlan _powerPlan;
    private string _name;

    public PowerPlan PowerPlan
    {
        get => _powerPlan;
        set => SetValue(ref _powerPlan, value);
    }

    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    public PowerMode()
    {
        
    }

    public PowerMode(PowerPlan powerPlan, string name)
    {
        PowerPlan = powerPlan;
        Name = name;
    }
}