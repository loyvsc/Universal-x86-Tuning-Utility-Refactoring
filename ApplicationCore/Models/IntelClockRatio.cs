using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class IntelClockRatio : NotifyPropertyChangedBase
{
    private double _ratio;
    
    public int CoreGroupIndex { get; set; }

    public double Ratio
    {
        get => _ratio;
        set => SetValue(ref _ratio, value);
    }
}