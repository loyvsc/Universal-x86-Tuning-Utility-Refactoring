namespace ApplicationCore.Models;

public class PremadePreset : Preset
{
    public string RyzenAdjParameters
    {
        get => _ryzenAdjParameters;
        set => SetValue(ref _ryzenAdjParameters, value);
    }

    public string Description
    {
        get => _description;
        set => SetValue(ref _description, value);
    }
    
    private string _ryzenAdjParameters;
    private string _description;
}