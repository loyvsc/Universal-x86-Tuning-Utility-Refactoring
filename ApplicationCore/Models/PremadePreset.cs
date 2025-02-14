using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class PremadePreset : NotifyPropertyChangedBase
{
    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    public string RyzenAdjParameters
    {
        get => _ryzenAdjParameters;
        set => SetValue(ref _ryzenAdjParameters, value);
    }

    private string _name;
    private string _ryzenAdjParameters;
}