namespace ApplicationCore.Models;

public class GameData
{
    public string GameName { get; init; }
    
    public string FpsData { get; set; } = "No Data";
    public string FpsAverageData { get; set; } = "0,0,0,0,0,0,0,0,0,0";
    public string MsData { get; set; } = "No Data";
    public string MsAverageData { get; set; } = "0,0,0,0,0";
}