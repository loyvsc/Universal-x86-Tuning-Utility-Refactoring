using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class AmdCcdCoreState : NotifyPropertyChangedBase
{
    /// <summary>
    /// From 0 to infinity
    /// </summary>
    public int CoreIndex { get; set; }
    
    private bool _isEnabled;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetValue(ref _isEnabled, value);
    }

    private int _value;

    public int Value
    {
        get => _value;
        set => SetValue(ref _value, value);
    }
}