namespace ApplicationCore.Models;

public class FanData
{
    public int MinFanSpeed { get; set; }
    public int MaxFanSpeed { get; set; }
    public int MinFanSpeedPercentage { get; set; }
    public string FanControlAddress { get; set; }
    public string FanSetAddress { get; set; }
    public string EnableToggleAddress { get; set; }
    public string DisableToggleAddress { get; set; }
    public string RegAddress { get; set; }
    public string RegData { get; set; }
}