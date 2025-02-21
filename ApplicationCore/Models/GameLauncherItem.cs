using ApplicationCore.Enums;
using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class GameLauncherItem : NotifyPropertyChangedBase
{
    public string GameId { get; set; }
    public GameType GameType { get; set; }
    public string GameName { get; set; }
    public string Path { get; set; }
    public string Executable { get; set; }
    public string ImageLocation { get; set; }
    public string IconPath { get; set; }

    public string FpsData
    {
        get => _fpsData;
        set => SetValue(ref _fpsData, value);
    }

    public string MillisecondData
    {
        get => _millisecondData;
        set => SetValue(ref _millisecondData, value);
    }
    
    private string _fpsData;
    private string _millisecondData;
}