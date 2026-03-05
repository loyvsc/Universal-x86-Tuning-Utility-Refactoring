namespace ApplicationCore.Models;

public class GameData
{
    public string GameName { get; init; }

    public List<double> FpsData { get; } = [];
    public List<double> MsData { get; } = [];
}