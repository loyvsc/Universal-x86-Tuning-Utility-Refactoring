using ApplicationCore.Enums;
using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class AmdPowerProfile : NotifyPropertyChangedBase
{
    private AmdBoostProfile _boostPlan;
    private string _name;

    public AmdBoostProfile BoostPlan
    {
        get => _boostPlan;
        set => SetValue(ref _boostPlan, value);
    }

    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    public AmdPowerProfile()
    {
        
    }

    public AmdPowerProfile(AmdBoostProfile boostPlan, string name)
    {
        BoostPlan = boostPlan;
        Name = name;
    }
}