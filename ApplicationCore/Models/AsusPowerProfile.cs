using ApplicationCore.Enums;
using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class AsusPowerProfile : NotifyPropertyChangedBase
{
    private string _title;

    public string Title
    {
        get => _title;
        set => SetValue(ref _title, value);
    }
    
    public AsusMode PowerProfileMode { get; set; }

    public AsusPowerProfile(string title, AsusMode powerProfileMode)
    {
        _title = title;
        PowerProfileMode = powerProfileMode;
    }
}