using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class CcdAffinityMode : NotifyPropertyChangedBase
{
    private int _value;
    private string _name;

    public int Value
    {
        get => _value;
        set => SetValue(ref _value, value);
    }

    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    public CcdAffinityMode(int value, string name)
    {
        Value = value;
        Name = name;
    }
}