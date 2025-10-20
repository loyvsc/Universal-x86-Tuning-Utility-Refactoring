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

    public string IconPath
    {
        get => _iconPath;
        set => SetValue(ref _iconPath, value);
    }

    public string AverageFps
    {
        get => _averageFps;
        set => SetValue(ref _averageFps, value);
    }

    public string AverageMillisecond
    {
        get => _averageMillisecond;
        set => SetValue(ref _averageMillisecond, value);
    }
    
    private string _averageFps;
    private string _averageMillisecond;
    private string _iconPath;

    private const string NoData = "No Data";

    public void SetAverageFps(ICollection<double>? values)
    {
        if (values != null && values.Count != 0)
        {
            var average = values.Average(x => x);
            AverageFps = $"{average} FPS";
        }
        else
        {
            AverageFps = NoData;
        }
    }

    public void SetAverageMs(ICollection<double>? values)
    {
        if (values != null && values.Count != 0)
        {
            var average = values.Average(x => x);
            AverageMillisecond = $"{average} ms";
        }
        else
        {
            AverageMillisecond = NoData;
        }
    }

    public void RaiseIconChanged()
    {
        OnPropertyChanged(nameof(IconPath));
    }
}