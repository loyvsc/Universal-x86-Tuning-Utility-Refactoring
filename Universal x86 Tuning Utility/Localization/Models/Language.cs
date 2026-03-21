using ApplicationCore.Utilities;

namespace Universal_x86_Tuning_Utility.Localization.Models;

public class Language : NotifyPropertyChangedBase
{
    private string _name;
    private string _shortName = string.Empty;
    private string _key;

    public static readonly Language UnknownLang = new Language(string.Empty, string.Empty);

    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    public string ShortName
    {
        get => _shortName;
        set => SetValue(ref _shortName, value);
    }

    public string Key
    {
        get => _key;
        set => SetValue(ref _key, value);
    }

    public Language(string key, string name)
    {
        _key = key;
        var keyValues = key.Split('-');
        if (keyValues.Length == 2)
        {
            _shortName = keyValues[1];
        }
        _name = name;
    }
}