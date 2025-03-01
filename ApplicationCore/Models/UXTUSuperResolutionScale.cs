using ApplicationCore.Enums;
using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class UXTUSuperResolutionScale : NotifyPropertyChangedBase
{
    private ResolutionScale _resolutionScale;
    private string _name;

    public ResolutionScale ResolutionScale
    {
        get => _resolutionScale;
        set => SetValue(ref _resolutionScale, value);
    }

    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    public UXTUSuperResolutionScale()
    {
        
    }

    public UXTUSuperResolutionScale(ResolutionScale resolutionScale, string name)
    {
        ResolutionScale = resolutionScale;
        Name = name;
    }
}